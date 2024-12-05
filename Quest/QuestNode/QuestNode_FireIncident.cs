using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

public class QuestNode_FireIncident : QuestNode
{
    [NoTranslate]
    public SlateRef<string> inSignal;

    public SlateRef<IncidentDef> incidentDef;
    public SlateRef<IncidentParms> parms;
    public SlateRef<bool> worldIncident;
    public SlateRef<MapParent> mapParent;

    public SlateRef<float> minPoints = -1f;
    public SlateRef<float> maxPoints = 99999f;
    public SlateRef<float> currentPointsFactor = 1f;

    protected override bool TestRunInt(Slate slate)
    {
        if (incidentDef.GetValue(slate) == null)
        {
            return false;
        }
        if (minPoints.GetValue(slate) > maxPoints.GetValue(slate))
        {
            return false;
        }
        if (currentPointsFactor.GetValue(slate) <= 0f)
        {
            return false;
        }
        return true;
    }
    protected virtual IncidentParms ResolveParms(Slate slate)
    {
        return parms.GetValue(slate);
    }
    protected bool ResolveIncidentTarget(Slate slate)
    {
        if (!worldIncident.GetValue(slate))
        {
            if (mapParent.GetValue(slate) != null)
            {
                return true;
            }
            else if (parms.GetValue(slate) != null)
            {
                return parms.GetValue(slate).target is Map;
            }
            else
            {
                return false;
            }
        }
        return true;
    }
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if(!ResolveIncidentTarget(slate))
        {
            return;
        }
        IncidentParms incidentParms = ResolveParms(slate);
        QuestPart_FireIncident questPart_FireIncident = new()
        {
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal"),
            incident = incidentDef.GetValue(slate),
            minPoints = minPoints.GetValue(slate),
            maxPoints = maxPoints.GetValue(slate),
            currentPointsFactor = currentPointsFactor.GetValue(slate),
        };
        if (worldIncident.GetValue(slate))
        {
            questPart_FireIncident.SetIncidentParms_World(incidentParms);
        }
        else
        {
            questPart_FireIncident.SetIncidentParms_MapParent(incidentParms, mapParent.GetValue(slate));
        }
        QuestGen.quest.AddPart(questPart_FireIncident);
    }
}
