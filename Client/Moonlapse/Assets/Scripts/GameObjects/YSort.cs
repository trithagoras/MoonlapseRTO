using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YSort : MonoBehaviour
{
    new SpriteRenderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        renderer.sortingOrder = -(int)transform.position.y;
    }
}
