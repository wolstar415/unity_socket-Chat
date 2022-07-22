using System;
using TMPro;
using UnityEngine;

public class RoomPrefab : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI cntText;
    public String name;

    public void ClickFunc()
    {
        GameManager.inst.lobyManager.JoinRoom(name);
    }
}