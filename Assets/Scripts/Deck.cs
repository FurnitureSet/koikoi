using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    private List<Card> cards = new List<Card>();
    
    /// <summary>
    /// The amount of cards currently in the deck.
    /// </summary>
    public int CardCount => cards.Count;
    /// <summary>
    /// Returns true if the deck is empty.
    /// </summary>
    public bool IsEmpty => CardCount == 0;
    
    /// <summary>
    /// Add a card to the Deck's stack.
    /// </summary>
    /// <param name="card">Reference to the card being added to the Deck.</param>
    public void AddCard(Card card) {
        cards.Add(card);
    }

    /// <summary>
    /// Take the last card from the Deck.
    /// </summary>
    /// <returns>Returns the top card if one exists, otherwise returns null.</returns>
    public Card DrawCard() {
        if (cards.Count == 0) return null;
        
        Card card = cards[^1]; // The [^n] notation takes the n'th last element from a list, super weird...
        cards.RemoveAt(cards.Count - 1);
        return card;
    }

    /// <summary>
    /// Shuffles the Deck.
    /// </summary>
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

    /// <summary>
    /// Log the debug info of each card currently in the deck to the console.
    /// </summary>
    public void PrintDebugInfo() {
        foreach (Card card in cards)
            card.printCardDebugInfo();
    }
}
