using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Darbe Zıplama Gücü")]
    public float bounceForce = 12f;

    [Header("Saldırı Menzil Ayarları")]
    public float attackRange = 3.5f;
    private Transform playerTransform;
    private Animator enemyAnimator;
    private bool wasPlayerClose = false;

    [Header("Görsel Yön Ayarı")]
    public Transform enemyVisual;

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

        if (enemyVisual == null)
        {
            enemyVisual = this.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // --- DÜZELTİLEN ALAN: Yön Döndürme (Flipping) ---
        // Objenin mevcut boyutunu (Scale) alıp bozmadan sadece x eksenini eksi/artı yapıyoruz
        Vector3 currentScale = transform.localScale;

        if (playerTransform.position.x > transform.position.x)
        {
            // Oyuncu düşmanın sağındaysa sola baksın (X'i negatif yap)
            currentScale.x = -Mathf.Abs(currentScale.x);
        }
        else if (playerTransform.position.x < transform.position.x)
        {
            // Oyuncu düşmanın solundaysa sağa baksın (X'i pozitif yap)
            currentScale.x = Mathf.Abs(currentScale.x);
        }

        transform.localScale = currentScale;

        // --- SALDIRI KONTROLÜ ---
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        bool isPlayerCloseNow = distanceToPlayer <= attackRange;

        if (isPlayerCloseNow != wasPlayerClose)
        {
            wasPlayerClose = isPlayerCloseNow;
            if (enemyAnimator != null) enemyAnimator.SetBool("isAttacking", isPlayerCloseNow);

            // Düşman kılıç sallama animasyonuna geçtiği o ilk karede ses tetiklenir
            if (isPlayerCloseNow && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.enemySwordSwingSound);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (Time.time < nextDamageTime) return;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.takeDamageSound);
            }

            CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
            if (cam != null)
            {
                cam.TriggerShake(0.15f, 0.1f);
            }

            if (ScreenEffectManager.Instance != null)
            {
                ScreenEffectManager.Instance.TriggerBloodEffect();
            }

            bool hasShieldSavedUs = false;

            if (ShieldManager.Instance != null)
            {
                hasShieldSavedUs = ShieldManager.Instance.TakeShieldDamage();
            }

            // Eğer kalkan bizi kurtaramadıysa (Kalkan yoksa veya bittiyse) oyuncu ölür
            if (!hasShieldSavedUs)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.enemyDeathSound);
                }

                if (cam != null)
                {
                    cam.TriggerShake(0.35f, 0.25f);
                }

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

            // Eğer kalkan bizi kurtardıysa
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