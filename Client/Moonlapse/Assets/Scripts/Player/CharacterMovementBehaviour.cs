using UnityEngine;
using System.Collections;

public class CharacterMovementBehaviour : MonoBehaviour
{
    public float MoveSpeed = 64f;

    public Vector3 move;
    public Vector3 velocity;

    protected Animator animator;

    new protected Collider2D collider;

    protected Rigidbody2D rb;

    // Use this for initialization
    protected virtual void Start()
    {
        move = new Vector3();
        velocity = new Vector3();
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        animator.SetFloat("Horizontal", move.x);
        animator.SetFloat("Vertical", move.y);

        move.Normalize();
        velocity = (move * MoveSpeed);

        //transform.position += velocity * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + (velocity * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name);
    }
}
