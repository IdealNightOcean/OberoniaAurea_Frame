using RimWorld;

namespace OberoniaAurea_Frame;

public interface IQuestAssociate
{
    Quest AssociatedQuest { get; }
    void SetAssociatedQuest(Quest quest);
}
