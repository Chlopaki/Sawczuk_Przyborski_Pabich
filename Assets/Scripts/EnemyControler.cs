using UnityEngine;
using System.Collections;

public class EnemyControler : MonoBehaviour

{

    [Range(0.01f, 20.0f)][SerializeField] private float moveSpeed = 0.1f;
    private bool isFacingRight = false;
    private Animator animator;
    private float startPositionX;
    [SerializeField] private float moveRange = 0.1f;
    private bool isMovingRight = true;
    private Rigidbody2D rigidBody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isMovingRight)
        {
            if (this.transform.position.x < startPositionX + moveRange)
            {
                moveRight();
            }
            else
            {
                moveLeft();
                //transform.Translate(0.1f, 0.0f, 0.0f, Space.World);
                isMovingRight = false;
            }
        }
        else
        {
            if (this.transform.position.x > startPositionX - moveRange)
            {
                moveLeft();
            }
            else
            {
                moveRight();
                isMovingRight = true;
            }
        }
        
    }

    void Awake()
    {
        //rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startPositionX = this.transform.position.x;


    }

    void Flip()
    {
        Vector3 theScale = transform.localScale;
        isFacingRight = !isFacingRight;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    void moveRight()
    {
        transform.Translate(moveSpeed * Time.deltaTime, 0.0f, 0.0f, Space.World);
        if (!isFacingRight)
        {
            Flip();
        }
    }
    void moveLeft()
    {
        transform.Translate(-moveSpeed * Time.deltaTime, 0.0f, 0.0f, Space.World);
        if (isFacingRight)
        {
            Flip();
        }
    }

    IEnumerator KillOnAnimationEnd()
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Gracz jest wyżej → wróg ginie
            if (collision.transform.position.y > transform.position.y)
            {
                animator.SetBool("IsDead",true);
                StartCoroutine(KillOnAnimationEnd());
            }
            else
            {
                Debug.Log("Koniec gry!");
            }
        }
    }

    
}
