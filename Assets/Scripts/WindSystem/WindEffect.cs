
using UnityEngine;

public class WindEffect : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float maxLifetime = 8f;

    private Transform target;
    private System.Action onFinished;
    private float life;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }


    public void Init(Transform target, System.Action onFinished)
    {
        this.target = target;
        this.onFinished = onFinished;
        if (audioSource != null)
            audioSource.Play();
    }

    private void Update()
    {
        life += Time.deltaTime;
        Vector2 dir = (target.position - transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.5f || life >= maxLifetime)
            Dismiss();
    }

    public void Dismiss()
    {
        if (audioSource != null)
            audioSource.Stop();
        onFinished?.Invoke();
        Destroy(gameObject);
    }
}