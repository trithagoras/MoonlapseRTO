using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimatorBehaviour : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer sprite;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        var dx = animator.GetFloat("Horizontal");
        var dy = animator.GetFloat("Vertical");

        var name = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        if (IsZero(dx) && IsZero(dy))
        {
            animator.Play(name, 0, 0);
            return;
        }

        if (!IsZero(dx))
        {
            sprite.flipX = Mathf.Sign(dx) < -Mathf.Epsilon;
        }

        if (IsZero(dy))
        {
            animator.Play("WalkRight");
            return;
        }

        if (IsZero(dx))
        {
            if (dy < -Mathf.Epsilon)
            {
                animator.Play("WalkDown");
            }
            else if (dy > Mathf.Epsilon)
            {
                animator.Play("WalkUp");
            }
        }
        else
        {
            if (dy < -Mathf.Epsilon)
            {
                animator.Play("WalkDiagDown");
            }
            else if (dy > Mathf.Epsilon)
            {
                animator.Play("WalkDiagUp");
            }
        }
    }

    bool IsZero(float f)
    {
        return Mathf.Abs(f) < Mathf.Epsilon;
    }
}
