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
        });
        SocketManager.inst.socket.OnUnityThread("LeaveRoom", data =>
        //방을 나갑니다
        {
            GameManager.inst.loadingOb.SetActive(false);
            
            GameManager.inst.lobyManager.RoomReset();
            //방을 나갔으니 방갱신을 해야합니다.
        });
        
        SocketManager.inst.socket.OnUnityThread("ChatGet",
            data => { ChatGet(data.GetValue(0).ToString(), data.GetValue(1).ToString()); });
        //채팅을 받고 올리는 이벤트입니다.
    }

    public void LeaveBtn()
    //나가기 버튼을 누를 때
    {
        SocketManager.inst.socket.Emit("LeaveRoomCheck", GameManager.inst.room, players.Length);
        GameManager.inst.chatOb.SetActive(false);
        GameManager.inst.joinOb.SetActive(true);
        GameManager.inst.loadingOb.SetActive(true);
        GameManager.inst.room = "";
        GameManager.inst.IsChat = false;
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
}