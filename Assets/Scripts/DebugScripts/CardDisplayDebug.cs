using System.Collections.Generic;
using UnityEngine;

public class CardDisplayDebug : MonoBehaviour {
    [SerializeField] private Card cardPrefab;
    [SerializeField] private float offset_x = 0f;
    [SerializeField] private float offset_y = 0f;
    [SerializeField] private Deck deck;

    void Start() {
        for (int i = 0; i < 48; i++) {
            Card card = Instantiate(cardPrefab);
            deck.AddCard(card);
            card.SetBinaryData(i);
            card.gameObject.name = i.ToString();
        }

        MoveCards();
    }

    public void MoveCards() {
        deck.Shuffle();
        for (int i = 0; i < 48; i++) {
            Card card = deck.DrawCard();
            int id_x = i % 12 - 6;
            int id_y = i / 12 - 2;
            card.transform.position = new Vector3(id_x + id_x * offset_x, id_y + id_y * offset_y);
            card.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }
}
