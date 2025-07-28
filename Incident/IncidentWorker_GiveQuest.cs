using RimWorld;
using Verse;

namespace OberoniaAurea_Frame;

public class IncidentWorker_GiveQuest : RimWorld.IncidentWorker_GiveQuest
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return parms.questScriptDef is not null && base.CanFireNowSub(parms);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (parms.questScriptDef is null)
        {
            Log.Error($"parms.questScriptDef is null in {nameof(IncidentWorker_GiveQuest)}.");
            return false;
        }

        return base.TryExecuteWorker(parms);
    }
}