using RimWorld;
using System;
using System.Runtime.CompilerServices;
using Verse;

namespace OberoniaAurea_Frame;

public static class OAFrame_DiaUtility
{
    public static DiaOption DefaultConfirmOption => new("Confirm".Translate()) { resolveTree = true };
    public static DiaOption DefaultCancelOption => new("Cancel".Translate()) { resolveTree = true };
    public static DiaOption DefaultCloseOption => new("Close".Translate()) { resolveTree = true };
    public static DiaOption DefaultPostponeOption => new("PostponeLetter".Translate()) { resolveTree = true };

    /// <summary>
    /// 创建默认确认对话框。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Dialog_NodeTree DefaultConfirmDiaNodeTree(TaggedString text, Action acceptAction = null, Action rejectAction = null)
    {
        return new Dialog_NodeTree(ConfirmDiaNode(text, "Confirm".Translate(), acceptAction, "Close".Translate(), rejectAction));
    }

    /// <summary>
    /// 创建带派系信息的默认确认对话框。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Dialog_NodeTreeWithFactionInfo DefaultConfirmDiaNodeTreeWithFactionInfo(TaggedString text, Faction faction, Action acceptAction = null, Action rejectAction = null)
    {
        return new Dialog_NodeTreeWithFactionInfo(ConfirmDiaNode(text, "Confirm".Translate(), acceptAction, "Close".Translate(), rejectAction), faction);
    }

    /// <summary>
    /// 创建确认对话框。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Dialog_NodeTree ConfirmDiaNodeTree(TaggedString text, string acceptText = null, Action acceptAction = null, string rejectText = null, Action rejectAction = null)
    {
        return new Dialog_NodeTree(ConfirmDiaNode(text, acceptText, acceptAction, rejectText, rejectAction));
    }

    /// <summary>
    /// 创建带派系信息的确认对话框。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Dialog_NodeTreeWithFactionInfo ConfirmDiaNodeTreeWithFactionInfo(TaggedString text, Faction faction, string acceptText = null, Action acceptAction = null, string rejectText = null, Action rejectAction = null)
    {
        return new Dialog_NodeTreeWithFactionInfo(ConfirmDiaNode(text, acceptText, acceptAction, rejectText, rejectAction), faction);
    }

    /// <summary>
    /// 创建确认对话节点。
    /// </summary>
    public static DiaNode ConfirmDiaNode(TaggedString text, string acceptText = null, Action acceptAction = null, string rejectText = null, Action rejectAction = null)
    {
        DiaNode diaNode = new(text);
        if (acceptText is not null)
        {
            DiaOption accept = new(acceptText)
            {
                action = acceptAction,
                resolveTree = true
            };
            diaNode.options.Add(accept);
        }
        if (rejectText is not null)
        {
            DiaOption reject = new(rejectText)
            {
                action = rejectAction,
                resolveTree = true
            };
            diaNode.options.Add(reject);
        }

        return diaNode;
    }
}