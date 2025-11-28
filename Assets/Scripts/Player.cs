using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player
{
    public enum ListType {
        Hand, Bright, Animal, Ribbon, Plain
    }
    
    // List of player scores for each round
    private List<ScoreData> scores = new List<ScoreData>();

    // All held cards
    private List<Card> list_hand = new List<Card>();
    
    // Specifically special cards
    private List<Card> list_brightCards = new List<Card>();
    private List<Card> list_animalCards = new List<Card>();
    private List<Card> list_ribbonCards = new List<Card>();
    private List<Card> list_plainCards = new List<Card>();

    public int CountBrightCards => list_brightCards.Count;
    public int CountAnimalCards => list_animalCards.Count;
    public int CountRibbonCards => list_ribbonCards.Count;
    public int CountPlainCards => list_plainCards.Count;
    public int CountTotalCards => list_hand.Count;

    public ScoreData CurrentScoreData => scores[^1];
    public int CurrentScore => scores.Count >= 1 ? scores[^1].Total : -1;
    public int LastScore => scores.Count >= 2 ? scores[^2].Total : -1;

    public string Name { get; private set; }

    public Player(string name) {
        Name = name;
    }

    public void OnRoundSwap() {
        scores.Add(new ScoreData());
    }

    public void AddCard(Card card) {
        List<Card> list_cardType = card.CardTypeData switch {
            Card.CardType.Animal => list_animalCards,
            Card.CardType.Bright => list_brightCards,
            Card.CardType.Ribbon => list_ribbonCards,
            _ => list_plainCards
        };
        list_hand.Add(card);
        list_cardType.Add(card);
    }

    public void ResetHand() {
        list_brightCards.Clear();
        list_animalCards.Clear();
        list_ribbonCards.Clear();
        list_plainCards.Clear();
    }

    public int CountCardsByLambda(ListType list_to_search, Func<Card,bool> predicate) =>
        GetListByType(list_to_search).Count(predicate);
    
    public bool AnyCardsByLambda(ListType list_to_search, Func<Card,bool> predicate) =>
        GetListByType(list_to_search).Any(predicate);

    public bool ContainsCard(Card card) => list_hand.Contains(card);

    private List<Card> GetListByType(ListType list_to_search) =>
        list_to_search switch {
            ListType.Animal => list_animalCards,
            ListType.Bright => list_brightCards,
            ListType.Ribbon => list_ribbonCards,
            ListType.Plain => list_plainCards,
            _ => list_hand
    };
}
