using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using System;
using TMPro;

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
    [SerializeField] private List<Card> playerBrightCards = new List<Card>();
    [SerializeField] private List<Card> opponentBrightCards = new List<Card>();
    [SerializeField] private List<Card> playerAnimalCards = new List<Card>();
    [SerializeField] private List<Card> opponentAnimalCards = new List<Card>();
    [SerializeField] private List<Card> playerRibbonCards = new List<Card>();
    [SerializeField] private List<Card> opponentRibbonCards = new List<Card>();
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
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private TMP_Text opponentScoreText;
    [SerializeField] private TMP_Text currentRoundText;

    private bool playerWin = false;
    private bool opponentWin = false;
    private bool playerKoiKoi = false;
    private bool opponentKoiKoi = false;

    private int previousPlayerTempScore = 0;
    private int playerTempScore = 0;
    private int previousOpponentTempScore = 0;
    private int opponentTempScore = 0;

    [SerializeField] private TMP_Text playerWinText;
    [SerializeField] private TMP_Text opponentWinText;
    [SerializeField] private TMP_Text tempPlayerPointsText;
    [SerializeField] private TMP_Text tempOpponentPointsText;
    [SerializeField] private GameObject playerClaimWinButton;
    [SerializeField] private GameObject opponentClaimWinButton;
    [SerializeField] private GameObject playerKoiKoiButton;
    [SerializeField] private GameObject opponentKoiKoiButton;
    [SerializeField] private TMP_Text drawText;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private TMP_Text playerWinDataText;
    [SerializeField] private TMP_Text opponentWinDataText;

    IEnumerator Start()
    {
        playerTurn = true;
        cameraController = FindFirstObjectByType<CameraController>();
        LoadCards();
        CreateDeckOnScreen();
        currentRoundText.text = CurrentRoundSwitch(currentRound);
        yield return StartCoroutine(DealCardsCoroutine());
        CheckIfValidGame();
        CheckAndHandleThreeOfAKindOnBoard();
        StartCoroutine(GameLoop());
    }

    public void NewGame()
    {
        
        newGameButton.SetActive(false);
        StopAllCoroutines();
        if (currentRound < 11)
        {
            currentRound++;
        }
        else
        {
            currentRound = 0;
            playerScore = 0;
            playerScoreText.text = "Player Score: " + playerScore.ToString();
            opponentScore = 0;
            opponentScoreText.text = "Opponent Score: " + opponentScore.ToString();
        }        
        currentRoundText.text = CurrentRoundSwitch(currentRound);
        cameraController.ResetView();
        StartCoroutine(NewGameCoroutine());
    }

    String CurrentRoundSwitch(int round)
    {
        switch (round)
        {
            case 0:
                return "January";
            case 1:
                return "February";
            case 2:
                return "March";
            case 3:
                return "April";
            case 4:
                return "May";
            case 5:
                return "June";
            case 6:
                return "July";
            case 7:
                return "August";
            case 8:
                return "September";
            case 9:
                return "October";
            case 10:
                return "November";
            case 11:
                return "December";
            default:
                return "Unknown";
        }
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
        timeToDraw = false;
        playerKoiKoi = false;
        opponentKoiKoi = false;
        playerWin = false;
        opponentWin = false;

        previousPlayerTempScore = 0;
        playerTempScore = 0;
        previousOpponentTempScore = 0;
        opponentTempScore = 0;

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
        gameOverText.gameObject.SetActive(false);
        gameOver = false;
    }

    public void MoveCards()
    {
        // Display all cards in a grid to verify correct loading
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
        Quaternion baseRotation = Quaternion.Euler(180, 0, 0);
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
        bool canMatch = false;
        if (timeToDraw)
        {
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
        tempMatchedCards.Add(selectedCard);

        selectedCard = null;

        if (timeToDraw)
        {
            yield return new WaitForSeconds(0.8f); // short pause before claiming matches
            ClaimMatches();
            RestructurePlayerHand();
            RestructureOpponentHand();
            timeToDraw = false;
        }
        else
        {
            drawText.gameObject.SetActive(true);
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
            RestructurePlayerHand();
            RestructureOpponentHand();
            timeToDraw = false;
        }
        else
        {
            drawText.gameObject.SetActive(true);
            timeToDraw = true;
        }
    }

    public void DrawFromDeck()
    {
        drawText.gameObject.SetActive(false);
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
            timeToDraw = false;
            RestructurePlayerHand();
            RestructureOpponentHand();
            SwitchPlayerTurn();
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
        if (playerTurn)
        {
            Debug.Log("Player turn, figuring out what to do");
            // check for player yaku
            if (playerKoiKoi)
            {
                Debug.Log("Checking for player Koi-Koi Yaku");
                CheckYakuPlayerKoiKoi();
                return;
            }
            else
            {
                Debug.Log("Checking for player Yaku");
                int playerScoreThisTurn = CheckYakuPlayer();
                if (playerScoreThisTurn > 0)
                {
                    ShowPlayerWinScreen();
                }
                else
                {
                    SwitchPlayerTurn();
                    return;
                }
            }
        }
        if (opponentTurn)
        {
            Debug.Log("Opponent turn, figuring out what to do");
            // check for opponent yaku
            if (opponentKoiKoi)
            {
                Debug.Log("Checking for opponent Koi-Koi Yaku");
                CheckYakuOpponentKoiKoi();
                return;
            }
            else
            {
                Debug.Log("Checking for opponent Yaku");
                int opponentScoreThisTurn = CheckYakuOpponent();
                if (opponentScoreThisTurn > 0)
                {
                    ShowOpponentWinScreen();
                }
                else
                {
                    SwitchPlayerTurn();
                    return;
                }
            }
        }
    }

    void SwitchPlayerTurn()
    {
        playerTurn = !playerTurn;
        opponentTurn = !opponentTurn;
        if (playerTurn)
        {
            Debug.Log("PLAYER TURN");
        }
        else if (opponentTurn)
        {
            Debug.Log("OPPONENT TURN");
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

    int CheckYakuPlayer()
    {
        String winData = "";
        playerTempScore = 0;
        int winnings = 0;

        // 10 shit cards
        if (playerShitCards.Count >= 10)
        {
            // 10 shit cards = 1 point + 1 point for each additional
            winnings = playerShitCards.Count - 9;
            playerTempScore += winnings;
            winData += $"Shit cards: {winnings}  ";
        }
        // Poetry AND Blue ribbons
        if (playerRibbonCards.Count(c => c.RibbonName == "Text") >= 3 && 
            playerRibbonCards.Count(c => c.RibbonName == "Blue") >= 3)
        {
            playerTempScore += 12;
            winData += "Poetry AND Blue ribbons: 12  ";
            // +1 for each additional ribbon
            if (playerRibbonCards.Count > 6)
            {
                playerTempScore += playerRibbonCards.Count - 6;
                winData += $" +{playerRibbonCards.Count - 6} for additional ribbons  ";
            }
        }
        // Red poetry ribbons
        else if (playerRibbonCards.Count(c => c.RibbonName == "Text") >= 3)
        {
            playerTempScore += 6;
            winData += "Red poetry ribbons: 6  ";
            // +1 for each additional ribbon
            if (playerRibbonCards.Count > 3)
            {
                playerTempScore += playerRibbonCards.Count - 3;
                winData += $" +{playerRibbonCards.Count - 3} for additional ribbons  ";
            }
        }
        // Blue ribbons
        else if (playerRibbonCards.Count(c => c.RibbonName == "Blue") >= 3)
        {
            playerTempScore += 6;
            winData += "Blue ribbons: 6  ";
            // +1 for each additional ribbon
            if (playerRibbonCards.Count > 3)
            {
                playerTempScore += playerRibbonCards.Count - 3;
                winData += $" +{playerRibbonCards.Count - 3} for additional ribbons  ";
            }
        }
        // 5 ribbon cards
        else if (playerRibbonCards.Count >= 5)
        {
            // 5 ribbon cards = 1 point + 1 point for each additional
            winnings = playerRibbonCards.Count - 4;
            playerTempScore += winnings;
            winData += $"Ribbon cards: {winnings}  ";
        }
        // Ino-shika-cho
        bool hasBoar = playerAnimalCards.Any(c => c.AnimalName == "Boar");
        bool hasDeer = playerAnimalCards.Any(c => c.AnimalName == "Deer");
        bool hasButterfly = playerAnimalCards.Any(c => c.AnimalName == "Butterfly");

        if (hasBoar && hasDeer && hasButterfly)
        {
            playerTempScore += 5;
            winData += "Ino-shika-cho: 5  ";
            // +1 for each additional animal
            if (playerAnimalCards.Count > 3)
            {
                playerTempScore += playerAnimalCards.Count - 3;
                winData += $" +{playerAnimalCards.Count - 3} for additional animals  ";
            }
        }
        // 5 animal cards
        else if (playerAnimalCards.Count >= 5)
        {
            // 5 animal cards = 1 point + 1 point for each additional
            winnings = playerAnimalCards.Count - 4;
            playerTempScore += winnings;
            winData += $"Animal cards: {winnings}  ";
        }
        // Brights
        int brightCount = playerBrightCards.Count;
        if (brightCount >= 5)
        {
            playerTempScore += 15;
            winData += "Five brights: 15  ";
        }
        else if (brightCount == 4 && !playerBrightCards.Any(c => c.BrightName == "RainMan"))
        {
            playerTempScore += 10;
            winData += "Four brights (no RainMan): 10  ";
        }
        else if (brightCount == 4 && playerBrightCards.Any(c => c.BrightName == "RainMan"))
        {
            playerTempScore += 8;
            winData += "Four brights (with RainMan): 8  ";
        }
        else if (brightCount == 3 && !playerBrightCards.Any(c => c.BrightName == "RainMan"))
        {
            playerTempScore += 6;
            winData += "Three brights (no RainMan): 6  ";
        }
        // Flower/Moon viewing
        bool hasSakura = playerBrightCards.Any(c => c.BrightName == "Sakura");
        bool hasMoon = playerBrightCards.Any(c => c.BrightName == "Moon");
        bool hasSakeCup = playerAnimalCards.Any(c => c.AnimalName == "Sake");
        if (hasMoon && hasSakeCup)
        {
            playerTempScore += 5;
            winData += "Moon viewing: 5  ";
        }
        if (hasSakura && hasSakeCup)
        {
            playerTempScore += 5;
            winData += "Sakura viewing: 5  ";
        }
        // Cards of the Month
        String currentRoundMonth = CurrentRoundSwitch(currentRound);
        if (playerMatchedCards.Count(c => c.MonthName == currentRoundMonth) >= 4)
        {
            playerTempScore += 4;
            winData += "Cards of the month: 4  ";
        }
        // Double points for 7 or more
        if (playerTempScore >= 7)
        {
            winData += $"\n>=7 points, doubled: {playerTempScore} * 2 = {playerTempScore * 2}  ";
            playerTempScore *= 2; // double points for 7 or more
        }
        if (previousPlayerTempScore == 0 && playerTempScore > 0)
        {
            previousPlayerTempScore = playerTempScore;
            tempPlayerPointsText.text = "Winning points: " + playerTempScore.ToString();
        }
        playerWinDataText.text = winData;
        return playerTempScore;
    }

    void CheckYakuPlayerKoiKoi()
    {
        Debug.Log("Entered CheckYakuPlayerKoiKoi");
        int score = CheckYakuPlayer();
        if (score > previousPlayerTempScore)
        {
            Debug.Log("Player has achieved a new Yaku with a score of: " + score);
            previousPlayerTempScore = score;
            playerTempScore = score * 2; // double points for Koi-Koi
            tempPlayerPointsText.text = "Winning points: " + playerTempScore.ToString();
            playerWinDataText.text += "\nKoi-Koi! Points doubled to: " + playerTempScore.ToString();
            ShowPlayerWinScreen();
        }
        else
        {
            Debug.Log("Player did not achieve a new Yaku during Koi-Koi.");
            if (playerHandCards.Count == 0 && opponentHandCards.Count == 0 && !playerWin && !opponentWin)
            {
                ExhuastiveDraw();
            }
            else
            {
                SwitchPlayerTurn();
            }
        }
    }

    void ShowPlayerWinScreen()
    {
        playerWin = true;
        playerWinText.gameObject.SetActive(true);
        tempPlayerPointsText.gameObject.SetActive(true);
        playerClaimWinButton.gameObject.SetActive(true);
        playerWinDataText.gameObject.SetActive(true);
        if (playerHandCards.Count > 0)
        {
            playerKoiKoiButton.gameObject.SetActive(true);
        }
    }

    void DisablePlayerWinScreen()
    {
        playerWin = false;
        playerWinText.gameObject.SetActive(false);
        tempPlayerPointsText.gameObject.SetActive(false);
        playerClaimWinButton.gameObject.SetActive(false);
        playerKoiKoiButton.gameObject.SetActive(false);
        playerWinDataText.gameObject.SetActive(false);
    }

    public void PlayerWin()
    {
        playerScore += playerTempScore;
        playerScoreText.text = "Player Score: " + playerScore.ToString();
        DisablePlayerWinScreen();
        if (currentRound < 11)
        {
            NewGame();
        }
        else
        {
            gameOverText.gameObject.SetActive(true);
            newGameButton.SetActive(true);
        }
    }

    public void PlayerKoiKoi()
    {
        previousPlayerTempScore = CheckYakuPlayer();
        playerKoiKoi = true;
        playerWin = false;
        DisablePlayerWinScreen();
        playerTempScore = 0;
        tempPlayerPointsText.text = "Winning points: " + playerTempScore.ToString();
        SwitchPlayerTurn();
    }

    int CheckYakuOpponent()
    {
        opponentTempScore = 0;
        string winData = "";
        int winnings = 0;

        // 10 shit cards
        if (opponentShitCards.Count >= 10)
        {
            // 10 shit cards = 1 point + 1 point for each additional
            winnings = opponentShitCards.Count - 9;
            opponentTempScore += winnings;
            winData += $"Shit cards: {winnings}  ";
        }
        // Poetry AND Blue ribbons
        if (opponentRibbonCards.Count(c => c.RibbonName == "Text") >= 3 &&
            opponentRibbonCards.Count(c => c.RibbonName == "Blue") >= 3)
        {
            opponentTempScore += 12;
            winData += "Poetry AND Blue ribbons: 12  ";
            // +1 for each additional ribbon
            if (opponentRibbonCards.Count > 6)
            {
                opponentTempScore += opponentRibbonCards.Count - 6;
                winData += $" +{opponentRibbonCards.Count - 6} for additional ribbons  ";
            }
        }
        // Red poetry ribbons
        else if (opponentRibbonCards.Count(c => c.RibbonName == "Text") >= 3)
        {
            opponentTempScore += 6;
            winData += "Red poetry ribbons: 6  ";
            // +1 for each additional ribbon
            if (opponentRibbonCards.Count > 3)
            {
                opponentTempScore += opponentRibbonCards.Count - 3;
                winData += $" +{opponentRibbonCards.Count - 3} for additional ribbons  ";
            }
        }
        // Blue ribbons
        else if (opponentRibbonCards.Count(c => c.RibbonName == "Blue") >= 3)
        {
            opponentTempScore += 6;
            winData += "Blue ribbons: 6  ";
            // +1 for each additional ribbon
            if (opponentRibbonCards.Count > 3)
            {
                opponentTempScore += opponentRibbonCards.Count - 3;
                winData += $" +{opponentRibbonCards.Count - 3} for additional ribbons  ";
            }
        }
        // 5 ribbon cards
        else if (opponentRibbonCards.Count >= 5)
        {
            // 5 ribbon cards = 1 point + 1 point for each additional
            winnings = opponentRibbonCards.Count - 4;
            opponentTempScore += winnings;
            winData += $"Ribbon cards: {winnings}  ";
        }
        // Ino-shika-cho
        bool hasBoar = opponentAnimalCards.Any(c => c.AnimalName == "Boar");
        bool hasDeer = opponentAnimalCards.Any(c => c.AnimalName == "Deer");
        bool hasButterfly = opponentAnimalCards.Any(c => c.AnimalName == "Butterfly");

        if (hasBoar && hasDeer && hasButterfly)
        {
            opponentTempScore += 5;
            winData += "Ino-shika-cho: 5  ";
            if (opponentAnimalCards.Count > 3)
            {
                opponentTempScore += opponentAnimalCards.Count - 3;
                winData += $" +{opponentAnimalCards.Count - 3} for additional animals  ";
            }
        }
        else if (opponentAnimalCards.Count >= 5)
        {
            // 5 animal cards = 1 point + 1 point for each additional
            winnings = opponentAnimalCards.Count - 4;
            opponentTempScore += winnings;
            winData += $"Animal cards: {winnings}  ";
        }
        // Brights
        int brightCount = opponentBrightCards.Count;
        if (brightCount >= 5)
        {
            opponentTempScore += 15;
            winData += "Five Brights: 15  ";
        }
        else if (brightCount == 4 && !opponentBrightCards.Any(c => c.BrightName == "RainMan"))
        {
            opponentTempScore += 10;
            winData += "Four Brights (no RainMan): 10  ";
        }
        else if (brightCount == 4 && opponentBrightCards.Any(c => c.BrightName == "RainMan"))
        {
            opponentTempScore += 8;
            winData += "Four Brights (with RainMan): 8  ";
        }
        else if (brightCount == 3 && !opponentBrightCards.Any(c => c.BrightName == "RainMan"))
        {
            opponentTempScore += 6;
            winData += "Three Brights (no RainMan): 6  ";
        }
        // Flower/Moon viewing
        bool hasSakura = opponentBrightCards.Any(c => c.BrightName == "Sakura");
        bool hasMoon = opponentBrightCards.Any(c => c.BrightName == "Moon");
        bool hasSakeCup = opponentAnimalCards.Any(c => c.AnimalName == "Sake");
        if (hasMoon && hasSakeCup)
        {
            opponentTempScore += 5;
            winData += "Moon and Sake Cup: 5  ";
        }
        if (hasSakura && hasSakeCup)
        {
            opponentTempScore += 5;
            winData += "Sakura and Sake Cup: 5  ";
        }
        // Cards of the month
        String currentRoundMonth = CurrentRoundSwitch(currentRound);
        if (opponentMatchedCards.Count(c => c.MonthName == currentRoundMonth) >= 4)
        {
            opponentTempScore += 4;
            winData += "Cards of the month: 4  ";
        }
        // Double points for 7 or more
        if (opponentTempScore >= 7)
        {
            winData += $"\n>=7 points, doubled: {opponentTempScore} * 2 = {opponentTempScore * 2}  ";
            opponentTempScore *= 2; // double points for 7 or more
        }
        if (previousOpponentTempScore == 0 && opponentTempScore > 0)
        {
            previousOpponentTempScore = opponentTempScore;
            tempOpponentPointsText.text = "Winning points: " + opponentTempScore.ToString();
        }
        opponentWinDataText.text = winData;
        return opponentTempScore;
    }

    void CheckYakuOpponentKoiKoi()
    {
        Debug.Log("Entered CheckYakuOpponentKoiKoi");
        int score = CheckYakuOpponent();
        if (score > previousOpponentTempScore)
        {
            Debug.Log("Opponent has achieved a new Yaku with a score of: " + score);
            previousOpponentTempScore = score;
            opponentTempScore = score * 2; // double points for Koi-Koi
            opponentWinDataText.text += $"\nKoi-Koi! Points doubled to: {opponentTempScore}";
            tempOpponentPointsText.text = "Winning points: " + opponentTempScore.ToString();
            ShowOpponentWinScreen();
        }
        else
        {
            Debug.Log("Opponent did not achieve a new Yaku during Koi-Koi.");
            if (playerHandCards.Count == 0 && opponentHandCards.Count == 0 && !playerWin && !opponentWin)
            {
                ExhuastiveDraw();
            }
            else
            {
                SwitchPlayerTurn();
            }
        }
    }

    void ShowOpponentWinScreen()
    {
        opponentWin = true;
        opponentWinText.gameObject.SetActive(true);
        tempOpponentPointsText.gameObject.SetActive(true);
        opponentClaimWinButton.gameObject.SetActive(true);
        opponentWinDataText.gameObject.SetActive(true);
        if (opponentHandCards.Count > 0)
        {
            opponentKoiKoiButton.gameObject.SetActive(true);
        }
    }

    void DisableOpponentWinScreen()
    {
        opponentWin = false;
        opponentWinText.gameObject.SetActive(false);
        tempOpponentPointsText.gameObject.SetActive(false);
        opponentClaimWinButton.gameObject.SetActive(false);
        opponentKoiKoiButton.gameObject.SetActive(false);
        opponentWinDataText.gameObject.SetActive(false);
    }

    public void OpponentWin()
    {
        opponentScore += opponentTempScore;
        opponentScoreText.text = "Opponent Score: " + opponentScore.ToString();
        DisableOpponentWinScreen();
        if (currentRound < 11)
        {
            NewGame();
        }
        else
        {
            gameOverText.gameObject.SetActive(true);
            newGameButton.SetActive(true);
        }
    }

    public void OpponentKoiKoi()
    {
        previousOpponentTempScore = CheckYakuOpponent();
        opponentKoiKoi = true;
        opponentWin = false;
        DisableOpponentWinScreen();
        opponentTempScore = 0;
        tempOpponentPointsText.text = "Winning points: " + opponentTempScore.ToString();
        SwitchPlayerTurn();
    }
    
    void ExhuastiveDraw()
    {
        Debug.Log("Exhaustive Draw! No more cards in hands.");
        NewGame();
    }

}
