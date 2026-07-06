using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public Transform playerTransform;
    public TextMeshProUGUI scoreText;

    [Header("Game Over UI Ayarları")] public GameObject gameOverPanel;

    private float highestY = 0f;
    private int highScore = 0;
    private bool isGameOver = false;
    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (playerTransform == null || isGameOver) return;

        if (playerTransform.position.y > highestY)
        {
            highestY = playerTransform.position.y;
            scoreText.text = Mathf.RoundToInt(highestY).ToString();
        }

        if (playerTransform.position.y < Camera.main.transform.position.y - 7f)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        int finalScore = Mathf.RoundToInt(highestY);

        // --- GÜNCELLENEN ALAN: REKOR KONTROLÜ VE SES TETİKLEMESİ ---
        if (finalScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", finalScore);
            PlayerPrefs.Save();

            // Yeni rekor kırıldıysa coşkulu rekor sesini çal
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.highScoreSound);
            }
        }
        else
        {
            // Rekor kırılmadıysa normal oyun bitti sesini çal
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.gameOverSound);
            }
        }
    }

    public void TriggerEnemyDeath(GameObject playerObj)
    {
        if (isGameOver) return;
        isGameOver = true;

        PlayerController controller = playerObj.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.DieWithAnimation();
        }

        StartCoroutine(ExecuteDelayedGameOver());
    }

    private IEnumerator ExecuteDelayedGameOver()
    {
        yield return new WaitForSeconds(2f);

        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        int finalScore = Mathf.RoundToInt(highestY);

        // --- GÜNCELLENEN ALAN: GECİKMELİ ÖLÜMDE REKOR KONTROLÜ ---
        if (finalScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", finalScore);
            PlayerPrefs.Save();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.highScoreSound);
            }
        }
        else
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.gameOverSound);
            }
        }
    }

    public void RestartGame()
    {
        if (AudioManager.Instance != null)
        {
            // YENİ EKLENEN: Önce çalan Game Over sesini anında durdur
            AudioManager.Instance.StopAllSFX();

            // Sonra buton tıklama sesini çal
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClickSound);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void GoToMainMenu()
    {
        if (AudioManager.Instance != null)
        {
            // YENİ EKLENEN: Önce çalan Game Over sesini anında durdur
            AudioManager.Instance.StopAllSFX();

            // Sonra buton tıklama sesini çal
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClickSound);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}