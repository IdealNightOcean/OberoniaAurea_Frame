using RimWorld;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// <see href="ThingDefRequest"/>的物品定义请求扩展。
/// </summary>
public class WorkGiver_ThingDefRequestExtension : DefModExtension
{
    /// <summary>
    /// 需要扫描的物品定义。
    /// </summary>
    public ThingDef thingDef;
    /// <summary>
    /// 对应的工作定义。
    /// </summary>
    public JobDef jobDef;
}

/// <summary>
/// 扫描指定物品定义的工作给予者。
/// </summary>
public class WorkGiver_ThingDefScanner : WorkGiver_Scanner
{
    private WorkGiver_ThingDefRequestExtension workThingDefRequest;
    /// <summary>
    /// 获取扫描所需的 <see cref="WorkGiver_ThingDefRequestExtension"/> 扩展。
    /// </summary>
    protected WorkGiver_ThingDefRequestExtension WorkThingDefRequest => workThingDefRequest ??= def.GetModExtension<WorkGiver_ThingDefRequestExtension>();

    /// <inheritdoc/>
    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(WorkThingDefRequest.thingDef);
}