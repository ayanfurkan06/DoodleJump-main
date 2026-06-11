using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Müzik Kaynağı")]
    public AudioSource bgmSource; // Arka plan müziği için

    // Artık tek bir sfxSource yok. Her sesin KENDİ AudioSource'u var.
    [Header("Ses Efektleri (AudioSource)")]
    public AudioSource jumpSound;
    public AudioSource springSound;
    public AudioSource collectHealthSound;
    public AudioSource takeDamageSound;
    public AudioSource gameOverSound;
    public AudioSource buttonClickSound; 

    // Tüm SFX kaynaklarını kolayca döngüye sokup susturmak/açmak için bir dizi
    private AudioSource[] allSFXSources;

    private bool isMutedBGM = false;
    private bool isMutedSFX = false;

    void Awake()
    {
        // Sahneler arası geçişte müziğin kesilmemesini sağlayan mimari (Singleton)
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
            jumpSound, springSound, collectHealthSound, 
            takeDamageSound, gameOverSound, buttonClickSound
        };

        // Hafıza durumuna göre sesleri ilk açılışta ayarla
        bgmSource.mute = isMutedBGM;
        UpdateSFXMuteState(); // SFX'leri ayarlayan yeni yardımcı fonksiyonumuz

        // Müziği başlat
        if (!bgmSource.isPlaying) bgmSource.Play();
    }

    // --- SES EFEKTİ ÇALMA FONKSİYONU ---
    // Artık parametre olarak AudioClip değil, doğrudan çalınacak AudioSource'u alıyor
    public void PlaySFX(AudioSource source)
    {
        if (source != null && !isMutedSFX)
        {
            // source.PlayOneShot() yerine source.Play() kullanıyoruz. 
            // Çünkü Inspector'dan ayarladığın Loop vb. özelliklerin direkt çalışması için Play() gereklidir.
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

    // Mevcut durumları buton yazıları için dışarıya bildiren yardımcı fonksiyonlar
    public bool IsBGMMuted() { return isMutedBGM; }
    public bool IsSFXMuted() { return isMutedSFX; }
}