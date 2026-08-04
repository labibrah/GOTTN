using UnityEngine;

public class RevealHiddenChest : MonoBehaviour
{
    [SerializeField] private GameObject QuestGiverObject;
    private QuestGiver _questGiver;
    private SignalListener _listener;

    void Start()
    {
        if (GameProgress.Instance != null && GameProgress.Instance.totemChestOpened)
        {
            Destroy(gameObject);
            return;
        }

        _questGiver = QuestGiverObject.GetComponent<QuestGiver>();
        _listener = new SignalListener();
        _listener.response.AddListener(RevealChest);
        _questGiver.questComplete.RegisterListener(_listener);
        gameObject.SetActive(false);
    }

    void Update()
    {
    }

    void RevealChest()
    {
        gameObject.SetActive(true);
    }
}