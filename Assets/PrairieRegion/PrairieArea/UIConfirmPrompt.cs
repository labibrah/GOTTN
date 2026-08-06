using UnityEngine;
using TMPro;

public class UIConfirmPrompt : MonoBehaviour
{
    public static UIConfirmPrompt Instance;

    public static bool IsPromptOpen = false;

    public GameObject confirmPanel;
    public GameObject infoPanel;

    public TMP_Text confirmText;
    public TMP_Text infoText;

    private System.Action onYesAction;

    public GameObject panel;
    public TMP_Text promptText;

    void Awake()
    {
        Instance = this;

        confirmPanel.SetActive(false);
        infoPanel.SetActive(false);
    }

    public void ShowConfirmation(string message, System.Action yesAction)
    {
        confirmPanel.SetActive(true);
        infoPanel.SetActive(false);
        confirmText.text = message;
        onYesAction = yesAction;
        Time.timeScale = 0f;
        IsPromptOpen = true;
    }

    public void ShowInfo(string message)
    {
        infoPanel.SetActive(true);
        confirmPanel.SetActive(false);
        infoText.text = message;
        Time.timeScale = 0f;
        IsPromptOpen = true;
    }

    public void OnOkPressed()
    {
        Time.timeScale = 1f;
        infoPanel.SetActive(false);
        IsPromptOpen = false;
    }

    public void OnYesPressed()
    {
        Debug.Log("YES pressed");
        Time.timeScale = 1f;
        confirmPanel.SetActive(false);
        IsPromptOpen = false;
        onYesAction?.Invoke();
    }

    public void OnNoPressed()
    {
        Debug.Log("NO pressed");
        Time.timeScale = 1f;
        confirmPanel.SetActive(false);
        IsPromptOpen = false;
    }
}