using UnityEngine;

public class PersistentAssetHolder : MonoBehaviour
{
    public static PersistentAssetHolder Instance;

    public Inventory playerInventory; // drag the SAME asset here

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}