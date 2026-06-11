using UnityEngine;

public class BreakingPlatform : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Karakter yukaridan asagi dogru duserken bu basamaga bastiysa
        if (collision.gameObject.CompareTag("Player") && collision.relativeVelocity.y <= 0.1f)
        {
            // Karakteri kesin olarak yukari firlat
            collision.gameObject.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(collision.gameObject.GetComponent<Rigidbody2D>().linearVelocity.x, 12f);

            // --- ANIMASYON TETIKLEME KATMANI ---
            // Karakterin altindaki Animator bilesenini bulup bacak esnetmesini tetikliyoruz
            Animator playerAnim = collision.gameObject.GetComponentInChildren<Animator>();

            // --- KESÝN ÇÖZÜM: EKSÝK OLAN TETÝKLEYÝCÝ EMÝR ---
            if (playerAnim != null)
            {
                playerAnim.ResetTrigger("BounceTrigger");
                playerAnim.SetTrigger("BounceTrigger");
            }

            // Basamagi aninda yok et
            Destroy(gameObject);
        }
    }
}