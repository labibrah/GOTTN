using UnityEngine;

public class AreaExploredNotifier : MonoBehaviour
{
    public BoolValue targetFlag;
    public WindGuideController controller;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !collision.isTrigger)
        {
            Debug.Log($"[Notifier] Marking explored: {targetFlag.name}, current value before = {targetFlag.runtimeValue}");
            controller.MarkExplored(targetFlag);
        }
    }
}