using UnityEngine;

public class GeneratedPlatforms : MonoBehaviour
{
    [SerializeField] private GameObject platformPrefab;
    private const int PLATFORMS_NUM = 6; //

    private GameObject[] platforms;

    [Header("Ustawienia ruchu")]
    [SerializeField] private float speed = 1.0f;  // Prêdkoœæ obrotu
    [SerializeField] private float radius = 3.0f; // Promieñ okrêgu

    void Awake()
    {
        platforms = new GameObject[PLATFORMS_NUM]; //

        for (int i = 0; i < PLATFORMS_NUM; i++)
        {
            // Tworzymy platformy (ich pozycja zostanie ustawiona w Update)
            platforms[i] = Instantiate(platformPrefab, transform.position, Quaternion.identity);
            platforms[i].transform.SetParent(this.transform);
        }
    }

    void Update()
    {
        // Implementacja cyklicznego ruchu po okrêgu
        for (int i = 0; i < platforms.Length; i++)
        {
            // 1. Obliczamy k¹t dla danej platformy (rozstawienie + ruch w czasie)
            // Ka¿da platforma jest przesuniêta o sta³y u³amek okrêgu (2 * PI / Liczba)
            float angle = i * Mathf.PI * 2 / PLATFORMS_NUM + Time.time * speed;

            // 2. Wyznaczamy now¹ pozycjê X i Y na podstawie funkcji Sin i Cos
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            // 3. Pozycja docelowa wzglêdem œrodka generatora
            Vector3 targetPos = new Vector3(x, y, 0) + transform.position;

            // 4. Przesuniêcie platformy (u¿ywamy MoveTowards dla p³ynnoœci)
            platforms[i].transform.position = Vector3.MoveTowards(
                platforms[i].transform.position,
                targetPos,
                speed * Time.deltaTime
            );
        }
    }
}