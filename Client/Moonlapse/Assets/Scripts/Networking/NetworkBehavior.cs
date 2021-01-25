using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Moonlapse.Networking;

public class NetworkBehavior : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void OnApplicationQuit()
    {
        NetworkState.Stop();
    }
}
