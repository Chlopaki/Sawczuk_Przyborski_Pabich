using UnityEngine;

public class MovingPlatformController : MonoBehaviour
{
    [Range(0.01f, 20.0f)][SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float moveRange = 3.0f;

    private float startPositionX;
    private bool isMovingRight = true;

    void Awake()
    {
        // Zapamiêtujemy pozycjê startow¹
        startPositionX = this.transform.position.x;
    }

    void Update()
    {
        // Logika poruszania siê lewo-prawo
        if (isMovingRight)
        {
            if (this.transform.position.x < startPositionX + moveRange)
                MoveRight();
            else
                isMovingRight = false;
        }
        else
        {
            if (this.transform.position.x > startPositionX - moveRange)
                MoveLeft();
            else
                isMovingRight = true;
        }
    }

    void MoveRight()
    {
        transform.Translate(moveSpeed * Time.deltaTime, 0.0f, 0.0f, Space.World);
    }

    void MoveLeft()
    {
        transform.Translate(-moveSpeed * Time.deltaTime, 0.0f, 0.0f, Space.World);
    }
}