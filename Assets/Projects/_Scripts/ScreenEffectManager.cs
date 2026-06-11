using UnityEngine;

public class ScreenEffectManager : MonoBehaviour
{
    public static ScreenEffectManager Instance { get; private set; }

    [Header("Efekt Prefablari (Animasyonlu)")]
    public GameObject screenBloodPrefab;
    public GameObject screenHexPrefab;
    public GameObject screenGradientPrefab;
    public GameObject screenWindPrefab;

    private GameObject activeHexInstance;
    private GameObject activeWindInstance;
    private Transform camTransform;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }
    }

    // --- 1. HASAR ALMA / ÖLÜM EFEKTÝ ---
    public void TriggerBloodEffect()
    {
        if (screenBloodPrefab != null && camTransform != null)
        {
            GameObject blood = Instantiate(screenBloodPrefab, camTransform);
            ResetEffectTransform(blood);

            // --- KESÝN ÇÖZÜM: Efekti 1 saniye sonra sahneden otomatik siler ---
            Destroy(blood, 1f);
        }
    }

    // --- 2. YAKIT TOPLAMA EFEKTÝ ---
    public void TriggerGradientEffect()
    {
        if (screenGradientPrefab != null && camTransform != null)
        {
            GameObject gradient = Instantiate(screenGradientPrefab, camTransform);
            ResetEffectTransform(gradient);

            // --- KESÝN ÇÖZÜM: Efekti 1 saniye sonra sahneden otomatik siler ---
            Destroy(gradient, 1f);
        }
    }

    // --- 3. KALKAN AKTÝFLEÞTÝRME ---
    public void SetHexEffectActive(bool isActive)
    {
        if (isActive)
        {
            if (activeHexInstance == null && screenHexPrefab != null && camTransform != null)
            {
                activeHexInstance = Instantiate(screenHexPrefab, camTransform);
                ResetEffectTransform(activeHexInstance);
            }
        }
        else
        {
            if (activeHexInstance != null)
            {
                Destroy(activeHexInstance);
                activeHexInstance = null;
            }
        }
    }

    // --- 4. RÜZGAR / UÇUÞ EFEKTÝ ---
    public void SetWindEffectActive(bool isActive)
    {
        if (isActive)
        {
            if (activeWindInstance == null && screenWindPrefab != null && camTransform != null)
            {
                activeWindInstance = Instantiate(screenWindPrefab, camTransform);
                ResetEffectTransform(activeWindInstance);
            }
        }
        else
        {
            if (activeWindInstance != null)
            {
                Destroy(activeWindInstance);
                activeWindInstance = null;
            }
        }
    }
    // --- YAY ÝÇÝN KISA SÜRELÝ RÜZGAR EFEKTÝ TETÝKLEYÝCÝSÝ ---
    // --- YAY ÝÇÝN GARANTÝLÝ RÜZGAR EFEKTÝ TETÝKLEYÝCÝSÝ ---
    public void TriggerShortWindEffect(float duration)
    {
        // Eðer arkada çalýþan eski bir rüzgar kapatma süreci varsa önce onu durdurur
        StopAllCoroutines();
        // Yeni rüzgar sürecini milisaniyelik hassasiyetle baþlatýr
        StartCoroutine(ShortWindRoutine(duration));
    }

    private System.Collections.IEnumerator ShortWindRoutine(float duration)
    {
        // Rüzgar efektini aç
        SetWindEffectActive(true);

        // Belirtilen süre (0.4 saniye) kadar burada kesin olarak bekle
        yield return new WaitForSeconds(duration);

        // Süre bitince rüzgarý kesin olarak kapat
        SetWindEffectActive(false);
    }

    private void ResetEffectTransform(GameObject effectGo)
    {
        effectGo.transform.SetParent(camTransform);
        effectGo.transform.localPosition = new Vector3(0f, 0f, 5f);
        effectGo.transform.localRotation = Quaternion.identity;
        effectGo.transform.localScale = Vector3.one;

        // DÜZELTÝLEN SATIR: transform kaldýrýldý, doðrudan nesneye atandý
        effectGo.layer = LayerMask.NameToLayer("Default");

        MonoBehaviour hvlScript = effectGo.GetComponent("HS_ScreenEffect") as MonoBehaviour;
        if (hvlScript != null)
        {
            hvlScript.enabled = false;
        }

        ParticleSystem ps = effectGo.GetComponent<ParticleSystem>();
        if (ps == null) ps = effectGo.GetComponentInChildren<ParticleSystem>();

        if (ps != null)
        {
            ParticleSystemRenderer psr = ps.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                psr.sortingLayerName = "Default";
                psr.sortingOrder = 500;
            }
        }
    }
} // Scriptin en sonundaki ana sýnýf parantezi