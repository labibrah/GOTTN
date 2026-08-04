using UnityEngine;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance;

    public int npcsTalkedTo;
    public bool longhouseUnlocked;
    public bool quizCompleted;
    public bool totemPuzzleSolved;
    public bool totemChestOpened;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}