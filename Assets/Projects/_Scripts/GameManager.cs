using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; // Hatal� kelime d�zeltildi

public class GameManager : MonoBehaviour
{
    public Transform playerTransform;
    public TextMeshProUGUI scoreText;

    [Header("Game Over UI Ayarlar�")] public GameObject gameOverPanel;

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

        int finalScore = Mathf.RoundToInt(highestY);
        if (finalScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", finalScore);
            PlayerPrefs.Save();
        }

        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.gameOverSound);
        }
    }

    // D��man taraf�ndan �ld�r�l�nce �a�r�lacak gecikmeli fonksiyon
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

        int finalScore = Mathf.RoundToInt(highestY);
        if (finalScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", finalScore);
            PlayerPrefs.Save();
        }

        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.gameOverSound);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}