using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// NPC 머리 위에 표시되는 말풍선 UI. World Space Canvas 하위에 배치합니다.
/// </summary>
public class SpeechBubbleUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text messageText;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine activeRoutine;

    private void Awake()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void LateUpdate()
    {
        if (canvasGroup != null && canvasGroup.alpha > 0f && Camera.main != null)
            transform.forward = Camera.main.transform.forward;
    }

    public void Show(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        if (canvasGroup == null || messageText == null) return;

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(ShowRoutine(message));
    }

    private IEnumerator ShowRoutine(string message)
    {
        messageText.text = message;

        yield return FadeCanvasGroup(0f, 1f);
        yield return new WaitForSeconds(displayDuration);
        yield return FadeCanvasGroup(1f, 0f);

        activeRoutine = null;
    }

    private IEnumerator FadeCanvasGroup(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
