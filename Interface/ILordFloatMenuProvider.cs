using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

public interface ILordFloatMenuProvider
{
    IEnumerable<FloatMenuOption> ExtraFloatMenuOptions(Pawn target, Pawn forPawn);
}