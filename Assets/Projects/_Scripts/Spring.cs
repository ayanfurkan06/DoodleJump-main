using UnityEngine;

public class Spring : MonoBehaviour
{
    public float jumpForce = 25f; // Normal ziplamanin yaklasik 2 katindan fazla bir guc

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Karakterimiz yaya bastiginda (Karakterin Tag'ini artik Player yaptigimiz icin týkýr týkýr calisacak!)
        if (collision.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

            // Karakter asagi dogru dusuyorsa yayi tetikle
            if (rb != null && rb.linearVelocity.y <= 0.1f)
            {
                // Karakteri muthis bir gucle yukari firlat
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

                // --- YENÝ: MERKEZÝ RÜZGAR MOTORUNU TETÝKLÝYORUZ ---
                if (ScreenEffectManager.Instance != null)
                {
                    // Rüzgar efektini açacak ve 0.4 saniye sonra otomatik yok edecek
                    ScreenEffectManager.Instance.TriggerShortWindEffect(0.4f);
                }

                // --- YENÝ EKLENEN: YAY VE RÜZGAR SESÝ ---
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.springSound);
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.windStormSound); // Rüzgar efekti çýktýðý için bu ses de çalacak
                }

                // Ezilme ve eski haline geri dönme sürecini baþlatan Coroutine fonksiyonunu çaðýrýyoruz
                StartCoroutine(SpringBounceRoutine());
            }
        }
    }

    private System.Collections.IEnumerator SpringBounceRoutine()
    {
        Vector3 originalScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
        Vector3 crushedScale = new Vector3(originalScale.x, originalScale.y * 0.5f, originalScale.z);

        // 1. Yayý anýnda %50 ezilme boyutuna getiriyoruz
        transform.localScale = crushedScale;

        // 2. Çok kýsa bir an (0.05 saniye) ezilmiþ olarak bekletiyoruz ki basýlma hissi tam geçsin
        yield return new WaitForSeconds(0.05f);

        // 3. Yayý 0.3 saniye içinde adým adým, yumuþakça eski haline döndürüyoruz
        float elapsedTime = 0f;
        float duration = 0.3f; // Eski haline geri dönme süresi (Saniyeyi buradan ayarlayabilirsin)

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Mathf.Lerp ile ezilmiþ boyuttan orijinal boyuta pürüzsüz bir geçiþ saðlýyoruz
            transform.localScale = Vector3.Lerp(crushedScale, originalScale, elapsedTime / duration);

            yield return null; // Bir sonraki kareye kadar bekle
        }

        // 4. Ýþlemin sonunda tam orijinal boyutunda kaldýðýndan emin oluyoruz
        transform.localScale = originalScale;
    }
}