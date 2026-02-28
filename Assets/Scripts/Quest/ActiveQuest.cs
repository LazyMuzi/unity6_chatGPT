/// <summary>
/// 진행 중인 퀘스트의 런타임 상태. 템플릿(QuestData)과 실행 상태를 분리합니다.
/// </summary>
public class ActiveQuest
{
    public QuestData data;

    public ActiveQuest(QuestData data)
    {
        this.data = data;
    }
}
