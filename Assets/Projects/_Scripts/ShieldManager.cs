using UnityEngine;

public class ShieldManager : MonoBehaviour
{
    public static ShieldManager Instance { get; private set; }

    [Header("Kalkan Ayarlarý")]
    public int currentShieldLevel = 0;
    public int shieldHealth = 0;

    [Header("Bileþenler")]
    public Transform shieldSlot;
    private SpriteRenderer shieldSpriteRenderer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (shieldSlot != null)
        {
            shieldSpriteRenderer = shieldSlot.GetComponent<SpriteRenderer>();
            if (shieldSpriteRenderer == null)
            {
                shieldSpriteRenderer = shieldSlot.gameObject.AddComponent<SpriteRenderer>();
            }
        }
    }

    public void EquipShield(int level, Sprite shieldSprite)
    {
        currentShieldLevel = level;
        shieldHealth = level;

        // --- YENÝ EKLENEN: KALKAN KUÞANMA SESÝ ---
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.shieldEquipSound);
        }

        if (shieldSpriteRenderer != null)
        {
            shieldSpriteRenderer.sprite = shieldSprite;
            shieldSpriteRenderer.sortingLayerName = "Player";
            shieldSpriteRenderer.sortingOrder = 30;
        }

        if (shieldSlot != null)
        {
            shieldSlot.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        }

        ClearInvalidShieldsInScene();

        if (ScreenEffectManager.Instance != null)
        {
            ScreenEffectManager.Instance.SetHexEffectActive(true);
        }
    }

    private void ClearInvalidShieldsInScene()
    {
        ShieldItem[] activeShields = FindObjectsByType<ShieldItem>(FindObjectsSortMode.None);

        foreach (ShieldItem shield in activeShields)
        {
            if (shield.shieldLevel <= currentShieldLevel)
            {
                Destroy(shield.gameObject);
            }
        }
    }

    public bool TakeShieldDamage()
    {
        if (currentShieldLevel == 0) return false;

        shieldHealth--;

        if (shieldHealth <= 0)
        {
            BreakShield(); // Kýrýlma sesi bu fonksiyonun içinde çalacak
        }
        else
        {
            // --- YENÝ EKLENEN: KALKAN VARKEN HASAR ALMA SESÝ ---
            // Kalkanýn caný henüz bitmediyse sadece darbe emme sesi çalar
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.shieldHitSound);
            }
        }

        return true;
    }

    public void BreakShield()
    {
        currentShieldLevel = 0;
        shieldHealth = 0;

        // --- YENÝ EKLENEN: KALKAN KIRILMA SESÝ ---
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.shieldBreakSound);
        }

        if (ScreenEffectManager.Instance != null)
        {
            ScreenEffectManager.Instance.SetHexEffectActive(false);
        }

        if (shieldSpriteRenderer != null)
        {
            shieldSpriteRenderer.sprite = null;
        }
    }
}