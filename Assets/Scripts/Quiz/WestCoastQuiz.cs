using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class WestCoastQuiz : Interactable
{
    [SerializeField] private MiniGame_MultipleChoice minigame;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject dialogBox;
    public TextMeshProUGUI dialogText;
    public string[] dialogs;
    public bool dialogActive;
    public int currentDialogIndex = 0;

    // --- Continue indicator (animated dots) ---
    public GameObject ContinueIndicator;
    private TMP_Text continueDotsLabel;
    private float dotTimer = 0f;
    [SerializeField] private float dotInterval = 0.4f;
    [SerializeField] private int maxDots = 3;
    private int dotCount = 0;

    public override void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        if (dialogBox == null)
        {
            dialogBox = GameObject.FindGameObjectWithTag("DialogBox");
        }
        if (dialogText == null)
        {
            dialogText = dialogBox.GetComponentInChildren<TextMeshProUGUI>();
        }
        if (ContinueIndicator != null)
        {
            continueDotsLabel = ContinueIndicator.GetComponentInChildren<TMP_Text>();
        }
        if (firstInteractionDone.runtimeValue == true)
        {
            if (flashingAnimator != null)
            {
                flashingAnimator.SetBool("isFlashing", false);
            }
        }

    }

    public virtual void Update()
    {
        UpdateContinueIndicator();

        if (dialogActive && Input.GetKeyDown(KeyCode.E) && firstInteractionDone.runtimeValue == true)
        {
            if (LonghouseGreeter.Instance != null && LonghouseGreeter.Instance.CanInteract())
            {
                TriggerQuiz();
                dialogActive = false;
            }
            else
            {
                if (!dialogBox.activeSelf)
                {
                    dialogText.text = "You must speak with everyone else in the village first.";
                    dialogBox.SetActive(true);
                }
                else
                {
                    dialogBox.SetActive(false);
                }
            }
        }
        else if (dialogActive && Input.GetKeyDown(KeyCode.E))
        {
            if (audioSource != null && interactSound != null)
            {
                audioSource.PlayOneShot(interactSound);
            }

            if (!dialogBox.activeSelf)
            {
                dialogBox.SetActive(true);
                currentDialogIndex = 0;
                dialogText.text = dialogs.Length > 0 ? dialogs[currentDialogIndex] : "";
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
                    currentDialogIndex = 0;
                    Debug.Log("Dialog ended, calling Interact.");
                    Interact();
                }
            }
        }
        else if (dialogActive && Input.GetKeyDown(KeyCode.Escape))
        {
            dialogBox.SetActive(false);
            dialogActive = false;
            currentDialogIndex = 0;
        }
        else if (dialogActive && Input.GetKeyDown(KeyCode.Space))
        {
            dialogBox.SetActive(false);
            dialogActive = false;
            currentDialogIndex = 0;
        }
        else if (dialogActive && Input.GetKeyDown(KeyCode.Return))
        {
            dialogBox.SetActive(false);
            dialogActive = false;
            currentDialogIndex = 0;
        }
    }

    private void UpdateContinueIndicator()
    {
        if (ContinueIndicator == null || continueDotsLabel == null) return;

        bool hasMoreText = dialogActive && dialogBox.activeSelf && currentDialogIndex < dialogs.Length - 1;

        ContinueIndicator.SetActive(hasMoreText);

        if (hasMoreText)
        {
            dotTimer += Time.deltaTime;
            if (dotTimer >= dotInterval)
            {
                dotTimer = 0f;
                dotCount = (dotCount + 1) % (maxDots + 1);
                continueDotsLabel.text = new string('.', dotCount).PadRight(maxDots);
            }
        }
        else
        {
            dotTimer = 0f;
            dotCount = 0;
        }
    }


    public override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            dialogActive = true;
            currentDialogIndex = 0;
            InteractablesInRange++;
            context.Raise();
        }
    }

    public override void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            dialogActive = false;
            dialogBox.SetActive(false);
            InteractablesInRange = Mathf.Max(0, InteractablesInRange - 1);
            context.Raise();
            currentDialogIndex = 0;
        }
    }


    public void TriggerQuiz()
    {
        minigame.LaunchQuestion(minigame.questionsSet, OnQuizCompletion);
    }

    public void OnQuizCompletion(bool allCorrect)
    {
        if (allCorrect)
        {
            GameProgress.Instance.quizCompleted = true;
        }
        gameObject.SetActive(!GameProgress.Instance.quizCompleted);
    }
}