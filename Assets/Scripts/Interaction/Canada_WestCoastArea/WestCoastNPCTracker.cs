using UnityEngine;

public class WestCoastNPCTracker : MonoBehaviour
{
    public static WestCoastNPCTracker Instance;
    public int totalNPCs = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Invoke(nameof(CountNPCs), 0.2f);
    }

    private void CountNPCs()
    {
        totalNPCs = FindObjectsByType<TrackableNPC>(FindObjectsSortMode.None).Length;
        Debug.Log("Total NPCs found: " + totalNPCs);
        UpdateUI(); // NOW called AFTER counting
    }

    public void RegisterNPCTalkedTo()
    {
        GameProgress.Instance.npcsTalkedTo++;
        Debug.Log("Talked to: " + GameProgress.Instance.npcsTalkedTo + " / " + totalNPCs);
        UpdateUI();
        if (GameProgress.Instance.npcsTalkedTo >= totalNPCs)
        {
            Debug.Log("All NPCs talked to! Unlocking Longhouse Greeter.");
            LonghouseGreeter.Instance.Unlock();
        }
    }

    private void UpdateUI()
    {
        if (WestCoastNPCUI.Instance != null)
            WestCoastNPCUI.Instance.UpdateCounter(GameProgress.Instance.npcsTalkedTo, totalNPCs);
        else
            Debug.LogError("WestCoastNPCUI.Instance is null!");
    }
}