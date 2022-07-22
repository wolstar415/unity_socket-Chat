using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager inst;


    public String nickName; //닉네임
    public String room; // 현재 접속한 방이름
    [Range(2, 8)] public int maxRoom = 2; // 방옵션
    public bool IsChat = false;//현재 채팅중인지 아닌지

    public GameObject lobyOb;
    public GameObject joinOb;
    public GameObject chatOb;
    public GameObject loadingOb;
    public GameObject loginWarningOb;
    public GameObject roomWarningOb;
    public ChatManager chatManager;
    public LobyManager lobyManager;
    
    //UI들

    private void Awake()
    {
        inst = this;
    }
}