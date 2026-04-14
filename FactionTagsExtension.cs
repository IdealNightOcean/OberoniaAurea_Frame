using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

public class FactionTagsExtension : DefModExtension
{
    public List<string> factionTags = [];

    /// <summary>
    /// 检查是否包含指定标签。
    /// </summary>
    public bool HasTag(string tag)
    {
        return factionTags.Contains(tag);
    }
}

public class RatkinFactionFlag : DefModExtension;