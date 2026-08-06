using UnityEngine;

public class BossFightGhost : MonoBehaviour
{
    [Header("Expressions")]
    public Sprite[] expressions;      // drag your 5 sprites here, in order
    public float frameInterval = 0.6f; // time between expression changes
    public bool randomOrder = true;    // false = cycle in order, true = random pick

    [Header("Hover")]
    public float bobHeight = 0.15f;
    public float bobSpeed = 1.5f;

    [Header("Pulse")]
    public float pulseMin = 0.25f;
    public float pulseMax = 0.45f;
    public float pulseSpeed = 1f;

    private SpriteRenderer sr;
    private Vector3 startPos;
    private float frameTimer;
    private int currentIndex;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startPos = transform.position;

        if (expressions.Length > 0)
            sr.sprite = expressions[0];
    }

    void Update()
    {
        // Bob
        float y = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPos + new Vector3(0, y, 0);

        // Pulse alpha
        float alpha = Mathf.Lerp(pulseMin, pulseMax, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;

        // Expression cycling
        if (expressions.Length > 1)
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= frameInterval)
            {
                frameTimer = 0f;
                currentIndex = randomOrder
                    ? Random.Range(0, expressions.Length)
                    : (currentIndex + 1) % expressions.Length;
                sr.sprite = expressions[currentIndex];
            }
        }
    }
}