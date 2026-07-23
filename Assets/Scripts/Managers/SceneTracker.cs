using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTracker : MonoBehaviour
{
    public static SceneTracker Instance;
    public string previousSceneName;
    public Vector3 playerReturnPosition;
    public bool isReturningFromBattle = false;
    private bool shouldRespawnAfterLoad = false;
    private Vector3 pendingRespawnPosition;

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

    public void RecordSceneAndPosition(Vector3 playerPos)
    {
        previousSceneName = SceneManager.GetActiveScene().name;
        playerReturnPosition = playerPos + new Vector3(0f, .6f, 0f);
    }

    public bool TryConsumePendingPosition(out Vector3 position)
    {
        if (shouldRespawnAfterLoad)
        {
            position = pendingRespawnPosition;
            shouldRespawnAfterLoad = false;
            return true;
        }
        if (isReturningFromBattle)
        {
            position = playerReturnPosition;
            isReturningFromBattle = false;
            return true;
        }
        position = Vector3.zero;
        return false;
    }

    public void ReturnToPreviousScene(bool isWin)
    {
        if (string.IsNullOrEmpty(previousSceneName))
        {
            Debug.LogWarning("No previous scene recorded!");
            SceneManager.LoadScene("World1_Revamped");
        }
        if (isWin)
        {
            isReturningFromBattle = true;
            SceneManager.LoadScene(previousSceneName);
        }
        else
        {
            if (PlayerRespawnManager.Instance != null)
            {
                pendingRespawnPosition = PlayerRespawnManager.Instance.GetRespawnPoint();
                shouldRespawnAfterLoad = true;
                SceneManager.LoadScene(previousSceneName);
            }
            else
            {
                Debug.LogWarning("PlayerRespawnManager not found!");
                SceneManager.LoadScene(previousSceneName);
            }
        }
    }
}