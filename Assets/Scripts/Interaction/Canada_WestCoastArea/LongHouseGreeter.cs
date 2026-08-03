using UnityEngine;

public class LonghouseGreeter : MonoBehaviour, IInteractable
{
    public static LonghouseGreeter Instance;
    public GameObject lockedMessage;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log("Greeter started. isUnlocked = " + GameProgress.Instance.longhouseUnlocked);

        if (GameProgress.Instance != null && GameProgress.Instance.quizCompleted)
        {
            gameObject.SetActive(false);
        }
    }

    public void Interact()
    {
        Debug.Log("Interact called. isUnlocked = " + GameProgress.Instance.longhouseUnlocked);
        if (!GameProgress.Instance.longhouseUnlocked)
        {
            Debug.Log("Talk to all NPCs first!");
            return;
        }
        Debug.Log("Launching Longhouse game!");
    }

    public void Unlock()
    {
        GameProgress.Instance.longhouseUnlocked = true;
    }

    public bool CanInteract()
    {
        return GameProgress.Instance.longhouseUnlocked;
    }
}