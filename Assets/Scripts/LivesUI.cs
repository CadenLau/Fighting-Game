using UnityEngine;
using UnityEngine.UI;

public class LivesUI : MonoBehaviour
{
    [SerializeField] private Image lifeIcon;
    [SerializeField] private int startingLives = 5;
    [SerializeField] private int startingSpecials = 1;
    private int livesRemaining;
    public int LivesRemaining => livesRemaining;
    private int specialsRemaining;
    private Image[] lifeIcons;

    private void Start()
    {
        livesRemaining = startingLives;
        specialsRemaining = startingSpecials;
        lifeIcons = new Image[startingLives + startingSpecials];

        for (int i = 0; i < startingLives + startingSpecials; i++)
        {
            Image icon = Instantiate(lifeIcon, transform);
            lifeIcons[i] = icon;
        }
        for (int i = startingLives; i < startingLives + startingSpecials; i++)
        {
            lifeIcons[i].gameObject.GetComponent<Image>().color = Color.black;
        }
    }

    public void RemoveLife()
    {
        if (livesRemaining <= 0)
        {
            return;
        }
        livesRemaining--;
        specialsRemaining++;
        lifeIcons[livesRemaining].gameObject.GetComponent<Image>().color = Color.black;
    }

    public bool HasSpecial()
    {
        return specialsRemaining > 0;
    }

    public void UseSpecial()
    {
        if (specialsRemaining <= 0)
        {
            return;
        }
        specialsRemaining--;
        lifeIcons[livesRemaining + specialsRemaining].gameObject.SetActive(false);
    }

    public void ResetGame()
    {
        foreach (var icon in lifeIcons)
        {
            Destroy(icon.gameObject);
        }
        Start();
    }
}
