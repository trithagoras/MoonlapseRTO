using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moonlapse.Networking;
using UnityEngine.SceneManagement;

public class GeneralSceneController : MonoBehaviour
{
    public int PlayerId { get; private set; }

    public Dictionary<int, GameObject> visisbleInstances;
    public GameObject Player => visisbleInstances[PlayerId];

    private string nextSceneName;

    private Queue<string> logLines;
    private const int maxLogLines = 100;
    private GameObject logContent;

    [SerializeField]
    TimeTracker timeTracker;

    [SerializeField]
    private GameObject Character;

    // Start is called before the first frame update
    void Start()
    {
        visisbleInstances = new Dictionary<int, GameObject>();
        logLines = new Queue<string>();
        logContent = GameObject.FindGameObjectWithTag("ScrollContent");

    }

    // Update is called once per frame
    void Update()
    {
        ProcessNextPacket();

        if (Input.GetButtonDown("Cancel"))
        {
            NetworkState.SendPacket(Packet.ConstructLogoutPacket());
            SceneManager.LoadScene("MainMenu");
        }
    }

    Packet ProcessNextPacket()
    {
        if (NetworkState.Packets.Count == 0)
        {
            return null;
        }

        Packet p = NetworkState.Packets.Peek();

        int id;
        GameObject character;

        switch (p.Action)
        {
            case "Id":
                PlayerId = (int)(long)p.Payloads[0];
                character = Instantiate(Character);
                visisbleInstances[PlayerId] = character;
                character.AddComponent<PlayerMovementBehavior>();
                GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraBehaviour>().target = character.GetComponent<Transform>();
                break;

            case "Hello":
                id = (int)(long)p.Payloads[0];
                character = Instantiate(Character);
                visisbleInstances[id] = character;
                character.AddComponent<OtherMovementBehaviour>();
                break;

            case "Time":
                timeTracker.minute = (int)(long)p.Payloads[0];
                timeTracker.UpdateLabel();
                break;

            case "Details":
                id = (int)(long)p.Payloads[0];
                var name = p.Payloads[1] as string;
                character = visisbleInstances[id];
                character.GetComponent<EntityDetails>().entityName = name;
                if (id != PlayerId)
                {
                    AddToLog($"{character.GetComponent<EntityDetails>().entityName} has arrived.");
                }
                break;

            case "Position":
                id = (int)(long)p.Payloads[0];
                var x = (float)(double)p.Payloads[1];
                var y = (float)(double)p.Payloads[2];

                character = visisbleInstances[id];
                character.GetComponent<Transform>().position = new Vector3(x, y);
                break;

            case "Move":
                id = (int)(long)p.Payloads[0];
                var dx = (float)(double)p.Payloads[1];
                var dy = (float)(double)p.Payloads[2];

                character = visisbleInstances[id];
                character.GetComponent<OtherMovementBehaviour>().move = new Vector3(dx, dy);
                break;

            case "Goodbye":
                id = (int)(long)p.Payloads[0];
                character = visisbleInstances[id];
                AddToLog($"{character.GetComponent<EntityDetails>().entityName} has departed.");
                Destroy(character);
                visisbleInstances.Remove(id);
                break;

            case "Log":
                var message = p.Payloads[0] as string;
                message = $"<color=yellow>{message}</color>";
                AddToLog(message);
                break;

            case "Chat":
                var username = p.Payloads[0] as string;
                var context = p.Payloads[1] as string;
                message = p.Payloads[2] as string;

                message = $"{ColorRTFromChatType(context)}{username}: {message}</color>";
                AddToLog(message);
                break;

            default:
                return null;
        }

        NetworkState.Packets.Dequeue();
        return p;
    }

    void AddToLog(string message)
    {
        logLines.Enqueue(message);
        if (logLines.Count == maxLogLines)
        {
            logLines.Dequeue();
        }

        logContent.GetComponentInChildren<TMPro.TextMeshProUGUI>().text += $"\n{message}";
    }

    void ChatboxSelected()
    {
        Player.GetComponent<PlayerMovementBehavior>().isChatting = true;
    }

    void ChatboxDeselected()
    {
        Player.GetComponent<PlayerMovementBehavior>().isChatting = false;

    }

    string ColorRTFromChatType(string chatType)
    {
        switch (chatType)
        {
            case "Yell":
                return "<color=orange>";

            case "Whisper":
                return "<color=blue>";

            default:
                return "<color=white>";
        }
    }
}
