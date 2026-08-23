using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class ParticipantSetupUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField participantIdInput;
    [SerializeField] private Button startButton;
    [SerializeField] private AnalyticsLogger analyticsLogger; // drag AnalyticsLogger GameObject in
    [SerializeField] private string sceneToLoadAfterSetup = "World1";

    private void Start()
    {
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void OnStartClicked()
    {
        string participantId = participantIdInput.text.Trim();
        if (string.IsNullOrEmpty(participantId))
        {
            Debug.LogWarning("Participant ID cannot be empty.");
            return;
        }

        string relativeOutputFolder = Path.Combine("ParticipantData", participantId);
        analyticsLogger.SetParticipant(participantId, relativeOutputFolder);

        ResetAllProgressFlags();

        SceneManager.LoadScene(GameManager.Instance.startingSceneName);
    }

    private void ResetAllProgressFlags()
    {
        var allBoolValues = Resources.FindObjectsOfTypeAll<BoolValue>();
        foreach (var flag in allBoolValues)
        {
            flag.runtimeValue = flag.initialValue;
        }
        Debug.Log($"Reset {allBoolValues.Length} BoolValue flags for new participant session.");
    }
}