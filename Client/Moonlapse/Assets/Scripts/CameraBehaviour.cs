using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public Transform target;

    private Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        offset = new Vector3(0, 0, -10);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = target.position + offset;
    }
}
