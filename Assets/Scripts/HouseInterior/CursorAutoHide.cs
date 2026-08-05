using UnityEngine;

public class CursorAutoHide : MonoBehaviour
{
    public static CursorAutoHide Instance;

    [SerializeField] private float idleTimeBeforeHide = 2f;

    private Vector3 lastMousePosition;
    private float idleTimer;
    private bool isHidden;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        lastMousePosition = Input.mousePosition;
    }

    void Update()
    {
        if (Input.mousePosition != lastMousePosition)
        {
            lastMousePosition = Input.mousePosition;
            idleTimer = 0f;

            if (isHidden)
            {
                Cursor.visible = true;
                isHidden = false;
            }
        }
        else
        {
            idleTimer += Time.deltaTime;

            if (!isHidden && idleTimer >= idleTimeBeforeHide)
            {
                Cursor.visible = false;
                isHidden = true;
            }
        }
    }
}