using System.Collections;
using UnityEngine;
using TMPro;

public class ContinueIndicatorAnimator : MonoBehaviour
{
    public GameObject ContinueIndicator;

    [SerializeField] private float interval = 0.4f;
    [SerializeField] private int maxDots = 3;

    private TMP_Text label;
    private Coroutine routine;

    private void Awake()
    {
        label = ContinueIndicator.GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        routine = StartCoroutine(AnimateDots());
    }

    private void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
    }

    private IEnumerator AnimateDots()
    {
        int dotCount = 0;
        var wait = new WaitForSeconds(interval);

        while (true)
        {
            label.text = new string('.', dotCount).PadRight(maxDots);
            dotCount = (dotCount + 1) % (maxDots + 1);
            yield return wait;
        }
    }
}