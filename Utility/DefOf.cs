using RimWorld;
using Verse;
namespace OberoniaAurea_Frame;

/// <summary>
/// 框架Def 定义类。
/// </summary>
[DefOf]
public static class OAFrameDefOf
{
    /// <summary>
    /// 附加文本 mote。
    /// </summary>
    public static ThingDef OAFrame_Mote_AttachedText;

    /// <summary>
    /// 固定远行队的默认<see cref="WorldObjectDef"/>。
    /// </summary>
    public static WorldObjectDef OAFrame_FixedCaravan;

    /// <summary>
    /// 给予任务事件定义。
    /// </summary>
    public static IncidentDef OAFrame_GiveQuest;

    /// <summary>
    /// 给予地图任务事件定义。
    /// </summary>
    public static IncidentDef OAFrame_GiveQuest_Map;
}
