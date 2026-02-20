using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// NPC/아이템 근처에서 표시되는 인터랙션 프롬프트.
/// 전달 가능 시 "전달하기" 버튼을 콜백 기반으로 함께 표시합니다.
/// </summary>
public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private Button interactButton;
    [SerializeField] private Button deliverButton;

    private IInteractable owner;
    private Camera mainCam;
    private Action onDeliverCallback;

    private void Awake()
    {
        owner = GetComponentInParent<IInteractable>();
        mainCam = Camera.main;
        Hide();
    }

    public void Show()
    {
        promptRoot.SetActive(true);
        SetDeliverOption(false);
    }

    public void Hide()
    {
        promptRoot.SetActive(false);
        onDeliverCallback = null;
    }

    /// <summary>
    /// 전달하기 버튼 표시 여부를 설정합니다. show=true일 때 콜백을 등록하면 클릭 시 호출됩니다.
    /// </summary>
    public void SetDeliverOption(bool show, Action callback = null)
    {
        onDeliverCallback = callback;
        if (deliverButton != null)
            deliverButton.gameObject.SetActive(show);
    }

    private void OnEnable()
    {
        interactButton.onClick.AddListener(OnButtonClicked);
        if (deliverButton != null)
            deliverButton.onClick.AddListener(OnDeliverClicked);
    }

    private void OnDisable()
    {
        interactButton.onClick.RemoveListener(OnButtonClicked);
        if (deliverButton != null)
            deliverButton.onClick.RemoveListener(OnDeliverClicked);
    }

    private void OnButtonClicked()
    {
        owner?.Interact(PlayerCharacter.Instance);
        Hide();
    }

    private void OnDeliverClicked()
    {
        onDeliverCallback?.Invoke();
        Hide();
    }

    private void LateUpdate()
    {
        if (!promptRoot.activeSelf || mainCam == null) return;

        promptRoot.transform.forward = mainCam.transform.forward;
    }
}
