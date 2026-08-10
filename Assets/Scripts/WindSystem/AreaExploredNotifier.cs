using UnityEngine;

public class AreaExploredNotifier : MonoBehaviour
{
    public BoolValue targetFlag;
    public WindGuideController controller;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !collision.isTrigger)
        {
            controller.MarkExplored(targetFlag);
        }
    }
}