using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum ePlayType
{
    None,
    BLACK,
    WHITE
}

public class omokManager : MonoBehaviour
{
    public Sprite[] dollImg;
    public static omokManager inst;
    public ePlayType myPlayType;
    public bool IsPlay = false;

    public readonly int SIZE = 15;
    public bool IsBlackTurn = false;
    public List<GameObject> noObs;

    public int[,] ball = new int[,]
    {
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    };

    public GameObject[] ballOb;

    public GameObject ballScan(int row, int column)
    {
        return ballOb[row + (column * SIZE)];
    }

    private void Awake()
    {
        inst = this;
    }

    private void Start()
    {
        SocketManager.inst.socket.OnUnityThread("GameEnd", data =>
            //플레이어 목록을 갱신합니다. 나가거나 들어올 때 방안에있는 사람들이 받는 이벤트입니다.
        {
            String victory = data.GetValue(0).GetString();
            String defeat = data.GetValue(1).GetString();

            if (GameManager.inst.nickName==victory)
            {
                GameManager.inst.chatManager.Victory();
                GameManager.inst.Warning("승리 했습니다.");
            }
            else if(GameManager.inst.nickName==defeat)
            {
                GameManager.inst.chatManager.Defeat();
                GameManager.inst.Warning("패배 했습니다.");
            }

            GameManager.inst.chatManager.gameInfo.text = $"{victory}님께서 승리 했습니다.";
            IsPlay = false;
            myPlayType = ePlayType.None;
            if (GameManager.inst.Player1==GameManager.inst.nickName)
            {
                GameManager.inst.chatManager.startBtn.SetActive(true);
            }
        });
        
        SocketManager.inst.socket.OnUnityThread("Turn", data =>
            //플레이어 목록을 갱신합니다. 나가거나 들어올 때 방안에있는 사람들이 받는 이벤트입니다.
        {
            int row = data.GetValue(1).GetInt32();
            int column = data.GetValue(2).GetInt32();

            if (data.GetValue(0).GetString()=="Black")
            {
                ball[column, row] = 1;
                ballScan(row,column).GetComponent<omokClick>().ballClick(1);
            }
            else
            {
                ball[column, row] = 2;
                ballScan(row,column).GetComponent<omokClick>().ballClick(2);
                
            }
            GameManager.inst.chatManager.gameInfo.text = $"{data.GetValue(3).GetString()}님의 차례입니다.";
            TurnChange();
        });
        SocketManager.inst.socket.OnUnityThread("GameStart", data =>
            //플레이어 목록을 갱신합니다. 나가거나 들어올 때 방안에있는 사람들이 받는 이벤트입니다.
        {
            GameStart(data.GetValue(0).GetInt32());
        });
    }

    public void StartBtn()
    {
        if (GameManager.inst.Player1==GameManager.inst.nickName&&GameManager.inst.Player2!=""&&IsPlay==false)
        {
            GameManager.inst.chatManager.startBtn.SetActive(false);

            int ran = Random.Range(0, 2);
            
            SocketManager.inst.socket.Emit("GameStart",ran);
            
        }
    }

