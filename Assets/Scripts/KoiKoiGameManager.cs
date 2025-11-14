using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using System;

public class KoiKoiGameManager : MonoBehaviour
{
    [SerializeField] private Card cardPrefab;
    private float offset_x = 0f;
    private float offset_y = 0f;
    private float verticalSpacing = 0.06f;
    private float horizontalSpacing = 0.03f;
    private Transform stackAnchor;
    private float stackHeightStep = 0.0004f;
    [SerializeField] private Deck deck;
    private float xPlayerStartPos = 0.105f;
    private float xCenterStartPos = 0.045f;
    private float xOffset = 0.03f;
    private float cardRestingHeight = 0.7055f;
    public Card selectedCard;
    public List<Card> playerHandCards = new List<Card>();
    public List<Card> centerCards = new List<Card>();
    public List<Card> opponentHandCards = new List<Card>();
    public List<Card> deckCards = new List<Card>();
    public bool playerTurn = false;
    public bool opponentTurn = false;
    public bool timeToDraw = false;
    public List<Card> playerMatchedCards = new List<Card>();
    public List<Card> opponentMatchedCards = new List<Card>();
    public GameObject[] topRowMarkers = new GameObject[8];
    public GameObject[] bottomRowMarkers = new GameObject[8];
    public GameObject[] topRowCards = new GameObject[8];
    public GameObject[] bottomRowCards = new GameObject[8];
    private List<Card> tempMatchedCards = new List<Card>();
    private List<Card> playerBrightCards = new List<Card>();
    private List<Card> opponentBrightCards = new List<Card>();
    private List<Card> playerAnimalCards = new List<Card>();
    private List<Card> opponentAnimalCards = new List<Card>();
    private List<Card> playerRibbonCards = new List<Card>();
    private List<Card> opponentRibbonCards = new List<Card>();
    private List<Card> playerShitCards = new List<Card>();
    private List<Card> opponentShitCards = new List<Card>();

    private Vector3 playerShitCardsPosition = new Vector3(-0.215f, 0.7055f, -0.21f);
    private Vector3 playerRibbonCardsPosition = new Vector3(-0.215f, 0.7055f, -0.17f);
    private Vector3 playerAnimalCardsPosition = new Vector3(-0.215f, 0.7055f, -0.13f);
    private Vector3 playerBrightCardsPosition = new Vector3(-0.215f, 0.7055f, -0.09f);

    private Vector3 opponentShitCardsPosition = new Vector3(0.215f, 0.7055f, 0.21f);
    private Vector3 opponentRibbonCardsPosition = new Vector3(0.215f, 0.7055f, 0.17f);
    private Vector3 opponentAnimalCardsPosition = new Vector3(0.215f, 0.7055f, 0.13f);
    private Vector3 opponentBrightCardsPosition = new Vector3(0.215f, 0.7055f, 0.09f);
    [SerializeField] private GameObject viewMatchesButton;
    [SerializeField] private GameObject viewOpponentMatchesButton;
    private bool gameOver = false;
    private int playerScore = 0;
    private int opponentScore = 0;
    private int currentRound = 0;

    private CameraController cameraController;
    [SerializeField] private GameObject newGameButton;

    IEnumerator Start()
    {
        cameraController = FindFirstObjectByType<CameraController>();
        LoadCards();
        CreateDeckOnScreen();
        yield return StartCoroutine(DealCardsCoroutine());
        CheckIfValidGame();
        CheckAndHandleThreeOfAKindOnBoard();
        StartCoroutine(GameLoop());
    }

    public void NewGame()
    {
        newGameButton.SetActive(false);
        StopAllCoroutines();
        cameraController.ResetView();
        StartCoroutine(NewGameCoroutine());
    }

    public IEnumerator NewGameCoroutine()
    {
        DestroyEverything();
        LoadCards();
        CreateDeckOnScreen();
        yield return StartCoroutine(DealCardsCoroutine());
        CheckIfValidGame();
        CheckAndHandleThreeOfAKindOnBoard();
        StartCoroutine(GameLoop());
    }

    public void LoadCards()
    {
        // Load card data from resources and instantiate cards
        CardData[] cardDataArray = Resources.LoadAll<CardData>("GameData/CardData");
        if (stackAnchor == null)
        {
            stackAnchor = transform;
        }

        for (int i = 0; i < 48; i++)
        {
            Card card = Instantiate(cardPrefab, stackAnchor.position, stackAnchor.rotation);
            deck.AddCard(card);
            deckCards.Add(card);
            Debug.Log($"Loaded Card Data: {cardDataArray[i].name}");
            card.LoadCardData(cardDataArray[i]);
            card.gameObject.name = card.MonthName + card.AnimalName + card.BrightName + card.RibbonName;
        }
    }

