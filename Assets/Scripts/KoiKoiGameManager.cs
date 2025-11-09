using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using Unity.VisualScripting;
using UnityEngine;

public class KoiKoiGameManager : MonoBehaviour {
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

    void Start() {
        CardData[] cardDataArray = Resources.LoadAll<CardData>("GameData/CardData");
        if (stackAnchor == null) {
            stackAnchor = transform;
        }

        for (int i = 0; i < 48; i++) {
            Card card = Instantiate(cardPrefab, stackAnchor.position, stackAnchor.rotation);
            deck.AddCard(card);
            deckCards.Add(card);
            Debug.Log($"Loaded Card Data: {cardDataArray[i].name}");
            card.LoadCardData(cardDataArray[i]);
            card.gameObject.name = card.MonthName + card.AnimalName + card.BrightName + card.RibbonName;
        }

        CreateDeckOnScreen();
        DealCards();
        // MoveCards();
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
            // card.GetComponent<Rigidbody>().isKinematic = true;
            card.GetComponent<Rigidbody>().freezeRotation = true;
            card.GetComponent<Rigidbody>().isKinematic = true;
            // card.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    public void DealCards()
    {
        StartCoroutine(DealCardsCoroutine());
    }

    private IEnumerator DealCardsCoroutine()
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
        yield return new WaitForSeconds(0.5f);
        playerTurn = true;
        CheckHands();
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
        // Four of a kind
        // Four pairs
        // Instant win
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
                    Debug.Log($"Match found! Player {selectedCard.MonthName} matches center {centerCard.MonthName}");
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
                    Debug.Log($"Comparing Player Card: {playerCard.MonthName} with Center Card: {centerCard.MonthName}");
                    if (centerCard == null) continue;

                    if (playerCard.MonthName == centerCard.MonthName)
                    {
                        canMatch = true;
                        Debug.Log($"Match found! Player {playerCard.MonthName} matches center {centerCard.MonthName}");
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
                    Debug.Log($"Comparing Opponent Card: {opponentCard.MonthName} with Center Card: {centerCard.MonthName}");
                    if (centerCard == null) continue;

                    if (opponentCard.MonthName == centerCard.MonthName)
                    {
                        canMatch = true;
                        Debug.Log($"Match found! Opponent {opponentCard.MonthName} matches center {centerCard.MonthName}");
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
        StartCoroutine(MoveRigidbodyToPosition(selectedCard.GetComponent<Rigidbody>(), 
        new Vector3(centerCard.transform.position.x, cardRestingHeight + 0.002f, 
        centerCard.transform.position.z - 0.01f),
        selectedCard.transform.rotation,
        0.1f));
        DeactivateParticlesOnCards();
        playerHandCards.Remove(selectedCard);
        centerCards.Remove(centerCard);
        playerMatchedCards.Add(selectedCard);
        playerMatchedCards.Add(centerCard);
        tempMatchedCards.Add(selectedCard);
        tempMatchedCards.Add(centerCard);
        selectedCard = null;
        if (timeToDraw)
        {
            ClaimMatches();
            RestructurePlayerHand();
        }
        timeToDraw = true;
        HideMarkers();
    }

    public void MoveCardToPlaceholder(Vector3 position)
    {
        StartCoroutine(MoveRigidbodyToPosition(selectedCard.GetComponent<Rigidbody>(), 
        new Vector3(position.x, cardRestingHeight, position.z), 
        selectedCard.transform.rotation, 0.1f));
        DeactivateParticlesOnCards();
        HideMarkers();
        selectedCard = null;
        if (timeToDraw)
        {
            ClaimMatches();
            RestructurePlayerHand();
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
        CheckHands();
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
            if (card.IsBright)
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
            else if (card.IsAnimal)
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
            else if (card.IsRibbon)
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
            else
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
        }
        tempMatchedCards.Clear();
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
}
