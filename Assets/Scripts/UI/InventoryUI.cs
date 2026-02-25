using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 패널 표시와 목록 텍스트 렌더링을 담당합니다.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Text inventoryText;
    [SerializeField] private Button closeButton;
    [SerializeField] private string emptyMessage = "인벤토리가 비어 있습니다.";

    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    private void Awake()
    {
        SetVisible(false);
    }

    private void OnEnable()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }
    }

    private void OnDisable()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseClicked);
        }
    }

    public void SetVisible(bool visible)
    {
        if (panelRoot == null)
        {
            return;
        }

        panelRoot.SetActive(visible);
        if (visible)
        {
            RefreshView();
        }
    }

    public void RefreshView()
    {
        if (inventoryText == null)
        {
            return;
        }

        if (PlayerInventory.Instance == null)
        {
            inventoryText.text = emptyMessage;
            return;
        }

        IReadOnlyDictionary<string, int> snapshot = PlayerInventory.Instance.GetSnapshot();
        if (snapshot.Count == 0)
        {
            inventoryText.text = emptyMessage;
            return;
        }

        StringBuilder builder = new StringBuilder();
        foreach (KeyValuePair<string, int> pair in snapshot)
        {
            builder.Append("- ")
                .Append(pair.Key)
                .Append(" x")
                .Append(pair.Value)
                .Append('\n');
        }

        inventoryText.text = builder.ToString();
    }

    private void OnCloseClicked()
    {
        UIManager.Instance?.ChangeState(UIStateType.Gameplay);
    }
}
