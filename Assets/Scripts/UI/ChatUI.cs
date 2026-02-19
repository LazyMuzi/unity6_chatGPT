using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChatUI : MonoBehaviour
{
    public static ChatUI Instance;

    [Header("UI References")]
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Text dialogueText;
    [SerializeField] private InputField inputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button closeButton;

    private RectTransform contentRect;
    private NPCCharacter currentNPC;
    private bool waitingForResponse;
    private int currentTurn;
    private int maxTurns;

    private void Awake()
    {
        Instance = this;
        contentRect = scrollRect.content;
        chatPanel.SetActive(false);
    }

    private void OnEnable()
    {
        sendButton.onClick.AddListener(OnSendClicked);
        closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        sendButton.onClick.RemoveListener(OnSendClicked);
        closeButton.onClick.RemoveListener(Close);
    }

    public void Open(NPCCharacter npc, string npcName)
    {
        currentNPC = npc;
        currentTurn = 0;
        maxTurns = npc.brain.relationship.GetMaxTurns();
        waitingForResponse = true;

        chatPanel.SetActive(true);
        dialogueText.text = $"<b>{npcName}</b>: ...";

        inputField.interactable = false;
        sendButton.interactable = false;
        ScrollToBottom();
    }

    public void UpdateGreeting(string npcName, string greeting)
    {
        dialogueText.text = $"<b>{npcName}</b>: {greeting}";
        waitingForResponse = false;

        inputField.interactable = true;
        sendButton.interactable = true;
        inputField.ActivateInputField();
        ScrollToBottom();
    }

    public void Close()
    {
        chatPanel.SetActive(false);
        inputField.text = "";

        if (currentNPC != null)
        {
            if (currentTurn > 0)
                currentNPC.brain.OnConversationEnd();

            currentNPC.OnInteractionEnd();
            currentNPC = null;
        }

        waitingForResponse = false;

        CameraManager.Instance.EndInteraction();
        PlayerCharacter.Instance.IsInputLocked = false;
    }

    public bool IsOpen => chatPanel.activeSelf;

    private void OnSendClicked()
    {
        if (waitingForResponse) return;
        if (string.IsNullOrWhiteSpace(inputField.text)) return;
        if (currentNPC == null) return;

        string message = inputField.text.Trim();
        inputField.text = "";

        dialogueText.text += $"\n<b>나</b>: {message}";
        ScrollToBottom();

        currentTurn++;
        waitingForResponse = true;
        sendButton.interactable = false;

        currentNPC.SendPlayerMessage(message);
    }

    public void AddNPCResponse(string npcName, string response)
    {
        dialogueText.text += $"\n<b>{npcName}</b>: {response}";
        waitingForResponse = false;

        if (currentTurn >= maxTurns)
        {
            string farewell = currentNPC.brain.relationship.GetFarewell();
            dialogueText.text += $"\n<b>{npcName}</b>: {farewell}";
            inputField.interactable = false;
            sendButton.interactable = false;
        }
        else
        {
            sendButton.interactable = true;
            inputField.ActivateInputField();
        }

        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        StartCoroutine(ScrollToBottomRoutine());
    }

    private IEnumerator ScrollToBottomRoutine()
    {
        yield return null;

        float preferredHeight = dialogueText.preferredHeight;
        dialogueText.rectTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical, preferredHeight);
        contentRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical, preferredHeight);

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void Update()
    {
        if (!chatPanel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnSendClicked();
        }
    }
}
