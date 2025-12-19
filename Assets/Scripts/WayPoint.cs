using UnityEngine;

public class WaypointFollower : MonoBehaviour
{
    // Tablica punktów kontrolnych
    [SerializeField] private GameObject[] waypoints;

    // Indeks bie¿¹cego punktu
    private int currentWaypointIndex = 0;

    // Prêdkoœæ poruszania siê
    [SerializeField] private float speed = 2.0f;

    void Update()
    {
        // 1. Oblicz odleg³oœæ do bie¿¹cego punktu
        if (Vector2.Distance(waypoints[currentWaypointIndex].transform.position, transform.position) < 0.1f)
        {
            // 2. Jeœli jesteœmy blisko, zwiêksz indeks (modulo zapêtla trasê)
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }

        // 3. Przesuñ platformê w stronê punktu
        transform.position = Vector2.MoveTowards(
            transform.position,
            waypoints[currentWaypointIndex].transform.position,
            speed * Time.deltaTime
        );
    }
}