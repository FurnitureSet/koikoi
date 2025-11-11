using System.Collections;
using UnityEngine;
using TMPro;

public class CameraController : MonoBehaviour
{
    [Header("Camera Views")]
    Vector3 defaultPosition = new Vector3(0f, 0.91f, 0f);
    Vector3 matchedPosition = new Vector3(-0.06f, 0.91f, -0.15f);
    Vector3 opponentMatchedPosition = new Vector3(0.06f, 0.91f, 0.15f);
    Quaternion defaultRotation = Quaternion.Euler(90f, 0f, 0f);
    Quaternion matchedRotation = Quaternion.Euler(90f, 0f, 0f);
    float moveDuration = 0.1f;

    [Header("UI")]
    public TMP_Text buttonText;
    public TMP_Text opponentButtonText;

    private bool showingMatches = false;
    private bool showingOpponentMatches = false;
    private Coroutine moveCoroutine;

    public void ToggleView()
    {
        showingMatches = !showingMatches;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveCamera(showingMatches));

        // Update button label
        if (buttonText != null)
        {
            buttonText.text = showingMatches ? "Return" : "View Matches";
        }
    }

    public void ToggleOpponentView()
    {
        showingOpponentMatches = !showingOpponentMatches;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveCameraToOpponentMatches(showingOpponentMatches));

        // Update button label
        if (opponentButtonText != null)
        {
            opponentButtonText.text = showingOpponentMatches ? "Return" : "Opponent Matches";
        }
    }

    private IEnumerator MoveCamera(bool toMatched)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 endPos = toMatched ? matchedPosition : defaultPosition;
        Quaternion endRot = toMatched ? matchedRotation : defaultRotation;

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);
            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
    }

    private IEnumerator MoveCameraToOpponentMatches(bool toMatched)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 endPos = toMatched ? opponentMatchedPosition : defaultPosition;
        Quaternion endRot = toMatched ? matchedRotation : defaultRotation;

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);
            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
    }
}