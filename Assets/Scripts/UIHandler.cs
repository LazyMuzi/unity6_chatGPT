using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    private AIService _chatHelper;
    public TMP_InputField inputField;
    public TMP_Text text;
    public Button submitButton;

    private bool _loaded = false;
    private Coroutine _corLoadingAnim;

    private IEnumerator CorLoadingAnim()
    {
        var loadingText = ".";
        var dots = new List<string> { ".", "..", "..." };
        while(!_loaded)
        {
            text.text = loadingText;
            loadingText = dots[(dots.IndexOf(loadingText) + 1) % dots.Count];
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ChatResponse(string response)
    {
        _loaded = true;
        StopCoroutine(_corLoadingAnim);

        text.text = response;
    }
}
