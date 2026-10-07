using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class CharacterController : MonoBehaviour
{
    public float speed;
    private Animator anim;
    private float move;
    private Rigidbody2D rb;

    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        move = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(move * speed, rb.velocity.y);

        if (move > .1f || move < -.1f)
        {
            anim.SetBool("isWalking", true);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }
    }
}