using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

/// <summary>
/// NPC와의 대화 UI를 관리하며 열림/닫힘 이벤트를 발행합니다.
/// </summary>
public class ChatUI : MonoBehaviour
{
    public static ChatUI Instance;
    public event Action Opened;
    public event Action Closed;

    [Header("UI References")]
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Text dialogueText;
    [SerializeField] private InputField inputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button closeButton;

    [Header("Quest UI")]
    [SerializeField] private GameObject questButtonGroup;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button rejectButton;

    [Header("Farewell")]
    [SerializeField] private float farewellDelay = 1.5f;

    private RectTransform contentRect;
    private NPCCharacter currentNPC;
    private bool waitingForResponse;
    private int currentTurn;
    private int maxTurns;
    private bool isQuestProposalMode;

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

        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAcceptQuest);
        if (rejectButton != null)
            rejectButton.onClick.AddListener(OnRejectQuest);
    }

    private void OnDisable()
    {
        sendButton.onClick.RemoveListener(OnSendClicked);
        closeButton.onClick.RemoveListener(Close);

        if (acceptButton != null)
            acceptButton.onClick.RemoveListener(OnAcceptQuest);
        if (rejectButton != null)
            rejectButton.onClick.RemoveListener(OnRejectQuest);
    }

    public void Open(NPCCharacter npc, string npcName)
    {
        currentNPC = npc;
        currentTurn = 0;
        maxTurns = npc.brain.relationship.GetMaxTurns();
        waitingForResponse = true;
        isQuestProposalMode = false;

        SetChatPanelActive(true);
        dialogueText.text = $"<b>{npcName}</b>: ...";

        SetChatInputVisible(false);
        SetQuestButtonsVisible(false);
        ScrollToBottom();
    }

    /// <summary>
    /// 일반 인사 표시.
    /// </summary>
    public void UpdateGreeting(string npcName, string greeting)
    {
        dialogueText.text = $"<b>{npcName}</b>: {greeting}";
        waitingForResponse = false;

        SetChatInputVisible(true);
        ScrollToBottom();
    }

    /// <summary>
    /// 인사 대신 퀘스트 제안 메시지를 표시하고 수락/거절 버튼을 보여줍니다.
    /// </summary>
    public void OpenQuestProposal(string npcName, string proposalMessage)
    {
        isQuestProposalMode = true;
        dialogueText.text = $"<b>{npcName}</b>: {proposalMessage}";
        waitingForResponse = false;

        SetChatInputVisible(false);
        SetQuestButtonsVisible(true);
        ScrollToBottom();
    }

    /// <summary>
    /// 프롬프트 UI에서 "전달하기" 버튼을 눌렀을 때 호출.
    /// 퀘스트 완료 메시지 + 보상 정보를 표시하고, 입력창을 열어 일반 대화로 전환할 수 있게 합니다.
    /// </summary>
    public void OpenDeliveryResult(NPCCharacter npc, string npcName, QuestCompletionResult result)
    {
        currentNPC = npc;
        currentTurn = 0;
        maxTurns = npc.brain.relationship.GetMaxTurns();
        waitingForResponse = false;
        isQuestProposalMode = false;

        SetChatPanelActive(true);

        string rewardLine = $"\n\n<color=#FFD700>★ 퀘스트 완료! 친밀도 +{result.affinityReward}</color>";
        dialogueText.text = $"<b>{npcName}</b>: {result.completionMessage}{rewardLine}";

        SetChatInputVisible(true);
        SetQuestButtonsVisible(false);
        ScrollToBottom();
    }

    public void Close()
    {
        SetChatPanelActive(false);
        inputField.text = "";

        SetQuestButtonsVisible(false);

        if (currentNPC != null)
        {
            if (currentTurn > 0)
                currentNPC.brain.OnConversationEnd();

            currentNPC.OnInteractionEnd();
            currentNPC = null;
        }

        waitingForResponse = false;
        isQuestProposalMode = false;

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
            SetChatInputVisible(false);
            StartCoroutine(ShowFarewellAfterDelay(npcName));
        }
        else
        {
            sendButton.interactable = true;
            inputField.ActivateInputField();
        }

        ScrollToBottom();
    }

    private IEnumerator ShowFarewellAfterDelay(string npcName)
    {
        yield return new WaitForSeconds(farewellDelay);

        if (currentNPC != null)
        {
            string farewell = currentNPC.brain.relationship.GetFarewell();
            dialogueText.text += $"\n<b>{npcName}</b>: {farewell}";
            ScrollToBottom();
        }
    }

    #region Quest Buttons

    private void OnAcceptQuest()
    {
        if (currentNPC == null) return;

        var handler = currentNPC.brain.questHandler;
        if (handler != null)
            handler.ActivateQuest();

        Close();
    }

    private void OnRejectQuest()
    {
        if (currentNPC == null) return;

        isQuestProposalMode = false;
        SetQuestButtonsVisible(false);

        string npcName = currentNPC.brain.profile.npcName;
        string greeting = currentNPC.GetTimeAwareGreeting();

        UpdateGreeting(npcName, greeting);
    }

    #endregion

    #region UI Helpers

    private void SetChatInputVisible(bool visible)
    {
        inputField.gameObject.SetActive(visible);
        sendButton.gameObject.SetActive(visible);

        if (visible)
        {
            inputField.interactable = true;
            sendButton.interactable = true;
            inputField.ActivateInputField();
        }
    }

    private void SetQuestButtonsVisible(bool visible)
    {
        if (questButtonGroup != null)
            questButtonGroup.SetActive(visible);
    }

    private void SetChatPanelActive(bool active)
    {
        bool wasActive = chatPanel.activeSelf;
        chatPanel.SetActive(active);

        if (!wasActive && active)
        {
            Opened?.Invoke();
        }
        else if (wasActive && !active)
        {
            Closed?.Invoke();
        }
    }


    #endregion

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
        if (isQuestProposalMode) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnSendClicked();
        }
    }
}
