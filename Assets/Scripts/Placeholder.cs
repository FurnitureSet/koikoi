using UnityEngine;

public class Placeholder : MonoBehaviour
{
    public KoiKoiGameManager gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = FindFirstObjectByType<KoiKoiGameManager>();
    }

    void OnMouseDown()
    {
        Debug.Log("Placeholder clicked");
        if (gameManager.timeToDraw && gameManager.selectedCard != null)
        {
            gameManager.MoveCardToPlaceholder(transform.position);
        }
    }
}