    public void DestroyEverything()
    {
        if (deck != null)
        {
            deck.ClearAllCards();
        }
        // Clear row arrays
        for (int i = 0; i < 8; i++)
        {
            topRowCards[i] = null;
            bottomRowCards[i] = null;
        }
        HideMarkers();
        deckCards.Clear();
        playerHandCards.Clear();
        opponentHandCards.Clear();
        centerCards.Clear();
        playerMatchedCards.Clear();
        opponentMatchedCards.Clear();
        tempMatchedCards.Clear();
        playerBrightCards.Clear();
        opponentBrightCards.Clear();
        playerAnimalCards.Clear();
        opponentAnimalCards.Clear();
        playerRibbonCards.Clear();
        opponentRibbonCards.Clear();
        playerShitCards.Clear();
        opponentShitCards.Clear();

        selectedCard = null;
        playerTurn = false;
        opponentTurn = false;
        timeToDraw = false;

        foreach (Card card in FindObjectsByType<Card>(FindObjectsSortMode.None))
        {
            if (card.gameObject.name == "OG Card" || card.gameObject.name == "Card Template") continue;
            Destroy(card.gameObject);
        }

        playerShitCardsPosition = new Vector3(-0.215f, 0.7055f, -0.21f);
        playerRibbonCardsPosition = new Vector3(-0.215f, 0.7055f, -0.17f);
        playerAnimalCardsPosition = new Vector3(-0.215f, 0.7055f, -0.13f);
        playerBrightCardsPosition = new Vector3(-0.215f, 0.7055f, -0.09f);

        opponentShitCardsPosition = new Vector3(0.215f, 0.7055f, 0.21f);
        opponentRibbonCardsPosition = new Vector3(0.215f, 0.7055f, 0.17f);
        opponentAnimalCardsPosition = new Vector3(0.215f, 0.7055f, 0.13f);
        opponentBrightCardsPosition = new Vector3(0.215f, 0.7055f, 0.09f);

        // Reset UI
        viewMatchesButton.SetActive(false);
        viewOpponentMatchesButton.SetActive(false);
        gameOver = false;
    }

