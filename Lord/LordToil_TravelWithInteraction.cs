using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace OberoniaAurea_Frame;

public class LordToil_TravelWithInteraction : LordToil_Travel
{
    public LordToil_TravelWithInteraction(IntVec3 dest) : base(dest) { }

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