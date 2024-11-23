using System;
using Verse;

namespace OberoniaAurea_Frame;

public static class OAFrame_DiaUtility
{
    public static Dialog_NodeTree DefaultConfirmDiaNodeTree(TaggedString text, Action acceptAction = null, Action rejectAction = null)
    {
        return ConfirmDiaNodeTree(text, "Confirm".Translate(), acceptAction, "GoBack".Translate(), rejectAction);
    }
    public static Dialog_NodeTree ConfirmDiaNodeTree(TaggedString text, string acceptText = null, Action acceptAction = null, string rejectText = null, Action rejectAction = null)
    {
        DiaNode rootnode = new(text);
        if (acceptText != null)
        {
            DiaOption accept = new(acceptText)
            {
                resolveTree = true,
                action = acceptAction
            };
            rootnode.options.Add(accept);
        }
        if (rejectText != null)
        {
            DiaOption reject = new(rejectText)
            {
                resolveTree = true,
                action = rejectAction
            };
            rootnode.options.Add(reject);
        }
        Dialog_NodeTree nodeTree = new(rootnode);
        return nodeTree;
    }
}
