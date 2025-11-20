using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HeaderAttribute( "Movment parameters")]
    [Range(0.01f, 20.0f)] [SerializeField] private float jumpForce = 1.0f;
    [Space(10)]
    [Range( 0.01f, 20.0f)] [SerializeField] private float moveSpeed = 0.1f;
    private Rigidbody2D rigidBody;
    private Animator animator;
    private bool isRunning;
    private bool isFacingRight = true;
    private Vector2 startPosition;
   
    [SerializeField] private LayerMask groundLayer;
    const float rayLength = 0.25f;

    [Header("Ground Check")]
    [SerializeField] private Vector2 boxSize = new Vector2(0.2f, 0.5f); // Szerokoœæ (X) i wysokoœæ (Y) strefy detekcji
    [SerializeField] private float checkDistance = 0.4f;                 // Jak daleko pod graczem jest rzutowany ten prostok¹t

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (GameManager.instance.currentGameState == GameState.GAME)
        {
            isRunning = false;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                transform.Translate(moveSpeed * Time.deltaTime, 0.0f, 0.0f, Space.World);
                isRunning = true;
                //isFacingRight = true;
                if (!isFacingRight)
                {
                    Flip();
                }
            }
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                transform.Translate(-moveSpeed * Time.deltaTime, 0.0f, 0.0f, Space.World);
                isRunning = true;
                //isFacingRight = false;
                if (isFacingRight)
                {
                    Flip();
                }
            }
            if (Input.GetMouseButtonDown(0) || Input.GetKey(KeyCode.Space))
            {
                Jump();
            }
            Debug.DrawRay(transform.position, rayLength * Vector3.down, Color.blue, 0.2f, false);
            animator.SetBool("IsGrounded", IsGround());
            animator.SetBool("IsRunning", isRunning);
        }
    }

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startPosition = transform.position;
    }
    bool IsGround()
    {
        return Physics2D.Raycast(this.transform.position, Vector2.down, rayLength, groundLayer.value);
    }

    /* bool IsGround()
     {
         // U¿ywamy BoxCast do rzutowania prostok¹ta w dó³, co daje szerok¹ "stopê".
         // RigidBody.position to zazwyczaj œrodek kolidera.
         RaycastHit2D hit = Physics2D.BoxCast(
             rigidBody.position,    // Centralny punkt startowy (œrodek gracza)
             boxSize,               // Rozmiar boxa (Twoja szerokoœæ i wysokoœæ)
             0f,                    // K¹t rotacji (zostaw 0)
             Vector2.down,          // Kierunek rzutu (w dó³)
             checkDistance,         // Dystans rzutu
             groundLayer            // Warstwa gruntu
         );

         // Wizualizacja BoxCast w edytorze
         // Zmieniamy kolor w zale¿noœci od tego, czy trafiliœmy w ziemiê.
         Color rayColor = hit.collider != null ? Color.green : Color.red;

         // Rysowanie BoxCast w edytorze
         // U¿ywamy tego w zamian za Debug.DrawRay
         Debug.DrawRay(rigidBody.position + Vector2.up * (boxSize.y / 2f), Vector2.down * (checkDistance + boxSize.y), rayColor);

         return hit.collider != null;
     }*/

    void Jump()
    {
        if (IsGround())
        {
            rigidBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        Debug.Log("Lisu Skacze wariacie");
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("LevelExit"))
        {
            Debug.Log("Game over");
            
        }
        if (collision.gameObject.CompareTag("LevelFall"))
        {
            Debug.Log("You fall");
            GameManager.instance.AddLife(-1);
            Debug.Log("You have lost 1 life");
        }
        if (collision.gameObject.CompareTag("Bonus"))
        {
            Debug.Log("Bonus");
            GameManager.instance.AddPoints(10);
            collision.gameObject.SetActive(false);
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (transform.position.y > collision.gameObject.transform.position.y) Debug.Log("Killed an enemy");
            else
            {
                Debug.Log("Koniec Gry");
                GameManager.instance.AddLife(-1);
                //collision.gameObject.SetActive(false);
            }
        }
        if (collision.gameObject.CompareTag("Key"))
        {
            Debug.Log("Key found");
            GameManager.instance.AddKeys();
            collision.gameObject.SetActive(false);
        }
        if (collision.gameObject.CompareTag("Live"))
        {
            Debug.Log("Live obtained");
            GameManager.instance.AddLife(1);
            collision.gameObject.SetActive(false);
        }
    }

    void Flip()
    {   
        Vector3 theScale = transform.localScale;
        isFacingRight = !isFacingRight;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private void OnDrawGizmosSelected()
    {
        if (rigidBody != null)
        {
            // Ustawienie koloru (np. ¿ó³ty)
            Gizmos.color = Color.yellow;

            // Rysowanie obrysu prostok¹ta, który jest u¿ywany do detekcji
            // Pozycja musi byæ skorygowana, aby pasowa³a do logiki BoxCast
            Vector2 boxCenter = rigidBody.position + Vector2.down * (checkDistance + boxSize.y) / 2f;

            Gizmos.DrawWireCube(boxCenter, boxSize + Vector2.up * checkDistance);
        }
    }
}
