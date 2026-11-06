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
    [Tooltip("Pusty obiekt pod stopami postaci - sluzy do wykrywania ziemi.")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Kucanie i collider")]
    public float standColliderHeight = 1.6f;
    public float crouchColliderHeight = 1.0f;
    public float standColliderOffsetY = 0f;
    public float crouchColliderOffsetY = -0.3f;

    [Header("Sterowanie (przypisz w edytorze - inne dla kazdego gracza)")]
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

    [Header("Battle Integration")]
    [Tooltip("Wylaczane przez BattleManager po zakonczeniu walki.")]
    public bool inputEnabled = true;
    [Tooltip("Tag triggera pod arena - wypadniecie = KO.")]
    public string deathZoneTag = "DeathZone";

    // --- stan ---
    public bool IsCrouching { get; private set; }
    public bool IsBlockingHigh { get; private set; }
    public bool IsBlockingLow { get; private set; }
    public bool IsFacingRight { get; private set; } = true;
    public bool IsInHitstun => _hitstunTimer > 0f;
    public bool IsAttacking => _isAttacking;
    public bool IsGrounded { get; private set; }

    // --- komponenty ---
    private Rigidbody2D _rb;
    private Animator _anim;
    private Health _health;
    private CapsuleCollider2D _capsule;

    // --- timery ---
    private float _hitstunTimer;
    private float _attackCooldownTimer;
    private bool _isAttacking;
    private float _moveInput = 0f;

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
        EnsureOpponent();

        if (groundCheck == null)
            Debug.LogWarning($"[{name}] Brak groundCheck - skok i wykrywanie ziemi nie beda dzialac.", this);
    }

    void EnsureOpponent()
    {
        if (opponent == null)
        {
            var all = FindObjectsByType<FighterController>(FindObjectsSortMode.None);
            foreach (var f in all)
            {
                if (f != this)
                {
                    opponent = f;
                    break;
                }
            }
        }
    }

    void Update()
    {
        if (_hitstunTimer > 0f) _hitstunTimer -= Time.deltaTime;
        if (_attackCooldownTimer > 0f) _attackCooldownTimer -= Time.deltaTime;

        UpdateGrounded();

        // Ciagle upewniamy sie ze gracze sie obracaja do siebie (nawet jesli input wylaczony)
        UpdateFacing();

        if (!inputEnabled) return;
        if (_health != null && _health.IsDead) return;

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

    // ---------- AUTO-FACING (OBRACANIE W STRONE DRUGIEGO GRACZA) ----------
    void UpdateFacing()
    {
        if (opponent == null)
        {
            EnsureOpponent();
            if (opponent == null) return;
        }

        if (_isAttacking) return;
        if (IsInHitstun) return;
        if (opponent.Health != null && opponent.Health.IsDead) return;

        // Okreslamy kierunek patrzenia na podstawie pozycji X przeciwnika
        bool shouldFaceRight = opponent.transform.position.x >= transform.position.x;
        if (shouldFaceRight != IsFacingRight)
        {
            IsFacingRight = shouldFaceRight;
            var scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (IsFacingRight ? 1f : -1f);
            transform.localScale = scale;
        }
    }

    // ---------- KUCANIE ----------
    void HandleCrouch()
    {
        if (IsInHitstun) return;
        if (!IsGrounded) { SetCrouch(false); return; }
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
        bool canBlock = IsGrounded && opponent != null && opponent.Health != null && !opponent.Health.IsDead && !_isAttacking && !IsInHitstun;
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

        float h = _moveInput;
        if (Input.GetKey(leftKey)) h -= 1f;
        if (Input.GetKey(rightKey)) h += 1f;

        h = Mathf.Clamp(h, -1f, 1f);

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
            ExecuteJump();
        }
    }

    private void ExecuteJump()
    {
        if (_rb != null)
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
        if (Input.GetKeyDown(highAttackKey) && highAttack != null)
            StartAttack(highAttack);
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
        if (_health != null && _health.IsDead) return;
        bool blocked = false;
        if (IsBlockingLow) blocked = true;
        else if (IsBlockingHigh && attack.attackType == AttackType.High) blocked = true;
        if (blocked)
        {
            if (_anim != null) _anim.SetTrigger("BlockHit");
            return;
        }

        if (_health != null) _health.TakeDamage(attack.GetDamage());
        _hitstunTimer = attack.GetHitstunDuration();
        Vector2 dir = ((Vector2)transform.position - attackerPosition).normalized;
        if (Mathf.Abs(dir.x) < 0.01f) dir.x = IsFacingRight ? -1f : 1f;
        if (_rb != null) _rb.linearVelocity = new Vector2(dir.x * attack.GetKnockbackForce(), _rb.linearVelocity.y);
        if (_anim != null) _anim.SetTrigger("Hit");

        // Wizualizacja otrzymanego ciosu
        var vis = GetComponent<AttackVisualizer>();
        if (vis != null) vis.PlayReceiveHit();
    }

    // ---------- BATTLE INTEGRATION ----------
    public void ResetForBattle(Vector3 spawnPosition)
    {
        transform.position = spawnPosition;
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }
        _hitstunTimer = 0f;
        _attackCooldownTimer = 0f;
        _isAttacking = false;
        _moveInput = 0f;
        SetCrouch(false);
        IsBlockingHigh = false;
        IsBlockingLow = false;
        if (_anim != null)
        {
            _anim.SetBool("isWalking", false);
            _anim.SetBool("isBlockingHigh", false);
            _anim.SetBool("isBlockingLow", false);
            _anim.SetBool("isCrouching", false);
        }

        if (_health != null)
        {
            _health.ResetHealth();
        }

        EnsureOpponent();
        UpdateFacing();
        inputEnabled = true;
    }

    public void DisableInput()
    {
        inputEnabled = false;
        _moveInput = 0f;
        if (_rb != null)
            _rb.linearVelocity = Vector2.zero;
    }

    // ---------- KO (wyjscie poza arene) ----------
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(deathZoneTag)) return;
        if (_health == null) return;
        _health.TakeDamage(_health.maxHealth);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = IsGrounded ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    // ==================== MOBILE INPUT HELPERS ====================
    #region Mobile-input helpers
    public void MobilePressLeft(bool pressed)
    {
        if (pressed) _moveInput = -1f;
        else if (_moveInput < 0f) _moveInput = 0f;
    }

    public void MobilePressRight(bool pressed)
    {
        if (pressed) _moveInput = 1f;
        else if (_moveInput > 0f) _moveInput = 0f;
    }

    public void MobileJump()
    {
        if (IsGrounded && !IsCrouching && !_isAttacking && !IsInHitstun)
        {
            ExecuteJump();
        }
    }

    public void MobileHighAttack()
    {
        if (highAttack != null && inputEnabled && !IsInHitstun && !_isAttacking && _attackCooldownTimer <= 0f)
            StartAttack(highAttack);
    }

    public void MobileLowAttack()
    {
        if (lowAttack != null && inputEnabled && IsGrounded && !IsInHitstun && !_isAttacking && _attackCooldownTimer <= 0f)
            StartAttack(lowAttack);
    }
    #endregion
}
