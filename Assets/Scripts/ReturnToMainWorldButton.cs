using UnityEngine;

public class ReturnToMainWorldButton : MonoBehaviour
{
    public void ReturnToMainWorld()
    {
        SceneTracker.Instance.ReturnToPreviousScene(true);
    }
}