using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HeaderAttribute( "Movment parameters")]
    [Range(0.01f, 20.0f)] [SerializeField] private float jumpForce = 1.0f;
    [Space(10)]
    [Range( 0.01f, 20.0f)] [SerializeField] private float moveSpeed = 0.1f;
    private Rigidbody2D rigidBody;
    [SerializeField] private LayerMask groundLayer;
    const float rayLength = 0.25f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            transform.Translate(moveSpeed * Time.deltaTime, 0.0f, 0.0f, Space.World);
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            transform.Translate(-moveSpeed * Time.deltaTime, 0.0f, 0.0f, Space.World);
        }
        if (Input.GetMouseButtonDown(0) || Input.GetKey(KeyCode.Space))
        {
            Jump();
        }
        Debug.DrawRay(transform.position, rayLength * Vector3.down, Color.blue, 0.2f, false);
    }

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();

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
    }
}
