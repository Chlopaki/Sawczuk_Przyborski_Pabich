using UnityEngine;
using UnityEngine.UI;

public class SmartEnemy : MonoBehaviour
{
    public enum EnemyType { Patroller, Chaser, BossEagle }

    [Header("Konfiguracja")]
    public EnemyType type = EnemyType.Patroller;
    public int maxHealth = 1;
    [SerializeField] private float speed = 2f;
    [SerializeField] private int damageToPlayer = 1;

    [Header("Waypoints (Trasa)")]
    public Transform[] waypoints;
    private int currentPointIndex = 0;

    [Header("Wykrywanie (Pies)")]
    public float detectRange = 5f;
    public Transform playerTransform; // Przypiszemy automatycznie

    [Header("Ustawienia Bossa (Orze³)")]
    public float diveSpeed = 10f;     // Szybkoœæ ataku
    public float prepareTime = 0.8f;  // Czas celowania przed atakiem
    public float waitOnGround = 0.5f; // Ile le¿y na ziemi zanim wróci
    private float attackCooldown = 0f; //Ile czasu jest miêdzy atakami
    [Tooltip("Jak daleko w lewo/prawo orze³ widzi gracza")]
    [SerializeField] private float horizontalDetectRange = 8.0f;
    public Slider bossHealthBar;
    [Header("Nagroda za Bossa")]
    [SerializeField] private GameObject lootDrop;

    // Maszyna Stanów Bossa
    private enum BossState { Patrolling, Preparing, Diving, Recovering, Returning }
    private BossState bossState = BossState.Patrolling;

    private Vector2 targetDivePosition; // Gdzie uderzy
    private float timer;
    private int currentHealth;
    private bool isDead = false;
    private SpriteRenderer sr;
    private Animator anim;
    private Rigidbody2D rb;

