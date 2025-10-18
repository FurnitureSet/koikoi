using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardDisplayDebug : MonoBehaviour {
    [SerializeField] private Card cardPrefab;
    [SerializeField] private float offset_x = 0f;
    [SerializeField] private float offset_y = 0f;
    [SerializeField, Range(0.005f, 0.1f)] private float verticalSpacing = 0.06f;
    [SerializeField, Range(0.005f, 0.1f)] private float horizontalSpacing = 0.03f;
    [SerializeField] private Deck deck;

    void Start() {
        for (int i = 0; i < 48; i++) {
            Card card = Instantiate(cardPrefab);
            deck.AddCard(card);
            card.SetBinaryData(i);
            card.gameObject.name = card.MonthName + card.AnimalName + card.BrightName + card.RibbonName;
        }

        MoveCards();
    }

    public void MoveCards() {
        deck.Shuffle();
        const int columns = 12;
        const int rows = 4;
        for (int i = 0; i < 48; i++) {
            Card card = deck.DrawCard();
            float xIndex = (i % columns) - (columns - 1) * 0.5f;
            float zIndex = (i / columns) - (rows - 1) * 0.5f;
            float xPos = xIndex * horizontalSpacing + offset_x;
            float zPos = zIndex * verticalSpacing + offset_y;
            card.transform.position = new Vector3(xPos, 0.01f, zPos);
            card.transform.rotation = Quaternion.Euler(-90, 180, 0);
            card.AddComponent<Rigidbody>();
            BoxCollider boxCollider = card.GetComponent<BoxCollider>();
            if (boxCollider == null) {
                boxCollider = card.AddComponent<BoxCollider>();
            }
            boxCollider.size = new Vector3(0.61f, 1f, 0.04f);
            // card.transform.position = new Vector3(id_x + id_x * offset_x, id_y + id_y * offset_y);
            // card.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }
}
