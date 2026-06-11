using UnityEngine;

public class ShieldItem : MonoBehaviour
{
    [Header("Kalkan Özellikleri")]
    [Range(1, 3)] public int shieldLevel = 1; // Müfettiþten 1, 2 veya 3 seçeceðiz

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kalkan nesnesine çarpan þey "Player" etiketine sahipse
        if (collision.CompareTag("Player"))
        {
            // Bu kalkanýn üzerindeki görseli otomatik alalým
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            Sprite shieldSprite = (sr != null) ? sr.sprite : null;

            // ShieldManager'a karakterin bu kalkaný kuþandýðýný haber veriyoruz
            if (ShieldManager.Instance != null)
            {
                ShieldManager.Instance.EquipShield(shieldLevel, shieldSprite);
            }

            // Kalkan karakter tarafýndan alýndýðý için sahneden yok olsun
            Destroy(gameObject);
        }
    }
}