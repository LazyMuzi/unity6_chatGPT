using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private Button interactButton;

    private IInteractable owner;
    private Camera mainCam;

    private void Awake()
    {
        owner = GetComponentInParent<IInteractable>();
        mainCam = Camera.main;
        Hide();
    }

    public void Show()
    {
        promptRoot.SetActive(true);
    }

    public void Hide()
    {
        promptRoot.SetActive(false);
    }

    private void OnEnable()
    {
        interactButton.onClick.AddListener(OnButtonClicked);
    }

    private void OnDisable()
    {
        interactButton.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        owner?.Interact(PlayerCharacter.Instance);
        Hide();
    }

    private void LateUpdate()
    {
        if (!promptRoot.activeSelf || mainCam == null) return;

        promptRoot.transform.forward = mainCam.transform.forward;
    }
}