    private float minX, maxX; // dla chaserów

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // ZnajdŸ gracza
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p) playerTransform = p.transform;

        // Pasek HP
        if (bossHealthBar != null)
        {
            bossHealthBar.maxValue = maxHealth;
            bossHealthBar.value = currentHealth;
            //bossHealthBar.gameObject.SetActive(type == EnemyType.BossEagle);
            bossHealthBar.gameObject.SetActive(false);
        }

        if (waypoints.Length > 0)
        {
            minX = waypoints[0].position.x;
            maxX = waypoints[0].position.x;

            foreach (Transform wp in waypoints)
            {
                if (wp.position.x < minX) minX = wp.position.x; // ZnajdŸ lew¹ granicê
                if (wp.position.x > maxX) maxX = wp.position.x; // ZnajdŸ praw¹ granicê
            }
        }
        else
        {
            // Zabezpieczenie jakbyœ zapomnia³ daæ waypointów (pies ma nieskoñczony wybieg)
            minX = -9999f;
            maxX = 9999f;
        }
    }

    void Update()
    {
        if (isDead) return;

        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }

        // Wybór zachowania zale¿nie od typu
        switch (type)
        {
            case EnemyType.Patroller:
                Patrol();
                break;
            case EnemyType.Chaser:
                HandleChaser();
                break;
            case EnemyType.BossEagle:
                HandleBossEagle();
                break;
        }
    }

    // --- LOGIKA OR£A (BOSS) ---
    void HandleBossEagle()
    {

        if (bossHealthBar != null && !isDead)
        {
            // Obliczamy dystans do gracza
            float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // Ustalamy zasiêg UI jako 1.75x zasiêgu wykrywania
            float uiRange = horizontalDetectRange * 1.75f;

            // Jeœli gracz jest blisko -> Poka¿ pasek. Jeœli daleko -> Ukryj.
            if (distToPlayer < uiRange)
            {
                if (!bossHealthBar.gameObject.activeSelf)
                    bossHealthBar.gameObject.SetActive(true);
            }
            else
            {
                if (bossHealthBar.gameObject.activeSelf)
                    bossHealthBar.gameObject.SetActive(false);
            }
        }

        // Zabezpieczenie: Jeœli gracz nie istnieje/zgin¹³ -> Wracaj na górê
        if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
        {
            bossState = BossState.Returning;
        }

        switch (bossState)
        {
            case BossState.Patrolling:
                Patrol(); // Lata lewo-prawo miêdzy waypointami


                // 1. Dystans poziomy (czy jest blisko w lewo/prawo)
                float distSide = Mathf.Abs(playerTransform.position.x - transform.position.x);

                // 2. Dystans pionowy (Orze³ musi byæ WY¯EJ ni¿ gracz)
                // transform.position.y > playerTransform.position.y
                bool isPlayerBelow = transform.position.y > playerTransform.position.y + 0.5f; // +0.5f marginesu

                // Jeœli gracz jest blisko (10 kratek) I jest pod spodem -> ATAK
                if (attackCooldown <= 0 && distSide < horizontalDetectRange && isPlayerBelow)
                {
                    bossState = BossState.Preparing;
                    timer = prepareTime;
                    FlipTowards(playerTransform.position);
                    if (anim) anim.SetBool("IsPreparing", true);
                }
                break;

            case BossState.Preparing:
                // Zatrzymanie w powietrzu (Telegrafowanie ataku)
                timer -= Time.deltaTime;

                // Ci¹gle œledzimy gracza wzrokiem i celownikiem
                FlipTowards(playerTransform.position);
                targetDivePosition = playerTransform.position;

                // Jeœli w trakcie przygotowania gracz ucieknie NA GÓRÊ (nad or³a), przerywamy atak
                if (transform.position.y < playerTransform.position.y)
                {
                    bossState = BossState.Patrolling;
                    Debug.Log("Orze³: Gracz uciek³ na górê, przerywam.");
                }

                if (timer <= 0)
                {
                    bossState = BossState.Diving;
                    if (anim) anim.SetBool("IsPreparing", false);
                    if (anim) anim.SetTrigger("Dive");
                }
                break;

            case BossState.Diving:
                // Lecimy w dó³ (do zapamiêtanej pozycji)
                transform.position = Vector2.MoveTowards(transform.position, targetDivePosition, diveSpeed * Time.deltaTime);

                // Sprawdzamy czy uderzy³ w ziemiê/cel
                if (Vector2.Distance(transform.position, targetDivePosition) < 0.1f)
                {
                    timer = waitOnGround; // Le¿y chwilê na ziemi
                    bossState = BossState.Recovering;

                    // Opcjonalnie: Efekt uderzenia o ziemiê (py³)
                }
                break;

            case BossState.Recovering:
                timer -= Time.deltaTime;
                // Gracz ma teraz czas, ¿eby uderzyæ le¿¹cego or³a!
                if (timer <= 0)
                {
                    bossState = BossState.Returning;
                }
                break;

            case BossState.Returning:
                // Powrót na górê (do Waypointów)
                Transform home = waypoints[0];

                // Wybieramy bli¿szy punkt, ¿eby nie lecia³ przez ca³¹ mapê
                if (waypoints.Length > 1)
                {
                    float d0 = Vector2.Distance(transform.position, waypoints[0].position);
                    float d1 = Vector2.Distance(transform.position, waypoints[1].position);
                    if (d1 < d0) home = waypoints[1];
                }

                transform.position = Vector2.MoveTowards(transform.position, home.position, speed * Time.deltaTime);

                // Jak wróci na miejsce -> Znowu patroluje
                if (Vector2.Distance(transform.position, home.position) < 0.5f)
                {
                    bossState = BossState.Patrolling;

                    attackCooldown = 2.0f;
                }
                break;
        }
    }

    void OnDrawGizmos()
    {
        if (type == EnemyType.BossEagle)
        {
            Gizmos.color = Color.red;
            // Rysujemy prostok¹t: szerokoœæ to 2x zasiêg (lewo+prawo), wysokoœæ du¿a w dó³
            Gizmos.DrawWireCube(transform.position + Vector3.down * 5, new Vector3(horizontalDetectRange * 2, 10, 0));

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, horizontalDetectRange * 1.75f);

        }
        else if (type == EnemyType.Chaser)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectRange);
        }
    }

    // --- INNE LOGIKI ---
    void Patrol()
    {
        if (waypoints.Length == 0) return;
        Transform target = waypoints[currentPointIndex];

        // Cel jest tworzony na ziemi
        Vector2 targetOnGround = new Vector2(target.position.x, transform.position.y);

        // Idziemy do "celu na ziemi"
        transform.position = Vector2.MoveTowards(transform.position, targetOnGround, speed * Time.deltaTime);

        FlipTowards(target.position);

        // Sprawdzamy tylko dystans w poziomie (X), ¿eby zaliczy³ punkt nawet jak jest nad nim
        if (Mathf.Abs(transform.position.x - target.position.x) < 0.2f)
        {
            currentPointIndex = (currentPointIndex + 1) % waypoints.Length;
        }
    }

    void HandleChaser()
    {
        float dist = Vector2.Distance(transform.position, playerTransform.position);

        if (dist < detectRange)
        {
            // 1. Gdzie jest gracz?
            float targetX = playerTransform.position.x;

            // 2. Jeœli gracz jest poza terenem, pies celuje w granicê (p³ot)
            targetX = Mathf.Clamp(targetX, minX, maxX);

            // 3. Obliczamy, czy pies ma gdzie iœæ (odleg³oœæ do zablokowanego celu)
            float xDiff = targetX - transform.position.x;

            // --- Ruch i Animacja ---
            if (Mathf.Abs(xDiff) > 0.2f)
            {
                float direction = Mathf.Sign(xDiff);

                if (rb != null)
                {
                    // Idziemy w stronê targetX (czyli albo gracza, albo granicy terytorium)
                    rb.linearVelocity = new Vector2(direction * speed * 1.5f, rb.linearVelocity.y);
                }
                else
                {
                    Vector2 targetPos = new Vector2(targetX, transform.position.y);
                    transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * 1.5f * Time.deltaTime);
                }

                if (anim) anim.SetBool("IsRunning", true);

                // Obracamy siê w stronê celu (¿eby pies szczeka³ na gracza stoj¹c przy granicy)
                FlipTowards(new Vector3(playerTransform.position.x, transform.position.y, 0));
            }
            else
            {
                // Pies dobieg³ do granicy (lub do gracza) i stoi
                if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                if (anim) anim.SetBool("IsRunning", false);

                // Ci¹gle patrz na gracza, nawet jak stoisz przy "p³ocie"
                FlipTowards(playerTransform.position);
            }
        }
        else
        {
            // Gracz uciek³ bardzo daleko (poza wzrok) -> Wróæ do patrolu
            Patrol();
            if (anim) anim.SetBool("IsRunning", false);
        }
    }

    void FlipTowards(Vector3 target)
    {
        // Obliczamy ró¿nicê w poziomie (X)
        float xDiff = target.x - transform.position.x;

        if (Mathf.Abs(xDiff) < 0.5f) return;


        if (target.x > transform.position.x) transform.localScale = new Vector3(-1, 1, 1);
        else transform.localScale = new Vector3(1, 1, 1);
    }

    // --- OTRZYMYWANIE OBRA¯EÑ ---
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (bossHealthBar) bossHealthBar.value = currentHealth;

        if (anim) anim.SetTrigger("Hurt");
        StartCoroutine(FlashRed());

        if (type == EnemyType.BossEagle && currentHealth > 0)
        {

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

 
            bossState = BossState.Returning;

            timer = 0;
        }

        if (currentHealth <= 0) Die();
    }

    void Die()
    {

        if (isDead) return;
        isDead = true;
        GetComponent<Collider2D>().enabled = false;


        if (bossHealthBar) bossHealthBar.gameObject.SetActive(false);
        if (anim) anim.SetTrigger("Death");

        if (lootDrop != null)
        {
            Instantiate(lootDrop, transform.position, Quaternion.identity);
        }

        GameManager.instance.AddEnemyKill();
        Destroy(gameObject, 0.5f);
    }

    System.Collections.IEnumerator FlashRed()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }
}