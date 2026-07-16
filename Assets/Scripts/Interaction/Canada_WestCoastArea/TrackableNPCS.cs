using UnityEngine;

public class TrackableNPC : MonoBehaviour
{
    private bool hasBeenTalkedTo = false;
    void Start()
    {
        Debug.Log("TrackableNPC registered: " + gameObject.name);
    }
    public void RegisterInteraction()
    {
        Debug.Log("RegisterInteraction called on " + gameObject.name);

        if (hasBeenTalkedTo)
        {
            Debug.Log("Already counted.");
            return;
        }

        hasBeenTalkedTo = true;
        Debug.Log("Calling WestCoastNPCTracker...");
        WestCoastNPCTracker.Instance.RegisterNPCTalkedTo();
    }
}