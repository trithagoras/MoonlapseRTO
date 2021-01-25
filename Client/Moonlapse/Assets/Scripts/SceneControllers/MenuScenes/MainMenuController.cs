using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Moonlapse.Networking;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField]
    GameObject network;

    GameObject mainMenu, loginMenu, registerMenu, serverMenu;
    TextMeshProUGUI feedbackText;

    TMP_InputField loginUserField, loginPassField, registerUserField, registerPassField, registerConfirmField, serverField, portField;

    MenuState menuState;

    bool readyToChangeScene;

    void Awake()
    {
        if (!GameObject.FindGameObjectWithTag("Network"))
        {
            Instantiate(network);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        mainMenu = GameObject.Find("MainMenu");
        loginMenu = GameObject.Find("LoginMenu");
        registerMenu = GameObject.Find("RegisterMenu");
        serverMenu = GameObject.Find("ServerMenu");

        feedbackText = GameObject.Find("FeedbackText").GetComponent<TextMeshProUGUI>();

        // main menu
        var mainLoginButton = GameObject.Find("MainMenu/LoginButton").GetComponent<Button>();
        var mainRegisterButton = GameObject.Find("MainMenu/RegisterButton").GetComponent<Button>();
        var mainServerButton = GameObject.Find("MainMenu/ServerButton").GetComponent<Button>();

        mainLoginButton.onClick.AddListener(ClickMainLogin);
        mainRegisterButton.onClick.AddListener(ClickMainRegister);
        mainServerButton.onClick.AddListener(ClickMainServer);

        // cancel buttons
        var loginCancelButton = GameObject.Find("LoginMenu/CancelButton").GetComponent<Button>();
        var registerCancelButton = GameObject.Find("RegisterMenu/CancelButton").GetComponent<Button>();
        var serverCancelButton = GameObject.Find("ServerMenu/CancelButton").GetComponent<Button>();

        loginCancelButton.onClick.AddListener(ClickCancel);
        registerCancelButton.onClick.AddListener(ClickCancel);
        serverCancelButton.onClick.AddListener(ClickCancel);

        // login menu
        loginUserField = GameObject.Find("LoginMenu/Username/Field").GetComponent<TMP_InputField>();
        loginPassField = GameObject.Find("LoginMenu/Password/Field").GetComponent<TMP_InputField>();

        var loginButton = GameObject.Find("LoginMenu/LoginButton").GetComponent<Button>();
        loginButton.onClick.AddListener(ClickLogin);

        // register menu
        registerUserField = GameObject.Find("RegisterMenu/Username/Field").GetComponent<TMP_InputField>();
        registerPassField = GameObject.Find("RegisterMenu/Password/Field").GetComponent<TMP_InputField>();
        registerConfirmField = GameObject.Find("RegisterMenu/ConfirmPassword/Field").GetComponent<TMP_InputField>();

        var registerButton = GameObject.Find("RegisterMenu/RegisterButton").GetComponent<Button>();
        registerButton.onClick.AddListener(ClickRegister);

        // change server menu
        serverField = GameObject.Find("ServerMenu/Server/Field").GetComponent<TMP_InputField>();
        portField = GameObject.Find("ServerMenu/Port/Field").GetComponent<TMP_InputField>();

        var connectButton = GameObject.Find("ServerMenu/ConnectButton").GetComponent<Button>();
        connectButton.onClick.AddListener(ClickConnect);

        // this needs to be done last, because GameObject.Find does not find inactive gameobjects
        loginMenu.SetActive(false);
        registerMenu.SetActive(false);
        serverMenu.SetActive(false);
        feedbackText.text = "";
    }

    void ClickMainLogin()
    {
        loginMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    void ClickMainRegister()
    {
        registerMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    void ClickMainServer()
    {
        serverMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    void ClickCancel()
    {
        loginMenu.SetActive(false);
        registerMenu.SetActive(false);
        serverMenu.SetActive(false);
        mainMenu.SetActive(true);
        feedbackText.text = "";
    }

    void ClickLogin()
    {
        if (!NetworkState.Running)
        {
            feedbackText.text = "You are not connected to any server. Press cancel and enter server details.";
            return;
        }

        var user = loginUserField.text;
        var pass = loginPassField.text;

        if (user == "" || pass == "")
        {
            feedbackText.text = "Username or password cannot be empty";
            return;
        }

        menuState = MenuState.Login;

        NetworkState.SendPacket(Packet.ConstructLoginPacket(user, pass));
    }

    void ClickRegister()
    {
        if (!NetworkState.Running)
        {
            feedbackText.text = "You are not connected to any server. Press cancel and enter server details.";
            return;
        }

        var user = registerUserField.text;
        var pass = registerPassField.text;
        var conf = registerConfirmField.text;

        if (user == "" || pass == "" || conf == "")
        {
            feedbackText.text = "Username or password cannot be empty";
            return;
        }

        if (pass != conf)
        {
            feedbackText.text = "Passwords do not match";
            return;
        }

        menuState = MenuState.Register;

        NetworkState.SendPacket(Packet.ConstructRegisterPacket(user, pass));
    }

    void ClickConnect()
    {
        NetworkState.host = serverField.text;
        NetworkState.port = int.Parse(portField.text);

        if (NetworkState.Running)
        {
            NetworkState.Stop();
        }

        if (NetworkState.Start())
        {
            feedbackText.text = "Connection established";
        }
        else
        {
            feedbackText.text = "Connection failed. Is the server up?.";
        }
    }

    void Update()
    {
        if (readyToChangeScene)
        {
            SceneManager.LoadScene("Sample");
        }
        else
        {
            ProcessNextPacket();
        }
    }

    Packet ProcessNextPacket()
    {
        if (!NetworkState.Running)
        {
            return null;
        }

        if (NetworkState.Packets.Count == 0)
        {
            return null;
        }

        Packet p = NetworkState.Packets.Peek();

        switch (p.Action)
        {
            case "Deny":
                feedbackText.text = (string)p.Payloads[0];
                break;

            case "Ok":
                switch (menuState)
                {
                    case MenuState.Login:
                        // switch to game scene happens here
                        readyToChangeScene = true;
                        break;

                    case MenuState.Register:
                        // rego successful
                        feedbackText.text = "Registration successful. Return to previous screen to login.";
                        break;

                    default:
                        break;
                }
                menuState = MenuState.None;
                break;
            default:
                return null;
        }

        NetworkState.Packets.Dequeue();
        return p;
    }
}

public enum MenuState
{
    None,
    Login,
    Register
}