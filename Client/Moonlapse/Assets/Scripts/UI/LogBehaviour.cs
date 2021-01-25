using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Moonlapse.Networking;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class LogBehaviour : MonoBehaviour
{
    GeneralSceneController controller;
    TMP_InputField chatbox;

    bool isSubmitting;
    ChatType chatType;

    TextMeshProUGUI chatContext;

    // Start is called before the first frame update
    void Start()
    {
        controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<GeneralSceneController>();
        chatbox = GameObject.Find("Chatbox").GetComponent<TMP_InputField>();

        chatContext = GameObject.Find("ChatContext").GetComponent<TextMeshProUGUI>();
        chatType = ChatType.Say;

        chatbox.onFocusSelectAll = false;

        chatbox.onSelect.AddListener(OnSelect);
        chatbox.onSubmit.AddListener(OnSubmit);
        chatbox.onEndEdit.AddListener(OnDeselect);
        chatbox.onValueChanged.AddListener(OnValueChanged);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSubmitting)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                chatbox.Select();
            }
        }

        if (!chatbox.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.Slash))
            {
                chatbox.Select();
                chatbox.text = "/";
                chatbox.caretPosition = 1;
            }
        }

        isSubmitting = false;
    }

    void OnSubmit(string text)
    {
        text = text.Replace("<", "");
        text = text.Replace(">", "");

        NetworkState.SendPacket(Packet.ConstructChatPacket(chatType.ToString(), text));
        chatbox.text = "";
        EventSystem.current.SetSelectedGameObject(null);    // deselect chatbox
        isSubmitting = true;
    }

    void OnSelect(string text)
    {
        controller.SendMessage("ChatboxSelected");
    }

    void OnDeselect(string text)
    {
        controller.SendMessage("ChatboxDeselected");
    }

    void OnValueChanged(string text)
    {
        switch (text)
        {
            // do regex here
            //case "/w ":
            //case "/whisper ":
            //    ChangeChatType(ChatType.Whisper);
            //    break;

            case "/say ":
            case "/s ":
                ChangeChatType(ChatType.Say);
                break;

            case "/yell ":
            case "/y ":
                ChangeChatType(ChatType.Yell);
                break;

            default:
                return;
        }

        chatbox.text = "";
        chatbox.caretPosition = chatbox.text.Length - 1;
    }

    void ChangeChatType(ChatType newType)
    {
        switch (newType)
        {
            case ChatType.Say:
                chatContext.text = "";
                break;

            case ChatType.Yell:
                chatContext.text = "Yelling";
                break;

            case ChatType.Whisper:
                chatContext.text = "Whispering to {}";
                break;
        }

        chatType = newType;
    }
}

public enum ChatType
{
    Say,
    Whisper,
    Yell
}