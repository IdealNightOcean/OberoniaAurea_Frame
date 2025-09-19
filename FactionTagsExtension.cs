using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

public class FactionTagsExtension : DefModExtension
{
    public List<string> factionTags;

    public bool HasTag(string tag)
    {
        return factionTags?.Contains(tag) ?? false;
    }
}

public class RatkinFactionFlag : DefModExtension;