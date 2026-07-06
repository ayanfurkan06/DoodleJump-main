using UnityEngine;

public class BreakingPlatform : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Temas eden obje karakter mi kontrol et
        if (collision.gameObject.CompareTag("Player"))
        {
            // Karakterin fiziksel bileşenine (Rigidbody2D) erişiyoruz
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

            // Karakter sadece AŞAĞI doğru düşerken bu platform kırılsın
            // (Yukarı zıplarken içinden sorunsuz geçebilmesi için)
            if (playerRb != null && playerRb.linearVelocity.y <= 0.1f)
            {
                // --- KIRILAN PLATFORM SESİ ---
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.brokenPlatformJumpSound);
                }

                // Platformu anında yok et
                Destroy(gameObject);
            }
        }
    }
}