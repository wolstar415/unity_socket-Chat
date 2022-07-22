using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

[System.Serializable]
public class RoomInfo
{
    public int currentCnt;
    public int RoomMaxCnt;
    public string name;
}

public class LobyManager : MonoBehaviour
{
    public TextMeshProUGUI nickname;
    public TMP_InputField logiInputField;
    public TMP_InputField createInputField;
    public RoomInfo[] roomsInfo;
    public Transform roomParent;
    public GameObject roomPrefab;
    public List<GameObject> roomobs;

    public void LoginBtn()
    //접속 버튼 누르면 실행
    {
        if (logiInputField.text == "")
        {
            return;
        }


        GameManager.inst.loadingOb.SetActive(true);

        SocketManager.inst.socket.Emit("LoginCheck", logiInputField.text);
    }

    public void CreateBtn()
    //방생성 버튼시 실행하는 함수
    {
        if (createInputField.text == "")
        {
            return;
        }

        GameManager.inst.loadingOb.SetActive(true);

        SocketManager.inst.socket.Emit("CreateCheck", createInputField.text, GameManager.inst.maxRoom);
    }

    public void OnEndEditEventMethod()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            LoginBtn();
        }
    }

    private void Start()
    {
        logiInputField.Select();


        SocketManager.inst.socket.OnUnityThread("Login", data =>
        {
            GameManager.inst.nickName = logiInputField.text;
            GameManager.inst.loadingOb.SetActive(false);
            GameManager.inst.lobyOb.SetActive(false);
            GameManager.inst.joinOb.SetActive(true);
            nickname.text = logiInputField.text;
            SocketManager.inst.socket.Emit("RoomListCheck", null);
        });
        SocketManager.inst.socket.OnUnityThread("LoginFailed", data =>
        {
            GameManager.inst.loadingOb.SetActive(false);
            GameManager.inst.loginWarningOb.SetActive(true);
        });

        SocketManager.inst.socket.OnUnityThread("Create", data =>
        {
            GameManager.inst.room = createInputField.text;
            GameManager.inst.loadingOb.SetActive(false);
            GameManager.inst.joinOb.SetActive(false);
            GameManager.inst.chatOb.SetActive(true);
            GameManager.inst.chatManager.ChatStart();
        });
        SocketManager.inst.socket.OnUnityThread("CreateFailed", data =>
        {
            GameManager.inst.loadingOb.SetActive(false);
            GameManager.inst.roomWarningOb.SetActive(true);
        });
        SocketManager.inst.socket.OnUnityThread("RoomReset", data =>
        {
            roomsInfo = JsonConvert.DeserializeObject<RoomInfo[]>(data.GetValue(0).ToString());
            RoomReset();
        });
        SocketManager.inst.socket.OnUnityThread("RoomList", data =>
        {
            if (data.ToString() == "[[]]")
            {
                return;
            }

            roomsInfo = JsonConvert.DeserializeObject<RoomInfo[]>(data.GetValue(0).ToString());
            RoomReset();
        });
        SocketManager.inst.socket.OnUnityThread("Join", data =>
        {
            GameManager.inst.room = data.GetValue(0).ToString();
            GameManager.inst.loadingOb.SetActive(false);
            GameManager.inst.joinOb.SetActive(false);
            GameManager.inst.chatOb.SetActive(true);
            GameManager.inst.chatManager.ChatStart();
        });
        SocketManager.inst.socket.OnUnityThread("JoinFailed", data =>
        {
            GameManager.inst.loadingOb.SetActive(false);
            SocketManager.inst.socket.Emit("RoomListCheck", null);
        });
    }


    public void MaxRoomChange(int value)
    {
        GameManager.inst.maxRoom = value;
    }

    public void JoinRoom(string name)
    {
        GameManager.inst.loadingOb.SetActive(true);
        SocketManager.inst.socket.Emit("JoinRoomCheck", name);
    }

    public void RoomReset()
    {
        if (roomobs.Count > 0)
        {
            for (int i = 0; i < roomobs.Count; i++)
            {
                Destroy(roomobs[i]);
            }
        }

        roomobs.Clear();
        for (int i = 0; i < roomsInfo.Length; i++)
        {
            if (roomsInfo[i].currentCnt < roomsInfo[i].RoomMaxCnt)
            {
                GameObject room = Instantiate(roomPrefab, roomParent);
                var roominfo = room.GetComponent<RoomPrefab>();
                roominfo.nameText.text = roomsInfo[i].name;
                roominfo.name = roomsInfo[i].name;
                roominfo.cntText.text = $"{roomsInfo[i].currentCnt}/{roomsInfo[i].RoomMaxCnt}";
                roomobs.Add(room);
            }
        }
    }
}