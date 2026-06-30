using RimWorld;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// <see cref="ThingDefRequest"/>的物品定义请求扩展。
/// </summary>
public class WorkGiver_ThingDefRequestExtension : DefModExtension
{
    public ThingDef thingDef;
    public JobDef jobDef;
}

/// <summary>
/// 扫描指定物品定义的工作给予者。
/// </summary>
public class WorkGiver_ThingDefScanner : WorkGiver_Scanner
{
    private WorkGiver_ThingDefRequestExtension workThingDefRequest;
    protected WorkGiver_ThingDefRequestExtension WorkThingDefRequest => workThingDefRequest ??= def.GetModExtension<WorkGiver_ThingDefRequestExtension>();

    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(WorkThingDefRequest.thingDef);
}