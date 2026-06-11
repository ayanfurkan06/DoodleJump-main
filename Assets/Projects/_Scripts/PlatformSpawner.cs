using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Header("Platform Prefablarý")]
    public GameObject[] platformPrefabs;

    [Header("Ayarlar")]
    public int platformCount = 20;
    public float spawnWidth = 4f;
    public float minY = 1.5f;
    public float maxY = 2.5f;

    [Header("Yay Üretim Ayarlarý")]
    public GameObject springPrefab;

    [Header("Düþman Üretim Ayarlarý")]
    public GameObject enemyPrefab;

    [Header("Yakýt Üretim Ayarlarý")]
    public GameObject fuelPrefab;
    [Range(0f, 1f)] public float fuelSpawnChance = 0.15f;

    [Header("Yeni Kalkan Üretim Ayarlarý")]
    public GameObject shield1Prefab; // Seviye 1 Kalkan Prefabý
    public GameObject shield2Prefab; // Seviye 2 Kalkan Prefabý
    public GameObject shield3Prefab; // Seviye 3 Kalkan Prefabý
    [Range(0, 100)] public int shieldSpawnChance = 15; // Kalkan çýkma þansý (%)

    private Vector3 spawnPosition = new Vector3();

    void Start()
    {
        spawnPosition.y = -2f;

        for (int i = 0; i < platformCount; i++)
        {
            spawnPosition.y += Random.Range(minY, maxY);
            spawnPosition.x = Random.Range(-spawnWidth, spawnWidth);

            GameObject chosenPrefab = (i == 0) ? platformPrefabs[0] : GetRandomPlatform();
            GameObject newPlatform = Instantiate(chosenPrefab, spawnPosition, Quaternion.identity);

            newPlatform.AddComponent<PlatformDestroyer>();
        }
    }

    public void SpawnNewPlatform()
    {
        spawnPosition.y += Random.Range(minY, maxY);
        spawnPosition.x = Random.Range(-spawnWidth, spawnWidth);

        GameObject chosenPrefab = GetRandomPlatform();
        GameObject newPlatform = Instantiate(chosenPrefab, spawnPosition, Quaternion.identity);

        newPlatform.AddComponent<PlatformDestroyer>();

        // --- YAY ÜRETÝMÝ ---
        if (chosenPrefab == platformPrefabs[0] && Random.Range(0, 100) < 15)
        {
            Vector3 springPos = new Vector3(newPlatform.transform.position.x, newPlatform.transform.position.y + 0.9f, 0);
            GameObject spawnedSpring = Instantiate(springPrefab, springPos, Quaternion.identity);
            spawnedSpring.transform.SetParent(newPlatform.transform);
        }

        // --- SENÝN ÖZEL KALKAN ÜRETÝM MOTORUN ---
        TrySpawnShield(newPlatform);

        // --- DÜÞMAN ÜRETÝMÝ ---
        if (newPlatform.transform.childCount == 0 && Random.Range(0, 100) < 10)
        {
            Vector3 enemyPos = new Vector3(newPlatform.transform.position.x, newPlatform.transform.position.y + 1.5f, 0);
            GameObject spawnedEnemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
            spawnedEnemy.transform.SetParent(newPlatform.transform);
        }

        // --- YAKIT ÜRETÝMÝ ---
        if (newPlatform.transform.childCount == 0 && fuelPrefab != null && Random.value < fuelSpawnChance)
        {
            Vector3 fuelPosition = new Vector3(newPlatform.transform.position.x, newPlatform.transform.position.y + 1.2f, spawnPosition.z);
            GameObject spawnedFuel = Instantiate(fuelPrefab, fuelPosition, Quaternion.identity);
            spawnedFuel.transform.SetParent(newPlatform.transform);
        }
    }

    private void TrySpawnShield(GameObject platform)
    {
        if (platform.transform.childCount > 0) return;
        if (Random.Range(0, 100) >= shieldSpawnChance) return;

        int currentShield = 0;
        if (ShieldManager.Instance != null)
        {
            currentShield = ShieldManager.Instance.currentShieldLevel;
        }

        // Kural: Karakterde 3. seviye kalkan varsa veya daha büyüðü, haritada HÝÇBÝR kalkan spawn olamaz
        if (currentShield >= 3) return;

        // YENÝ KATÝ KURAL: Haritada SADECE karakterin elindeki kalkanýn BÝR ÜST SEVÝYESÝ doðabilir.
        // Elinde kalkan yoksa (0) -> Sadece 1 doðar
        // Elinde 1 varsa          -> Sadece 2 doðar
        // Elinde 2 varsa          -> Sadece 3 doðar
        int nextRequiredLevel = currentShield + 1;

        GameObject shieldToSpawn = null;

        if (nextRequiredLevel == 1 && shield1Prefab != null) shieldToSpawn = shield1Prefab;
        else if (nextRequiredLevel == 2 && shield2Prefab != null) shieldToSpawn = shield2Prefab;
        else if (nextRequiredLevel == 3 && shield3Prefab != null) shieldToSpawn = shield3Prefab;

        if (shieldToSpawn != null)
        {
            Vector3 shieldPos = new Vector3(platform.transform.position.x, platform.transform.position.y + 1.2f, 0);
            GameObject spawnedShield = Instantiate(shieldToSpawn, shieldPos, Quaternion.identity);
            spawnedShield.transform.SetParent(platform.transform);
        }
    }

    private GameObject GetRandomPlatform()
    {
        int chance = Random.Range(0, 100);
        if (chance < 60) return platformPrefabs[0];
        else if (chance < 80) return platformPrefabs[1];
        else return platformPrefabs[2];
    }
}