using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator _animator;
    private float nextJumpTime = 0f;

    [Header("Roket Ayakkabı Ayarları")]
    public UnityEngine.UI.Slider fuelSlider;
    public float maxFuel = 100f;
    public float flyForce = 10f;
    public float fuelBurnRate = 25f;

    private float currentFuel;
    private bool isFlying = false;

    [Header("Hareket Ayarları")]
    public float moveSpeed = 8f;
    private Rigidbody2D rb;
    private float horizontalInput;

    [Header("Dokunmatik ve Çift Tıklama Ayarları")]
    public float doubleTapTimeLimit = 0.3f;
    private float lastTapTime = 0f;
    private bool isDoubleTapHolding = false;

    private int controlMethod = 0;

    [Header("Karakter Görsel Nesnesi")]
    public Transform characterVisual;

    [Header("Yeni Ölüm Ayarları")]
    public SpriteRenderer playerEyeSpriteRenderer;
    public Sprite deadEyeSprite;

    void Start()
    {
        _animator = GetComponent<Animator>();
        currentFuel = 10f;
        if (fuelSlider != null) fuelSlider.value = currentFuel;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Eğer karakter öldüyse hiçbir tuş girdisini veya hareketi algılama, kilitlensin
        if (playerEyeSpriteRenderer != null && playerEyeSpriteRenderer.sprite == deadEyeSprite) return;

        // Uçuş sesinin cızırtı yapmasını engellemek için önceki durumu kaydediyoruz
        bool wasFlying = isFlying;

        controlMethod = PlayerPrefs.GetInt("ControlMethod", 0);

        if (controlMethod == 0)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                horizontalInput = Input.acceleration.x * 1.5f;
            }

            if ((Input.GetKey(KeyCode.Space) || Input.touchCount > 0) && currentFuel > 0)
            {
                isFlying = true;
                if (ScreenEffectManager.Instance != null) ScreenEffectManager.Instance.SetWindEffectActive(true);
            }
            else
            {
                isFlying = false;
                if (ScreenEffectManager.Instance != null) ScreenEffectManager.Instance.SetWindEffectActive(false);
            }
        }
        else
        {
            HandleTouchInput();
        }

        // --- YENİ EKLENEN: YAKIT KULLANMA / UÇMA SESİ ---
        // Sadece uçuşa YENİ başladığı o ilk an sesi tetikleriz.
        if (isFlying && !wasFlying)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.useFuelSound);
                AudioManager.Instance.PlaySFX(AudioManager.Instance.windStormSound); // Roket ayakkabı rüzgar da çıkarttığı için
            }
        }

        if (isFlying)
        {
            horizontalInput = 0f;
        }

        if (fuelSlider != null)
        {
            fuelSlider.value = currentFuel;
        }

        if (transform.position.x < -4.5f)
        {
            transform.position = new Vector3(4.5f, transform.position.y, transform.position.z);
        }
        else if (transform.position.x > 4.5f)
        {
            transform.position = new Vector3(-4.5f, transform.position.y, transform.position.z);
        }

        if (horizontalInput < -0.1f && characterVisual != null)
        {
            characterVisual.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (horizontalInput > 0.1f && characterVisual != null)
        {
            characterVisual.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    void FixedUpdate()
    {
        // Karakter öldüyse fiziksel kontrolleri de engelle
        if (playerEyeSpriteRenderer != null && playerEyeSpriteRenderer.sprite == deadEyeSprite) return;

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        if (isFlying)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, flyForce);
            currentFuel -= fuelBurnRate * Time.fixedDeltaTime;
            if (currentFuel < 0) currentFuel = 0;
        }
    }

    public bool IsDead()
    {
        if (playerEyeSpriteRenderer != null && deadEyeSprite != null)
        {
            return playerEyeSpriteRenderer.sprite == deadEyeSprite;
        }
        return false;
    }

    public void DieWithAnimation()
    {
        if (playerEyeSpriteRenderer != null && deadEyeSprite != null)
        {
            playerEyeSpriteRenderer.sprite = deadEyeSprite;
        }

        // --- YENİ EKLENEN: AŞAĞI DÜŞÜP ÖLME SESİ ---
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.gameOverSound);
        }

        transform.localScale = new Vector3(1f, 1f, 1f);
        if (characterVisual != null)
        {
            characterVisual.localScale = new Vector3(1f, 1f, 1f);
        }

        if (_animator != null) _animator.enabled = false;

        Collider2D[] extColliders = GetComponents<Collider2D>();
        foreach (Collider2D col in extColliders) col.enabled = false;

        Collider2D[] childColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in childColliders) col.enabled = false;

        if (rb != null)
        {
            rb.freezeRotation = false;

            float randomVelocityX = Random.Range(-3f, 3f);
            rb.linearVelocity = new Vector2(randomVelocityX, 5f);

            float randomRotationSpeed = Random.Range(-360f, 360f);
            rb.angularVelocity = randomRotationSpeed;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (Time.time < nextJumpTime) return;

        bool platformCarpti = collision.gameObject.CompareTag("Platform");
        bool yayaCarpti = collision.gameObject.CompareTag("Spring");

        if (platformCarpti || yayaCarpti)
        {
            if (transform.position.y > collision.transform.position.y + 0.1f)
            {
                nextJumpTime = Time.time + 0.4f;
                if (platformCarpti && rb != null)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 12f);

                    // NORMAL ZIPLAMA SESİ BURADA ZATEN VARDI
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.jumpSound);
                    }
                    if (_animator != null)
                    {
                        _animator.ResetTrigger("BounceTrigger");
                        _animator.SetTrigger("BounceTrigger");
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Time.time < nextJumpTime) return;

        if (collision.CompareTag("Spring"))
        {
            if (transform.position.y > collision.transform.position.y + 0.1f)
            {
                nextJumpTime = Time.time + 0.4f;
                if (_animator != null)
                {
                    _animator.ResetTrigger("BounceTrigger");
                    _animator.SetTrigger("BounceTrigger");
                }
            }
        }
    }

    void HandleTouchInput()
    {
        horizontalInput = 0f;
        bool anyTouchActive = false;

        if (Input.GetMouseButton(0))
        {
            anyTouchActive = true;
            if (Input.mousePosition.x < Screen.width / 2f) horizontalInput = -1f;
            else horizontalInput = 1f;

            if (Input.GetMouseButtonDown(0))
            {
                float timeSinceLastTap = Time.time - lastTapTime;
                if (timeSinceLastTap <= doubleTapTimeLimit) isDoubleTapHolding = true;
                lastTapTime = Time.time;
            }
        }

        if (Input.GetMouseButtonUp(0)) isDoubleTapHolding = false;

        if (Input.touchCount > 0)
        {
            anyTouchActive = true;
            Touch touch = Input.GetTouch(0);

            if (touch.position.x < Screen.width / 2f) horizontalInput = -1f;
            else horizontalInput = 1f;

            if (touch.phase == TouchPhase.Began)
            {
                float timeSinceLastTap = Time.time - lastTapTime;
                if (timeSinceLastTap <= doubleTapTimeLimit) isDoubleTapHolding = true;
                lastTapTime = Time.time;
            }
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) isDoubleTapHolding = false;
        }

        if (anyTouchActive && isDoubleTapHolding && currentFuel > 0) isFlying = true;
        else isFlying = false;
    }

    public void AddFuel(float amount)
    {
        currentFuel += amount;
        if (currentFuel > maxFuel) currentFuel = maxFuel;
    }
}