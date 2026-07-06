using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Header("Platform Prefabları")]
    public GameObject[] platformPrefabs;

    [Header("Ayarlar")]
    public int platformCount = 35;
    public float spawnWidth = 4f;
    public float minY = 0.8f;
    public float maxY = 1.5f;

    [Header("Yerleşim Yüksekliği (Offset) Ayarları")]
    [Tooltip("Objelerin platformun ne kadar üzerinde doğacağını ayarlar. Küçülttüğün objeler için bu değerleri düşür.")]
    public float springOffsetY = 0.5f; // Eskiden 0.9f idi
    public float enemyOffsetY = 0.6f;  // Eskiden 1.5f idi
    public float itemOffsetY = 0.7f;   // Kalkan ve Yakıt için (Eskiden 1.2f idi)

    [Header("Yay Üretim Ayarları")]
    public GameObject springPrefab;

    [Header("Düşman Üretim Ayarları")]
    public GameObject enemyPrefab;

    [Header("Yakıt Üretim Ayarları")]
    public GameObject fuelPrefab;
    [Range(0f, 1f)] public float fuelSpawnChance = 0.15f;

    [Header("Kalkan Üretim Ayarları")]
    public GameObject shield1Prefab; 
    public GameObject shield2Prefab; 
    public GameObject shield3Prefab; 
    [Range(0, 100)] public int shieldSpawnChance = 15; 

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

        // --- YAY ÜRETİMİ ---
        if (chosenPrefab == platformPrefabs[0] && Random.Range(0, 100) < 15)
        {
            // Yeni springOffsetY değişkenini kullanıyoruz
            Vector3 springPos = new Vector3(newPlatform.transform.position.x, newPlatform.transform.position.y + springOffsetY, 0);
            GameObject spawnedSpring = Instantiate(springPrefab, springPos, Quaternion.identity);
            spawnedSpring.transform.SetParent(newPlatform.transform);
        }

        TrySpawnShield(newPlatform);

        // --- DÜŞMAN ÜRETİMİ ---
        if (newPlatform.transform.childCount == 0 && Random.Range(0, 100) < 10)
        {
            // Yeni enemyOffsetY değişkenini kullanıyoruz
            Vector3 enemyPos = new Vector3(newPlatform.transform.position.x, newPlatform.transform.position.y + enemyOffsetY, 0);
            GameObject spawnedEnemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
            spawnedEnemy.transform.SetParent(newPlatform.transform);
        }

        // --- YAKIT ÜRETİMİ ---
        if (newPlatform.transform.childCount == 0 && fuelPrefab != null && Random.value < fuelSpawnChance)
        {
            // Yeni itemOffsetY değişkenini kullanıyoruz
            Vector3 fuelPosition = new Vector3(newPlatform.transform.position.x, newPlatform.transform.position.y + itemOffsetY, spawnPosition.z);
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

        if (currentShield >= 3) return;

        int nextRequiredLevel = currentShield + 1;
        GameObject shieldToSpawn = null;

        if (nextRequiredLevel == 1 && shield1Prefab != null) shieldToSpawn = shield1Prefab;
        else if (nextRequiredLevel == 2 && shield2Prefab != null) shieldToSpawn = shield2Prefab;
        else if (nextRequiredLevel == 3 && shield3Prefab != null) shieldToSpawn = shield3Prefab;

        if (shieldToSpawn != null)
        {
            // Yeni itemOffsetY değişkenini kullanıyoruz
            Vector3 shieldPos = new Vector3(platform.transform.position.x, platform.transform.position.y + itemOffsetY, 0);
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