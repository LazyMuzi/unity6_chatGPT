using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI 상태머신 전이를 관리하는 루트 매니저입니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private ChatUI chatUI;
    [SerializeField] private GameObject playerControllerUIRoot;
    [SerializeField] private InventoryUI inventoryUI;

    [Header("Debug Keys")]
    [SerializeField] private bool useKeyboardToggle = true;
    [SerializeField] private KeyCode inventoryToggleKey = KeyCode.I;

    private readonly Dictionary<UIStateType, IUIState> states = new Dictionary<UIStateType, IUIState>();
    private IUIState currentState;
    public UIStateType CurrentStateType => currentState?.StateType ?? UIStateType.Gameplay;

    private void Awake()
    {
        Instance = this;

        UIStateContext context = new UIStateContext(
            playerControllerUIRoot,
            inventoryUI);

        states[UIStateType.Gameplay] = new GameplayUIState(context);
        states[UIStateType.Chat] = new ChatUIState(context);
        states[UIStateType.Inventory] = new InventoryUIState(context);
    }

    private void OnEnable()
    {
        if (chatUI != null)
        {
            chatUI.Opened += OnChatOpened;
            chatUI.Closed += OnChatClosed;
        }
    }

    private void Start()
    {
        if (chatUI == null)
        {
            Debug.LogWarning("[UIManager] ChatUI reference is missing.");
        }

        if (chatUI != null && chatUI.IsOpen)
        {
            ChangeState(UIStateType.Chat);
            return;
        }

        ChangeState(UIStateType.Gameplay);
    }

    private void Update()
    {
        if (!useKeyboardToggle || chatUI == null || chatUI.IsOpen)
        {
            return;
        }

        if (Input.GetKeyDown(inventoryToggleKey))
        {
            ToggleInventory();
        }
    }

    private void OnDisable()
    {
        if (chatUI != null)
        {
            chatUI.Opened -= OnChatOpened;
            chatUI.Closed -= OnChatClosed;
        }
    }

    private void OnChatOpened()
    {
        ChangeState(UIStateType.Chat);
    }

    private void OnChatClosed()
    {
        ChangeState(UIStateType.Gameplay);
    }

    public void ToggleInventory()
    {
        if (chatUI != null && chatUI.IsOpen)
        {
            return;
        }

        UIStateType next = CurrentStateType == UIStateType.Inventory
            ? UIStateType.Gameplay
            : UIStateType.Inventory;

        ChangeState(next);
    }

    public void ChangeState(UIStateType newStateType)
    {
        if (!states.TryGetValue(newStateType, out IUIState nextState))
        {
            return;
        }

        if (currentState != null && currentState.StateType == newStateType)
        {
            return;
        }

        currentState?.Exit();
        currentState = nextState;
        currentState.Enter();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
