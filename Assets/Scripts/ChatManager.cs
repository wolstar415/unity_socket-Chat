using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{

    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Transform textParent;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Transform playerparent;
    [SerializeField] private string[] players;
    [SerializeField] private List<GameObject> textobs;


    private void Start()
    {
        SocketManager.inst.socket.OnUnityThread("ChatOn", data =>
        {
            GameManager.inst.loadingOb.SetActive(false);
            players = JsonConvert.DeserializeObject<String[]>(data.GetValue(0).ToString());
            PlayerReSet();
        });
        SocketManager.inst.socket.OnUnityThread("PlayerReset", data =>
        {
            players = JsonConvert.DeserializeObject<String[]>(data.GetValue(0).ToString());
            PlayerReSet();
        });
        SocketManager.inst.socket.OnUnityThread("LeaveRoom", data =>
        {
            GameManager.inst.loadingOb.SetActive(false);
            GameManager.inst.lobyManager.RoomReset();
        });
        SocketManager.inst.socket.OnUnityThread("ChatGet", data =>
        {
            ChatGet(data.GetValue(0).ToString(),data.GetValue(1).ToString());
        });
    }

    public void LeaveBtn()
    {
        SocketManager.inst.socket.Emit("LeaveRoomCheck",GameManager.inst.room,players.Length);
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
    {
        GameManager.inst.IsChat = true;
        for (int i = 0; i < 8; i++)
        {
            playerparent.GetChild(i).GetComponent<TextMeshProUGUI>().text = "";
        }

        if (textobs.Count>0)
        {
            for (int i = 0; i < textobs.Count; i++)
            {
                Destroy(textobs[i]);
            }
        }
        textobs.Clear();
        GameManager.inst.loadingOb.SetActive(true);
        SocketManager.inst.socket.Emit("ChatCheck",GameManager.inst.room);

    }

    public void UpdateChat()
    {
        if (inputField.text.Equals(""))
        {
            return;
        }

        GameObject ob = Instantiate(textPrefab, textParent);

        ob.GetComponent<TextMeshProUGUI>().text = $"<color=red>{GameManager.inst.nickName} </color>: {inputField.text}";
        textobs.Add(ob);
        SocketManager.inst.socket.Emit("Chat",GameManager.inst.nickName,inputField.text,GameManager.inst.room);
       
        inputField.text = "";
    }
    public void ChatGet(string nickname,string text)
    {
        GameObject ob = Instantiate(textPrefab, textParent);
        textobs.Add(ob);
        ob.GetComponent<TextMeshProUGUI>().text = $"{nickname} : {text}";
    }

    private void Update()
    {
        if (GameManager.inst.IsChat)
        {
            if (Input.GetKeyDown(KeyCode.Return)&&inputField.isFocused==false)
            {
                inputField.ActivateInputField();
            }
        }
    }
}
