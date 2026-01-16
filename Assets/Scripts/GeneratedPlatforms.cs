using UnityEngine;

public class GeneratedPlatforms : MonoBehaviour
{
    [SerializeField] private GameObject platformPrefab;
    private const int PLATFORMS_NUM = 6;

    private GameObject[] platforms;

    [Header("Ustawienia ruchu")]
    [SerializeField] private float rotationSpeed = 1.0f;  // Pr�dko�� obrotu (radiany na sekund�)
    [SerializeField] private float radius = 9.0f; // Promie� okr�gu

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
        // Obliczamy odst�p k�towy mi�dzy platformami (sta�y)
        float angleStep = Mathf.PI * 2 / PLATFORMS_NUM;

        for (int i = 0; i < platforms.Length; i++)
        {
            // 1. Obliczamy aktualny k�t
            // i * angleStep -> pozycja startowa platformy
            // Time.time * rotationSpeed -> przesuni�cie w czasie (obr�t ca�ego uk�adu)
            float currentAngle = (i * angleStep) + (Time.time * rotationSpeed);

            // 2. Wyznaczamy pozycj� na okr�gu
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;

            // 3. Przypisujemy pozycj� BEZPO�REDNIO
            // Usuwamy MoveTowards, aby platforma by�a "przyklejona" do okr�gu
            platforms[i].transform.position = transform.position + new Vector3(x, y, 0);
        }
    }
}