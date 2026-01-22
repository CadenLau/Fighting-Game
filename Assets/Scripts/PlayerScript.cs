using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerScript : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    private Vector2 spawnPoint;
    private bool flipped = false;

    [Header("Health")]
    [SerializeField] private float startHealth = 100f;
    private float currentHealth;
    private RectTransform healthBar;
    private float healthBarSize;
    private LivesUI livesUI;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    private Vector2 moveDirection;

    [Header("Ground Movement")]
    [SerializeField] private float groundAccel = 200f;
    [SerializeField] private float groundDecel = 500f;

    [Header("Air Movement")]
    [SerializeField] private float airAccel = 120f;
    [SerializeField] private float airDecel = 20f;

    [Header("Actions")]
    [SerializeField] private float actionRecovery = 0.6f;
    [SerializeField] private float inputBufferTime = 0.15f;
    private float actionTimer = 0f;
    private System.Action bufferedAction;
    private float bufferExpireTime;
    private Image cooldownRing;
    private List<GameObject> activeProjectiles;

    [Header("Dash")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float maxDashSpeed;
    [SerializeField] private float dashResistance;
    [SerializeField] private int dashMaxCount = 2;
    private int dashCount;
    private bool isDashing;

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Melee")]
    [SerializeField] private GameObject meleePrefab;

    [Header("Special")]
    [SerializeField] private GameObject specialPrefab;

    [Header("Gravity")]
    [SerializeField] private float gravityScale;
    [SerializeField] private float gravityScalingFactor;

    [Header("Aim")]
    [SerializeField] private Transform aimPointer;
    [SerializeField] private float aimPointerDistance = 0.4f;
    private Vector2 aimDirection = Vector2.right;

    // [Header("Dodge")]
    // [SerializeField] private float dodgeDuration = 0.3f;
    // [SerializeField] private float dodgeAlpha = 0.5f;

    private bool isAlive = true;

    private HashSet<Collider2D> groundColliders;
    private bool Grounded => groundColliders.Count > 0;

    private PlayerInput playerInput;
    public PlayerInput Input => playerInput;

    #region Unity Callbacks
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        activeProjectiles = new List<GameObject>();
        groundColliders = new HashSet<Collider2D>();
    }

    private void OnEnable()
    {
        playerInput.actions["Dash"].performed += Dash;
        playerInput.actions["Shoot"].performed += Shoot;
        playerInput.actions["Melee"].performed += Melee;
        playerInput.actions["Special"].performed += Special;
        // playerInput.actions["Dodge"].performed += Dodge;
    }

    private void Start()
    {
        rb.gravityScale = gravityScale;
        dashCount = dashMaxCount;
        currentHealth = startHealth;
        aimPointer.GetComponent<SpriteRenderer>().color = GetComponent<SpriteRenderer>().color;
    }

    private void Update()
    {
        if (LivesRemaining() <= 0) return;

        if (actionTimer > 0f)
        {
            actionTimer -= Time.deltaTime;
        }

        if (cooldownRing != null)
        {
            float t = 1f - actionTimer / actionRecovery;
            cooldownRing.fillAmount = Mathf.SmoothStep(0f, 1f, t);
        }

        if (actionTimer <= 0f && bufferedAction != null && Time.time <= bufferExpireTime)
        {
            bufferedAction.Invoke();
            ResetActionTimer();
            bufferedAction = null;
        }

        moveDirection = playerInput.actions["Move"].ReadValue<Vector2>();

        Vector2 aimInput = playerInput.actions["Aim"].ReadValue<Vector2>();
        if (aimInput.sqrMagnitude > 0.05f)
            aimDirection = aimInput.normalized;

        if (transform.position.y < -10f)
        {
            LoseLife();
        }
    }

    private void LateUpdate()
    {
        if (LivesRemaining() <= 0) return;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;

        Vector2 aimPos = new(transform.position.x + aimDirection.x * aimPointerDistance, transform.position.y + aimDirection.y * aimPointerDistance);
        aimPointer.SetPositionAndRotation(aimPos, Quaternion.Euler(0, 0, angle));
    }

    private void FixedUpdate()
    {
        if (isDashing || LivesRemaining() <= 0)
            return;

        float targetSpeed = moveDirection.x * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;

        float accelRate;

        if (Grounded)
        {
            accelRate = Mathf.Abs(targetSpeed) > 0.05f
                ? groundAccel
                : groundDecel;
        }
        else
        {
            accelRate = Mathf.Abs(targetSpeed) > 0.05f
                ? airAccel
                : airDecel;
        }

        float movement = speedDiff * accelRate * Time.fixedDeltaTime;
        rb.AddForce(Vector2.right * movement, ForceMode2D.Force);

        // Gravity scaling
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = gravityScale * gravityScalingFactor;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.9f)
                {
                    if (groundColliders.Add(collision.collider)) dashCount = dashMaxCount;
                    break;
                }
                else if (Mathf.Abs(contact.normal.x) > 0.9f)
                {
                    dashCount += 1;
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (groundColliders.Contains(collision.collider)) dashCount = dashMaxCount;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (groundColliders.Remove(collision.collider))
            {
                if (groundColliders.Count == 0) dashCount = dashMaxCount - 1;
            }
        }
    }


    private void OnDisable()
    {
        playerInput.actions["Dash"].performed -= Dash;
        playerInput.actions["Shoot"].performed -= Shoot;
        playerInput.actions["Melee"].performed -= Melee;
        playerInput.actions["Special"].performed -= Special;
        // playerInput.actions["Dodge"].performed -= Dodge;
    }

    private void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.actions["Dash"].performed -= Dash;
            playerInput.actions["Shoot"].performed -= Shoot;
            playerInput.actions["Melee"].performed -= Melee;
            playerInput.actions["Special"].performed -= Special;
            // playerInput.actions["Dodge"].performed -= Dodge;
        }
    }
    #endregion

    #region Actions
    private void Dash(InputAction.CallbackContext obj)
    {
        if (dashCount <= 0 || !isAlive)
            return;

        dashCount--;

        Vector2 dashDir = moveDirection;

        // No dash if no input direction
        if (dashDir == Vector2.zero)
            return;

        isDashing = true;

        rb.gravityScale = 0f;
        if (Mathf.Sign(dashDir.y) != Mathf.Sign(rb.linearVelocity.y))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * dashResistance); // cancel vertical velocity if dashing against it
        }
        if (Mathf.Sign(dashDir.x) != Mathf.Sign(rb.linearVelocity.x))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * dashResistance, rb.linearVelocity.y); // cancel horizontal velocity if dashing against it
        }
        rb.AddForce(dashDir * dashSpeed, ForceMode2D.Impulse);
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxDashSpeed);

        Invoke(nameof(EndDash), 0.2f);
    }

    private void EndDash()
    {
        isDashing = false;
        rb.gravityScale = gravityScale;
    }

    private void Shoot(InputAction.CallbackContext obj)
    {
        TryPerformAction(() =>
        {
            // Safety check
            if (projectilePrefab == null || aimPointer == null || gameObject == null) return;

            GameObject projectile = Instantiate(projectilePrefab, aimPointer.position, Quaternion.identity);
            Projectile projScript = projectile.GetComponent<Projectile>();
            projScript.SetDirection(aimDirection);
            projScript.SetOwner(gameObject);
            activeProjectiles.Add(projectile);
        });
    }

    private void Melee(InputAction.CallbackContext obj)
    {
        TryPerformAction(() =>
        {
            // Safety check
            if (meleePrefab == null || aimPointer == null || gameObject == null) return;

            GameObject melee = Instantiate(meleePrefab, aimPointer.position, aimPointer.rotation);
            Melee meleeScript = melee.GetComponent<Melee>();
            meleeScript.SetDirection(aimDirection);
            meleeScript.SetOwner(gameObject);
            activeProjectiles.Add(melee);
        });
    }

    private void Special(InputAction.CallbackContext obj)
    {
        if (livesUI != null && !livesUI.HasSpecial()) return;

        TryPerformAction(() =>
        {
            // Safety check
            if (specialPrefab == null || aimPointer == null || gameObject == null) return;

            GameObject special = Instantiate(specialPrefab, aimPointer.position, aimPointer.rotation);
            Special specialScript = special.GetComponent<Special>();
            specialScript.SetStartDirection(aimDirection);
            specialScript.SetOwner(gameObject);
            livesUI.UseSpecial();
            activeProjectiles.Add(special);
        });
    }

    // private void Dodge(InputAction.CallbackContext obj)
    // {
    //     TryPerformAction(() =>
    //     {
    //         SetDodgingTrue(dodgeDuration);
    //     });
    // }


    // public void SetDodgingTrue(float duration)
    // {
    //     gameObject.layer = LayerMask.NameToLayer("PlayerDodging");
    //     SetAlpha(dodgeAlpha);
    //     CancelInvoke(nameof(SetDodgingFalse)); // Safeguard
    //     Invoke(nameof(SetDodgingFalse), duration);
    // }

    // private void SetDodgingFalse()
    // {
    //     gameObject.layer = LayerMask.NameToLayer("Player");
    //     SetAlpha(1f);
    // }

    private void SetAlpha(float alpha)
    {
        Color color = GetComponent<SpriteRenderer>().color;
        color.a = alpha;
        GetComponent<SpriteRenderer>().color = color;

        Color aimColor = aimPointer.GetComponent<SpriteRenderer>().color;
        aimColor.a = alpha;
        aimPointer.GetComponent<SpriteRenderer>().color = aimColor;
    }

    private void TryPerformAction(System.Action action)
    {
        if (!isAlive) return;

        if (actionTimer <= 0f)
        {
            action.Invoke();
            ResetActionTimer();
            bufferedAction = null;
        }
        else
        {
            bufferedAction = action;
            bufferExpireTime = Time.time + inputBufferTime;
        }
    }

    private void ResetActionTimer()
    {
        actionTimer = actionRecovery;
    }
    #endregion

    #region Combat
    public void ApplyKnockback(Vector2 dir, float strength)
    {
        if (!isAlive) return;

        Vector2 force = dir.normalized * strength;
        rb.AddForce(force, ForceMode2D.Impulse);
    }
    #endregion

    #region Health & Lives
    public void SubtractHealth(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f) LoseLife();

        ResetHealthBar();
    }

    private void LoseLife()
    {
        currentHealth = startHealth;
        ResetHealthBar();
        if (livesUI != null) livesUI.RemoveLife();

        isAlive = false;
        rb.simulated = false;

        if (LivesRemaining() <= 0)
        {
            GameManager.instance.CheckWin();
            return;
        }

        transform.position = spawnPoint;

        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForFixedUpdate();

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.simulated = true;
        isAlive = true;
    }

    public int LivesRemaining()
    {
        if (livesUI != null)
        {
            return livesUI.LivesRemaining;
        }
        return 0;
    }
    #endregion

    #region Setters
    public void SetSpawnPoint(Vector2 sp)
    {
        spawnPoint = sp;
        transform.position = spawnPoint;
    }

    public void SetHealthbar(RectTransform hb)
    {
        healthBar = hb;
        healthBarSize = healthBar.rect.width;
    }

    public void SetLivesUI(LivesUI lives)
    {
        livesUI = lives;
    }

    public void SetCooldownRing(Image ring)
    {
        cooldownRing = ring;
    }
    #endregion

    #region Player State & UI
    public void SetGameover(bool state)
    {
        if (state) playerInput.SwitchCurrentActionMap("UI");
        else playerInput.SwitchCurrentActionMap("Gameplay");
    }

    public void ResetPlayer()
    {
        isAlive = true;
        currentHealth = startHealth;
        ResetHealthBar();
        livesUI.ResetGame();

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.position = spawnPoint;

        aimDirection = Vector2.right;
        if (flipped) aimDirection = new Vector2(-aimDirection.x, aimDirection.y);

        // SetDodgingFalse();
        EndDash();
        actionTimer = 0f;
        bufferedAction = null;

        for (int i = 0; i < activeProjectiles.Count; i++)
        {
            if (activeProjectiles[i] != null)
            {
                Destroy(activeProjectiles[i]);
            }
        }
        activeProjectiles.Clear();

        SetGameover(false);

        rb.simulated = true;
    }

    private void ResetHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.sizeDelta = new Vector2(currentHealth / startHealth * healthBarSize, healthBar.sizeDelta.y);
        }
    }

    public void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        flipped = true;

        aimDirection = new Vector2(-aimDirection.x, aimDirection.y);
    }
    #endregion
}
