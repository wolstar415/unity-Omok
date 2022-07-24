using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    [SerializeField] private GameObject textPrefab; // 채팅프리팹
    [SerializeField] private Transform textParent; //채팅 생성시킬곳
    [SerializeField] private TMP_InputField inputField;//채팅 입력칸
    [SerializeField] private TextMeshProUGUI roomNameText;//방이름
    [SerializeField] private Transform playerparent; //플레이어 목록
    [SerializeField] private string[] players; //플레이어 목록들
    
    [SerializeField] private List<GameObject> textobs;

    [SerializeField] private TextMeshProUGUI player1;
    [SerializeField] private TextMeshProUGUI player2;
    [SerializeField] private TextMeshProUGUI player1_record;
    [SerializeField] private TextMeshProUGUI player2_record;

    public GameObject startBtn;
    
    public TextMeshProUGUI gameInfo;


    private void Start()
    {
        SocketManager.inst.socket.OnUnityThread("ChatOn", data =>
        //채팅을 시작합니다.
        {
            GameManager.inst.loadingOb.SetActive(false);
            //로딩 끄게합니다.
            
            players = JsonConvert.DeserializeObject<String[]>(data.GetValue(0).ToString());
            PlayerReSet();
            //플레이어 목록을 받고 설정합니다.
        });
        SocketManager.inst.socket.OnUnityThread("PlayerReset", data =>
        //플레이어 목록을 갱신합니다. 나가거나 들어올 때 방안에있는 사람들이 받는 이벤트입니다.
        {
            players = JsonConvert.DeserializeObject<String[]>(data.GetValue(0).ToString());
            PlayerReSet();

            if (GameManager.inst.Player1==data.GetValue(1).ToString())
            {
                if (omokManager.inst.IsPlay)
                {
                    if (GameManager.inst.Player2==GameManager.inst.nickName)
                    {
                        
                        
                        Victory();
                        
                    }
                }
                gameInfo.text = "게임시작 전";

                

                //GameManager.inst.Player1 = "";
                //player1.text = "";
                //player1_record.text = "";
            }
            else if (GameManager.inst.Player2==data.GetValue(1).ToString())
            {
                if (omokManager.inst.IsPlay)
                {
                    if (GameManager.inst.Player1==GameManager.inst.nickName)
                    {
                        
                        Victory();
                    }
                }

                gameInfo.text = "게임시작 전";
                //GameManager.inst.Player2 = "";
                //player2.text = "";
                //player2_record.text = "";
            }
        });
        SocketManager.inst.socket.OnUnityThread("LeaveRoom", data =>
        //방을 나갑니다
        {
            
            GameManager.inst.loadingOb.SetActive(false);
            
            GameManager.inst.lobyManager.RoomReset();
            
            
            for (int i = 0; i < omokManager.inst.SIZE; i++)
            {
                for (int j = 0; j < omokManager.inst.SIZE; j++)
                {
                    if (omokManager.inst.ball[i,j]==0)
                    {
                        continue;
                    }

                    omokManager.inst.ballScan(j, i).GetComponent<omokClick>().Clear();
                }
            }

            omokManager.inst.myPlayType = ePlayType.None;

            //방을 나갔으니 방갱신을 해야합니다.
        });
        
        SocketManager.inst.socket.OnUnityThread("ChatGet",
            data => { ChatGet(data.GetValue(0).ToString(), data.GetValue(1).ToString()); });
        //채팅을 받고 올리는 이벤트입니다.
        
        SocketManager.inst.socket.OnUnityThread("PlayerChagne1", data =>
        {
            if (data.GetValue(0).ToString()=="")
            {
                GameManager.inst.Player1 = "";
                player1.text = "";
                player1_record.text = "";
            }
            else
            {
                GameManager.inst.Player1 = data.GetValue(0).GetProperty("name").ToString();
                player1.text = GameManager.inst.Player1;
                int victory=data.GetValue(0).GetProperty("victory").GetInt32();
                int defeat=data.GetValue(0).GetProperty("defeat").GetInt32();
                player1_record.text = $"{victory}승 {defeat}패";
            }
        });
        SocketManager.inst.socket.OnUnityThread("PlayerChagne2", data =>
        {
            if (data.GetValue(0).ToString()=="")
            {
                GameManager.inst.Player2 = "";
                player2.text = "";
                player2_record.text = "";
            }
            else
            {
                GameManager.inst.Player2 = data.GetValue(0).GetProperty("name").ToString();
                player2.text = GameManager.inst.Player2;
                int victory=data.GetValue(0).GetProperty("victory").GetInt32();
                int defeat=data.GetValue(0).GetProperty("defeat").GetInt32();
                player2_record.text = $"{victory}승 {defeat}패";
            }
        });
        
        SocketManager.inst.socket.OnUnityThread("LeavePlayer", data =>
        {
            if (GameManager.inst.Player1==data.GetValue(0).GetString())
            {
                if (GameManager.inst.Player2==GameManager.inst.nickName&&omokManager.inst.IsPlay)
                {
                    Victory();
                }
                gameInfo.text = "게임시작 전";
            }
            else if (GameManager.inst.Player2==data.GetValue(0).GetString())
            {
                if (GameManager.inst.Player1==GameManager.inst.nickName&&omokManager.inst.IsPlay)
                {
                    Victory();
                }
                gameInfo.text = "게임시작 전";
            }
        });
        
        
        SocketManager.inst.socket.OnUnityThread("PlayerEnter", data =>
            //플레이어 목록을 갱신합니다. 나가거나 들어올 때 방안에있는 사람들이 받는 이벤트입니다.
        {
            players = JsonConvert.DeserializeObject<String[]>(data.GetValue(0).ToString());
            PlayerReSet();
            
            if (GameManager.inst.Player1==GameManager.inst.nickName)
            {
                string s = JsonConvert.SerializeObject(omokManager.inst.ball);
                SocketManager.inst.socket.Emit("EnterFunc",s,player1.text,player2.text,player1_record.text,player2_record.text,data.GetValue(1).ToString(),omokManager.inst.IsPlay,omokManager.inst.IsBlackTurn);
            }
            else if (GameManager.inst.Player1 == "" && GameManager.inst.Player2 == GameManager.inst.nickName)
            {
                string s = JsonConvert.SerializeObject(omokManager.inst.ball);
                SocketManager.inst.socket.Emit("EnterFunc",s,player1.text,player2.text,player1_record.text,player2_record.text,data.GetValue(1).ToString(),omokManager.inst.IsPlay,omokManager.inst.IsBlackTurn);
            }
        });
        
        SocketManager.inst.socket.OnUnityThread("EnterFunc", data =>
            //플레이어 목록을 갱신합니다. 나가거나 들어올 때 방안에있는 사람들이 받는 이벤트입니다.
        {
                GameManager.inst.Player1 = data.GetValue(1).GetString();
                GameManager.inst.Player2 = data.GetValue(2).GetString();
                player1.text = GameManager.inst.Player1;
                player2.text = GameManager.inst.Player2;
                player1_record.text = data.GetValue(3).GetString();
                player2_record.text = data.GetValue(4).GetString();
                omokManager.inst.ball = JsonConvert.DeserializeObject<int[,]>(data.GetValue(0).ToString());
                omokManager.inst.StoneSetting();
                omokManager.inst.IsPlay = data.GetValue(6).GetBoolean();
                omokManager.inst.IsBlackTurn = data.GetValue(7).GetBoolean();

                if (omokManager.inst.IsPlay)
                {
                    if (omokManager.inst.IsBlackTurn)
                    {
                        gameInfo.text = "흑 차례입니다";
                    }
                    else
                    {
                        gameInfo.text = "백 차례입니다";
                    }
                }
                
                

        });
    }

    public void LeaveBtn()
    //나가기 버튼을 누를 때
    {
        if (omokManager.inst.IsPlay&&omokManager.inst.myPlayType!=ePlayType.None)
        {
            Defeat();
        }
        SocketManager.inst.socket.Emit("LeaveRoomCheck", GameManager.inst.room, players.Length);
        GameManager.inst.chatOb.SetActive(false);
        GameManager.inst.joinOb.SetActive(true);
        GameManager.inst.loadingOb.SetActive(true);
        GameManager.inst.room = "";
        GameManager.inst.IsChat = false;
        
        
        
        
        
        
        
        
        
    }

    public void PlayerMove(int idx)
    {
        if (idx==1)
        {
            if (GameManager.inst.Player1!="")
            {
                return;
            }

            if (GameManager.inst.Player2==GameManager.inst.nickName)
            {
                GameManager.inst.Player2 = "";
            }

            GameManager.inst.Player1 = GameManager.inst.nickName;
            startBtn.SetActive(true);
            
            
        }
        else
        {
            if (GameManager.inst.Player2!="")
            {
                return;
            }
            if (GameManager.inst.Player1==GameManager.inst.nickName)
            {
                GameManager.inst.Player1 = "";
                startBtn.SetActive(false);
            }

            GameManager.inst.Player2 = GameManager.inst.nickName;
        }
        
        SocketManager.inst.socket.Emit("PlayerCheck", GameManager.inst.room,GameManager.inst.Player1,GameManager.inst.Player2);
    }

    private void PlayerReSet()
    {
        for (int i = 0; i < 8; i++)
        {
            playerparent.GetChild(i).GetComponent<TextMeshProUGUI>().text = "";
        }

        for (int i = 0; i < players.Length; i++)
        {
            playerparent.GetChild(i).GetComponent<TextMeshProUGUI>().text = players[i];
        }
    }

    public void OnEndEditEventMethod()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            UpdateChat();
        }
    }

    
    public void ChatStart()
    //채팅 실행
    {
        GameManager.inst.Player1 = "";
        GameManager.inst.Player2 = "";
        player1.text = "";
        player2.text = "";
        player1_record.text = "";
        player2_record.text = "";
        
        gameInfo.text = "게임시작 전";
        startBtn.SetActive(false);
        roomNameText.text = GameManager.inst.room;
        GameManager.inst.IsChat = true;
        for (int i = 0; i < 8; i++)
        {
            playerparent.GetChild(i).GetComponent<TextMeshProUGUI>().text = "";
        }
        if (textobs.Count > 0)
        {
            for (int i = 0; i < textobs.Count; i++)
            {
                Destroy(textobs[i]);
            }
        }
        textobs.Clear();
        //기존에 있던 채팅 모두 삭제합니다.
        GameManager.inst.loadingOb.SetActive(true);
        //로딩
        SocketManager.inst.socket.Emit("ChatCheck", GameManager.inst.room);
        
    }

    public void UpdateChat()
    //채팅을 입력시 이벤트
    {
        if (inputField.text.Equals(""))
        {
            return;
        }
        //아무것도없다면 리턴

        GameObject ob = Instantiate(textPrefab, textParent);
        ob.GetComponent<TextMeshProUGUI>().text = $"<color=red>{GameManager.inst.nickName} </color>: {inputField.text}";
        textobs.Add(ob);
        
        SocketManager.inst.socket.Emit("Chat", GameManager.inst.nickName, inputField.text, GameManager.inst.room);
        //딴사람들에게도 채팅내용을 받아야하니 이벤트를 보냅니다.
        
        inputField.text = "";
    }

    public void ChatGet(string nickname, string text)
    //다른사람들이 채팅 이벤트 받으면 생성시킵니다.
    {
        GameObject ob = Instantiate(textPrefab, textParent);
        textobs.Add(ob);
        ob.GetComponent<TextMeshProUGUI>().text = $"{nickname} : {text}";
    }

    private void Update()
    {
        if (GameManager.inst.IsChat)
        {
            if (Input.GetKeyDown(KeyCode.Return) && inputField.isFocused == false)
            {
                inputField.ActivateInputField();
            }
        }

    }
    public void Victory()
    {
        GameManager.inst.victory++;
        SocketManager.inst.socket.Emit("Victory", GameManager.inst.ID, GameManager.inst.victory,GameManager.inst.room);
        GameManager.inst.lobyManager.recordText.text = $"{GameManager.inst.victory}승 {GameManager.inst.defeat}패";
        if (GameManager.inst.Player1==GameManager.inst.nickName)
        {
            startBtn.SetActive(true);
        }
        GameManager.inst.Warning("승리 했습니다.");
        omokManager.inst.myPlayType = ePlayType.None;
        omokManager.inst.IsPlay = false;
    }
    public void Defeat()
    {
        GameManager.inst.defeat++;
        SocketManager.inst.socket.Emit("Defeat", GameManager.inst.ID, GameManager.inst.defeat,GameManager.inst.room);
        GameManager.inst.lobyManager.recordText.text = $"{GameManager.inst.victory}승 {GameManager.inst.defeat}패";
    }
}