using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

[StaticConstructorOnStartup]
public static class IconUtility
{
    public static readonly Texture2D RecombineIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/RecombineGenes");

    public static readonly Texture2D InsertPawnIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/InsertPawn");
    public static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

    public static readonly Texture2D CommandContinueWorkTrue = ContentFinder<Texture2D>.Get("UI/Icons/OAGene_CommandAutoSelectTrue");
    public static readonly Texture2D CommandContinueWorkFalse = ContentFinder<Texture2D>.Get("UI/Icons/OAGene_CommandAutoSelectFalse");

    public static readonly Texture2D TradeCommandIcon = ContentFinder<Texture2D>.Get("UI/Commands/FulfillTradeRequest");
}
