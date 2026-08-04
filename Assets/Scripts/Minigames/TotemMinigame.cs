using UnityEngine;
using UnityEngine.Events;

public class TotemMinigame : MonoBehaviour
{
    [SerializeField] private GameObject TopPiece;
    [SerializeField] private GameObject TopPieceSpot;
    [SerializeField] private GameObject MiddlePiece;
    [SerializeField] private GameObject MiddlePieceSpot;
    [SerializeField] private GameObject BottomPiece;
    [SerializeField] private GameObject BottomPieceSpot;
    private QuestGiver totemQuest;
    private SignalListener checkCorrect;

    void Start()
    {
        totemQuest = GetComponent<QuestGiver>();

        checkCorrect = new SignalListener();
        checkCorrect.response.AddListener(OnCorrectnessChecked);
        totemQuest.checkQuestCompletion.RegisterListener(checkCorrect);

        if (GameProgress.Instance != null && GameProgress.Instance.totemPuzzleSolved)
        {
            SnapPiecesIntoPlace();
            totemQuest.SetQuestGiven(true);
            totemQuest.MarkQuestDone();
        }
    }

    void Update()
    {
    }

    void OnCorrectnessChecked()
    {
        if (CheckTotemPieces())
        {
            totemQuest.MarkQuestDone();
            GameProgress.Instance.totemPuzzleSolved = true;
        }
        totemQuest.SetIsChecking(false);
    }

    public bool CheckTotemPieces()
    {
        Debug.Log("Checking pieces...");
        if (!TopPiece.GetComponent<Rigidbody2D>().IsTouching(TopPieceSpot.GetComponent<BoxCollider2D>()))
        {
            return false;
        }
        if (!MiddlePiece.GetComponent<Rigidbody2D>().IsTouching(MiddlePieceSpot.GetComponent<BoxCollider2D>()))
        {
            return false;
        }
        if (!BottomPiece.GetComponent<Rigidbody2D>().IsTouching(BottomPieceSpot.GetComponent<BoxCollider2D>()))
        {
            return false;
        }
        Debug.Log("Pieces correct!");
        return true;
    }

    private void SnapPiecesIntoPlace()
    {
        TopPiece.transform.position = TopPieceSpot.transform.position;
        MiddlePiece.transform.position = MiddlePieceSpot.transform.position;
        BottomPiece.transform.position = BottomPieceSpot.transform.position;
    }
}