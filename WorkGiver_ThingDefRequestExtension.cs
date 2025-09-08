using RimWorld;
using Verse;

namespace OberoniaAurea_Frame;

public class WorkGiver_ThingDefRequestExtension : DefModExtension
{
    public ThingDef thingDef;
    public JobDef jobDef;
}

public class WorkGiver_ThingDefScanner : WorkGiver_Scanner
{
    private WorkGiver_ThingDefRequestExtension workThingDefRequest;
    protected WorkGiver_ThingDefRequestExtension WorkThingDefRequest => workThingDefRequest ??= def.GetModExtension<WorkGiver_ThingDefRequestExtension>();

    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(WorkThingDefRequest.thingDef);
}