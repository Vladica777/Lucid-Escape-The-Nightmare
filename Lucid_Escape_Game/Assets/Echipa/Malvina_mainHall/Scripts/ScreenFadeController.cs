using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFadeController : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float fadeOutDuration = 0.75f;
    [SerializeField] private float messageHoldDuration = 1.15f;
    [SerializeField] private float fadeInDuration = 0.75f;

    [Header("UI")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private Text messageText;

    public bool IsRunning { get; private set; }

    public void Configure(Image fadeOverlay, Text centerMessage)
    {
        fadeImage = fadeOverlay;
        messageText = centerMessage;
        SetFadeAlpha(0f);

        if (messageText != null)
        {
            messageText.enabled = false;
        }
    }

    public void PlayDoorPreview(string message, MainHallFirstPersonController movement, PlayerInteraction interaction)
    {
        if (!IsRunning)
        {
            StartCoroutine(DoorPreviewRoutine(message, movement, interaction));
        }
    }

    private IEnumerator DoorPreviewRoutine(string message, MainHallFirstPersonController movement, PlayerInteraction interaction)
    {
        IsRunning = true;
        movement?.SetMovementEnabled(false);
        interaction?.SetInteractionsEnabled(false);

        yield return FadeRoutine(0f, 1f, fadeOutDuration);

        if (messageText != null)
        {
            messageText.text = message;
            messageText.enabled = true;
        }

        yield return new WaitForSeconds(messageHoldDuration);

        if (messageText != null)
        {
            messageText.enabled = false;
        }

        yield return FadeRoutine(1f, 0f, fadeInDuration);

        interaction?.SetInteractionsEnabled(true);
        movement?.SetMovementEnabled(true);
        IsRunning = false;
    }

    private IEnumerator FadeRoutine(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            SetFadeAlpha(to);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetFadeAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }

        SetFadeAlpha(to);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null)
        {
            return;
        }

        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }
}
