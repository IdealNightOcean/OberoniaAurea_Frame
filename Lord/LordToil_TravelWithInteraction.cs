using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace OberoniaAurea_Frame;

/// <summary>
/// 带有交互功能的旅行职责。
/// </summary>
public class LordToil_TravelWithInteraction : LordToil_Travel
{
    /// <summary>
    /// 根据旅行目的地初始化带有交互功能的旅行职责。
    /// </summary>
    /// <param name="dest">旅行目的地。</param>
    public LordToil_TravelWithInteraction(IntVec3 dest) : base(dest) { }

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