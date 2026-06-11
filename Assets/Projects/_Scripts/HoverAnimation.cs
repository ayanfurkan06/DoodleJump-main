using UnityEngine;

public class HoverAnimation : MonoBehaviour
{
    public float movementSpeed = 2f;
    public float movementRange = 0.2f;

    private Vector3 startLocalPosition; // Dünya deðil, lokal pozisyonu tutacaðýz

    void Start()
    {
        // Platformun içindeki ilk yerel pozisyonunu hafýzaya alýyoruz
        startLocalPosition = transform.localPosition;
    }

    void Update()
    {
        // Hareketi sadece kendi lokal Y ekseninde yapýyoruz
        float newLocalY = startLocalPosition.y + Mathf.Sin(Time.time * movementSpeed) * movementRange;

        // Platformu etkilemeden sadece kendi yerel konumunu güncelliyoruz
        transform.localPosition = new Vector3(startLocalPosition.x, newLocalY, startLocalPosition.z);
    }
}