using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Win Panel")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMPro.TextMeshProUGUI winText;
    private PlayerScript player1, player2;
    private bool gameOver = false;

    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (gameOver)
        {
            if (player1.Input.actions["Restart"].IsPressed() || player2.Input.actions["Restart"].IsPressed())
            {
                RestartGame();
            }
        }
    }

    public void RegisterPlayer(PlayerScript player)
    {
        if (player1 == null)
            player1 = player;
        else if (player2 == null)
            player2 = player;
    }

    public void CheckWin()
    {
        if (player1.LivesRemaining() <= 0)
        {
            ShowWin("Player 2");
        }
        else if (player2.LivesRemaining() <= 0)
        {
            ShowWin("Player 1");
        }
    }

    private void ShowWin(string winner)
    {
        Time.timeScale = 0f;
        gameOver = true;
        player1.SetGameover(true);
        player2.SetGameover(true);
        winPanel.SetActive(true);
        winText.text = winner + " Wins!\nPress start to play again";
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        gameOver = false;
        winPanel.SetActive(false);

        player1.ResetPlayer();
        player2.ResetPlayer();
    }
}
