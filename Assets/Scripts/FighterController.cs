using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(Health))]
public class FighterController : MonoBehaviour
{
    [Header("Przeciwnik (wymagane)")]
    public FighterController opponent;

    [Header("Ruch")]
    public float moveSpeed = 5f;
    public float crouchSpeedMultiplier = 0.4f;

    [Header("Skok")]
    public float jumpForce = 12f;
    [Tooltip("Pusty obiekt pod stopami postaci — służy do wykrywania ziemi.")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Kucanie i collider")]
    public float standColliderHeight = 1.6f;
    public float crouchColliderHeight = 1.0f;
    public float standColliderOffsetY = 0f;
    public float crouchColliderOffsetY = -0.3f;

    [Header("Sterowanie (przypisz w edytorze — inne dla każdego gracza)")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode crouchKey = KeyCode.S;
    public KeyCode jumpKey = KeyCode.W;
    public KeyCode highAttackKey = KeyCode.J;
    public KeyCode lowAttackKey = KeyCode.K;

    [Header("Ataki (przypisz AttackData)")]
    public AttackData highAttack;
    public AttackData lowAttack;

    [Header("Referencje")]
    public Transform attackOrigin;

    // --- stan ---
    public bool IsCrouching { get; private set; }
    public bool IsBlockingHigh { get; private set; }
    public bool IsBlockingLow { get; private set; }
    public bool IsFacingRight { get; private set; } = true;
    public bool IsInHitstun => _hitstunTimer > 0f;
    public bool IsAttacking => _isAttacking;
    public bool IsGrounded { get; private set; }

    private Rigidbody2D _rb;
    private Animator _anim;
    private Health _health;
    private CapsuleCollider2D _capsule;

    private float _hitstunTimer;
    private float _attackCooldownTimer;
    private bool _isAttacking;

    public Health Health => _health;
    public Transform AttackOrigin => attackOrigin != null ? attackOrigin : transform;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _health = GetComponent<Health>();
        _capsule = GetComponent<CapsuleCollider2D>();
    }

    void Start()
    {
        if (opponent == null)
            Debug.LogWarning($"[{name}] Brak przypisanego opponent. Auto-facing i blok kierunkowy nie będą działać.", this);
        if (groundCheck == null)
            Debug.LogWarning($"[{name}] Brak groundCheck — skok i wykrywanie ziemi nie będą działać.", this);
    }

    void Update()
    {
        if (_hitstunTimer > 0f) _hitstunTimer -= Time.deltaTime;
        if (_attackCooldownTimer > 0f) _attackCooldownTimer -= Time.deltaTime;

        UpdateGrounded();
        if (_health.IsDead) return;

        UpdateFacing();
        HandleCrouch();
        HandleBlock();
        HandleMovement();
        HandleJump();
        HandleAttacks();
    }

    // ---------- GROUND CHECK ----------
    void UpdateGrounded()
    {
        if (groundCheck == null) { IsGrounded = false; return; }
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // ---------- AUTO-FACING ----------
    void UpdateFacing()
    {
        if (opponent == null) return;
        if (_isAttacking) return;
        if (IsInHitstun) return;
        if (opponent.Health.IsDead) return;

        IsFacingRight = opponent.transform.position.x > transform.position.x;

        var scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (IsFacingRight ? 1f : -1f);
        transform.localScale = scale;
    }

    // ---------- KUCAŃ ----------
    void HandleCrouch()
    {
        if (IsInHitstun) return;
        if (!IsGrounded) { SetCrouch(false); return; } // w powietrzu nie kucamy

        bool wantCrouch = Input.GetKey(crouchKey);
        if (_isAttacking) wantCrouch = IsCrouching;

        SetCrouch(wantCrouch);
    }

    void SetCrouch(bool value)
    {
        if (value == IsCrouching) return;
        IsCrouching = value;
        ApplyCrouchCollider();
        if (_anim != null) _anim.SetBool("isCrouching", IsCrouching);
    }

    void ApplyCrouchCollider()
    {
        if (_capsule == null) return;
        var size = _capsule.size;
        size.y = IsCrouching ? crouchColliderHeight : standColliderHeight;
        _capsule.size = size;
        var off = _capsule.offset;
        off.y = IsCrouching ? crouchColliderOffsetY : standColliderOffsetY;
        _capsule.offset = off;
    }

    // ---------- BLOK ----------
    void HandleBlock()
    {
        // Blok tylko na ziemi, z żywym przeciwnikiem, bez ataku, bez hitstunu
        bool canBlock = IsGrounded
                        && opponent != null
                        && !opponent.Health.IsDead
                        && !_isAttacking
                        && !IsInHitstun;

        bool holdingBack = false;
        if (canBlock)
        {
            KeyCode backKey = IsFacingRight ? leftKey : rightKey;
            holdingBack = Input.GetKey(backKey);
        }

        IsBlockingHigh = canBlock && holdingBack && !IsCrouching;
        IsBlockingLow = canBlock && holdingBack && IsCrouching;

        if (_anim != null)
        {
            _anim.SetBool("isBlockingHigh", IsBlockingHigh);
            _anim.SetBool("isBlockingLow", IsBlockingLow);
        }
    }

    // ---------- RUCH ----------
    void HandleMovement()
    {
        if (IsInHitstun || _isAttacking)
        {
            if (_anim != null) _anim.SetBool("isWalking", false);
            return;
        }

        float h = 0f;
        if (Input.GetKey(leftKey)) h -= 1f;
        if (Input.GetKey(rightKey)) h += 1f;

        float speed = IsCrouching ? moveSpeed * crouchSpeedMultiplier : moveSpeed;
        _rb.linearVelocity = new Vector2(h * speed, _rb.linearVelocity.y);

        if (_anim != null) _anim.SetBool("isWalking", Mathf.Abs(h) > 0.01f);
    }

    // ---------- SKOK ----------
    void HandleJump()
    {
        if (!IsGrounded) return;
        if (_isAttacking || IsInHitstun) return;
        if (IsCrouching) return;

        if (Input.GetKeyDown(jumpKey))
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            if (_anim != null) _anim.SetTrigger("Jump");
        }
    }

    // ---------- ATAKI ----------
    void HandleAttacks()
    {
        if (IsInHitstun || _isAttacking || _attackCooldownTimer > 0f) return;
        if (IsBlockingHigh || IsBlockingLow) return;

        // Atak wysoki — dozwolony na ziemi i w powietrzu
        if (Input.GetKeyDown(highAttackKey) && highAttack != null)
            StartAttack(highAttack);

        // Atak niski — tylko na ziemi (w powietrzu nie ma sensu, nie da się kucać)
        if (IsGrounded && Input.GetKeyDown(lowAttackKey) && lowAttack != null)
            StartAttack(lowAttack);
    }

    void StartAttack(AttackData data)
    {
        _isAttacking = true;
        var executor = GetComponent<AttackExecutor>();
        if (executor == null) executor = gameObject.AddComponent<AttackExecutor>();
        executor.Execute(this, data, () =>
        {
            _isAttacking = false;
            _attackCooldownTimer = data.recoveryTime;
        });
    }

    // ---------- OTRZYMYWANIE CIOSU ----------
    public void ReceiveHit(AttackData attack, Vector2 attackerPosition)
    {
        if (_health.IsDead) return;

        bool blocked = false;
        if (IsBlockingLow) blocked = true;
        else if (IsBlockingHigh && attack.attackType == AttackType.High) blocked = true;

        if (blocked)
        {
            if (_anim != null) _anim.SetTrigger("BlockHit");
            return;
        }

        _health.TakeDamage(attack.GetDamage());
        _hitstunTimer = attack.GetHitstunDuration();

        Vector2 dir = ((Vector2)transform.position - attackerPosition).normalized;
        if (Mathf.Abs(dir.x) < 0.01f) dir.x = IsFacingRight ? -1f : 1f;
        _rb.linearVelocity = new Vector2(dir.x * attack.GetKnockbackForce(), _rb.linearVelocity.y);

        if (_anim != null) _anim.SetTrigger("Hit");
    }

    // Rysowanie groundCheck w Scene view — pomocne przy dostrajaniu
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = IsGrounded ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}