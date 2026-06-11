using UnityEngine;

public class ShieldManager : MonoBehaviour
{
    public static ShieldManager Instance { get; private set; }

    [Header("Kalkan Ayarlarý")]
    public int currentShieldLevel = 0; // 0 = Kalkan yok, 1 = Seviye 1, 2 = Seviye 2, 3 = Seviye 3
    public int shieldHealth = 0;       // Kalkanýn kalan darbe dayanýklýlýðý

    [Header("Bileþenler")]
    public Transform shieldSlot;       // Karakterin elindeki ShieldSlot nesnesi
    private SpriteRenderer shieldSpriteRenderer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // ShieldSlot üzerindeki SpriteRenderer bileþenine ulaþalým
        if (shieldSlot != null)
        {
            shieldSpriteRenderer = shieldSlot.GetComponent<SpriteRenderer>();
            if (shieldSpriteRenderer == null)
            {
                // Eðer ShieldSlot nesnesinde SpriteRenderer yoksa otomatik eklesin
                shieldSpriteRenderer = shieldSlot.gameObject.AddComponent<SpriteRenderer>();
            }
        }
    }
    public void EquipShield(int level, Sprite shieldSprite)
    {
        currentShieldLevel = level;
        shieldHealth = level;

        if (shieldSpriteRenderer != null)
        {
            shieldSpriteRenderer.sprite = shieldSprite;

            // --- KESÝN ÖNCELÝK KÝLÝTLEME MOTORU ---
            // Kalkanýn hangi katmanda duracaðýný kodla "Player" katmanýna zorluyoruz.
            shieldSpriteRenderer.sortingLayerName = "Player";

            // Sýra numarasýný 10 yerine 30 yapýyoruz. 
            // Böylece karakterin gövdesi, kolu, montu kaç olursa olsun kalkan hepsinin EN ÜSTÜNE çizilecek!
            shieldSpriteRenderer.sortingOrder = 30;
        }

        // --- YENÝ BOYUT KÝLÝTLEME MOTORU ---
        if (shieldSlot != null)
        {
            shieldSlot.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        }

        // Haritadaki eski kalkanlarý temizle
        ClearInvalidShieldsInScene();

        if (ScreenEffectManager.Instance != null)
        {
            ScreenEffectManager.Instance.SetHexEffectActive(true);
        }
    }

    // Sahnede önceden üretilmiþ hatalý kalkanlarý bulan ve silen fonksiyon
    private void ClearInvalidShieldsInScene()
    {
        // Sahnede aktif olan tüm ShieldItem bileþenlerini bulur
        ShieldItem[] activeShields = FindObjectsByType<ShieldItem>(FindObjectsSortMode.None);

        foreach (ShieldItem shield in activeShields)
        {
            // YENI KURAL: Oyuncunun elindeki kalkan seviyesinden KÜÇÜK veya EÞÝT olan 
            // sahnedeki tüm kalkanlarý anýnda yok et.
            // Örneðin elinde 1 varsa sahnedeki tüm 1'leri siler. Elinde 2 varsa tüm 1 ve 2'leri siler.
            if (shield.shieldLevel <= currentShieldLevel)
            {
                Destroy(shield.gameObject);
            }
        }
    }

    // Düþmana çarpýldýðýnda kalkanýn hasar alma mekanizmasý
    public bool TakeShieldDamage()
    {
        if (currentShieldLevel == 0) return false; // Kalkan yoksa hasarý engelleme, karakter doðrudan etkilensin

        shieldHealth--;

        if (shieldHealth <= 0)
        {
            BreakShield();
        }

        return true; // Hasar baþarýyla kalkan tarafýndan emildi
    }

    // Kalkan kýrýldýðýnda sýfýrlama ve elden düþürme mantýðý
    public void BreakShield()
    {
        currentShieldLevel = 0;
        shieldHealth = 0;

        if (ScreenEffectManager.Instance != null)
        {
            ScreenEffectManager.Instance.SetHexEffectActive(false);
        }

        if (shieldSpriteRenderer != null)
        {
            shieldSpriteRenderer.sprite = null; // Elindeki görseli temizler
        }
    }
}