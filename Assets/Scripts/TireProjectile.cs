using UnityEngine;

public class TireProjectile : MonoBehaviour
{
    [SerializeField] public float speed = 10f;
    [SerializeField] public float lifeTime = 2f;
    [SerializeField] public int damage = 1;
    [SerializeField] public AudioClip hitSound;
    [SerializeField] public AudioClip killSound;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // 1. NAPRAWA ZNIKANIA: Ustawiamy "timer" œmierci od razu na starcie
        Destroy(gameObject, lifeTime);

        // 2. NAPRAWA RUCHU I KIERUNKU:
        // Sprawdzamy w któr¹ stronê opona jest "odwrócona" wizualnie (Scale X).
        // Jeœli Scale X to 1 -> leci w prawo. Jeœli -1 -> leci w lewo.
        float direction = Mathf.Sign(transform.localScale.x);

        // Nadajemy prêdkoœæ fizyczn¹ raz na starcie
        rb.linearVelocity = new Vector2(speed * direction, 0);
        // UWAGA: W starszym Unity u¿yj: rb.velocity zamiast rb.linearVelocity
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (gameObject == null) return;
        if (hitInfo.CompareTag("Player")) return;

        // Sprawdzamy czy trafiliœmy w coœ sensownego (Wroga, Ziemiê lub Œcianê)
        if (hitInfo.CompareTag("Enemy")|| hitInfo.CompareTag("Wall"))
        {

            // NOWE: DŸwiêk uderzenia (zrób to PRZED Destroy)
            if (hitSound != null)
            {
                // Tworzy dŸwiêk w miejscu, gdzie jest opona
                AudioSource.PlayClipAtPoint(hitSound, transform.position);
            }

            // Logika zabijania wroga (jeœli trafi³eœ wroga)
            if (hitInfo.CompareTag("Enemy"))
            {
                SmartEnemy boss = hitInfo.GetComponent<SmartEnemy>();

                Debug.Log("Trafiono wroga!");
                if (boss != null)
                {
                    boss.TakeDamage(1); // Zadaj obra¿enia Bossowi
                                        // UWAGA: Nie niszczymy bossa tutaj! On sam zniknie jak HP spadnie do 0.
                }
                else
                {
                    // Zwyk³y wróg - giñ
                    Destroy(hitInfo.gameObject);
                    GameManager.instance.AddEnemyKill();
                }
                if (killSound != null)
                {
                    AudioSource.PlayClipAtPoint(killSound, transform.position);
                }
                //Destroy(hitInfo.gameObject);
                //GameManager.instance.AddEnemyKill();

            }

            Destroy(gameObject); // Opona znika
        }
    }
}