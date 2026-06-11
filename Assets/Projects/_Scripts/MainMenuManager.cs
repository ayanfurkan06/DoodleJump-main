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
            bool isMuted = AudioManager.Instance.ToggleBGM();
            musicButtonText.text = isMuted ? "MUZIK: KAPALI" : "MUZIK: ACIK";
        }
    }

    public void OnClickSFXButton()
    {
        if (AudioManager.Instance != null)
        {
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

        // Haf�zadaki kontrol y�ntemine g�re buton yaz�lar�n� g�ncelle (Aktif olan� belli et)
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

    // --- KONTROL METODU SE��M BUTONLARI ---
    public void SetControlToGyro()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClickSound);
        }
        PlayerPrefs.SetInt("ControlMethod", 0);
        PlayerPrefs.Save();
        UpdateAllButtonTexts();
        Debug.Log("Kontrol Sistemi De�i�ti: Jiroskop Aktif");
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
        Debug.Log("Kontrol Sistemi De�i�ti: Dokunmatik Ekran Aktif");
    }
}