    public void MoveCards()
    {
        deck.Shuffle();
        const int columns = 12;
        const int rows = 4;
        for (int i = 0; i < 48; i++)
        {
            Card card = deck.DrawCard();
            float xIndex = (i % columns) - (columns - 1) * 0.5f;
            float zIndex = (i / columns) - (rows - 1) * 0.5f;
            float xPos = xIndex * horizontalSpacing + offset_x;
            float zPos = zIndex * verticalSpacing + offset_y;
            card.transform.position = new Vector3(xPos, 0.01f, zPos);
            card.transform.rotation = Quaternion.Euler(-90, 180, 0);
            card.AddComponent<Rigidbody>();
            BoxCollider boxCollider = card.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = card.AddComponent<BoxCollider>();
            }
            boxCollider.size = new Vector3(0.61f, 1f, 0.06f);
            card.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            // card.transform.position = new Vector3(id_x + id_x * offset_x, id_y + id_y * offset_y);
            // card.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    public void CreateDeckOnScreen()
    {
        deck.Shuffle();
        Vector3 basePosition = stackAnchor != null ? stackAnchor.position : Vector3.zero;
        Quaternion baseRotation = Quaternion.Euler(180, 180, 0);
        for (int i = 0; i < 48; i++)
        {
            Card card = deck.GetCardAt(i);
            if (card == null)
            {
                break;
            }
            Vector3 stackedPosition = basePosition + Vector3.up * (stackHeightStep * i);
            card.transform.SetPositionAndRotation(stackedPosition, baseRotation);
            card.AddComponent<Rigidbody>();
            BoxCollider boxCollider = card.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = card.AddComponent<BoxCollider>();
            }
            boxCollider.size = new Vector3(0.024f, 0.001f, 0.034f);
            card.GetComponent<Rigidbody>().freezeRotation = true;
            card.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    IEnumerator DealCardsCoroutine()
    {
        float moveDuration = 0.1f;
        float delayBetweenCards = 0.1f;
        Vector3 topPlayerDealPosition = new Vector3(xPlayerStartPos, cardRestingHeight, 0.09f);
        Vector3 bottomPlayerDealPosition = new Vector3(xPlayerStartPos, cardRestingHeight, -0.09f);
        Vector3 centerDealPositionTop = new Vector3(xCenterStartPos, cardRestingHeight, 0.025f);
        Vector3 centerDealPositionBottom = new Vector3(xCenterStartPos, cardRestingHeight, -0.025f);
        // to track the array of cards to use with no match indicators later
        int arrayIndex = 5;

        // Deal two cards at a time, four times
        for (int y = 0; y < 4; y++)
        {
            // Deal top player's cards
            for (int i = 0; i < 2; i++)
            {
                Card card = deck.DrawCard();
                opponentHandCards.Insert(0, card);
                deckCards.Remove(card);
                if (card == null)
                {
                    break;
                }
                Rigidbody rb = card.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = card.gameObject.AddComponent<Rigidbody>();
                }
                rb.isKinematic = false;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                Quaternion dealRotation = Quaternion.Euler(0, 0, 0);
                yield return StartCoroutine(MoveRigidbodyToPosition(rb, topPlayerDealPosition, dealRotation, moveDuration));
                topPlayerDealPosition -= new Vector3(xOffset, 0f, 0f);
                rb.position = new Vector3(rb.position.x, cardRestingHeight, rb.position.z);
                yield return new WaitForSeconds(delayBetweenCards);
            }

            // Deal to the center
            for (int i = 0; i < 1; i++)
            {
                Card card = deck.DrawCard();
                centerCards.Add(card);
                deckCards.Remove(card);
                topRowCards[arrayIndex] = card.gameObject;
                if (card == null)
                {
                    break;
                }
                Rigidbody rb = card.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = card.gameObject.AddComponent<Rigidbody>();
                }
                rb.isKinematic = false;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                Quaternion dealRotation = Quaternion.Euler(0, 180, 0);
                yield return StartCoroutine(MoveRigidbodyToPosition(rb, centerDealPositionTop, dealRotation, moveDuration));
                centerDealPositionTop -= new Vector3(xOffset, 0f, 0f);
                rb.position = new Vector3(rb.position.x, cardRestingHeight, rb.position.z);
                yield return new WaitForSeconds(delayBetweenCards);

                Card card2 = deck.DrawCard();
                centerCards.Add(card2);
                deckCards.Remove(card2);
                bottomRowCards[arrayIndex] = card2.gameObject;
                arrayIndex--;
                if (card2 == null)
                {
                    break;
                }
                Rigidbody rb2 = card2.GetComponent<Rigidbody>();
                if (rb2 == null)
                {
                    rb2 = card2.gameObject.AddComponent<Rigidbody>();
                }
                rb2.isKinematic = false;
                rb2.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rb2.interpolation = RigidbodyInterpolation.Interpolate;
                yield return StartCoroutine(MoveRigidbodyToPosition(rb2, centerDealPositionBottom, dealRotation, moveDuration));
                centerDealPositionBottom -= new Vector3(xOffset, 0f, 0f);
                rb2.position = new Vector3(rb2.position.x, cardRestingHeight, rb2.position.z);
                yield return new WaitForSeconds(delayBetweenCards);
            }

            // Deal bottom player's cards
            for (int i = 0; i < 2; i++)
            {
                Card card = deck.DrawCard();
                playerHandCards.Insert(0, card);
                deckCards.Remove(card);
                if (card == null)
                {
                    break;
                }
                Rigidbody rb = card.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = card.gameObject.AddComponent<Rigidbody>();
                }
                rb.isKinematic = false;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                Quaternion dealRotation = Quaternion.Euler(0, 180, 0);
                yield return StartCoroutine(MoveRigidbodyToPosition(rb, bottomPlayerDealPosition, dealRotation, moveDuration));
                bottomPlayerDealPosition -= new Vector3(xOffset, 0f, 0f);
                rb.position = new Vector3(rb.position.x, cardRestingHeight, rb.position.z);
                yield return new WaitForSeconds(delayBetweenCards);
            }
        }
    }

    IEnumerator MoveRigidbodyToPosition(Rigidbody rb, Vector3 target, Quaternion rotation, float duration)
    {
        Vector3 start = rb.position;
        Quaternion startRot = rb.rotation;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rb.MovePosition(Vector3.Lerp(start, target, t));
            rb.MoveRotation(Quaternion.Slerp(startRot, rotation, t));
            yield return new WaitForFixedUpdate(); // physics step
        }
        rb.isKinematic = true;
    }

    void CheckIfValidGame()
    {
        // Four of a kind or four pairs on the board, invalid
        bool invalidGame = CheckLucky(centerCards);
        if (invalidGame)
        {
            Debug.Log("Invalid game detected due to four of a kind or four pairs on the board.");
            gameOver = true;
            DeactivateParticlesOnCards();
            HideMarkers();
            newGameButton.SetActive(true);
            return;
        }

        // Four of a kind or four pairs in hand, instant win 6 points
        bool playerInstantWin = CheckLucky(playerHandCards);
        bool opponentInstantWin = CheckLucky(opponentHandCards);
        if (playerInstantWin ^ opponentInstantWin)
        {
            // exactly one player has an instant win
            if (playerInstantWin)
            {
                Debug.Log("Player wins instantly with a lucky hand!");
                playerScore += 6;
            }
            else
            {
                Debug.Log("Opponent wins instantly with a lucky hand!");
                opponentScore += 6;
            }
            gameOver = true;
            DeactivateParticlesOnCards();
            HideMarkers();
            newGameButton.SetActive(true);
        }
    }

    bool CheckLucky(List<Card> hand)
    {
        Dictionary<string, int> monthCounts = new Dictionary<string, int>();

        // Count months
        foreach (Card card in centerCards)
        {
            if (!monthCounts.ContainsKey(card.MonthName))
                monthCounts[card.MonthName] = 0;

            monthCounts[card.MonthName]++;
        }

        // Detect four of a kind
        bool fourOfAKind = monthCounts.ContainsValue(4);

        // Detect four pairs (4+ distinct month pairs)
        bool fourPairs = monthCounts.Values.Count(v => v == 2) >= 4;

        return fourOfAKind || fourPairs;
    }

    String CheckThreeOfAKind()
    {
        Dictionary<string, int> monthCounts = new Dictionary<string, int>();
        String monthWithThreeOfAKind = "";

        // Count months
        foreach (Card card in centerCards)
        {
            if (!monthCounts.ContainsKey(card.MonthName))
                monthCounts[card.MonthName] = 0;

            monthCounts[card.MonthName]++;
        }

        // Detect three of a kind
        if (monthCounts.ContainsValue(3))
        {
            foreach (var pair in monthCounts)
            {
                if (pair.Value == 3)
                {
                    monthWithThreeOfAKind = pair.Key;
                }
            }
        }
        return monthWithThreeOfAKind;
    }
    
    void CheckAndHandleThreeOfAKindOnBoard()
    {
        Dictionary<string, int> monthCounts = new Dictionary<string, int>();

        // Count months
        foreach (Card card in centerCards)
        {
            if (!monthCounts.ContainsKey(card.MonthName))
                monthCounts[card.MonthName] = 0;

            monthCounts[card.MonthName]++;
        }

        // Detect three of a kind
        bool threeOfAKind = monthCounts.ContainsValue(3);
        if (threeOfAKind)
        {
            foreach (var pair in monthCounts)
            {
                if (pair.Value == 3)
                {
                    StartCoroutine(MatchThreeOfAKind(pair.Key));
                    break;
                }
            }
        }
    }

    IEnumerator MatchThreeOfAKind(string month)
    {
        // locate first card of that month in center cards
        Card firstCard = centerCards.Find(card => card.MonthName == month);
        // locate second card of that month in center cards
        Card secondCard = centerCards.Find(card => card.MonthName == month && card != firstCard);
        // locate third card of that month in center cards
        Card thirdCard = centerCards.Find(card => card.MonthName == month && card != firstCard && card != secondCard);
        if (firstCard != null && secondCard != null && thirdCard != null)
        {
            yield return StartCoroutine(MoveRigidbodyToPosition(secondCard.GetComponent<Rigidbody>(),
                new Vector3(firstCard.transform.position.x, cardRestingHeight + 0.002f,
                firstCard.transform.position.z - 0.005f),
                Quaternion.Euler(0, 180, 0),
                0.1f));
            RemoveFromCenterGrid(secondCard);
            yield return StartCoroutine(MoveRigidbodyToPosition(thirdCard.GetComponent<Rigidbody>(),
                new Vector3(secondCard.transform.position.x, cardRestingHeight + 0.004f,
                secondCard.transform.position.z - 0.005f),
                Quaternion.Euler(0, 180, 0),
                0.1f));
            RemoveFromCenterGrid(thirdCard);
            if (playerTurn)
            {
                playerMatchedCards.Remove(firstCard);
                playerMatchedCards.Remove(secondCard);
                playerMatchedCards.Remove(thirdCard);
            }
            else if (opponentTurn)
            {
                opponentMatchedCards.Remove(firstCard);
                opponentMatchedCards.Remove(secondCard);
                opponentMatchedCards.Remove(thirdCard);
            }
            tempMatchedCards.Remove(firstCard);
            tempMatchedCards.Remove(secondCard);
            tempMatchedCards.Remove(thirdCard);
        }
    }

    void MatchThreeOfAKindFromDraw(Card drawnCard)
    {
        StartCoroutine(MatchThreeOfAKind(drawnCard.MonthName));
    }
    
    private void RemoveFromCenterGrid(Card card)
    {
        for (int i = 0; i < topRowCards.Length; i++)
        {
            if (topRowCards[i] != null && topRowCards[i].GetComponent<Card>() == card)
            {
                topRowCards[i] = null;
                return;
            }
            if (bottomRowCards[i] != null && bottomRowCards[i].GetComponent<Card>() == card)
            {
                bottomRowCards[i] = null;
                return;
            }
        }
    }

    void CheckHands()
    {
        Debug.Log("Checking hands for possible matches...");
        bool canMatch = false;
        if (timeToDraw)
        {
            Debug.Log("Time to draw!");
            foreach (Card centerCard in centerCards)
            {
                if (selectedCard.MonthName == centerCard.MonthName)
                {
                    selectedCard.transform.Find("Particle shape").gameObject.SetActive(true);
                    centerCard.transform.Find("Particle shape").gameObject.SetActive(true);
                    canMatch = true;
                }
            }
        }
        else if (playerTurn)
        {
            foreach (Card playerCard in playerHandCards)
            {
                if (playerCard == null) continue;

                foreach (Card centerCard in centerCards)
                {
                    if (centerCard == null) continue;

                    if (playerCard.MonthName == centerCard.MonthName)
                    {
                        canMatch = true;
                        playerCard.transform.Find("Particle shape").gameObject.SetActive(true);
                        centerCard.transform.Find("Particle shape").gameObject.SetActive(true);
                    }
                }
            }
        }
        else if (opponentTurn)
        {
            foreach (Card opponentCard in opponentHandCards)
            {
                if (opponentCard == null) continue;

                foreach (Card centerCard in centerCards)
                {
                    if (centerCard == null) continue;

                    if (opponentCard.MonthName == centerCard.MonthName)
                    {
                        canMatch = true;
                        opponentCard.transform.Find("Particle shape").gameObject.SetActive(true);
                        centerCard.transform.Find("Particle shape").gameObject.SetActive(true);
                    }
                }
            }
        }
        if (!canMatch)
        {
            ShowMarkers();
        }
    }

    public void SetCardSelectedBool(Card card)
    {
        if (selectedCard != null)
        {
            selectedCard.isSelected = false;
            StartCoroutine(MoveRigidbodyToPosition(selectedCard.GetComponent<Rigidbody>(),
                new Vector3(selectedCard.transform.position.x, cardRestingHeight, selectedCard.transform.position.z),
                selectedCard.transform.rotation,
                0.1f));
        }
        selectedCard = card;
        card.isSelected = true;
        StartCoroutine(MoveRigidbodyToPosition(card.GetComponent<Rigidbody>(),
            new Vector3(card.transform.position.x, cardRestingHeight + 0.01f, card.transform.position.z),
            selectedCard.transform.rotation,
            0.1f));
    }

    public void Match(Card centerCard)
    {
        StartCoroutine(MatchCoroutine(centerCard));
    }

    public IEnumerator MatchCoroutine(Card centerCard)
    {
        yield return StartCoroutine(MoveRigidbodyToPosition(selectedCard.GetComponent<Rigidbody>(),
            new Vector3(centerCard.transform.position.x, centerCard.transform.position.y + 0.002f,
            centerCard.transform.position.z - 0.01f),
            Quaternion.Euler(0, 180, 0),
            0.1f));

        DeactivateParticlesOnCards();
        if (playerTurn)
        {
            // Three of a kind on board, claim all cards of that month
            if (CheckThreeOfAKind() == selectedCard.MonthName)
            {
                foreach (Card card in centerCards)
                {
                    if (card.MonthName == selectedCard.MonthName)
                    {
                        playerMatchedCards.Add(card);
                        tempMatchedCards.Add(card);
                    }
                }
            }
            else
            {
                playerMatchedCards.Add(centerCard);
                tempMatchedCards.Add(centerCard);
            }            
            playerHandCards.Remove(selectedCard);
            playerMatchedCards.Add(selectedCard);
            centerCards.Add(selectedCard);
        }
        if (opponentTurn)
        {
            // Three of a kind on board, claim all cards of that month
            if (CheckThreeOfAKind() == selectedCard.MonthName)
            {
                foreach (Card card in centerCards)
                {
                    if (card.MonthName == selectedCard.MonthName)
                    {
                        opponentMatchedCards.Add(card);
                        tempMatchedCards.Add(card);
                    }
                }
            }
            else
            {
                opponentMatchedCards.Add(centerCard);
                tempMatchedCards.Add(centerCard);
            }  
            opponentHandCards.Remove(selectedCard);
            opponentMatchedCards.Add(selectedCard);
            opponentMatchedCards.Add(centerCard);
            centerCards.Add(selectedCard);
        }
        // centerCards.Remove(centerCard);
        tempMatchedCards.Add(selectedCard);
        
        selectedCard = null;

        if (timeToDraw)
        {
            yield return new WaitForSeconds(0.8f); // short pause before claiming matches
            ClaimMatches();
            if (playerTurn)
            {
                Debug.Log("Restructuring Player Hand...");
                RestructurePlayerHand();
            }
            else if (opponentTurn)
            {
                Debug.Log("Restructuring Opponent Hand...");
                RestructureOpponentHand();
            }
            timeToDraw = false;
            playerTurn = !playerTurn;
            opponentTurn = !opponentTurn;
        }
        else
        {
            timeToDraw = true;
        }  
        HideMarkers();
    }

    public void MoveCardToPlaceholder(Vector3 position)
    {
        StartCoroutine(MoveCardToPlaceholderCoroutine(position));
    }

    IEnumerator MoveCardToPlaceholderCoroutine(Vector3 position)
    {
        yield return StartCoroutine(MoveRigidbodyToPosition(selectedCard.GetComponent<Rigidbody>(),
            new Vector3(position.x, cardRestingHeight, position.z),
            Quaternion.Euler(0, 180, 0),
            0.1f));

        centerCards.Add(selectedCard);
        if (playerHandCards.Contains(selectedCard))
        {
            playerHandCards.Remove(selectedCard);
        }
        if (opponentHandCards.Contains(selectedCard))
        {
            opponentHandCards.Remove(selectedCard);
        }
        DeactivateParticlesOnCards();
        HideMarkers();
        selectedCard = null;

        if (timeToDraw)
        {
            yield return new WaitForSeconds(0.8f); // short pause before claiming matches
            ClaimMatches();
            if (playerTurn)
            {
                Debug.Log("Restructuring Player Hand...");
                RestructurePlayerHand();
            }
            else if (opponentTurn)
            {
                Debug.Log("Restructuring Opponent Hand...");
                RestructureOpponentHand();
            }
            timeToDraw = false;
            playerTurn = !playerTurn;
            opponentTurn = !opponentTurn;
        }
        else
        {
            timeToDraw = true;
        } 
    }

    public void DrawFromDeck()
    {
        Card card = deck.DrawCard();
        selectedCard = card;
        deckCards.Remove(card);
        Rigidbody rb = card.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        Quaternion dealRotation = Quaternion.Euler(0, 180, 0);
        StartCoroutine(MoveRigidbodyToPosition(rb,
        new Vector3(card.transform.position.x + 0.05f, card.transform.position.y + 0.02f, card.transform.position.z),
        dealRotation, 0.1f));
        if (tempMatchedCards.Count(c => c.MonthName == selectedCard.MonthName) >= 2 &&
            centerCards.Count(c => c.MonthName == selectedCard.MonthName) != 3)
        {
            selectedCard = null;
            tempMatchedCards.Clear();
            centerCards.Add(card);
            MatchThreeOfAKindFromDraw(card);
            // change turn
            if (playerTurn)
            {
                Debug.Log("Restructuring Player Hand...");
                RestructurePlayerHand();
            }
            else if (opponentTurn)
            {
                Debug.Log("Restructuring Opponent Hand...");
                RestructureOpponentHand();
            }
            timeToDraw = false;
            playerTurn = !playerTurn;
            opponentTurn = !opponentTurn;
        }
        else
        {
            CheckHands();
        }        
    }

    public void DeactivateParticlesOnCards()
    {
        foreach (Card playerCard in playerHandCards)
        {
            playerCard.transform.Find("Particle shape").gameObject.SetActive(false);
        }
        foreach (Card centerCard in centerCards)
        {
            centerCard.transform.Find("Particle shape").gameObject.SetActive(false);
        }
        foreach (Card opponentCard in opponentHandCards)
        {
            opponentCard.transform.Find("Particle shape").gameObject.SetActive(false);
        }
        if (selectedCard != null)
        {
            selectedCard.transform.Find("Particle shape").gameObject.SetActive(false);
        }
    }

    public void ShowMarkers()
    {
        bool hadSpaceInCenter = false;
        // show inside markers first
        for (int i = 2; i < topRowCards.Length - 2; i++)
        {
            if (topRowCards[i] == null)
            {
                topRowMarkers[i].SetActive(true);
                hadSpaceInCenter = true;
            }
            if (bottomRowCards[i] == null)
            {
                bottomRowMarkers[i].SetActive(true);
                hadSpaceInCenter = true;
            }
        }
        // show neighboring markers if no space in center
        if (!hadSpaceInCenter)
        {
            for (int i = 0; i < topRowCards.Length; i++)
            {
                bool hasNeighbor =
                    (i > 0 && topRowCards[i - 1] != null) ||
                    (i < topRowCards.Length - 1 && topRowCards[i + 1] != null);
                if (hasNeighbor && topRowCards[i] == null)
                {
                    topRowMarkers[i].SetActive(true);
                }
            }
            for (int i = 0; i < bottomRowCards.Length; i++)
            {
                bool hasNeighbor =
                    (i > 0 && bottomRowCards[i - 1] != null) ||
                    (i < bottomRowCards.Length - 1 && bottomRowCards[i + 1] != null);
                if (hasNeighbor && bottomRowCards[i] == null)
                {
                    bottomRowMarkers[i].SetActive(true);
                }
            }
        }
    }

    public void HideMarkers()
    {
        foreach (GameObject marker in topRowMarkers)
        {
            marker.SetActive(false);
        }
        foreach (GameObject marker in bottomRowMarkers)
        {
            marker.SetActive(false);
        }
    }

    void ClaimMatches()
    {
        foreach (Card card in tempMatchedCards)
        {
            centerCards.Remove(card);
            if (card.IsBright)
            {
                if (playerTurn)
                {
                    playerBrightCards.Add(card);
                    StartCoroutine(MoveRigidbodyToPosition(card.GetComponent<Rigidbody>(),
                    new Vector3(playerBrightCardsPosition.x,
                    playerBrightCardsPosition.y,
                    playerBrightCardsPosition.z),
                    card.transform.rotation,
                    0.1f));
                    if (playerBrightCards.Count % 3 == 0)
                    {
                        playerBrightCardsPosition.x += 0.04f;
                    }
                    else
                    {
                        playerBrightCardsPosition.x += 0.03f;
                    }
                }
                else if (opponentTurn)
                {
                    opponentBrightCards.Add(card);
                    StartCoroutine(MoveRigidbodyToPosition(card.GetComponent<Rigidbody>(),
                    new Vector3(opponentBrightCardsPosition.x,
                    opponentBrightCardsPosition.y,
                    opponentBrightCardsPosition.z),
                    card.transform.rotation,
                    0.1f));
                    if (opponentBrightCards.Count % 3 == 0)
                    {
                        opponentBrightCardsPosition.x -= 0.04f;
                    }
                    else
                    {
                        opponentBrightCardsPosition.x -= 0.03f;
                    }
                }
                
            }
            else if (card.IsAnimal)
            {
                if (playerTurn)
                {
                    playerAnimalCards.Add(card);
                    StartCoroutine(MoveRigidbodyToPosition(card.GetComponent<Rigidbody>(),
                        new Vector3(playerAnimalCardsPosition.x,
                        playerAnimalCardsPosition.y,
                        playerAnimalCardsPosition.z),
                        card.transform.rotation,
                        0.1f));
                    if (playerAnimalCards.Count % 3 == 0)
                    {
                        playerAnimalCardsPosition.x += 0.04f;
                    }
                    else
                    {
                        playerAnimalCardsPosition.x += 0.03f;
                    }
                }
                else if (opponentTurn)
                {
                    opponentAnimalCards.Add(card);
                    StartCoroutine(MoveRigidbodyToPosition(card.GetComponent<Rigidbody>(),
                        new Vector3(opponentAnimalCardsPosition.x,
                        opponentAnimalCardsPosition.y,
                        opponentAnimalCardsPosition.z),
                        card.transform.rotation,
                        0.1f));
                    if (opponentAnimalCards.Count % 3 == 0)
                    {
                        opponentAnimalCardsPosition.x -= 0.04f;
                    }
                    else
                    {
                        opponentAnimalCardsPosition.x -= 0.03f;
                    }
                }
            }
            else if (card.IsRibbon)
            {
                if (playerTurn)
                {
                    playerRibbonCards.Add(card);
                    StartCoroutine(MoveRigidbodyToPosition(card.GetComponent<Rigidbody>(),
                        new Vector3(playerRibbonCardsPosition.x,
                        playerRibbonCardsPosition.y,
                        playerRibbonCardsPosition.z),
                        card.transform.rotation,
                        0.1f));
                    if (playerRibbonCards.Count % 3 == 0)
                    {
                        playerRibbonCardsPosition.x += 0.04f;
                    }
                    else
                    {
                        playerRibbonCardsPosition.x += 0.03f;
                    }
                } 
                else if (opponentTurn)
                {
                    opponentRibbonCards.Add(card);
                    StartCoroutine(MoveRigidbodyToPosition(card.GetComponent<Rigidbody>(),
                        new Vector3(opponentRibbonCardsPosition.x,
                        opponentRibbonCardsPosition.y,
                        opponentRibbonCardsPosition.z),
                        card.transform.rotation,
                        0.1f));
                    if (opponentRibbonCards.Count % 3 == 0)
                    {
                        opponentRibbonCardsPosition.x -= 0.04f;
                    }
                    else
                    {
                        opponentRibbonCardsPosition.x -= 0.03f;
                    }
                }
            }
            else
            {
                if (playerTurn)
                {
                    playerShitCards.Add(card);
                    StartCoroutine(MoveRigidbodyToPosition(card.GetComponent<Rigidbody>(),
                        new Vector3(playerShitCardsPosition.x,
                        playerShitCardsPosition.y,
                        playerShitCardsPosition.z),
                        card.transform.rotation,
                        0.1f));
                    if (playerShitCards.Count % 3 == 0)
                    {
                        playerShitCardsPosition.x += 0.04f;
                    }
                    else
                    {
                        playerShitCardsPosition.x += 0.03f;
                    }
                }
                else if (opponentTurn)
                {
                    opponentShitCards.Add(card);
                    StartCoroutine(MoveRigidbodyToPosition(card.GetComponent<Rigidbody>(),
                        new Vector3(opponentShitCardsPosition.x,
                        opponentShitCardsPosition.y,
                        opponentShitCardsPosition.z),
                        card.transform.rotation,
                        0.1f));
                    if (opponentShitCards.Count % 3 == 0)
                    {
                        opponentShitCardsPosition.x -= 0.04f;
                    }
                    else
                    {
                        opponentShitCardsPosition.x -= 0.03f;
                    }
                }
            }
            // clear the indices for the markers
            for (int i = 0; i < topRowCards.Length; i++)
            {
                if (topRowCards[i] != null && topRowCards[i].GetComponent<Card>() == card)
                {
                    topRowCards[i] = null;
                }
                if (bottomRowCards[i] != null && bottomRowCards[i].GetComponent<Card>() == card)
                {
                    bottomRowCards[i] = null;
                }
            }
        }
        tempMatchedCards.Clear();
        if (playerBrightCards.Count > 0 || playerAnimalCards.Count > 0 || playerRibbonCards.Count > 0 || playerShitCards.Count > 0)
        {
            viewMatchesButton.SetActive(true);
        }
        if (opponentBrightCards.Count > 0 || opponentAnimalCards.Count > 0 || opponentRibbonCards.Count > 0 || opponentShitCards.Count > 0)
        {
            viewOpponentMatchesButton.SetActive(true);
        }
    }

    void RestructurePlayerHand()
    {
        if (playerHandCards.Count == 0) return;

        // Determine total width based on spacing (0.03 between each card)
        float spacing = 0.03f;
        float totalWidth = (playerHandCards.Count - 1) * spacing;

        // Center cards around x = 0
        float startX = -totalWidth / 2f;

        for (int i = 0; i < playerHandCards.Count; i++)
        {
            Card card = playerHandCards[i];
            if (card == null) continue;

            Rigidbody rb = card.GetComponent<Rigidbody>();

            // Calculate the new target position
            Vector3 targetPos = new Vector3(startX + i * spacing, cardRestingHeight, -0.09f);

            // Smoothly move cards back into place
            StartCoroutine(MoveRigidbodyToPosition(rb, targetPos, card.transform.rotation, 0.1f));
        }
    }

    void RestructureOpponentHand()
    {
        Debug.Log("Entered RestructureOpponentHand()");
        if (opponentHandCards.Count == 0) return;

        // Determine total width based on spacing (0.03 between each card)
        float spacing = 0.03f;
        float totalWidth = (opponentHandCards.Count - 1) * spacing;

        // Center cards around x = 0
        float startX = -totalWidth / 2f;

        for (int i = 0; i < opponentHandCards.Count; i++)
        {
            Card card = opponentHandCards[i];
            if (card == null) continue;

            Rigidbody rb = card.GetComponent<Rigidbody>();

            // Calculate the new target position
            Vector3 targetPos = new Vector3(startX + i * spacing, cardRestingHeight, 0.09f);

            // Smoothly move cards back into place
            StartCoroutine(MoveRigidbodyToPosition(rb, targetPos, card.transform.rotation, 0.1f));
        }
    }

    IEnumerator GameLoop()
    {
        // Main game loop logic
        playerTurn = true;
        while (!gameOver)
        {
            if (playerTurn)
            {
                yield return new WaitForSeconds(1.0f); // brief pause before player's turn
                yield return StartCoroutine(PlayerTurnCoroutine());
            }
            else if (opponentTurn)
            {
                yield return new WaitForSeconds(1.0f); // brief pause before opponent's turn
                yield return StartCoroutine(OpponentTurnCoroutine());
            }
        }
        Debug.Log("Game Over!");
    }

    IEnumerator PlayerTurnCoroutine()
    {
        CheckHands();
        yield return new WaitUntil(() => !playerTurn || gameOver);
    }

    IEnumerator OpponentTurnCoroutine()
    {
        CheckHands();
        yield return new WaitUntil(() => !opponentTurn || gameOver);
    }
}
