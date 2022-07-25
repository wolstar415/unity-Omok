using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    public TextMeshProUGUI nickname;
    public TMP_InputField loginInputField; //아이디 필드
    public TMP_InputField passWordInputField; //비밀번호 필드

    public TMP_InputField loginCheckInputField; //회원가입 아이디 필드
    public TMP_InputField passWordCheck1InputField; //회원가입 비밀번호 필드
    public TMP_InputField passWordCheck2InputField; //회원가입 비밀번호 필드

    public TMP_InputField nickNameInputField; //닉네임 필드

    public void LoginBtn()
        //접속 버튼 누르면 실행
    {
        if (loginInputField.text == "" || passWordInputField.text == "")
        {
            return;
        }


        GameManager.inst.loadingOb.SetActive(true);

        SocketManager.inst.socket.Emit("LoginCheck", loginInputField.text, passWordInputField.text);
    }

    public void CreateBtn()
        //접속 버튼 누르면 실행
    {
        if (loginCheckInputField.text == "" || passWordCheck1InputField.text == "" ||
            passWordCheck2InputField.text == "")
        {
            return;
        }

        if (passWordCheck1InputField.text != passWordCheck2InputField.text)
        {
            GameManager.inst.Warning("비밀번호가 맞지 않습니다.");
            return;
        }


        GameManager.inst.loadingOb.SetActive(true);

        SocketManager.inst.socket.Emit("CreateCheck", loginCheckInputField.text, passWordCheck1InputField.text);
    }

    public void NickNameBtn()
        //접속 버튼 누르면 실행
    {
        if (nickNameInputField.text == "")
        {
            return;
        }


        GameManager.inst.loadingOb.SetActive(true);

        SocketManager.inst.socket.Emit("NickNameCheck", nickNameInputField.text, GameManager.inst.ID);
    }

    // Start is called before the first frame update
    void Start()
    {
        loginInputField.Select();


        SocketManager.inst.socket.OnUnityThread("Login", data =>
        {
            GameManager.inst.nickName = loginInputField.text;
            GameManager.inst.loadingOb.SetActive(false);
            GameManager.inst.loginOb.SetActive(false);

            GameManager.inst.victory = data.GetValue(1).GetInt32();
            GameManager.inst.defeat = data.GetValue(2).GetInt32();

            GameManager.inst.lobyManager.recordText.text = $"{GameManager.inst.victory}승 {GameManager.inst.defeat}패";

            GameManager.inst.ID = loginInputField.text;
            GameManager.inst.nickName = data.GetValue(0).GetString();

            if (GameManager.inst.nickName == "")
            {
                nickNameInputField.Select();
                GameManager.inst.nickNameSetob.SetActive(true);
            }
            else
            {
                GameManager.inst.joinOb.SetActive(true);
                SocketManager.inst.socket.Emit("RoomListCheck", null);
                nickname.text = GameManager.inst.nickName;
            }
        });
        SocketManager.inst.socket.OnUnityThread("LoginFailed", data =>
        {
            GameManager.inst.loadingOb.SetActive(false);
            GameManager.inst.Warning("아이디와 비밀번호를 확인해주세요");
        });
        SocketManager.inst.socket.OnUnityThread("LoginFailed2", data =>
        {
            GameManager.inst.loadingOb.SetActive(false);
            GameManager.inst.Warning("이미 들어와있습니다.");
        });
        SocketManager.inst.socket.OnUnityThread("Create", data =>
        {
            loginInputField.text = loginCheckInputField.text;
            loginCheckInputField.text = "";
            passWordCheck1InputField.text = "";
            passWordCheck2InputField.text = "";
            GameManager.inst.loadingOb.SetActive(false);
            GameManager.inst.IdCreateOb.SetActive(false);
            GameManager.inst.loginOb.SetActive(true);

            passWordInputField.text = "";
            passWordInputField.Select();
        });
        SocketManager.inst.socket.OnUnityThread("CreateFailed", data =>
        {
            GameManager.inst.loadingOb.SetActive(false);
            GameManager.inst.Warning("해당 아이디가 존재합니다.");
        });
        SocketManager.inst.socket.OnUnityThread("NickName", data =>
        {
            GameManager.inst.nickName = nickNameInputField.text;
            GameManager.inst.loadingOb.SetActive(false);
            GameManager.inst.nickNameSetob.SetActive(false);
            GameManager.inst.joinOb.SetActive(true);
            nickname.text = GameManager.inst.nickName;
        });
        SocketManager.inst.socket.OnUnityThread("NickNameFailed", data =>
        {
            GameManager.inst.loadingOb.SetActive(false);
            GameManager.inst.Warning("해당 닉네임이 존재합니다.");
        });
    }


    public void OnEndEditEventMethod(int i)
    {
        switch (i)
        {
            case 1:
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    LoginBtn();
                }

                break;
            case 2:
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    CreateBtn();
                }

                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.inst.IdCreateOb.activeSelf || GameManager.inst.loginOb.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (GameManager.inst.IdCreateOb.activeSelf == false)
                {
                    if (loginInputField.isFocused)
                    {
                        passWordInputField.Select();
                    }
                    else
                    {
                        loginInputField.Select();
                    }
                }
                else
                {
                    if (loginCheckInputField.isFocused)
                    {
                        passWordCheck1InputField.Select();
                    }
                    else if (passWordCheck1InputField.isFocused)
                    {
                        passWordCheck2InputField.Select();
                    }
                    else
                    {
                        loginCheckInputField.Select();
                    }
                }
            }
        }
    }
}