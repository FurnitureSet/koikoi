using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    private List<Card> cards = new List<Card>();
    
    public int CardCount => cards.Count;
    public bool IsEmpty => CardCount == 0;
    
    public void AddCard(Card card) {
        cards.Add(card);
    }

    public Card DrawCard() {
        if (cards.Count == 0) return null;
        
        Card card = cards[^1]; // The [^n] notation takes the n'th last element from a list, super weird...
        cards.RemoveAt(cards.Count - 1);
        return card;
    }

    public void Shuffle() {
        List<Card> temp = new List<Card>();
        // Shuffle the deck first
        while (cards.Count != 0) {
            int i_nextCard = Random.Range(0, cards.Count);
            temp.Add(cards[i_nextCard]);
            cards.RemoveAt(i_nextCard);
        }

        // Do it again the other way because why not?
        while (temp.Count != 0) {
            int i_nextCard = Random.Range(0, temp.Count);
            cards.Add(temp[i_nextCard]);
            temp.RemoveAt(i_nextCard);
        }
    }

    public void PrintDebugInfo() {
        foreach (Card card in cards)
            card.printCardDebugInfo();
    }
}
