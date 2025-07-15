using RimWorld;

namespace OberoniaAurea_Frame;

public interface IQuestAssociate
{
    public Quest AssociatedQuest { get; }
    public void SetAssociatedQuest(Quest quest);
}
