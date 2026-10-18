using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：生成可交互的世界对象。 
/// </summary>
public class QuestNode_GenerateInteractiveWorldObject : QuestNode
{
    /// <summary>
    /// 待生成世界对象的 Def。
    /// </summary>
    public SlateRef<WorldObjectDef> def;
    /// <summary>
    /// 生成的世界对象所处地块。
    /// </summary>
    public SlateRef<PlanetTile> tile;
    /// <summary>
    /// 关联的派系（为空时不设置）。
    /// </summary>
    public SlateRef<Faction> faction;

    /// <summary>
    /// 存储标签：生成的交互世界对象。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> storeAs;

    /// <inheritdoc/>
    protected override bool TestRunInt(Slate slate)
    {
        return def.GetValue(slate) is not null;
    }

    /// <inheritdoc/>
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        WorldObject_InteractiveBase worldObject = GenerateWorldObject(slate);
        worldObject.Tile = tile.GetValue(slate);
        worldObject.SetAssociatedQuest(QuestGen.quest);
        if (faction.GetValue(slate) is not null)
        {
            worldObject.SetFaction(faction.GetValue(slate));
        }
        if (storeAs.GetValue(slate) is not null)
        {
            slate.Set(storeAs.GetValue(slate), worldObject);
        }
    }

    /// <summary>
    /// 根据 <see cref="def"/> 生成对应类型的可交互世界对象。
    /// </summary>
    /// <param name="slate">任务 Slate 数据。</param>
    /// <returns>生成的可交互世界对象。</returns>
    protected virtual WorldObject_InteractiveBase GenerateWorldObject(Slate slate)
    {
        return (WorldObject_InteractiveBase)WorldObjectMaker.MakeWorldObject(def.GetValue(slate));
    }
}