using UnityEngine;
using System.Collections.Generic;

public class WeatherManager : MonoBehaviour
{
    [System.Serializable]
    public class StormPhase
    {
        public string phaseName = "Fýrtýna Aþamasý";
        public float startScore;
        public float endScore;
        public float windForce;
    }

    [System.Serializable]
    public class CustomEffectSettings
    {
        public string effectName = "Efekt Ayarý";
        public GameObject prefab;
        public Vector3 localPosition = new Vector3(0f, 0f, 10f);
        public Vector3 localRotation = Vector3.zero;
        public Vector3 localScale = Vector3.one;
    }

    [Header("--- FIRTINA AYARLARI ---")]
    public List<StormPhase> stormPhases = new List<StormPhase>();

    [Header("--- SONSUZ DÖNGÜ AYARLARI ---")]
    public float infiniteStormDuration = 100f;
    public float infiniteCalmDuration = 100f;

    [Header("--- SARSINTI HIZI ---")]
    public float windChangeSpeed = 3f;

    [Header("--- DETAYLI FIRTINA EFEKT AYARLARI ---")]
    public List<CustomEffectSettings> stormEffects = new List<CustomEffectSettings>();

    private Camera mainCamera;
    private Rigidbody2D playerRb;
    private PlayerController playerCtrl; // Oyuncu kontrolcü referansý

    private float currentScore = 0f;
    private bool isWindActive = false;
    private float currentWindForce = 0f;

    private float noiseTime = 0f;

    private GameObject currentActiveEffect = null;
    private int lastSelectedEffectIndex = -1;
    private bool wasWindActiveInLastFrame = false;

    void Start()
    {
        mainCamera = Camera.main;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody2D>();
            playerCtrl = player.GetComponent<PlayerController>(); // Kodu buraya baðladýk
        }

        noiseTime = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (mainCamera != null)
        {
            currentScore = mainCamera.transform.position.y;
        }

        CheckStormStatus();
        HandleDynamicVisualEffect();
    }

    void CheckStormStatus()
    {
        for (int i = 0; i < stormPhases.Count; i++)
        {
            StormPhase phase = stormPhases[i];
            if (currentScore >= phase.startScore && currentScore < phase.endScore)
            {
                isWindActive = true;
                currentWindForce = phase.windForce;
                return;
            }
        }

        if (stormPhases.Count > 0)
        {
            StormPhase lastPhase = stormPhases[stormPhases.Count - 1];

            if (currentScore >= lastPhase.endScore)
            {
                float scoreAfterPhases = currentScore - lastPhase.endScore;
                float cycleLength = infiniteStormDuration + infiniteCalmDuration;
                float currentCycleProgress = scoreAfterPhases % cycleLength;

                if (currentCycleProgress < infiniteStormDuration)
                {
                    isWindActive = true;
                    currentWindForce = lastPhase.windForce;
                }
                else
                {
                    isWindActive = false;
                    currentWindForce = 0f;
                }
                return;
            }
        }

        isWindActive = false;
        currentWindForce = 0f;
    }

    void HandleDynamicVisualEffect()
    {
        // Önemli: Görsel efekt sadece fýrtýna skor aralýðýna göre çalýþýr, oyuncunun ölmesi efekti kapatmaz.
        if (isWindActive && !wasWindActiveInLastFrame)
        {
            if (stormEffects.Count > 0 && mainCamera != null)
            {
                int randomIndex = lastSelectedEffectIndex;

                if (stormEffects.Count > 1)
                {
                    while (randomIndex == lastSelectedEffectIndex)
                    {
                        randomIndex = Random.Range(0, stormEffects.Count);
                    }
                }
                else
                {
                    randomIndex = 0;
                }

                lastSelectedEffectIndex = randomIndex;
                CustomEffectSettings settings = stormEffects[randomIndex];

                if (settings.prefab != null)
                {
                    currentActiveEffect = Instantiate(settings.prefab, Vector3.zero, Quaternion.identity);
                    currentActiveEffect.transform.SetParent(mainCamera.transform, false);

                    currentActiveEffect.transform.localPosition = settings.localPosition;
                    currentActiveEffect.transform.localRotation = Quaternion.Euler(settings.localRotation);
                    currentActiveEffect.transform.localScale = settings.localScale;
                }
            }
        }
        else if (!isWindActive && currentActiveEffect != null)
        {
            Destroy(currentActiveEffect);
            currentActiveEffect = null;
        }

        wasWindActiveInLastFrame = isWindActive;
    }

    void FixedUpdate()
    {
        if (isWindActive && playerRb != null)
        {
            // KRÝTÝK DÜZELTME: Eðer oyuncu senin kodundaki IsDead() fonksiyonundan true dönüyorsa, rüzgar itmeyi anýnda kes!
            if (playerCtrl != null && playerCtrl.IsDead())
            {
                return; // Alttaki AddForce kodunu çalýþtýrmadan doðrudan fonksiyondan çýkar
            }

            noiseTime += Time.fixedDeltaTime * windChangeSpeed;

            float rawNoise = Mathf.PerlinNoise(noiseTime, 0f);
            float dynamicWindDirection = (rawNoise * 2f) - 1f;

            float finalCalculatedForce = dynamicWindDirection * currentWindForce;
            playerRb.AddForce(new Vector2(finalCalculatedForce, 0f), ForceMode2D.Force);
        }
    }
}