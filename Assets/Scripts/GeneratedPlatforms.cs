using UnityEngine;

public class GeneratedPlatforms : MonoBehaviour
{
    [SerializeField] private GameObject platformPrefab;
    private const int PLATFORMS_NUM = 6;

    private GameObject[] platforms;

    [Header("Ustawienia ruchu")]
    [SerializeField] private float rotationSpeed = 1.0f;  // Prêdkoœæ obrotu (radiany na sekundê)
    [SerializeField] private float radius = 9.0f; // Promieñ okrêgu

    void Awake()
    {
        platforms = new GameObject[PLATFORMS_NUM];

        for (int i = 0; i < PLATFORMS_NUM; i++)
        {
            platforms[i] = Instantiate(platformPrefab, transform.position, Quaternion.identity);
            platforms[i].transform.SetParent(this.transform);
        }
    }

    void Update()
    {
        // Obliczamy odstêp k¹towy miêdzy platformami (sta³y)
        float angleStep = Mathf.PI * 2 / PLATFORMS_NUM;

        for (int i = 0; i < platforms.Length; i++)
        {
            // 1. Obliczamy aktualny k¹t
            // i * angleStep -> pozycja startowa platformy
            // Time.time * rotationSpeed -> przesuniêcie w czasie (obrót ca³ego uk³adu)
            float currentAngle = (i * angleStep) + (Time.time * rotationSpeed);

            // 2. Wyznaczamy pozycjê na okrêgu
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;

            // 3. Przypisujemy pozycjê BEZPOŒREDNIO
            // Usuwamy MoveTowards, aby platforma by³a "przyklejona" do okrêgu
            platforms[i].transform.position = transform.position + new Vector3(x, y, 0);
        }
    }
}