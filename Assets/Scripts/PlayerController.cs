using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class PlayerController : MonoBehaviour
{
    [HeaderAttribute( "Movment parameters")]
    [Range(0.01f, 20.0f)] [SerializeField] private float jumpForce = 1.0f;
    [Range(0.0f, 1.0f)][SerializeField] private float jumpCutMultiplier = 0.5f; // Jak ucina skok po puszczeniu przycisku
    [Space(10)]
    [Range( 0.01f, 20.0f)] [SerializeField] private float moveSpeed = 0.1f;
    [Space(10)]
    [Range(0.01f, 20.0f)][SerializeField] private float climbSpeed = 3.0f;

    private Vector2 startPosition;
   

    //const float rayLength = 0.25f;



    [Header("Ground Check settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 boxSize = new Vector2(0.110422f, 0.1f); //wielkość sprawdzania
    [SerializeField] private float checkDistance = 0.05f; // dystans detekcji
    [SerializeField] private Vector2 groundCheckOffset;

    // Komponenty i flagi
    private Rigidbody2D rigidBody;
    private Animator animator;
    private bool isRunning;
    private bool isFacingRight = true;

    //drabina
    private bool canClimb = false;    
    private bool isClimbing = false;  
    private float originalGravity;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startPosition = transform.position;

        originalGravity = rigidBody.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {

        if (GameManager.instance.currentGameState == GameState.GAME)
        {
            isRunning = false;

            float verticalInput = Input.GetAxisRaw("Vertical");
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
            //if (Input.GetMouseButtonDown(0) || Input.GetKey(KeyCode.Space))
            //{
            //    Jump();
            //}

            if ((Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space)))
            {
                if (rigidBody.linearVelocity.y > 0)
                {
                    // zmmniejszanie prędkości wznoszenia
                    rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, rigidBody.linearVelocity.y * jumpCutMultiplier);
                }
            }

            if (canClimb && Mathf.Abs(verticalInput) > 0.1f)
            {
                isClimbing = true;
            }

            if (isClimbing)
            {
                // wyłączenie grawitacji i zmiana prędkości pionowej
                rigidBody.gravityScale = 0f;
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, verticalInput * climbSpeed);
            }
            else
            {
                // Przywrócenie grawitacji po wspinaniu się WAŻNE!!!
                rigidBody.gravityScale = originalGravity;
            }

            // Skok
            if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
            {
                if (IsGround() || isClimbing)
                {
                    Jump();
                }
            }

            // Kontrola wysokości skoku
            if ((Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space)))
            {
                if (rigidBody.linearVelocity.y > 0 && !isClimbing)
                {
                    rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, rigidBody.linearVelocity.y * jumpCutMultiplier);
                }
            }
            //Debug.DrawRay(transform.position, rayLength * Vector3.down, Color.blue, 0.2f, false);
            animator.SetBool("IsGrounded", IsGround());
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsClimbing", isClimbing);
            // Przesłanie szybkości wspiania się do animatora (by tego noo wiedzieć kiedy stoi podczas wpsinaczki)
            animator.SetFloat("ClimbSpeed", Mathf.Abs(verticalInput));
        }
    }



    bool IsGround()
    {
        //ustalenie środku z offsetem
        float direction = Mathf.Sign(transform.localScale.x);
        Vector2 realOffset = new Vector2(groundCheckOffset.x * direction, groundCheckOffset.y);
        Vector2 origin = (Vector2)transform.position + realOffset;

        // MARGINES BEZPIECZEŃSTWA:
        // Odejmuje trochę od szerokości, żeby promienie nie szły po samej krawędzi kolidera coś z tym by nie biegać po ścianach
        float footSpacing = (boxSize.x / 2) - 0.02f;

        // Pozycja lewej i prawej st00pki
        Vector2 leftOrigin = origin + Vector2.left * footSpacing;
        Vector2 rightOrigin = origin + Vector2.right * footSpacing;

        //Teraz są dwa promienie z każdej syrki
        RaycastHit2D leftHit = Physics2D.Raycast(leftOrigin, Vector2.down, checkDistance, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(rightOrigin, Vector2.down, checkDistance, groundLayer);

        // Jeśli któryś trafił to stoimy na ziemi (można później modyfikacje zrobić że gdy jeden jest tylko to połowa siły skoku jest tylko)
        return leftHit.collider != null || rightHit.collider != null;
    }


    void Jump()
    {
        isClimbing = false;
        rigidBody.gravityScale = originalGravity;

        rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, 0);
        rigidBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        //Debug.Log("Lisu Skacze wariacie");
        
        
    }
    

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Sprawdź tag i upewnij się, że platforma jest aktywna w hierarchii
        if (collision.CompareTag("Moving Platform") && collision.gameObject.activeInHierarchy)
        {
            transform.SetParent(collision.transform);
        }

        
        if (collision.CompareTag("Ladder"))
        {
            canClimb = true;
        }
        //logika z OnTriggerEnter2D w HandleCollisions
        /*if (collision.CompareTag("LevelExit"))
        {
            GameManager.instance.score=GameManager.instance.score + 100 * GameManager.instance.livesNum; // bonus za ukończenie poziomu
            GameManager.instance.LevelCompleted();
        }*/
        HandleCollisions(collision);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        // Po wyjściu z triggera platformy, czyścimy rodzica (parent = null)
        if (collision.CompareTag("Moving Platform"))
        {
            // Przed odpięciem sprawdź, czy transformacja jest możliwa
            if (this.gameObject.activeInHierarchy)
            {
                transform.SetParent(null);
            }
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
            GameManager.instance.score = GameManager.instance.score + 100 * GameManager.instance.livesNum; // bonus za ukończenie poziomu
            GameManager.instance.LevelCompleted();
        }
        else if (collision.gameObject.CompareTag("LevelFall"))
        {
            Debug.Log("You fall");
            transform.SetParent(null);
            GameManager.instance.AddLife(-1);
            Debug.Log("You have lost 1 life");
            transform.position = startPosition;
            rigidBody.linearVelocity = Vector2.zero;
            isClimbing = false; // reset wspinaczki po śmierci
        }
        else if (collision.gameObject.CompareTag("Bonus"))
        {
            GameManager.instance.AddPoints(10);
            collision.gameObject.SetActive(false);
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            if (transform.position.y > collision.gameObject.transform.position.y)
            {
                Debug.Log("Killed an enemy");
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, 0);
                rigidBody.AddForce(Vector2.up * (jumpForce / 1.5f), ForceMode2D.Impulse);
                GameManager.instance.AddEnemyKill();
            }
            else
            {
                Debug.Log("You lost 1 life");
                GameManager.instance.AddLife(-1);
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
            // Pobieramy skrypt z klucza, żeby wiedzieć jaki ma kolor
            KeyItem keyScript = collision.gameObject.GetComponent<KeyItem>();

            if (keyScript != null)
            {
                Debug.Log("Collected " + keyScript.keyColor + " Key");

                // Wysłanie konkretnego koloru
                GameManager.instance.AddKey(keyScript.keyColor);

                collision.gameObject.SetActive(false);
            }
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
        if (transform == null) return;

        // Oblicza pozycje startowe (tak samo jak w IsGround)
        float direction = Mathf.Sign(transform.localScale.x);
        Vector2 realOffset = new Vector2(groundCheckOffset.x * direction, groundCheckOffset.y);
        Vector2 origin = (Vector2)transform.position + realOffset;

        float footSpacing = (boxSize.x / 2) - 0.02f;
        Vector2 leftOrigin = origin + Vector2.left * footSpacing;
        Vector2 rightOrigin = origin + Vector2.right * footSpacing;

        // Sprawdzenie czy dotyka ziemi w celu wybrania koloru
        bool isHitLeft = Physics2D.Raycast(leftOrigin, Vector2.down, checkDistance, groundLayer);
        bool isHitRight = Physics2D.Raycast(rightOrigin, Vector2.down, checkDistance, groundLayer);

        // Wybieranie koloru: Zielony jeśli dotyka, Czerwony jeśli powietrze
        if (isHitLeft || isHitRight)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }

        // Rysowaine promieni
        Gizmos.DrawLine(leftOrigin, leftOrigin + Vector2.down * checkDistance);
        Gizmos.DrawLine(rightOrigin, rightOrigin + Vector2.down * checkDistance);
    }
}
