using UnityEngine;

[CreateAssetMenu(menuName = "NPC/Profile")]
public class NPCProfile : ScriptableObject
{
    public string npcId;
    public string npcName;
    [TextArea] public string personality;
    [TextArea] public string background;
    [TextArea] public string speechStyle;
}