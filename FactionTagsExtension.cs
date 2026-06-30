using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 派系标签扩展。
/// </summary>
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

/// <summary>
/// 鼠族派系标记扩展。
/// </summary>
public class RatkinFactionFlag : DefModExtension;