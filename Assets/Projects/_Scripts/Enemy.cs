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
        Vector3 globalScale = transform.lossyScale;

        if (playerTransform.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x / globalScale.x),
                Mathf.Abs(transform.localScale.y / globalScale.y),
                Mathf.Abs(transform.localScale.z / globalScale.z)
            );
        }
        else if (playerTransform.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x / globalScale.x),
                Mathf.Abs(transform.localScale.y / globalScale.y),
                Mathf.Abs(transform.localScale.z / globalScale.z)
            );
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        bool isPlayerCloseNow = distanceToPlayer <= attackRange;

        if (isPlayerCloseNow != wasPlayerClose)
        {
            wasPlayerClose = isPlayerCloseNow;
            enemyAnimator.SetBool("isAttacking", isPlayerCloseNow);

            // --- YENÝ EKLENEN: DÜÞMAN KILICINI SALLAMA SESÝ ---
            // Düþman kýlýç sallama animasyonuna geçtiði o ilk karede ses tetiklenir
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

            // Eger kalkan bizi kurtaramadiysa (Kalkan yoksa veya bittiyse) oyuncu olur
            if (!hasShieldSavedUs)
            {
                // --- YENÝ EKLENEN: KALKANSIZ DÜÞMANA ÖLME SESÝ ---
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