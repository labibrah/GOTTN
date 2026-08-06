using TMPro;
using UnityEngine;
public class Sign : Interactable
{
    public GameObject dialogBox;
    public TextMeshProUGUI dialogText;
    public string[] dialogs;
    public bool dialogActive;
    public int currentDialogIndex = 0;
    public PetBubble petBubble;

    [Header("Analytics")]
    [Tooltip("Identifier logged for this sign. Defaults to the GameObject name if left blank.")]
    public string signId;

    private float dialogStartTime;

    public override void Start()
    {
        if (dialogBox == null)
            dialogBox = GameObject.FindGameObjectWithTag("DialogBox");
        if (dialogText == null)
            dialogText = dialogBox.GetComponentInChildren<TextMeshProUGUI>();
        dialogActive = false;
        currentDialogIndex = 0;

        if (string.IsNullOrEmpty(signId))
            signId = gameObject.name;

        base.Start();
    }
    public virtual void Update()
    {
        if (playerInRange && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
        {
            if (audioSource != null && interactSound != null)
                audioSource.PlayOneShot(interactSound);
            if (!dialogBox.activeSelf)
            {
                if (petBubble != null)
                {
                    petBubble.playerHasInteracted = true;
                    petBubble.isPaused = true;
                }
                dialogBox.SetActive(true);
                currentDialogIndex = 0;
                dialogText.text = dialogs.Length > 0 ? dialogs[currentDialogIndex] : "";

                PetBubble.IsDialogueActive = true;

                dialogStartTime = Time.time;
                if (AnalyticsLogger.Instance != null)
                    AnalyticsLogger.Instance.LogEvent("dialogue_start",
                        $"sign={signId};total_lines={dialogs.Length}");
            }
            else
            {
                currentDialogIndex++;
                if (currentDialogIndex < dialogs.Length)
                {
                    dialogText.text = dialogs[currentDialogIndex];
                }
                else
                {
                    
                    dialogBox.SetActive(false);
                    dialogActive = false;

                    PetBubble.IsDialogueActive = false;

                    if (AnalyticsLogger.Instance != null)
                        AnalyticsLogger.Instance.LogEvent("dialogue_complete",
                            $"sign={signId};lines_read={dialogs.Length};total_lines={dialogs.Length};duration={(Time.time - dialogStartTime):F2}");

                    currentDialogIndex = 0;
                    if (petBubble != null) petBubble.isPaused = false;
                    Interact();

                    TrackableNPC trackable = GetComponent<TrackableNPC>();
                    if (trackable != null)
                    {
                        trackable.RegisterInteraction();
                    }

                    PrairieVisitNPC prairieVisit = GetComponent<PrairieVisitNPC>();
                    if (prairieVisit != null)
                    {
                        prairieVisit.MarkVisited();
                    }
                }
            }
        }
        else if (dialogBox.activeSelf && playerInRange && Input.GetKeyDown(KeyCode.Escape))
        {
            LogDialogueSkipped();
            dialogBox.SetActive(false);
            dialogActive = false;
            currentDialogIndex = 0;
            PetBubble.IsDialogueActive = false;
            if (petBubble != null) petBubble.isPaused = false;
        }
        else if (dialogBox.activeSelf && playerInRange && Input.GetKeyDown(KeyCode.Space))
        {
            LogDialogueSkipped();
            dialogBox.SetActive(false);
            dialogActive = false;
            currentDialogIndex = 0;
            PetBubble.IsDialogueActive = false;
            if (petBubble != null) petBubble.isPaused = false;
        }
        else if (dialogBox.activeSelf && playerInRange && Input.GetKeyDown(KeyCode.Return))
        {
            LogDialogueSkipped();
            dialogBox.SetActive(false);
            dialogActive = false;
            currentDialogIndex = 0;
            PetBubble.IsDialogueActive = false;
            if (petBubble != null) petBubble.isPaused = false;
        }
    }

    /// <summary>
    /// Logs an early-exit dialogue read: player closed the sign or walked
    /// away before reaching the last line. Guards against double-logging
    /// if dialogBox is somehow already inactive when called.
    /// </summary>
    private void LogDialogueSkipped()
    {
        if (!dialogBox.activeSelf) return;
        if (AnalyticsLogger.Instance == null) return;

        AnalyticsLogger.Instance.LogEvent("dialogue_skipped",
            $"sign={signId};lines_read={currentDialogIndex + 1};total_lines={dialogs.Length};duration={(Time.time - dialogStartTime):F2}");
    }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            playerInRange = true;
            dialogActive = true;
            currentDialogIndex = 0;
            context.Raise();
        }
    }
    public override void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            playerInRange = false;
            dialogActive = false;

            // Player walked away mid-dialogue without finishing or pressing
            // an explicit close key — still counts as an incomplete read.
            LogDialogueSkipped();

            dialogBox.SetActive(false);
            currentDialogIndex = 0;
            PetBubble.IsDialogueActive = false;
            if (petBubble != null) petBubble.isPaused = false;
            context.Raise();
        }
    }
}