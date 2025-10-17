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
   
    [SerializeField] private LayerMask groundLayer;
    const float rayLength = 0.25f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
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

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

    }
    bool IsGround()
    {
        return Physics2D.Raycast(this.transform.position, Vector2.down, rayLength, groundLayer.value);
    }
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
        }
        if (collision.gameObject.CompareTag("Bonus"))
        {
            Debug.Log("Bonus");
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
}
