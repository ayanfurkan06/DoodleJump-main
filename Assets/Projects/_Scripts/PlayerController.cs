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

    [Header("Kontrol Yöntemi Ayarları")]
    [Tooltip("Çift tıklama algılama süresi (saniye)")]
    [Range(0.1f, 1.5f)] // Inspector'de slider olarak görünmesini sağlar
    public float doubleTapTimeLimit = 0.3f; 
    public float gyroSensitivity = 2f; 
    private float lastTapTime = 0f;
    private bool isDoubleTapHolding = false;

    [Header("Karakter Görsel Nesnesi")]
    public Transform characterVisual;

    [Header("Ölüm Ayarları")]
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
        if (IsDead()) return;

        bool wasFlying = isFlying;

        ProcessInput();

        if (isFlying && !wasFlying)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.useFuelSound);
                AudioManager.Instance.PlaySFX(AudioManager.Instance.windStormSound);
            }
            if (ScreenEffectManager.Instance != null) ScreenEffectManager.Instance.SetWindEffectActive(true);
        }
        else if (!isFlying && wasFlying)
        {
            if (ScreenEffectManager.Instance != null) ScreenEffectManager.Instance.SetWindEffectActive(false);
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
        if (IsDead()) return;

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        if (isFlying)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, flyForce);
            currentFuel -= fuelBurnRate * Time.fixedDeltaTime;
            if (currentFuel < 0) currentFuel = 0;
        }
    }
    
    private void ProcessInput()
    {
        horizontalInput = 0f;
        isFlying = false;

        // MENÜDEN GELEN AYARI OKUYORUZ (0 = Jiroskop, 1 = Dokunmatik)
        bool useGyroscope = PlayerPrefs.GetInt("ControlMethod", 0) == 0;

        // 1. KLAVYE KONTROLÜ (A/D tuşları)
        horizontalInput = Input.GetAxisRaw("Horizontal"); 

        // 2. JİROSKOP (TILT) KONTROLÜ
        if (useGyroscope && SystemInfo.supportsAccelerometer)
        {
            if (horizontalInput == 0) // Klavyeden girilmediyse
            {
                float tilt = Input.acceleration.x * gyroSensitivity;
                horizontalInput = Mathf.Clamp(tilt, -1f, 1f);
            }
        }

        bool isTouching = false;
        Vector3 touchPosition = Vector3.zero;
        bool touchBegan = false;
        bool touchEnded = false;

        // 3. EKRAN DOKUNMATİK VEYA MOUSE ALGILAMASI
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            isTouching = true;
            touchPosition = touch.position;
            
            if (touch.phase == TouchPhase.Began) touchBegan = true;
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) touchEnded = true;
        }
        else if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0) || Input.GetMouseButtonDown(0))
        {
            isTouching = Input.GetMouseButton(0);
            touchPosition = Input.mousePosition;
            
            if (Input.GetMouseButtonDown(0)) touchBegan = true;
            if (Input.GetMouseButtonUp(0)) touchEnded = true;
        }

        if (isTouching)
        {
            // SADECE JİROSKOP KAPALIYSA EKRANA DOKUNARAK SAĞA SOLA GİT
            if (!useGyroscope && horizontalInput == 0) 
            {
                if (touchPosition.x < Screen.width / 2f) horizontalInput = -1f;
                else horizontalInput = 1f;
            }

            // ÇİFT TIKLAMA HER İKİ MODDA DA ÇALIŞMALI (Uçmak için)
            if (touchBegan)
            {
                float timeSinceLastTap = Time.time - lastTapTime;
                if (timeSinceLastTap <= doubleTapTimeLimit)
                {
                    isDoubleTapHolding = true;
                }
                lastTapTime = Time.time;
            }
        }

        if (touchEnded || !isTouching)
        {
            isDoubleTapHolding = false;
        }

        // 4. ÖZELLİK KULLANIMI (Space, W veya Çift Tık)
        if ((Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || isDoubleTapHolding) && currentFuel > 0)
        {
            isFlying = true;
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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.gameOverSound);
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
            // --- EKLENEN KISIM: Kırılan platform kontrolü ---
            // Eğer çarptığımız obje "Platform" etiketine sahipse ama üzerinde "BreakingPlatform" 
            // scripti varsa, bu normal bir zıplama platformu değildir! O yüzden zıplamayı iptal et.
            if (collision.gameObject.GetComponent<BreakingPlatform>() != null)
            {
                return; // Aşağıdaki zıplama kodlarını okumadan doğrudan fonksiyondan çık.
            }

            if (transform.position.y > collision.transform.position.y + 0.1f)
            {
                nextJumpTime = Time.time + 0.4f;
                if (platformCarpti && rb != null)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 12f);

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

    public void AddFuel(float amount)
    {
        currentFuel += amount;
        if (currentFuel > maxFuel) currentFuel = maxFuel;
    }
}