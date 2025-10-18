using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardDisplayDebug : MonoBehaviour {
    [SerializeField] private Card cardPrefab;
    [SerializeField] private float offset_x = 0f;
    [SerializeField] private float offset_y = 0f;
    [SerializeField, Range(0.005f, 0.1f)] private float verticalSpacing = 0.06f;
    [SerializeField, Range(0.005f, 0.1f)] private float horizontalSpacing = 0.03f;
    [SerializeField] private Transform stackAnchor;
    [SerializeField, Range(0f, 0.01f)] private float stackHeightStep = 0.004f;
    [SerializeField] private Deck deck;

    void Start() {
        if (stackAnchor == null) {
            stackAnchor = transform;
        }

        for (int i = 0; i < 48; i++) {
            Card card = Instantiate(cardPrefab, stackAnchor.position, stackAnchor.rotation);
            deck.AddCard(card);
            card.SetBinaryData(i);
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
        Quaternion baseRotation = Quaternion.Euler(90, 180, 0);
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
            boxCollider.size = new Vector3(0.61f, 1f, 0.06f);
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
        float cardSpacing = 0.04f;
        float moveDuration = 0.1f;
        float delayBetweenCards = 0.1f;
        Vector3 topPlayerDealPosition = new Vector3(0.14f, 0.002f, 0.12f);
        Vector3 bottomPlayerDealPosition = new Vector3(0.14f, 0.002f, -0.12f);
        Vector3 centerDealPositionTop = new Vector3(0.06f, 0.002f, 0.03f);
        Vector3 centerDealPositionBottom = new Vector3(0.06f, 0.002f, -0.03f);

        // Deal two cards at a time, four times
        for (int y = 0; y < 4; y++)
        {
            // Deal top player's cards
            for (int i = 0; i < 2; i++)
            {
                Card card = deck.DrawCard();
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
                Quaternion dealRotation = Quaternion.Euler(-90, 0, 0);
                yield return StartCoroutine(MoveRigidbodyToPosition(rb, topPlayerDealPosition, dealRotation, moveDuration));
                topPlayerDealPosition -= new Vector3(cardSpacing, 0f, 0f);
                yield return new WaitForSeconds(delayBetweenCards);
            }

            // Deal to the center
            for (int i = 0; i < 1; i++)
            {
                Card card = deck.DrawCard();
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
                Quaternion dealRotation = Quaternion.Euler(-90, 180, 0);
                yield return StartCoroutine(MoveRigidbodyToPosition(rb, centerDealPositionTop, dealRotation, moveDuration));
                centerDealPositionTop -= new Vector3(cardSpacing, 0f, 0f);
                yield return new WaitForSeconds(delayBetweenCards);

                Card card2 = deck.DrawCard();
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
                centerDealPositionBottom -= new Vector3(cardSpacing, 0f, 0f);
                yield return new WaitForSeconds(delayBetweenCards);
            }

            // Deal bottom player's cards
            for (int i = 0; i < 2; i++)
            {
                
                Card card = deck.DrawCard();
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
                Quaternion dealRotation = Quaternion.Euler(-90, 180, 0);
                yield return StartCoroutine(MoveRigidbodyToPosition(rb, bottomPlayerDealPosition, dealRotation, moveDuration));
                bottomPlayerDealPosition -= new Vector3(cardSpacing, 0f, 0f);
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
}
