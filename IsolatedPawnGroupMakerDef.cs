using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using Verse;

namespace OberoniaAurea_Frame;

public class IsolatedPawnGroupMakerDef : Def
{
    public List<PawnGroupMaker> groupMakers;
    public TraderKindDef traderKind;

    public bool TryGetRandomPawnGroupMaker(PawnGroupKindDef pawnGroupKindDef, out PawnGroupMaker pawnGroupMaker)
    {
        if (groupMakers is null)
        {
            pawnGroupMaker = null;
            return false;
        }
        return groupMakers.Where(g => g.kindDef == pawnGroupKindDef).TryRandomElementByWeight(g => g.commonality, out pawnGroupMaker);
    }

    public bool TryGetRandomAvailablePawnGroupMaker(PawnGroupMakerParms parms, out PawnGroupMaker pawnGroupMaker)
    {
        if (groupMakers is null)
        {
            pawnGroupMaker = null;
            return false;
        }
        if (parms.seed.HasValue)
        {
            Rand.PushState(parms.seed.Value);
        }
        bool result = groupMakers.Where(g => g.kindDef == parms.groupKind && g.CanGenerateFrom(parms)).TryRandomElementByWeight(g => g.commonality, out pawnGroupMaker);
        if (parms.seed.HasValue)
        {
            Rand.PopState();
        }
        return result;
    }
}

public class PawnGroupWithTagMakerDef : Def
{
    public List<PawnGroupOption> groupOptions;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetRandomPawnGroupOption(PawnGroupKindDef pawnGroupKindDef, out PawnGroupOption pawnGroupOption)
    {
        if (groupOptions is null)
        {
            pawnGroupOption = null;
            return false;
        }
        return groupOptions.Where(g => g.kindDef == pawnGroupKindDef).TryRandomElementByWeight(g => g.commonality, out pawnGroupOption);
    }
}

public class PawnGroupOption
{
    public PawnGroupKindDef kindDef;

    public float commonality = 100f;

    public List<PawnGenGroupWithTag> groups;

    public IReadOnlyList<PawnGenOption> GetOptionsWithTag(string tag)
    {
        if (groups.NullOrEmpty() || tag.NullOrEmpty())
        {
            return null;
        }

        foreach (PawnGenGroupWithTag tagOption in groups)
        {
            if (tagOption.tag == tag)
            {
                return tagOption.options;
            }
        }
        return null;
    }
}

public class PawnGenGroupWithTag
{
    public string tag;
    public List<PawnGenOption> options;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "tag", xmlRoot.Name);
        options = DirectXmlToObject.ObjectFromXml<List<PawnGenOption>>(xmlRoot, doPostLoad: true);
    }
}