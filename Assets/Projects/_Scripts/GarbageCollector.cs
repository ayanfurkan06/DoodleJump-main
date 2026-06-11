using UnityEngine;

public class GarbageCollector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Temizlik çizgisinin içine giren nesnenin etiketine (Tag) bakýyoruz
        // Platformlarý zaten kendi kodu siliyor, o yüzden burada diðer nesneleri yakalýyoruz
        if (collision.CompareTag("Enemy") || collision.CompareTag("Health") || collision.CompareTag("Fuel"))
        {
            // Nesneyi tamamen sahneden ve RAM'den siler
            Destroy(collision.gameObject);
        }
    }
}