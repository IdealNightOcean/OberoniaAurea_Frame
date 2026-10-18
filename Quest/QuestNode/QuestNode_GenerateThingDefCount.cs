using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 任务节点：生成物品的<see cref="ThingDef"/>及数量。 
/// </summary>
public class QuestNode_GenerateThingDefCount : QuestNode
{
    /// <summary>
    /// 需要生成的物品 Def。
    /// </summary>
    public SlateRef<ThingDef> thingDef;
    /// <summary>
    /// 需要生成的物品数量。
    /// </summary>
    public SlateRef<int> count;

    /// <summary>
    /// 结果是否以 <see cref="ThingDefCountClass"/> 形式存储。
    /// </summary>
    public SlateRef<bool> storeAsClass;
    /// <summary>
    /// 存储标签：生成的物品Def及数量。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> storeAs;
    /// <summary>
    /// 存储标签：结果要追加到的列表。
    /// </summary>
    [NoTranslate]
    public SlateRef<string> addToList;

    /// <inheritdoc/>
    protected override bool TestRunInt(Slate slate)
    {
        return thingDef.GetValue(slate) is not null && count.GetValue(slate) > 0;
    }
    /// <inheritdoc/>
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if (storeAsClass.GetValue(slate))
        {
            ThingDefCountClass thingDefCountClass = new(thingDef.GetValue(slate), count.GetValue(slate));
            if (storeAs.GetValue(slate) is not null)
            {
                QuestGen.slate.Set(storeAs.GetValue(slate), thingDefCountClass);
            }
            if (addToList.GetValue(slate) is not null)
            {
                QuestGenUtility.AddToOrMakeList(slate, addToList.GetValue(slate), thingDefCountClass);
            }
        }
        else
        {
            ThingDefCount thingDefCount = new(thingDef.GetValue(slate), count.GetValue(slate));
            if (storeAs.GetValue(slate) is not null)
            {
                QuestGen.slate.Set(storeAs.GetValue(slate), thingDefCount);
            }
            if (addToList.GetValue(slate) is not null)
            {
                QuestGenUtility.AddToOrMakeList(slate, addToList.GetValue(slate), thingDefCount);
            }
        }
    }
}