using UnityEngine;
using TMPro;
using System.Collections;

public class PrairieSack : MonoBehaviour
{
    [Header("Item")]
    public string itemName;
    public enum Category { Clothing, Food, Household }
    public Category category;

    public float speed = 10f;
    float AdjustedSpeed => speed * PrairieGameManager.Instance.difficultyMultiplier;

    public TextMeshProUGUI textLabel;

    private PrairieSwitch railSwitch;
    private bool redirected = false;
    private Transform targetCart;

    private SpriteRenderer[] sackRenderers;

    void Start()
    {
        sackRenderers = GetComponentsInChildren<SpriteRenderer>();
        railSwitch = FindObjectOfType<PrairieSwitch>();

        textLabel.text = itemName;
    }

    void Update()
    {
        if (!redirected)
        {
            transform.Translate(Vector2.right * AdjustedSpeed * Time.deltaTime);
        }
        else if (targetCart != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetCart.position,
                AdjustedSpeed * Time.deltaTime
            );
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SwitchPoint"))
        {
            Redirect();
        }

        PrairieCart cart = other.GetComponent<PrairieCart>();
        if (cart != null)
        {
            ResolveCart(cart);
        }
        


        
    }

void Redirect()
{
    Debug.Log("Redirect called!");

    redirected = true;
    targetCart = railSwitch.GetCurrentTarget();

    Debug.Log("Target: " + targetCart.name);
}

    void ResolveCart(PrairieCart cart)
    {
        bool correct = (cart.cartCategory == category);
        if (correct)
        {
            PrairieGameManager.Instance.RegisterCorrect();
            Debug.Log("Correct!");
        }
        else if (cart.cartCategory != category)
        {
            Debug.Log("Wrong!");
        }

            StartCoroutine(FlashColor(correct ? Color.green : Color.red));
    }

    IEnumerator FlashColor(Color color)
    {
        foreach (SpriteRenderer sr in sackRenderers)
        {
            sr.color = color;
        }

        yield return new WaitForSeconds(0.3f);

        Destroy(gameObject);
    }

}