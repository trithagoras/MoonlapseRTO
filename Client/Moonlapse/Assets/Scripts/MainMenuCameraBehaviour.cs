using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCameraBehaviour : MonoBehaviour
{
    RectInt bounds;
    Vector3 destination;

    [SerializeField]
    float speed;

    // Start is called before the first frame update
    void Start()
    {
        // X: -1393, -521
        // Y: -362, 230
        bounds = new RectInt(-1393, -362, 872, 592);
        transform.position = new Vector3(Random.Range(bounds.xMin, bounds.xMax), Random.Range(bounds.yMin, bounds.yMax), -10);
        destination = new Vector3(Random.Range(bounds.xMin, bounds.xMax), Random.Range(bounds.yMin, bounds.yMax), -10);
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(transform.position, destination) < 1)
        {
            destination = new Vector3(Random.Range(bounds.xMin, bounds.xMax), Random.Range(bounds.yMin, bounds.yMax), -10);
        }

        transform.position = Vector2.MoveTowards(transform.position, destination, speed);
        transform.position += new Vector3(0, 0, -10);
    }
}
