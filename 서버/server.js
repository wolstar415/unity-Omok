
const express = require('express');
var mysql = require('mysql');
const dbconfig = require('./config/config.js');
const sql = mysql.createConnection(dbconfig);
const bcrypt = require("bcrypt");
const app = express();
const http = require('http');
const server = http.createServer(app);
const port = 7777;
const { Server } = require("socket.io");
const e = require('express');
const io = new Server(server);





io.use((socket, next) => {
  if (socket.handshake.query.token === "UNITY" && socket.handshake.query.version === "0.1") {
    next();
  } else {
    next(new Error("인증 오류 "));
  }
});
var Users = [];
var Rooms = [];

io.on('connection', socket => {
  Users[socket.id] = {
    id: socket.id,
    loginID: "",
    nickname: "",
    Room: "",
    victory: 0,
    defeat: 0
  }




  function RoomResetGo() {

    var roomcheck = [];

    for (room in Rooms) {
      roomcheck.push({
        currentCnt: Rooms[room].currentCnt,
        RoomMaxCnt: Rooms[room].maxCnt,
        name: room
      })

    }
    io.emit('RoomReset', roomcheck)
  }

  socket.on('LoginCheck', (id, password) => {
    sql.query('SELECT * FROM users WHERE ID=?', [id], function (error, results) {
      if (error) throw error;
      if (results.length > 0) {

        if (bcrypt.compareSync(password, results[0].PassWord)) {
          //암호화된 비밀번호를 비교합니다.

          var check = true;
          for (var k in Users) {
            if (Users[k].loginID == id)
              check = false;
            break;
          }

          //현재 접속된 아이디가 있는지 파악합니다.
          if (check) {
            console.log(id + ": 로그인 성공")
            Users[socket.id].loginID = id;
            Users[socket.id].nickname = results[0].nickName;
            Users[socket.id].victory = results[0].victory
            Users[socket.id].defeat = results[0].defeat
            socket.emit('Login', results[0].nickName, results[0].victory, results[0].defeat)
            sql.query('UPDATE users SET loginlasttime=? WHERE ID=? ', [new Date(), id], function (error, results) {
              {
                if (error)
                  console.log(error)
              }
            })

          }
          else {
            //동일한 아이디가 들어왔으니 오류
            console.log("중복 불가!")
            socket.emit('LoginFailed2')
          }


        }
        else {
          //비밀번호가 틀리면 오류
          console.log("비밀번호 틀림")
          socket.emit('LoginFailed')
        }



      } else {
        //해당 아이디가 없으면 오류
        console.log("로그인 실패")
        socket.emit('LoginFailed')
      }
    })
  })
  socket.on('CreateCheck', (id, password) => {
    //회원가입 체크

    sql.query('SELECT * FROM users WHERE ID=?', [id], function (error, results) {
      //회원가입 하기전에 아이디 중복확인을 합니다.
      if (error) throw error;
      if (results.length > 0) {
        //아이디가 이미 있다면 실패
        console.log("회원가입 실패")
        socket.emit('CreateFailed')
      }
      else {
        sql.query('INSERT INTO users (ID, PassWord, createTime) VALUES(?,?,?)', [id, bcrypt.hashSync(password, 10), new Date()], function (error, results) {
          if (error) {
            console.log(error)
          }
          else {
            console.log(id + ": 회원가입 성공")
            socket.emit('Create')
          }
        })

      }
    })
  })

  socket.on('NickNameCheck', (nickName, id) => {

    sql.query('SELECT * FROM users WHERE nickName=?', [nickName], function (error, results) {
      //닉네임을 데이터베이스에서 찾습니다.
      if (error) throw error;
      if (results.length > 0) {
        //해당 닉네임이있다면 오류
        socket.emit('NickNameFailed')
        console.log("닉네임 겹침")
      } else {


        sql.query('UPDATE users SET nickName=? WHERE ID=? ', [nickName, id], function (error, results) {
          //데이터베이스에 수정합니다.
          {
            if (error) {
              console.log(error)
            }
            else {

              Users[socket.id].nickname = nickName;
              console.log(nickName + ": 닉네임 설정 성공")
              socket.emit('NickName')

            }
          }
        })
      }
    })
  })




  socket.on('JoinRoomCheck', (roomname) => {

    if (roomname in Rooms && Rooms[roomname].currentCnt < Rooms[roomname].maxCnt) {

      socket.join(roomname)
      socket.emit('Join', roomname)
      Users[socket.id].Room = roomname
      Rooms[roomname].currentCnt++

      var check = []
      socket.adapter.rooms.get(roomname).forEach((a) => {
        check.push(Users[a].nickname)
      })

      socket.to(roomname).emit('PlayerEnter', check, socket.id)
      RoomResetGo()
    }
    else {
      socket.emit('JoinFailed')
    }
  })




  socket.on('EnterFunc', (stone, player1, player2, player1_record, player2_record, id, bool1, bool2, infotext) => {
    socket.to(id).emit('EnterFunc', stone, player1, player2, player1_record, player2_record, id, bool1, bool2, infotext)
  })


  socket.on('CreateRoomCheck', (data, data2) => {
    if (data in Rooms) {
      //방이 있는지 없는지 확인
      console.log(" 방이름 겹침!")
      socket.emit('CreateRoomFailed')
      //방생성 실패
    }
    else {
      //방생성 성공
      socket.join(data);
      //들어갑니다.

      Users[socket.id].Room = data


      Rooms[data] = {
        currentCnt: 1,
        maxCnt: Number(data2),
        Player1: "",
        Player2: "",
      }

      console.log(data + ": 방진입 성공!")

      socket.emit('CreateRoom')
      //성공했다고 이벤트를 보냅니다.


      RoomResetGo()
      //방 목록을 전부 보내는 이벤트를 실행합니다.
    }

  })
  socket.on('LeaveRoomCheck', (data, data2) => {
    //방을 나갑니다

    socket.leave(data)
    //leave를 사용합니다.

    if (Number(data2) <= 1) {
      //현재 방인원이 1이라면 삭제를 시킵니다.
      delete Rooms[Users[socket.id].Room]
    }
    else {

      Rooms[data].currentCnt--
      //방 인원 하나 뺍니다


      var check = []
      socket.adapter.rooms.get(data).forEach((a) => {
        check.push(Users[a].nickname)
      })


      //현재 방인원 플레이어 목록을 갱신 시켜줍니다.


      if (Rooms[Users[socket.id].Room].Player1 == Users[socket.id].nickname) {
        Rooms[Users[socket.id].Room].Player1 = "";
      }
      else if (Rooms[Users[socket.id].Room].Player2 == Users[socket.id].nickname) {
        Rooms[Users[socket.id].Room].Player2 = "";
      }
      var player1name = ""
      var player2name = ""
      var player1record = ""
      var player2record = ""

      if (Rooms[Users[socket.id].Room].Player1 != "") {
        for (var a in Users) {
          if (Rooms[Users[socket.id].Room].Player1 == Users[a].nickname) {
            player1name = Users[a].nickname
            player1record = Users[a].victory + "승 " + Users[a].defeat + "패"
            break;
          }
        }
      }
      if (Rooms[Users[socket.id].Room].Player2 != "") {
        for (var a in Users) {
          if (Rooms[Users[socket.id].Room].Player2 == Users[a].nickname) {
            player2name = Users[a].nickname
            player2record = Users[a].victory + "승 " + Users[a].defeat + "패"
            break;
          }
        }
      }


      socket.to(data).emit('PlayerReset', check, player1name, player2name, player1record, player2record)

    }
    RoomResetGo()
    socket.emit('LeaveRoom')




    Users[socket.id].Room = ""
  })
  socket.on('RoomListCheck', (data) => {
    if (socket.adapter.rooms.size == 1) {
      return
    }
    var roomcheck = [];

    for (room in Rooms) {
      roomcheck.push({
        currentCnt: Rooms[room].currentCnt,
        RoomMaxCnt: Rooms[room].maxCnt,
        name: room
      })

    }

    console.log(roomcheck)
    socket.emit('RoomList', roomcheck)
  })


  socket.on('Chat', (nick, text, room) => {
    //채팅을 보냅니다.

    console.log(nick + ": " + text)
    socket.to(room).emit('ChatGet', nick, text)
    //보인을 제외한 방에 존재한 사람들에게 보냅니다.

  })
  socket.on('ChatCheck', (data) => {


    var check = []
    socket.adapter.rooms.get(data).forEach((a) => {
      check.push(Users[a].nickname)
    })
    //방안에있는 플레이어들의 목록을 불러옵니다.


    socket.emit('ChatOn', check)
  })
  console.log("연결함 : " + socket.id);



  function playercheckFunc(roomname) {
    var p1 = {
      name: "",
      victory: 0,
      defeat: 0
    }
    var p2 = {
      name: "",
      victory: 0,
      defeat: 0
    }
    if (Rooms[roomname].Player1 == "") {

      io.to(roomname).emit('PlayerChagne1', "")

    }
    else {

      sql.query('SELECT * FROM users WHERE nickName=?', [Rooms[roomname].Player1], function (error, results) {
        if (error) throw error;
        if (results.length > 0) {
          p1.name = Rooms[roomname].Player1
          p1.victory = results[0].victory
          p1.defeat = results[0].defeat

          io.to(roomname).emit('PlayerChagne1', p1)

        }
      })
    }

    if (Rooms[roomname].Player2 == "") {
      io.to(roomname).emit('PlayerChagne2', "")

    }
    else {
      sql.query('SELECT * FROM users WHERE nickName=?', [Rooms[roomname].Player2], function (error, results) {
        if (error) throw error;
        if (results.length > 0) {
          p2.name = Rooms[roomname].Player2
          p2.victory = results[0].victory
          p2.defeat = results[0].defeat


          io.to(roomname).emit('PlayerChagne2', p2)

        }
      })
    }




  }


  socket.on('PlayerCheck', (roomname, player1, player2) => {

    Rooms[roomname].Player1 = player1
    Rooms[roomname].Player2 = player2

    playercheckFunc(roomname)
  })

  socket.on('Victory', (id, cnt, roomname) => {

    sql.query('UPDATE users SET victory=? WHERE ID=? ', [cnt, id], function (error, results) {
      {
        if (error) {
          console.log(error)
        }
        else {
          console.log(id + ": 승리 추가")
          playercheckFunc(roomname)
        }
      }
    })


  })

  socket.on('GameStart', (ran) => {


    io.to(Users[socket.id].Room).emit('GameStart', ran)

  })

  socket.on('Defeat', (id, cnt, roomname) => {

    sql.query('UPDATE users SET defeat=? WHERE ID=? ', [cnt, id], function (error, results) {
      {
        if (error) {
          console.log(error)
        }
        else {
          console.log(id + ": 패배 추가 " + cnt)
          playercheckFunc(roomname)
        }
      }
    })


  })


  socket.on('Turn', (stone, row, column, enemyName) => {

    socket.to(Users[socket.id].Room).emit('Turn', stone, row, column, enemyName)


  })

  socket.on('GameEnd', (victory, defeat) => {

    io.to(Users[socket.id].Room).emit('GameEnd', victory, defeat)


  })

  socket.on('disconnect', zz => {
    console.log("연결끊김 : " + socket.id);
    //console.log(Users[socket.id].Room);
    if (Users[socket.id].Room != "") {
      //해당 유저가 방안이라면 
      if (Rooms[Users[socket.id].Room].currentCnt == 1) {
        //인원이 1이라면 삭제
        delete Rooms[Users[socket.id].Room]
      }
      else {
        Rooms[Users[socket.id].Room].currentCnt--

        var nick = Users[socket.id].nickname

        if (Rooms[Users[socket.id].Room].Player1 == nick) {
          Rooms[Users[socket.id].Room].Player1 = ""


          //socket.to(Users[socket.id].Room).emit('LeavePlayer', nick)
        }
        else if (Rooms[Users[socket.id].Room].Player2 == nick) {
          Rooms[Users[socket.id].Room].Player2 = ""
        }
        console.log(Rooms[Users[socket.id].Room].Player1)
        console.log(Rooms[Users[socket.id].Room].Player2)


        //룸 인원 빼기


        var check = []
        socket.adapter.rooms.get(Users[socket.id].Room).forEach((a) => {
          check.push(Users[a].nickname)
        })



        var player1name = ""
        var player2name = ""
        var player1record = ""
        var player2record = ""

        if (Rooms[Users[socket.id].Room].Player1 != "") {
          for (var a in Users) {
            if (Rooms[Users[socket.id].Room].Player1 == Users[a].nickname) {
              player1name = Users[a].nickname
              player1record = Users[a].victory + "승 " + Users[a].defeat + "패"
              break;
            }
          }
        }
        if (Rooms[Users[socket.id].Room].Player2 != "") {
          for (var a in Users) {
            if (Rooms[Users[socket.id].Room].Player2 == Users[a].nickname) {
              player2name = Users[a].nickname
              player2record = Users[a].victory + "승 " + Users[a].defeat + "패"
              break;
            }
          }
        }

        socket.to(Users[socket.id].Room).emit('PlayerReset', check, player1name, player2name, player1record, player2record)


        //그 안에있는 사람들 플레이어 목록 갱신



        RoomResetGo()


        //playercheckFunc(Users[socket.id].Room)

        //방 리셋 



      }
    }

    delete Users[socket.id]
    //유저 정보 삭제
  })



});




server.listen(port, () => {
  console.log('listening on *:' + port);
});




function victoryAdd(id) {
  Users[id].victory++
  sql.query('UPDATE users SET victory=? WHERE ID=? ', [Users[id].victory, Users[id].nickname], function (error, results) {
    {
      if (error)
        console.log(error)
    }
  })
}
function defeatAdd(id) {
  Users[id].defeat++
  sql.query('UPDATE users SET defeat=? WHERE ID=? ', [Users[id].defeat, Users[id].nickname], function (error, results) {
    {
      if (error)
        console.log(error)
    }
  })
}