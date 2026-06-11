using UnityEngine;

public class FuelItem : MonoBehaviour
{
    [Header("Yakýt Ayarý")]
    public float fuelAmount = 10f; // Bu yakýt alýndýðýnda barý ne kadar dolduracak? (0-100 arasý)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Temas eden nesne oyuncu mu kontrol et
        if (collision.CompareTag("Player"))
        {
            // Oyuncunun üzerindeki PlayerController koduna eriþ ve yakýt ekle
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.AddFuel(fuelAmount);

                if (ScreenEffectManager.Instance != null)
                {
                    ScreenEffectManager.Instance.TriggerGradientEffect();
                }

                // Ýleride ses efekti kutusu dolduðunda çalmasý için (Null-safe)
                if (AudioManager.Instance != null)
                {
                    // Þimdilik ses kutusu boþ olsa da hata vermez, can alma sesini veya yeni atayacaðýn sesi çalabilirsin
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.collectHealthSound);
                }

                // Yakýt alýndýðý için nesneyi sahneden sil
                Destroy(gameObject);
            }
        }
    }
}