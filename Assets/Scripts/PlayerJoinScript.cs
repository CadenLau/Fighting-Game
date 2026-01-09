using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerJoinScript : MonoBehaviour
{
    [SerializeField] private Transform spawnPos1, spawnPos2;
    [SerializeField] private RectTransform healthBar1, healthBar2;
    [SerializeField] private Image cooldownRing1, cooldownRing2;
    [SerializeField] private LivesUI livesUI1, livesUI2;
    [SerializeField] private GameManager gameManager;
    private bool hasStarted = false;

    // private void Start()
    // {
    //     var p1 = PlayerInputManager.instance.JoinPlayer(0, -1, null, Gamepad.all[0]);
    //     var p2 = PlayerInputManager.instance.JoinPlayer(1, -1, null, Gamepad.all[1]);

    //     AssignPlayer(p1, spawnPos1.position, Color.green, healthBar1, cooldownRing1, livesUI1);
    //     AssignPlayer(p2, spawnPos2.position, Color.red, healthBar2, cooldownRing2, livesUI2, flip: true);
    // }
    private void Start()
    {
        PlayerInputManager.instance.DisableJoining();
    }

    private void Update()
    {
        // Only check input if we haven't started yet
        if (!hasStarted)
        {
            // Any gamepad button
            if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
            {
                OnStartClicked();
            }

            // Optional keyboard fallback for testing
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                OnStartClicked();
            }
        }
    }

    public void OnStartClicked()
    {
        if (hasStarted)
            return;

        hasStarted = true;

        PlayerInputManager.instance.EnableJoining();
        JoinPlayers();
    }

    private void JoinPlayers()
    {
        int gamepadCount = Gamepad.all.Count;

        if (gamepadCount < 1)
        {
            Debug.LogError("No gamepads detected. Controllers are required.");
            return;
        }

        // Player 1 (always)
        var p1 = PlayerInputManager.instance.JoinPlayer(0);
        AssignPlayer(
            p1,
            spawnPos1.position,
            Color.green,
            healthBar1,
            cooldownRing1,
            livesUI1
        );

        // Player 2 (only if another controller exists)
        if (gamepadCount >= 2)
        {
            var p2 = PlayerInputManager.instance.JoinPlayer(1);
            AssignPlayer(
                p2,
                spawnPos2.position,
                Color.red,
                healthBar2,
                cooldownRing2,
                livesUI2,
                flip: true
            );
        }
        else
        {
            Debug.LogWarning("Only one gamepad detected. Starting in 1-player mode.");
        }
    }

    private void AssignPlayer(PlayerInput playerInput, Vector3 spawnPos, Color color, RectTransform healthBar, Image cooldownRing, LivesUI livesUI, bool flip = false)
    {
        var player = playerInput.gameObject;
        var playerScript = player.GetComponent<PlayerScript>();
        player.GetComponent<SpriteRenderer>().color = color;
        playerScript.SetSpawnPoint(spawnPos);
        playerScript.SetHealthbar(healthBar);
        playerScript.SetCooldownRing(cooldownRing);
        playerScript.SetLivesUI(livesUI);

        if (flip) playerScript.Flip();

        gameManager.RegisterPlayer(playerScript);
    }
}
