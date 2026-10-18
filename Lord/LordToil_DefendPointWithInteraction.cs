using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace OberoniaAurea_Frame;

/// <summary>
/// 带有交互功能的守卫职责。
/// </summary>
public class LordToil_DefendPointWithInteraction : LordToil_DefendPoint
{
    /// <summary>
    /// 初始化带有交互功能且仅满足基本需求的守卫职责。
    /// </summary>
    /// <param name="canSatisfyLongNeeds">是否允许满足长期需求。</param>
    public LordToil_DefendPointWithInteraction(bool canSatisfyLongNeeds = true) : base(canSatisfyLongNeeds) { }
    /// <summary>
    /// 根据守卫点和半径初始化带有交互功能的守卫职责。
    /// </summary>
    /// <param name="defendPoint">守卫点。</param>
    /// <param name="defendRadius">防御半径。</param>
    /// <param name="wanderRadius">游荡半径。</param>
    public LordToil_DefendPointWithInteraction(IntVec3 defendPoint, float? defendRadius = null, float? wanderRadius = null) : base(defendPoint, defendRadius, wanderRadius) { }

    /// <inheritdoc/>
    public override void DrawPawnGUIOverlay(Pawn pawn)
    {
        if (lord.LordJob is ILordJobWithTalk talkLordJob && talkLordJob.CanTalkWith(pawn))
        {
            pawn.Map.overlayDrawer.DrawOverlay(pawn, OverlayTypes.QuestionMark);
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<FloatMenuOption> ExtraFloatMenuOptions(Pawn target, Pawn forPawn)
    {
        if (lord.LordJob is ILordFloatMenuProvider menuProvider)
        {
            return menuProvider.ExtraFloatMenuOptions(target, forPawn);
        }
        else
        {
            return Enumerable.Empty<FloatMenuOption>();
        }
    }
}