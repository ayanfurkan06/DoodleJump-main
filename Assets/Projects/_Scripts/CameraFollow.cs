using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [Header("Takip Edilecek Karakter")]
    public Transform target; // Buraya Player'i baglayacagiz

    [Header("Sarsinti Ayarlari")]
    private Vector3 shakeOffset = Vector3.zero; // Sarsintidan kaynakli kayma miktari

    void LateUpdate()
    {
        // Eger karakter atanmadiysa veya yoksa kodu calistirma
        if (target == null) return;

        // Eger karakterin Y pozisyonu (yuksekligi), kameranýn mevcut yuksekliginden fazlaysa
        if (target.position.y > transform.position.y)
        {
            // Kamerayi sadece Y ekseninde (yukari dogru) karakterin hizasina tasiyoruz
            // X ve Z eksenlerini sabit tutuyoruz
            Vector3 newPosition = new Vector3(transform.position.x, target.position.y, transform.position.z);
            transform.position = newPosition;
        }

        // --- SARSINTI ETKÝSÝNÝ KAMERAYA UYGULAMA ---
        // Kameranin normal pozisyonunun uzerine sarsinti ofsetini ekliyoruz
        transform.position += shakeOffset;
    }

    // Bu fonksiyon disaridan (Düsman veya GameManager tarafindan) cagrilacak
    public void TriggerShake(float duration, float magnitude)
    {
        // Surekli ust uste sarsinti baslamasini engellemek icin oncekini durduruyoruz
        StopAllCoroutines();
        // Sarsinti zamanlayicisini baslatiyoruz
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Rastgele cok kucuk X ve Y degerleri uretiyoruz
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // Bu rastgele degerleri kayma miktari olarak kaydediyoruz
            shakeOffset = new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null; // Bir sonraki kareye kadar bekle
        }

        // Sarsinti bittiginde kamerayi tamamen sifirlayip eski haline getiriyoruz
        shakeOffset = Vector3.zero;
    }
}