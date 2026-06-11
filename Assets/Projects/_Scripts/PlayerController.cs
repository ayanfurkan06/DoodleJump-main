using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator _animator;
    private float nextJumpTime = 0f;

    [Header("Roket Ayakkab� Ayarlar�")]
    public UnityEngine.UI.Slider fuelSlider;
    public float maxFuel = 100f;
    public float flyForce = 10f;
    public float fuelBurnRate = 25f;

    private float currentFuel;
    private bool isFlying = false;

    [Header("Hareket Ayarlar�")]
    public float moveSpeed = 8f;
    private Rigidbody2D rb;
    private float horizontalInput;

    [Header("Dokunmatik ve �ift T�klama Ayarlar�")]
    public float doubleTapTimeLimit = 0.3f;
    private float lastTapTime = 0f;
    private bool isDoubleTapHolding = false;

    private int controlMethod = 0;
    [Header("Karakter G�rsel Nesnesi")]
    public Transform characterVisual;

    [Header("Yeni �l�m Ayarlar�")]
    public SpriteRenderer playerEyeSpriteRenderer; // M�fetti�ten karakterin alt�ndaki eye_8 nesnesini buraya ba�layaca��z
    public Sprite deadEyeSprite;                   // M�fetti�ten eye_17 g�rselini buraya takaca��z

    void Start()
    {
        _animator = GetComponent<Animator>();
        currentFuel = 10f;
        if (fuelSlider != null) fuelSlider.value = currentFuel;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // E�er karakter �ld�yse hi�bir tu� girdisini veya hareketi alg�lama, kilitlensin
        if (playerEyeSpriteRenderer != null && playerEyeSpriteRenderer.sprite == deadEyeSprite) return;

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
        // Karakter �ld�yse fiziksel kontrolleri de engelle
        if (playerEyeSpriteRenderer != null && playerEyeSpriteRenderer.sprite == deadEyeSprite) return;

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        if (isFlying)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, flyForce);
            currentFuel -= fuelBurnRate * Time.fixedDeltaTime;
            if (currentFuel < 0) currentFuel = 0;
        }
    }

    // Yeni �l�m Hareketi Fonksiyonu
    // --- GELECEKTEK� S�STEMLER ���N CANLILIK KONTROL� ---
    // D��ar�daki scriptler (WeatherManager gibi) oyuncunun hayatta olup olmad���n� buradan okuyacak.
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
        // 1. G�zleri spinal g�z ile de�i�tir (Bu andan itibaren IsDead() true d�necek)
        if (playerEyeSpriteRenderer != null && deadEyeSprite != null)
        {
            playerEyeSpriteRenderer.sprite = deadEyeSprite;
        }

        // --- YEN� EKLEME: ESNEKL��� SIFIRLAMA ---
        // Karakter z�plarken Animator y�z�nden esnemi� olabilir. 
        // Animator'� kapatmadan �nce boyutunu zorla orijinal (1, 1, 1) haline getiriyoruz.
        transform.localScale = new Vector3(1f, 1f, 1f);
        if (characterVisual != null)
        {
            characterVisual.localScale = new Vector3(1f, 1f, 1f);
        }
        // ----------------------------------------

        // 2. Animator bile�enini kapat�yoruz
        if (_animator != null) _animator.enabled = false;

        // 3. KARAKTER� ANINDA TEPETAKLA EDEN MOTORU ---
        if (characterVisual != null)
        {
            // Karakterin y�n�ne g�re X scale de�erini korumak istersen �stteki d�zleme yeterlidir,
            // ama tamamen orijinal boyut i�in buradaki i�lemleri de temizlemi� olduk.
        }

        // 4. Karakterin collider'lar�n� kapat�yoruz
        Collider2D[] extColliders = GetComponents<Collider2D>();
        foreach (Collider2D col in extColliders) col.enabled = false;

        Collider2D[] childColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in childColliders) col.enabled = false;

        // 5. Karakteri fiziksel olarak hafif�e yukar� f�rlat�p sonsuza d�k�lerek d���r�yoruz
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