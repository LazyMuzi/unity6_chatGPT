using UnityEngine;

/// <summary>
/// 게임 내 아이템을 정의하는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(menuName = "Item/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemId;
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
}
