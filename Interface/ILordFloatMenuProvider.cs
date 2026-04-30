using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// <see cref="Verse.AI.Group.Lord"/>浮动菜单选项提供者接口。
/// </summary>
public interface ILordFloatMenuProvider
{
    /// <summary>
    /// 获取额外的浮动菜单选项。
    /// </summary>
    IEnumerable<FloatMenuOption> ExtraFloatMenuOptions(Pawn target, Pawn forPawn);
}