using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Müzik Kaynağı")]
    public AudioSource bgmSource; // Arka plan müziği için

    [Header("Ses Efektleri (AudioSource)")]
    public AudioSource jumpSound;
    public AudioSource springSound;
    public AudioSource collectHealthSound;
    public AudioSource takeDamageSound;
    public AudioSource gameOverSound;
    public AudioSource buttonClickSound;

    // --- YENİ EKLENEN SES EFEKTLERİ ---
    public AudioSource brokenPlatformJumpSound;
    public AudioSource enemyDeathSound;
    public AudioSource shieldEquipSound;
    public AudioSource shieldHitSound;
    public AudioSource shieldBreakSound;
    public AudioSource enemySwordSwingSound;
    public AudioSource useFuelSound;
    public AudioSource windStormSound;
    public AudioSource highScoreSound;

    // Tüm SFX kaynaklarını döngüye sokup susturmak/açmak için dizi
    private AudioSource[] allSFXSources;

    private bool isMutedBGM = false;
    private bool isMutedSFX = false;

    void Awake()
    {
        // Sahneler arası geçişte müziğin kesilmemesini sağlayan Singleton mimari
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahne değişse de bu objeyi yok etme!
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Hafızadaki ses tercihlerini yükle (1 = Ses Açık, 0 = Ses Kapalı)
        isMutedBGM = PlayerPrefs.GetInt("MutedBGM", 1) == 0;
        isMutedSFX = PlayerPrefs.GetInt("MutedSFX", 1) == 0;
    }

    void Start()
    {
        // Bütün SFX'leri bir diziye atıyoruz ki tek tek kontrol etmek yerine kodumuz temiz olsun
        allSFXSources = new AudioSource[] {
            jumpSound, springSound, collectHealthSound, takeDamageSound, gameOverSound, buttonClickSound,
            brokenPlatformJumpSound, enemyDeathSound, shieldEquipSound, shieldHitSound, shieldBreakSound,
            enemySwordSwingSound, useFuelSound, windStormSound, highScoreSound
        };

        // Hafıza durumuna göre sesleri ilk açılışta ayarla
        bgmSource.mute = isMutedBGM;
        UpdateSFXMuteState(); // SFX'leri ayarlayan yardımcı fonksiyonumuz

        // Müziği başlat
        if (!bgmSource.isPlaying) bgmSource.Play();
    }

    // --- SES EFEKTİ ÇALMA FONKSİYONU ---
    public void PlaySFX(AudioSource source)
    {
        if (source != null && !isMutedSFX)
        {
            source.Play();
        }
    }

    // --- SFX SUSTURMA YARDIMCI FONKSİYONU ---
    private void UpdateSFXMuteState()
    {
        foreach (AudioSource source in allSFXSources)
        {
            if (source != null)
            {
                source.mute = isMutedSFX;
            }
        }
    }

    // --- BUTONLAR İÇİN AÇMA / KAPATMA FONKSİYONLARI ---
    public bool ToggleBGM()
    {
        isMutedBGM = !isMutedBGM;
        bgmSource.mute = isMutedBGM;
        PlayerPrefs.SetInt("MutedBGM", isMutedBGM ? 0 : 1);
        PlayerPrefs.Save();
        PlaySFX(buttonClickSound); // Butona basınca klik sesi çal
        return isMutedBGM;
    }

    public bool ToggleSFX()
    {
        isMutedSFX = !isMutedSFX;
        UpdateSFXMuteState(); // Tüm ses kaynaklarını sustur veya aç

        PlayerPrefs.SetInt("MutedSFX", isMutedSFX ? 0 : 1);
        PlayerPrefs.Save();
        PlaySFX(buttonClickSound); // Butona basınca klik sesi çal
        return isMutedSFX;
    }
    
    public void StopAllSFX()
    {
        if (allSFXSources == null) return;

        foreach (AudioSource source in allSFXSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }
    }

    // Mevcut durumları buton yazıları için dışarıya bildiren yardımcı fonksiyonlar
    public bool IsBGMMuted() { return isMutedBGM; }
    public bool IsSFXMuted() { return isMutedSFX; }
}