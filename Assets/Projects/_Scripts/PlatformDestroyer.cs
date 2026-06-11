using UnityEngine;

public class PlatformDestroyer : MonoBehaviour
{
    private PlatformSpawner spawner;
    private bool hasSpawnedNext = false; // Mukerrer uretimi engellemek icin kilit

    void Start()
    {
        // Sahnede duran PlatformSpawner objesine ve onun koduna erisiyoruz
        spawner = FindFirstObjectByType<PlatformSpawner>();
    }

    void Update()
    {
        // 1. HAMLE: Kamera genisledigi icin yukarida bosluk kalmamasi adina 
        // yeni platform uretim emrini hala kameraya yakin bir mesafede (-6f) tetikliyoruz.
        if (!hasSpawnedNext && transform.position.y < Camera.main.transform.position.y - 6f)
        {
            hasSpawnedNext = true; // Bu basamagin sadece 1 kere uretim tetiklemesini sagliyoruz

            if (spawner != null)
            {
                spawner.SpawnNewPlatform();
            }
        }

        // 2. HAMLE: Platformun fiziksel olarak sahneden silinmesini, kameranýn yenilenen 
        // alt sýnýrýnýn tamamen dýsýna cýkana kadar (-11f) erteliyoruz.
        if (transform.position.y < Camera.main.transform.position.y - 11f)
        {
            // Bu eski platformu hafizadan ve sahneden tamamen sil
            Destroy(gameObject);
        }
    }
}