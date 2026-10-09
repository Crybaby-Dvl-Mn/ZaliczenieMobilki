using UnityEngine;

public class SmashMovement2D : MonoBehaviour
{
    [Header("═══════════════ RUCH ═══════════════")]
    public float moveSpeed = 7f;
    public float acceleration = 50f;
    public float airControl = 0.5f;
    public float friction = 30f;

    [Header("═══════════════ SKOK ═══════════════")]
    public float jumpForce = 12f;
    public int maxJumps = 2;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("═══════════════ GROUND CHECK ═══════════════")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("═══════════════ KLAWISZE ═══════════════")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode upKey = KeyCode.W;

    private Rigidbody2D rb;
    private bool isGrounded;
    private int jumpsLeft;
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Brak Rigidbody2D!");
            return;
        }

        rb.freezeRotation = true;
        rb.gravityScale = 3f;
        jumpsLeft = maxJumps;
    }

    void Update()
    {
        // Sprawdzenie czy na ziemi
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        }

        // Reset skoków gdy na ziemi
        if (isGrounded)
            jumpsLeft = maxJumps;

        // Input ruchu
        moveInput = 0f;
        if (Input.GetKey(leftKey)) moveInput = -1f;
        if (Input.GetKey(rightKey)) moveInput = 1f;

        // Skok
        if (Input.GetKeyDown(jumpKey) && jumpsLeft > 0)
        {
            Jump();
        }

        // Skok w górę (opcjonalny)
        if (Input.GetKeyDown(upKey) && isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        // Ruch poziomy
        float targetSpeed = moveInput * moveSpeed;
        float accelRate = isGrounded ? acceleration : acceleration * airControl;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float movement = speedDiff * accelRate;

        rb.AddForce(Vector2.right * movement, ForceMode2D.Force);

        // Tarcie gdy brak inputu
        if (Mathf.Abs(moveInput) < 0.1f && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x * (1f - friction * Time.fixedDeltaTime),
                rb.linearVelocity.y
            );
        }

        // Lepsze spadanie
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(jumpKey))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpsLeft--;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}