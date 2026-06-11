using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float speed = 2f;      // Saga sola gidis hizi
    public float widthBound = 2.3f; // Ekranda donecegi sinir

    private int direction = 1;    // 1 ise saga, -1 ise sola gider

    void Update()
    {
        // Platformu her karede saga veya sola dogru hareket ettiriyoruz
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);

        // Ekran sinirlarina ulastiginda yonunu tersine cevir
        if (transform.position.x > widthBound)
        {
            direction = -1;
        }
        else if (transform.position.x < -widthBound)
        {
            direction = 1;
        }
    }
}