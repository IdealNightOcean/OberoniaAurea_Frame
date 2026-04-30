using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

[StaticConstructorOnStartup]
public static class OAFrame_IconUtility
{
    /// <summary>
    /// 重组基因图标。
    /// </summary>
    public static readonly Texture2D RecombineIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/RecombineGenes");

    /// <summary>
    /// 插入<see cref="Pawn"/>图标。
    /// </summary>
    public static readonly Texture2D InsertPawnIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/InsertPawn");
    /// <summary>
    /// 取消图标。
    /// </summary>
    public static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

    /// <summary>
    /// 交易命令图标。
    /// </summary>
    public static readonly Texture2D TradeCommandIcon = ContentFinder<Texture2D>.Get("UI/Commands/FulfillTradeRequest");
}