    public void StoneSetting()
    {
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                if (ball[i,j]==1||ball[i,j]==2)
                {
                    ballScan(j, i).GetComponent<omokClick>().ballClick(ball[i,j]);
                }
            }
        }
    }


    void GameStart(int num)
    {
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                if (ball[i,j]==0)
                {
                    continue;
                }

                ballScan(j, i).GetComponent<omokClick>().Clear();
            }
        }
        
        IsPlay = true;
        IsBlackTurn = true;
        if (num==0)
        {
            if (GameManager.inst.Player1==GameManager.inst.nickName)
            {
                myPlayType = ePlayType.BLACK;
                
            }
            else if (GameManager.inst.Player2==GameManager.inst.nickName)
            {
                myPlayType = ePlayType.WHITE;
            }
            GameManager.inst.chatManager.gameInfo.text = $"{GameManager.inst.Player1}님의 차례입니다.";
        }
        else
        {
            if (GameManager.inst.Player1==GameManager.inst.nickName)
            {
                myPlayType = ePlayType.WHITE;
            }
            else if (GameManager.inst.Player2==GameManager.inst.nickName)
            {
                myPlayType = ePlayType.BLACK;
            }
            GameManager.inst.chatManager.gameInfo.text = $"{GameManager.inst.Player2}님의 차례입니다.";
        }
        
    }

    public void BallClick(int row, int column)
    {
        if (IsBlackTurn)
        {
            ball[column, row] = 1;
        }
        else
        {
            ball[column, row] = 2;
        }

        if (VictoryCheck(row, column))
        {
            //Debug.Log("게임오버");
            //GameManager.inst.chatManager.Victory();
            //GameManager.inst.Warning("게임을 승리 했습니다.");

            if (GameManager.inst.Player1==GameManager.inst.nickName)
            {
                SocketManager.inst.socket.Emit("GameEnd",GameManager.inst.Player1,GameManager.inst.Player2);
            }
            else if (GameManager.inst.Player2==GameManager.inst.nickName)
            {
                SocketManager.inst.socket.Emit("GameEnd",GameManager.inst.Player2,GameManager.inst.Player1);
            }
            //IsPlay = false;
        }
        else
        {
            string enemyName = GameManager.inst.Player1 == GameManager.inst.nickName
                ? GameManager.inst.Player2
                : GameManager.inst.Player1;
            if (IsBlackTurn)
            {
                
                SocketManager.inst.socket.Emit("Turn","Black",row,column,enemyName);
            }
            else
            {
                SocketManager.inst.socket.Emit("Turn","White",row,column,enemyName);
            }

            GameManager.inst.chatManager.gameInfo.text = $"{enemyName}님의 차례입니다.";
            TurnChange();
        }
    }

    bool VictoryCheck(int row, int column)
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        int ballNum = 1;
        if (!IsBlackTurn)
        {
            ballNum = 2;
        }


        if (FiveCheck(ballNum))
        {
            return true;
        }


        sw.Stop();
        Debug.Log(sw.ElapsedMilliseconds + "ms");
        return false;
    }

    bool InRange(params int[] v)
    {
        for (int i = 0; i < v.Length; i++)
            if (!(v[i] >= 0 && v[i] < SIZE))
                return false;

        return true;
    }

    bool FiveCheck(int ballNum)
    {
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                if (ball[i, j] != ballNum)
                {
                    continue;
                }

                //→
                if (InRange(j + 4) && ball[i, j + 1] == ballNum && ball[i, j + 2] == ballNum &&
                    ball[i, j + 3] == ballNum && ball[i, j + 4] == ballNum)
                {
                    return true;
                }
                //↓
                else if (InRange(i + 4) && ball[i + 1, j] == ballNum && ball[i + 2, j] == ballNum &&
                         ball[i + 3, j] == ballNum && ball[i + 4, j] == ballNum)
                {
                    return true;
                }
                //↘
                else if (InRange(i + 4, j + 4) && ball[i + 1, j + 1] == ballNum && ball[i + 2, j + 2] == ballNum &&
                         ball[i + 3, j + 3] == ballNum && ball[i + 4, j + 4] == ballNum)
                {
                    return true;
                }
                //↙
                else if (InRange(i + 4, j - 4) && ball[i + 1, j - 1] == ballNum && ball[i + 2, j - 2] == ballNum &&
                         ball[i + 3, j - 3] == ballNum && ball[i + 4, j - 4] == ballNum)
                {
                    return true;
                }
            }
        }

        return false;
    }

    
    
    void SixCheck(int column, int row)
    {
        if (ball[column, row] != 0)
        {
            return;
        }

        ball[column, row] = 1;
        int ballNum = 1;
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                if (ball[i, j] != 1)
                {
                    continue;
                }

                //→
                if (InRange(j + 5) && ball[i, j + 1] == ballNum && ball[i, j + 2] == ballNum &&
                    ball[i, j + 3] == ballNum && ball[i, j + 4] == ballNum && ball[i, j + 5] == ballNum)
                {
                    ball[column, row] = 3;
                    ballScan(row, column).GetComponent<omokClick>().ballClick(3);
                    return;
                }
                //↓
                else if (InRange(i + 5) && ball[i + 1, j] == ballNum && ball[i + 2, j] == ballNum &&
                         ball[i + 3, j] == ballNum && ball[i + 4, j] == ballNum && ball[i + 5, j] == ballNum)
                {
                    ball[column, row] = 3;
                    ballScan(row, column).GetComponent<omokClick>().ballClick(3);
                    return;
                }
                //↘
                else if (InRange(i + 5, j + 5) && ball[i + 1, j + 1] == ballNum && ball[i + 2, j + 2] == ballNum &&
                         ball[i + 3, j + 3] == ballNum && ball[i + 4, j + 4] == ballNum &&
                         ball[i + 5, j + 5] == ballNum)
                {
                    ball[column, row] = 3;
                    ballScan(row, column).GetComponent<omokClick>().ballClick(3);
                    return;
                }
                //↙
                else if (InRange(i + 5, j - 5) && ball[i + 1, j - 1] == ballNum && ball[i + 2, j - 2] == ballNum &&
                         ball[i + 3, j - 3] == ballNum && ball[i + 4, j - 4] == ballNum &&
                         ball[i + 5, j - 5] == ballNum)
                {
                    ball[column, row] = 3;
                    ballScan(row, column).GetComponent<omokClick>().ballClick(3);
                    return;
                }
            }
        }

        ball[column, row] = 0;
    }


    public void NoClear()
    {
        if (myPlayType != ePlayType.BLACK)
        {
            return;
        }

        for (int i = 0; i < noObs.Count; i++)
        {
            if (noObs[i].TryGetComponent(out omokClick click))
            {
                click.NoOff();
            }
        }

        noObs.Clear();
    }

    public void TurnChange()
    {


        IsBlackTurn = !IsBlackTurn;
        if (IsBlackTurn)
        {
            //흑턴
            
            omokCheck();
        }
        else
        {
            //백턴
            NoClear();
        }
    }

    public void omokCheck()
    {
        if (myPlayType != ePlayType.BLACK)
        {
            return;
        }

        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                SixCheck(i, j);
            }
        }


        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                if (ball[i, j] != 0)
                {
                    continue;
                }

                ThreeThreeCheck(i, j);

            }
        }
    }


    bool NoClickCheck(int i, int j)
    {
        return false;
    }


    void ThreeThreeCheck(int i, int j)
    {
        if (ball[i, j] != 0)
        {
            return;
        }

        ball[i, j] = 1;
        int ThreeValue = 0;

        if (FiveCheck(1))
        {
            ball[i, j] = 0;
            return;
        }

        ;

        //→
        if (ThreeCheck(i, j - 4, i, j - 3, i, j - 2, i, j - 1, i, j + 1, i, j + 2, i, j + 3, i, j + 4)) ++ThreeValue;

        //↓
        if (ThreeCheck(i - 4, j, i - 3, j, i - 2, j, i - 1, j, i + 1, j, i + 2, j, i + 3, j, i + 4, j)) ++ThreeValue;

        //↘
        if (ThreeCheck(i - 4, j - 4, i - 3, j - 3, i - 2, j - 2, i - 1, j - 1, i + 1, j + 1, i + 2, j + 2, i + 3, j + 3,
                i + 4, j + 4)) ++ThreeValue;

        //↙
        if (ThreeCheck(i + 4, j - 4, i + 3, j - 3, i + 2, j - 2, i + 1, j - 1, i - 1, j + 1, i - 2, j + 2, i - 3, j + 3,
                i - 4, j + 4)) ++ThreeValue;


        if (ThreeValue >= 2)
        {
            ball[i, j] = 3;
            ballScan(j, i).GetComponent<omokClick>().ballClick(3);
        }
        else
        {
            ball[i, j] = 0;
        }
    }


    bool ThreeCheck(int im4, int jm4, int im3, int jm3, int im2, int jm2, int im1, int jm1, int ip1, int jp1, int ip2,
        int jp2, int ip3, int jp3, int ip4, int jp4)
    {
        if (InRange(im4, jm4, ip1, jp1))
            if (ball[im4, jm4] == 0 && ball[im3, jm3] == 1 && ball[ip1, jp1] == 0)
            {
                if (ball[im2, jm2] == 1 && ball[im1, jm1] == 0) return true;
                if (ball[im2, jm2] == 0 && ball[im1, jm1] == 1) return true;
            }

        if (InRange(im3, jm3, ip1, jp1))
            if (ball[im3, jm3] == 0 && ball[im2, jm2] == 1 && ball[im1, jm1] == 1 && ball[ip1, jp1] == 0)
                return true;

        if (InRange(im3, jm3, ip2, jp2))
            if (ball[im3, jm3] == 0 && ball[im2, jm2] == 1 && ball[im1, jm1] == 0 && ball[ip1, jp1] == 1 &&
                ball[ip2, jp2] == 0)
                return true;

        if (InRange(im2, jm2, ip2, jp2)) // 중앙
            if (ball[im2, jm2] == 0 && ball[im1, jm1] == 1 && ball[ip1, jp1] == 1 && ball[ip2, jp2] == 0)
                return true;

        if (InRange(im2, jm2, ip3, jp3))
            if (ball[im2, jm2] == 0 && ball[im1, jm1] == 1 && ball[ip1, jp1] == 0 && ball[ip2, jp2] == 1 &&
                ball[ip3, jp3] == 0)
                return true;

        if (InRange(im1, jm1, ip3, jp3))
            if (ball[im1, jm1] == 0 && ball[ip1, jp1] == 1 && ball[ip2, jp2] == 1 && ball[ip3, jp3] == 0)
                return true;

        if (InRange(im1, jm1, ip4, jp4))
            if (ball[im1, jm1] == 0 && ball[ip3, jp3] == 1 && ball[ip4, jp4] == 0)
            {
                if (ball[ip1, jp1] == 1 && ball[ip2, jp2] == 0) return true;
                if (ball[ip1, jp1] == 0 && ball[ip2, jp2] == 1) return true;
            }

        return false;
    }
}