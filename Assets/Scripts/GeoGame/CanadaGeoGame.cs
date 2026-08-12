using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CanadaGeoGame : MonoBehaviour
{
    public Inventory playerInventory;
    public Item winningItem;

    [Header("Colors")]
    public Color correctColor = new Color(0.13f, 0.5f, 0.13f);

    [Header("Scoring")]
    [Tooltip("Points awarded for each correct answer.")]
    public int pointsPerCorrectAnswer = 1000;

    private int currentIndex = 0;
    private int score = 0;
    private bool canAnswer = true;

    [Header("UI")]
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI scoreText;
    public Image questionImage;
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI ratingText;

    [Header("Province Images — for highlight on correct answer")]
    public Image atlantic;
    public Image farNorth;
    public Image maritimes;
    public Image ontario;
    public Image prairies;
    public Image quebec;
    public Image westCoast;

    [Header("Audio")]
    public AudioSource musicSource;    // background music — loop
    public AudioSource sfxSource;      // sound effects — no loop
    public AudioClip backgroundMusic;
    public AudioClip correctSound;
    public AudioClip wrongSound;

    [Header("Questions")]
    public List<ProvinceQuestion> questions;

    void Start()
    {
        // Start background music
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        ShuffleQuestions();
        LoadQuestion();
    }

    // No timer to tick anymore, so Update() has nothing to do per-frame.
    // Left in place in case you need it later; safe to delete otherwise.
    void Update()
    {
    }

    void ShuffleQuestions()
    {
        for (int i = 0; i < questions.Count; i++)
        {
            int rand = Random.Range(i, questions.Count);
            var temp = questions[i];
            questions[i] = questions[rand];
            questions[rand] = temp;
        }
    }

    void LoadQuestion()
    {
        if (currentIndex >= questions.Count)
        {
            EndGame();
            return;
        }

        canAnswer = true;
        feedbackText.text = "";
        ResetAllHighlights();

        ProvinceQuestion q = questions[currentIndex];
        questionText.text = q.questionText;

        if (questionImage != null && q.questionImage != null)
        {
            questionImage.gameObject.SetActive(true);
            questionImage.sprite = q.questionImage;
        }
        else if (questionImage != null)
        {
            questionImage.gameObject.SetActive(false);
        }
    }

    public void OnProvinceClicked(string provinceName)
    {
        Debug.Log("OnProvinceClicked called with: " + provinceName);
        Debug.Log("canAnswer: " + canAnswer);
        Debug.Log("questions count: " + questions.Count);

        if (!canAnswer) return;

        ProvinceQuestion q = questions[currentIndex];
        Debug.Log("Correct province: " + q.correctProvince);

        bool isCorrect = provinceName == q.correctProvince;
        Debug.Log("isCorrect: " + isCorrect);

        StartCoroutine(HandleAnswer(isCorrect, q.funFact));
    }

    private IEnumerator HandleAnswer(bool correct, string fact)
    {
        canAnswer = false;
        ProvinceQuestion q = questions[currentIndex];

        if (correct)
        {
            if (sfxSource != null && correctSound != null)
                sfxSource.PlayOneShot(correctSound);
            score += pointsPerCorrectAnswer;
            feedbackText.text = "Correct! +" + pointsPerCorrectAnswer + " points\n" + fact;
            feedbackText.color = correctColor;
            HighlightRegion(q.correctProvince, correctColor);
        }
        else
        {
            if (sfxSource != null && wrongSound != null)
                sfxSource.PlayOneShot(wrongSound);
            feedbackText.text = "Wrong! It was " + q.correctProvince + "\n" + fact;
            feedbackText.color = Color.red;
            HighlightRegion(q.correctProvince, correctColor); // show correct
        }

        scoreText.text = "Score: " + score;
        yield return new WaitForSeconds(2.5f);

        currentIndex++;
        LoadQuestion();
    }

    private void HighlightRegion(string name, Color color)
    {
        Image target = GetRegionImage(name);
        if (target != null)
            target.color = color;
    }

    public void ResetAllHighlights()
    {
        // Reset to original colors
        if (atlantic) atlantic.color = new Color(1f, 0.4f, 0.4f, 1f); // pink
        if (farNorth) farNorth.color = new Color(0.5f, 0.5f, 0.5f, 1f); // grey
        if (maritimes) maritimes.color = new Color(0.6f, 0.6f, 0.6f, 1f); // light grey
        if (ontario) ontario.color = new Color(0.9f, 0.5f, 0.1f, 1f); // orange
        if (prairies) prairies.color = new Color(0.9f, 0.9f, 0.1f, 1f); // yellow
        if (quebec) quebec.color = new Color(0.4f, 0.7f, 1f, 1f); // blue
        if (westCoast) westCoast.color = new Color(0.1f, 0.7f, 0.5f, 1f); // teal
    }

    private Image GetRegionImage(string name)
    {
        return name switch
        {
            "Atlantic" => atlantic,
            "FarNorth" => farNorth,
            "Maritimes" => maritimes,
            "Ontario" => ontario,
            "Prairies" => prairies,
            "Quebec" => quebec,
            "WestCoast" => westCoast,
            _ => null
        };
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainWorld()
    {
        SceneTracker.Instance.ReturnToPreviousScene(true);
    }

    void EndGame()
    {
        resultPanel.SetActive(true);
        resultText.text = "Final Score: " + score;

        int maxPossibleScore = questions.Count * pointsPerCorrectAnswer;
        bool perfectScore = score >= maxPossibleScore;

        if (perfectScore)
        {
            if (winningItem != null)
            {
                playerInventory.AddItem(winningItem);
            }
        }
        ratingText.text = GetRating(perfectScore);
    }

    string GetRating(bool perfectScore)
    {
        if (perfectScore) return "True Canadian! You get a reward key";

        float percentCorrect = (float)score / (questions.Count * pointsPerCorrectAnswer);
        if (percentCorrect >= 0.6f) return "Pretty good, eh!";
        if (percentCorrect >= 0.3f) return "Keep exploring Canada!";
        return "Time to study the map!";
    }
}