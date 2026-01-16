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

    [Header("Ustawienia Bossa (Orzeï¿½)")]
    public float diveSpeed = 10f;     // Szybkoï¿½ï¿½ ataku
    public float prepareTime = 0.8f;  // Czas celowania przed atakiem
    public float waitOnGround = 0.5f; // Ile leï¿½y na ziemi zanim wrï¿½ci
    private float attackCooldown = 0f; //Ile czasu jest miï¿½dzy atakami
    [Tooltip("Jak daleko w lewo/prawo orzeï¿½ widzi gracza")]
    [SerializeField] private float horizontalDetectRange = 8.0f;
    public Slider bossHealthBar;
    [Header("Nagroda za Bossa")]
    [SerializeField] private GameObject lootDrop;

    // Maszyna Stanï¿½w Bossa
    private enum BossState { Patrolling, Preparing, Diving, Recovering, Returning }
    private BossState bossState = BossState.Patrolling;

    private Vector2 targetDivePosition; // Gdzie uderzy
    private float timer;
    private int currentHealth;
    private bool isDead = false;
    private SpriteRenderer sr;
    private Animator anim;
    private Rigidbody2D rb;

    private float minX, maxX; // dla chaserï¿½w

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Znajdï¿½ gracza
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
                if (wp.position.x < minX) minX = wp.position.x; // Znajdï¿½ lewï¿½ granicï¿½
                if (wp.position.x > maxX) maxX = wp.position.x; // Znajdï¿½ prawï¿½ granicï¿½
            }
        }
        else
        {
            // Zabezpieczenie jakbyï¿½ zapomniaï¿½ daï¿½ waypointï¿½w (pies ma nieskoï¿½czony wybieg)
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

        // Wybï¿½r zachowania zaleï¿½nie od typu
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

    // --- LOGIKA ORï¿½A (BOSS) ---
    void HandleBossEagle()
    {

        if (bossHealthBar != null && !isDead)
        {
            // Obliczamy dystans do gracza
            float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // Ustalamy zasiï¿½g UI jako 1.75x zasiï¿½gu wykrywania
            float uiRange = horizontalDetectRange * 1.75f;

            // Jeï¿½li gracz jest blisko -> Pokaï¿½ pasek. Jeï¿½li daleko -> Ukryj.
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

        // Zabezpieczenie: Jeï¿½li gracz nie istnieje/zginï¿½ï¿½ -> Wracaj na gï¿½rï¿½
        if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
        {
            bossState = BossState.Returning;
        }

        switch (bossState)
        {
            case BossState.Patrolling:
                Patrol(); // Lata lewo-prawo miï¿½dzy waypointami


                // 1. Dystans poziomy (czy jest blisko w lewo/prawo)
                float distSide = Mathf.Abs(playerTransform.position.x - transform.position.x);

                // 2. Dystans pionowy (Orzeï¿½ musi byï¿½ WYï¿½EJ niï¿½ gracz)
                // transform.position.y > playerTransform.position.y
                bool isPlayerBelow = transform.position.y > playerTransform.position.y + 0.5f; // +0.5f marginesu

                // Jeï¿½li gracz jest blisko (10 kratek) I jest pod spodem -> ATAK
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

                // Ciï¿½gle ï¿½ledzimy gracza wzrokiem i celownikiem
                FlipTowards(playerTransform.position);
                targetDivePosition = playerTransform.position;

                // Jeï¿½li w trakcie przygotowania gracz ucieknie NA Gï¿½Rï¿½ (nad orï¿½a), przerywamy atak
                if (transform.position.y < playerTransform.position.y)
                {
                    bossState = BossState.Patrolling;
                    Debug.Log("Orzeï¿½: Gracz uciekï¿½ na gï¿½rï¿½, przerywam.");
                }

                if (timer <= 0)
                {
                    bossState = BossState.Diving;
                    if (anim) anim.SetBool("IsPreparing", false);
                    if (anim) anim.SetTrigger("Dive");
                }
                break;

            case BossState.Diving:
                // Lecimy w dï¿½ (do zapamiï¿½tanej pozycji)
                transform.position = Vector2.MoveTowards(transform.position, targetDivePosition, diveSpeed * Time.deltaTime);

                // Sprawdzamy czy uderzyï¿½ w ziemiï¿½/cel
                if (Vector2.Distance(transform.position, targetDivePosition) < 0.1f)
                {
                    timer = waitOnGround; // Leï¿½y chwilï¿½ na ziemi
                    bossState = BossState.Recovering;

                    // Opcjonalnie: Efekt uderzenia o ziemiï¿½ (pyï¿½)
                }
                break;

            case BossState.Recovering:
                timer -= Time.deltaTime;
                // Gracz ma teraz czas, ï¿½eby uderzyï¿½ leï¿½ï¿½cego orï¿½a!
                if (timer <= 0)
                {
                    bossState = BossState.Returning;
                }
                break;

            case BossState.Returning:
                // Powrï¿½t na gï¿½rï¿½ (do Waypointï¿½w)
                Transform home = waypoints[0];

                // Wybieramy bliï¿½szy punkt, ï¿½eby nie leciaï¿½ przez caï¿½ï¿½ mapï¿½
                if (waypoints.Length > 1)
                {
                    float d0 = Vector2.Distance(transform.position, waypoints[0].position);
                    float d1 = Vector2.Distance(transform.position, waypoints[1].position);
                    if (d1 < d0) home = waypoints[1];
                }

                transform.position = Vector2.MoveTowards(transform.position, home.position, speed * Time.deltaTime);

                // Jak wrï¿½ci na miejsce -> Znowu patroluje
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
            // Rysujemy prostokï¿½t: szerokoï¿½ï¿½ to 2x zasiï¿½g (lewo+prawo), wysokoï¿½ï¿½ duï¿½a w dï¿½
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

        // Sprawdzamy tylko dystans w poziomie (X), ï¿½eby zaliczyï¿½ punkt nawet jak jest nad nim
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

            // 2. Jeï¿½li gracz jest poza terenem, pies celuje w granicï¿½ (pï¿½ot)
            targetX = Mathf.Clamp(targetX, minX, maxX);

            // 3. Obliczamy, czy pies ma gdzie iï¿½ï¿½ (odlegï¿½oï¿½ï¿½ do zablokowanego celu)
            float xDiff = targetX - transform.position.x;

            // --- Ruch i Animacja ---
            if (Mathf.Abs(xDiff) > 0.2f)
            {
                float direction = Mathf.Sign(xDiff);

                if (rb != null)
                {
                    // Idziemy w stronï¿½ targetX (czyli albo gracza, albo granicy terytorium)
                    rb.linearVelocity = new Vector2(direction * speed * 1.5f, rb.linearVelocity.y);
                }
                else
                {
                    Vector2 targetPos = new Vector2(targetX, transform.position.y);
                    transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * 1.5f * Time.deltaTime);
                }

                if (anim) anim.SetBool("IsRunning", true);

                // Obracamy siï¿½ w stronï¿½ celu (ï¿½eby pies szczekaï¿½ na gracza stojï¿½c przy granicy)
                FlipTowards(new Vector3(playerTransform.position.x, transform.position.y, 0));
            }
            else
            {
                // Pies dobiegï¿½ do granicy (lub do gracza) i stoi
                if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                if (anim) anim.SetBool("IsRunning", false);

                // Ciï¿½gle patrz na gracza, nawet jak stoisz przy "pï¿½ocie"
                FlipTowards(playerTransform.position);
            }
        }
        else
        {
            // Gracz uciekï¿½ bardzo daleko (poza wzrok) -> Wrï¿½ï¿½ do patrolu
            Patrol();
            if (anim) anim.SetBool("IsRunning", false);
        }
    }

    void FlipTowards(Vector3 target)
    {
<<<<<<< HEAD
        // Obliczamy rï¿½nicï¿½ w poziomie (X)
=======
        // --- NOWOŒÆ: MARTWA STREFA ---
        // Obliczamy ró¿nicê w poziomie (X)
>>>>>>> 3b8118b3d6c8cf75ee511cf59f48b463c2c6f15c
        float xDiff = target.x - transform.position.x;

        // Jeœli gracz jest bli¿ej ni¿ 0.5f w poziomie, NIE OBRACAJ SIÊ.
        // To zapobiega "migotaniu" psa gdy stoisz nad nim.
        if (Mathf.Abs(xDiff) < 0.5f) return;
        // -----------------------------

        if (target.x > transform.position.x) transform.localScale = new Vector3(-1, 1, 1);
        else transform.localScale = new Vector3(1, 1, 1);
    }

    // --- OTRZYMYWANIE OBRAï¿½Eï¿½ ---
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (bossHealthBar) bossHealthBar.value = currentHealth;

        if (anim) anim.SetTrigger("Hurt");
        StartCoroutine(FlashRed());

        if (type == EnemyType.BossEagle && currentHealth > 0)
        {
            // 1. Resetujemy fizykê, ¿eby Orze³ nie kozio³kowa³ od uderzenia
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // 2. Zmuszamy go do odwrotu (lotu w górê do waypointów)
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

        if (type == EnemyType.BossEagle)
        {
            GameManager.instance.MoveGepard();
        }

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