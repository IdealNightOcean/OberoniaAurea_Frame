using System;
using Verse;

namespace OberoniaAurea_Frame;

public static class OAFrame_DiaUtility
{
    public static DiaNode DefaultConfirmDiaNode(TaggedString text, Action acceptAction = null, Action rejectAction = null)
    {
        return ConfirmDiaNode(text, "Confirm".Translate(), acceptAction, "GoBack".Translate(), rejectAction);
    }
    public static DiaNode ConfirmDiaNode(TaggedString text, string acceptText = null, Action acceptAction = null, string rejectText = null, Action rejectAction = null)
    {
        DiaNode root = new(text);
        if (acceptText != null)
        {
            DiaOption accept = new(acceptText)
            {
                resolveTree = true,
                action = acceptAction
            };
            root.options.Add(accept);
        }
        if (rejectAction != null)
        {
            DiaOption reject = new(rejectText)
            {
                resolveTree = true,
                action = rejectAction
            };
            root.options.Add(reject);
        }
        return root;
    }
}
