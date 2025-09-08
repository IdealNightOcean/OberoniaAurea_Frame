using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace OberoniaAurea_Frame;

public class LordToil_DefendPointWithInteraction : LordToil_DefendPoint
{
    public LordToil_DefendPointWithInteraction(bool canSatisfyLongNeeds = true) : base(canSatisfyLongNeeds) { }
    public LordToil_DefendPointWithInteraction(IntVec3 defendPoint, float? defendRadius = null, float? wanderRadius = null) : base(defendPoint, defendRadius, wanderRadius) { }

    public override void DrawPawnGUIOverlay(Pawn pawn)
    {
        if (lord.LordJob is ILordJobWithTalk talkLordJob && talkLordJob.CanTalkWith(pawn))
        {
            pawn.Map.overlayDrawer.DrawOverlay(pawn, OverlayTypes.QuestionMark);
        }
    }

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