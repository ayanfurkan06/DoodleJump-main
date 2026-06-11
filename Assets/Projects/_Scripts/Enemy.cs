using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Darbe Zýplama Gücü")]
    public float bounceForce = 12f;

    [Header("Saldýrý Menzil Ayarlarý")]
    public float attackRange = 3.5f;
    private Transform playerTransform;
    private Animator enemyAnimator;
    private bool wasPlayerClose = false;

    [Header("Görsel Yön Ayarý")]
    // Karakterde olduðu gibi düþmanýn yüzünün dönmesi için kendi görsel nesnesini (Transform) buraya baðlayacaðýz.
    // Eðer boþ býrakýrsan kod otomatik olarak bu objenin kendisini döndürür.
    public Transform enemyVisual;

    // Kalkan varken üst üste çok hýzlý hasar vermemesi için küçük bir zaman kilidi ekliyoruz (Saniyede 1 kere vursun)
    private float nextDamageTime = 0f;
    private float damageCooldown = 1f;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        enemyAnimator = GetComponent<Animator>();

        // Eðer Müfettiþten (Inspector) enemyVisual atanmadýysa, otomatik olarak bu objenin kendisini seç
        if (enemyVisual == null)
        {
            enemyVisual = this.transform;
        }
    }

    void Update()
    {
        // --- 1. ÝÞ: OYUNCUYU TAKÝP EDEN YÖN MOTORU (GLOBAL BOYUT KÝLÝTLÝ) ---
        // Düþman hangi platformun altýna baðlanýrsa baðlansýn, platformun yamuk boyutlarýný (Scale) tamamen sýfýrlar.
        // Düþmanýn ekrandaki net görüntüsünü her zaman senin prefabda tasarladýðýn nizami boyutuna sabitler!

        Vector3 globalScale = transform.lossyScale;

        // Oyuncunun X pozisyonu düþmandan büyükse oyuncu SAÐDADIR
        if (playerTransform.position.x > transform.position.x)
        {
            // Oyuncu saðdayken düþman sola (-1) dönecek
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x / globalScale.x),
                Mathf.Abs(transform.localScale.y / globalScale.y),
                Mathf.Abs(transform.localScale.z / globalScale.z)
            );
        }
        // Oyuncunun X pozisyonu düþmandan küçükse oyuncu SOLDADIR
        else if (playerTransform.position.x < transform.position.x)
        {
            // Oyuncu soldayken düþman saða (1) dönecek
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x / globalScale.x),
                Mathf.Abs(transform.localScale.y / globalScale.y),
                Mathf.Abs(transform.localScale.z / globalScale.z)
            );
        }

        // --- 2. ÝÞ: KILIÇ SALLAMA MENZÝL KONTROLÜ ---
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        bool isPlayerCloseNow = distanceToPlayer <= attackRange;

        if (isPlayerCloseNow != wasPlayerClose)
        {
            wasPlayerClose = isPlayerCloseNow;
            enemyAnimator.SetBool("isAttacking", isPlayerCloseNow);
        }
    }

    // KESÝN ÇÖZÜM: OnTriggerEnter2D yerine OnTriggerStay2D kullanýyoruz. 
    // Böylece kalkan varken düþman yok olmaz, oyuncu temas ettiði sürece hasar almaya devam eder!
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Eger hasar verme suresi gelmediyse (Cooldown) bekle, saniyede 1 kere vur
            if (Time.time < nextDamageTime) return;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.takeDamageSound);
            }

            // Kamera sarsintisini tetikliyoruz
            CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
            if (cam != null)
            {
                cam.TriggerShake(0.15f, 0.1f);
            }

            // --- YENÝ: HASAR ANINDA KAN EFEKTÝNÝ TETÝKLÝYORUZ ---
            if (ScreenEffectManager.Instance != null)
            {
                ScreenEffectManager.Instance.TriggerBloodEffect();
            }

            bool hasShieldSavedUs = false;

            if (ShieldManager.Instance != null)
            {
                hasShieldSavedUs = ShieldManager.Instance.TakeShieldDamage();
            }

            // Eger kalkan bizi kurtaramadiysa (Kalkan yoksa veya bittiyse) oyuncu olur
            if (!hasShieldSavedUs)
            {
                // Olum aninda daha guclu bir kamera sarsintisi yapýyoruz
                if (cam != null)
                {
                    cam.TriggerShake(0.35f, 0.25f);
                }

                // --- YENÝ: ÖLDÜÐÜMÜZDE KALKAN EFEKTÝNÝ TAMAMEN KAPATIYORUZ ---
                if (ScreenEffectManager.Instance != null)
                {
                    ScreenEffectManager.Instance.SetHexEffectActive(false);
                }

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerEnemyDeath(collision.gameObject);
                }
                return;
            }

            // Eger kalkan bizi kurtardiysa, dusman YOK OLMAZ!
            nextDamageTime = Time.time + damageCooldown;

            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}