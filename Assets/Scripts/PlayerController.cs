using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class PlayerController : MonoBehaviour
{
    [HeaderAttribute("Movment parameters")]
    [Range(0.01f, 20.0f)][SerializeField] private float jumpForce = 1.0f;
    [Range(0.0f, 1.0f)][SerializeField] private float jumpCutMultiplier = 0.5f;
    [Space(10)]
    [Range(0.01f, 20.0f)][SerializeField] private float moveSpeed = 0.1f;
    [Space(10)]
    [Range(0.01f, 20.0f)][SerializeField] private float climbSpeed = 3.0f;

    private Vector2 startPosition;

    [Header("Ground Check settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 boxSize = new Vector2(0.110422f, 0.1f);
    [SerializeField] private float checkDistance = 0.05f;
    [SerializeField] private Vector2 groundCheckOffset;

    // --- SYSTEM WIATRU: Nowe ustawienia ---
    [Header("Wind Settings")]
    [Tooltip("Włącz lub wyłącz całkowicie mechanikę wiatru.")]
    [SerializeField] private bool enableWindSystem = true; // NOWOŚĆ: Główny przełącznik

    [Tooltip("Obiekt grupulący efekty wizualne wiatru (np. 'Wiatr' z hierarchii).")]
    [SerializeField] private GameObject windEffectGroup;   // NOWOŚĆ: Obiekt z animacjami

    [Tooltip("Maksymalna siła wiatru w szczytowym momencie.")]
    [SerializeField] private float windMaxForce = 10.0f;
    [Tooltip("Jak długo trwa jeden podmuch.")]
    [SerializeField] private float windDuration = 3.0f;
    [Tooltip("Minimalny czas przerwy między podmuchami.")]
    [SerializeField] private float windInterval = 5.0f;
    [Tooltip("Szansa na zerwanie się wiatru (0.2 = 20%).")]
    [Range(0f, 1f)][SerializeField] private float windChance = 0.2f;

    // Zmienne wewnętrzne wiatru
    private bool isWindActive = false;
    private float windTimer = 0f;
    private float windCooldownTimer = 0f;
    private float nextWindCheckTime = 0f;
    private float windCheckRate = 0.25f;

    // Komponenty i flagi
    private Rigidbody2D rigidBody;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isRunning;
    private bool isFacingRight = true;

    //drabina
    private bool canClimb = false;
    private bool isClimbing = false;
    private float originalGravity;

    [Header("Combat Settings")]
    public bool isInvincible = false;
    public float damageImmunityTime = 1.5f;
    [SerializeField] private float parryDuration = 0.5f;
    [SerializeField] private float parryCooldown = 1.0f;
    private float lastParryTime;
    private Color originalColor;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject tirePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.5f;
    private float nextFireTime = 0f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip bSound;
    [SerializeField] private AudioClip keySound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip levelPassedSound;
    [SerializeField] private AudioClip LevelFailedSound;
    [SerializeField] private AudioClip killSound;
    [SerializeField] private AudioClip shootSound;

    private AudioSource source;

    void Start()
    {
        windCooldownTimer = windInterval;

        // NOWOŚĆ: Na starcie ukrywamy wizualizacje wiatru (jeśli są przypisane)
        if (windEffectGroup != null)
        {
            windEffectGroup.SetActive(false);
        }
    }

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        originalGravity = rigidBody.gravityScale;

        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (GameManager.instance.currentGameState == GameState.GAME)
        {
            HandleWind(); // Obsługa wiatru

            isRunning = false;

            float verticalInput = Input.GetAxisRaw("Vertical");
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                transform.Translate(moveSpeed * Time.deltaTime, 0.0f, 0.0f, Space.World);
                isRunning = true;
                if (!isFacingRight) Flip();
            }
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                transform.Translate(-moveSpeed * Time.deltaTime, 0.0f, 0.0f, Space.World);
                isRunning = true;
                if (isFacingRight) Flip();
            }

            else if (Input.GetKeyDown(KeyCode.F) && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }

            if ((Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space)))
            {
                if (rigidBody.linearVelocity.y > 0)
                {
                    rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, rigidBody.linearVelocity.y * jumpCutMultiplier);
                }
            }

            if (canClimb && Mathf.Abs(verticalInput) > 0.1f)
            {
                isClimbing = true;
            }

            if (isClimbing)
            {
                rigidBody.gravityScale = 0f;
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, verticalInput * climbSpeed);
            }
            else
            {
                rigidBody.gravityScale = originalGravity;
            }

            if ((Input.GetKeyDown(KeyCode.Space)))
            {
                if (IsGround() || isClimbing) Jump();
            }

            if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) && Time.time > lastParryTime + parryCooldown)
            {
                StartCoroutine(ActivateParry());
            }

            if ((Input.GetKeyUp(KeyCode.Space)))
            {
                if (rigidBody.linearVelocity.y > 0 && !isClimbing)
                {
                    rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, rigidBody.linearVelocity.y * jumpCutMultiplier);
                }
            }

            animator.SetBool("IsGrounded", IsGround());
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsClimbing", isClimbing);
            animator.SetFloat("ClimbSpeed", Mathf.Abs(verticalInput));
        }
    }

    // --- SYSTEM WIATRU: Zaktualizowana logika ---
    void HandleWind()
    {
        // 1. Sprawdzenie głównego przełącznika (NOWOŚĆ)
        if (!enableWindSystem)
        {
            // Jeśli wyłączyliśmy system w trakcie trwania wiatru, musimy posprzątać
            if (isWindActive)
            {
                isWindActive = false;
                if (windEffectGroup != null) windEffectGroup.SetActive(false);
            }
            return; // Wychodzimy, nie wykonujemy logiki wiatru
        }

        if (isWindActive)
        {
            windTimer += Time.deltaTime;

            float halfDuration = windDuration / 2.0f;
            float currentWindStrength = 0f;

            if (windTimer <= halfDuration)
            {
                currentWindStrength = Mathf.Lerp(0, windMaxForce, windTimer / halfDuration);
            }
            else
            {
                float timeInSecondHalf = windTimer - halfDuration;
                currentWindStrength = Mathf.Lerp(windMaxForce, 0, timeInSecondHalf / halfDuration);
            }

            rigidBody.AddForce(Vector2.left *  currentWindStrength / 2);

            // Zakończenie wiatru
            if (windTimer >= windDuration)
            {
                isWindActive = false;
                windCooldownTimer = windInterval;

                // NOWOŚĆ: Wyłączenie wizualizacji po zakończeniu
                if (windEffectGroup != null)
                {
                    windEffectGroup.SetActive(false);
                }

                Debug.Log("Koniec wiatru.");
            }
        }
        else
        {
            if (windCooldownTimer > 0)
            {
                windCooldownTimer -= Time.deltaTime;
            }
            else
            {
                if (Time.time >= nextWindCheckTime)
                {
                    nextWindCheckTime = Time.time + windCheckRate;
                    if (Random.value <= windChance)
                    {
                        StartWind();
                    }
                }
            }
        }
    }

    void StartWind()
    {
        isWindActive = true;
        windTimer = 0f;

        // NOWOŚĆ: Włączenie wizualizacji
        if (windEffectGroup != null)
        {
            windEffectGroup.SetActive(true);
        }

        Debug.Log("Wiatr się zerwał!");
    }
    // --- KONIEC SYSTEMU WIATRU ---


    bool IsGround()
    {
        float direction = Mathf.Sign(transform.localScale.x);
        Vector2 realOffset = new Vector2(groundCheckOffset.x * direction, groundCheckOffset.y);
        Vector2 origin = (Vector2)transform.position + realOffset;

        float footSpacing = (boxSize.x / 2) - 0.02f;

        Vector2 leftOrigin = origin + Vector2.left * footSpacing;
        Vector2 rightOrigin = origin + Vector2.right * footSpacing;

        RaycastHit2D leftHit = Physics2D.Raycast(leftOrigin, Vector2.down, checkDistance, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(rightOrigin, Vector2.down, checkDistance, groundLayer);

        return leftHit.collider != null || rightHit.collider != null;
    }

    void Jump()
    {
        isClimbing = false;
        rigidBody.gravityScale = originalGravity;

        rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, 0);
        rigidBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        if (source != null && jumpSound != null)
            source.PlayOneShot(jumpSound, AudioListener.volume);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("MovingPlatform") || collision.CompareTag("WayPointPlatform"))
        {
            if (collision.gameObject.activeInHierarchy)
            {
                transform.SetParent(collision.transform);
            }
        }

        if (collision.CompareTag("Ladder"))
        {
            canClimb = true;
        }

        HandleCollisions(collision);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("MovingPlatform") || collision.CompareTag("WayPointPlatform"))
        {
            transform.SetParent(null);
        }
        else if (collision.CompareTag("Ladder"))
        {
            canClimb = false;
            isClimbing = false;
        }
    }

    void HandleCollisions(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("LevelExit") && GameManager.instance.keyNum == 3)
        {
            Debug.Log("Game over");
            GameManager.instance.score = GameManager.instance.score + 100 * GameManager.instance.livesNum;
            if (source != null && levelPassedSound != null)
                source.PlayOneShot(levelPassedSound, AudioListener.volume);
            GameManager.instance.LevelCompleted();
        }
        else if (collision.gameObject.CompareTag("LevelFall"))
        {
            Debug.Log("You fall");
            GameManager.instance.AddLife(-1);
            Debug.Log("You have lost 1 life");
            transform.position = startPosition;
            rigidBody.linearVelocity = Vector2.zero;
            isClimbing = false;
        }
        else if (collision.gameObject.CompareTag("Bonus"))
        {
            GameManager.instance.AddPoints(10);
            if (source != null && bSound != null)
            {
                source.PlayOneShot(bSound, AudioListener.volume);
            }
            collision.gameObject.SetActive(false);
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            if (isInvincible)
            {
                Debug.Log("PARRY! Cios zablokowany!");
                return;
            }

            SmartEnemy smartEnemy = collision.gameObject.GetComponent<SmartEnemy>();
            if (smartEnemy == null) smartEnemy = collision.gameObject.GetComponentInParent<SmartEnemy>();

            float playerBottom = GetComponent<Collider2D>().bounds.min.y;
            bool isFalling = rigidBody.linearVelocity.y <= 0.1f;

            if (playerBottom > collision.bounds.center.y && isFalling)
            {
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, 0);
                rigidBody.AddForce(Vector2.up * (jumpForce / 1.5f), ForceMode2D.Impulse);

                StartCoroutine(BounceImmunity());

                if (source != null && killSound != null)
                    source.PlayOneShot(killSound, AudioListener.volume);

                if (smartEnemy != null)
                {
                    smartEnemy.TakeDamage(1);
                    Debug.Log("Boss dostał obrażenia! HP: " + smartEnemy.maxHealth);
                }
                else
                {
                    GameManager.instance.AddEnemyKill();
                    Debug.Log("Killed generic enemy");
                    collision.gameObject.SetActive(false);
                }
            }
            else
            {
                if (isInvincible) return;

                Debug.Log("You lost 1 life");
                GameManager.instance.AddLife(-1);
                StartCoroutine(DamageRecovery());
                if (source != null && deathSound != null)
                    source.PlayOneShot(deathSound, AudioListener.volume);
                transform.position = startPosition;
                rigidBody.linearVelocity = Vector2.zero;
                isClimbing = false;
            }
        }
        else if (collision.gameObject.CompareTag("Live"))
        {
            Debug.Log("Live obtained");
            GameManager.instance.AddLife(1);
            collision.gameObject.SetActive(false);
        }
        if (collision.gameObject.CompareTag("Key"))
        {
            KeyItem keyScript = collision.gameObject.GetComponent<KeyItem>();

            if (keyScript != null)
            {
                Debug.Log("Collected " + keyScript.keyColor + " Key");
                if (source != null && keySound != null)
                    source.PlayOneShot(keySound, AudioListener.volume);
                GameManager.instance.AddKey(keyScript.keyColor);
                collision.gameObject.SetActive(false);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollisions(collision.collider);
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            transform.SetParent(collision.transform);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            transform.SetParent(null);
        }
    }

    void Flip()
    {
        Vector3 theScale = transform.localScale;
        isFacingRight = !isFacingRight;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(tirePrefab, firePoint.position, firePoint.rotation);
        bullet.transform.localScale = transform.localScale;
        if (source != null && shootSound != null)
        {
            source.PlayOneShot(shootSound);
        }
    }

    System.Collections.IEnumerator ActivateParry()
    {
        isInvincible = true;
        lastParryTime = Time.time;
        animator.SetBool("IsCrouching", true);
        spriteRenderer.color = new Color(0.5f, 0.5f, 1f, 0.8f);
        yield return new WaitForSeconds(parryDuration);
        isInvincible = false;
        animator.SetBool("IsCrouching", false);
        spriteRenderer.color = originalColor;
    }

    System.Collections.IEnumerator BounceImmunity()
    {
        isInvincible = true;
        yield return new WaitForSeconds(0.2f);
        isInvincible = false;
    }

    System.Collections.IEnumerator DamageRecovery()
    {
        isInvincible = true;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = new Color(original.r, original.g, original.b, 0.5f);
            yield return new WaitForSeconds(damageImmunityTime);
            sr.color = original;
        }
        else
        {
            yield return new WaitForSeconds(damageImmunityTime);
        }
        isInvincible = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (transform == null) return;
        float direction = Mathf.Sign(transform.localScale.x);
        Vector2 realOffset = new Vector2(groundCheckOffset.x * direction, groundCheckOffset.y);
        Vector2 origin = (Vector2)transform.position + realOffset;
        float footSpacing = (boxSize.x / 2) - 0.02f;
        Vector2 leftOrigin = origin + Vector2.left * footSpacing;
        Vector2 rightOrigin = origin + Vector2.right * footSpacing;
        bool isHitLeft = Physics2D.Raycast(leftOrigin, Vector2.down, checkDistance, groundLayer);
        bool isHitRight = Physics2D.Raycast(rightOrigin, Vector2.down, checkDistance, groundLayer);
        if (isHitLeft || isHitRight) Gizmos.color = Color.green;
        else Gizmos.color = Color.red;
        Gizmos.DrawLine(leftOrigin, leftOrigin + Vector2.down * checkDistance);
        Gizmos.DrawLine(rightOrigin, rightOrigin + Vector2.down * checkDistance);
    }
}