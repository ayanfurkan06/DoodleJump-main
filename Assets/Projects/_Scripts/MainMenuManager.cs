using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public TextMeshProUGUI highScoreText;

    [Header("Ses Buton Metinleri")]
    public TextMeshProUGUI musicButtonText;
    public TextMeshProUGUI sfxButtonText;

    [Header("Kontrol Buton Metinleri")]
    public TextMeshProUGUI gyroButtonText;
    public TextMeshProUGUI touchButtonText;

    void Start()
    {
        int savedHighScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "EN YUKSEK SKOR: " + savedHighScore;

        UpdateAllButtonTexts();
    }

    public void StartGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClickSound);
        }
        SceneManager.LoadScene("SampleScene");
    }

    public void OnClickMusicButton()
    {
        if (AudioManager.Instance != null)
        {
            // AudioManager zaten kendi içinde klik sesini çalıyor, o yüzden buradaki manuel ses satırını kaldırdık!
            bool isMuted = AudioManager.Instance.ToggleBGM();
            musicButtonText.text = isMuted ? "MUZIK: KAPALI" : "MUZIK: ACIK";
        }
    }

    public void OnClickSFXButton()
    {
        if (AudioManager.Instance != null)
        {
            // Buradaki manuel ses satırını da kaldırdık, çift ses çalmayacak
            bool isMuted = AudioManager.Instance.ToggleSFX();
            sfxButtonText.text = isMuted ? "SES: KAPALI" : "SES: ACIK";
        }
    }

    public void UpdateAllButtonTexts()
    {
        if (AudioManager.Instance != null)
        {
            musicButtonText.text = AudioManager.Instance.IsBGMMuted() ? "MUZIK: KAPALI" : "MUZIK: ACIK";
            sfxButtonText.text = AudioManager.Instance.IsSFXMuted() ? "SES: KAPALI" : "SES: ACIK";
        }

        int currentMethod = PlayerPrefs.GetInt("ControlMethod", 0);
        if (currentMethod == 0)
        {
            gyroButtonText.text = "JIROSKOP: AKTIF";
            touchButtonText.text = "DOKUNMATIK";
        }
        else
        {
            gyroButtonText.text = "JIROSKOP";
            touchButtonText.text = "DOKUNMATIK: AKTIF";
        }
    }

    public void SetControlToGyro()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClickSound);
        }
        PlayerPrefs.SetInt("ControlMethod", 0);
        PlayerPrefs.Save();
        UpdateAllButtonTexts();
        Debug.Log("Kontrol Sistemi Değişti: Jiroskop Aktif");
    }

    public void SetControlToTouch()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClickSound);
        }

        PlayerPrefs.SetInt("ControlMethod", 1);
        PlayerPrefs.Save();
        UpdateAllButtonTexts();
        Debug.Log("Kontrol Sistemi Değişti: Dokunmatik Ekran Aktif");
    }
}