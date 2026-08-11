using UnityEngine;
public class PrairieSwitch : MonoBehaviour
{
    public int currentDirection = 1;
    public Transform topCart;
    public Transform middleCart;
    public Transform bottomCart;
    public Transform switchPoint;

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ActivateSwitch();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Interactable.InteractablesInRange++; // NEW
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Interactable.InteractablesInRange = Mathf.Max(0, Interactable.InteractablesInRange - 1); // NEW
        }
    }

    void ActivateSwitch()
    {
        if (!PrairieGameManager.Instance.gameStarted)
        {
            PrairieGameManager.Instance.gameStarted = true;
        }
        currentDirection++;
        if (currentDirection > 2)
            currentDirection = 0;
        RotateSwitchPoint();
    }

    void RotateSwitchPoint()
    {
        Debug.Log("Switch direction: " + currentDirection);
        if (currentDirection == 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 25);
            transform.position = new Vector3(-1.85f, 3.47f, 0);
            switchPoint.rotation = Quaternion.Euler(0, 0, 40);
        }
        else if (currentDirection == 1)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.position = new Vector3(-1.7f, 3.47f, 0);
            switchPoint.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (currentDirection == 2)
        {
            transform.rotation = Quaternion.Euler(0, 0, -25);
            transform.position = new Vector3(-1.5f, 3.47f, 0);
            switchPoint.rotation = Quaternion.Euler(0, 0, -40);
        }
    }

    public Transform GetCurrentTarget()
    {
        if (currentDirection == 0) return topCart;
        if (currentDirection == 1) return middleCart;
        return bottomCart;
    }
}