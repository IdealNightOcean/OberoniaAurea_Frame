using RimWorld;
using Verse;

namespace OberoniaAurea_Frame;

public class IncidentWorker_GiveQuest_Map : RimWorld.IncidentWorker_GiveQuest_Map
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return parms.questScriptDef is not null && base.CanFireNowSub(parms);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (parms.questScriptDef is null)
        {
            Log.Error($"parms.questScriptDef is null in {nameof(IncidentWorker_GiveQuest_Map)}.");
            return false;
        }

        return base.TryExecuteWorker(parms);
    }
}
