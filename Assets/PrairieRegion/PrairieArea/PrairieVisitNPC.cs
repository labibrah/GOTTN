using UnityEngine;
public class PrairieVisitNPC : MonoBehaviour
{
    public int npcID;

    public void MarkVisited()
    {
        PrairieNPCTracker.Instance.MarkVisited(npcID);
    }
}