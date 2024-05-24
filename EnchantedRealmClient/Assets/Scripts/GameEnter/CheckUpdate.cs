using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckUpdate : Singleton<CheckUpdate>
{
    public Text textMessage;
    public Scrollbar scrollbar;
    
    public void SetTextMessage(string msg)
    {
        textMessage.text = msg;
    }
    public void SetProcress(float value) {
        scrollbar.size = value;
    }

}