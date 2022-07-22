using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager inst;


    public String nickName;
    public String room;
    [Range(2, 8)] public int maxRoom = 2;
    public bool IsChat = false;

    public GameObject lobyOb;
    public GameObject joinOb;
    public GameObject chatOb;
    public GameObject loadingOb;
    public GameObject loginWarningOb;
    public GameObject roomWarningOb;

    public ChatManager chatManager;
    public LobyManager lobyManager;
    private void Awake()
    {
        inst = this;
    }
}
