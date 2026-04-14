using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Verse;

namespace OberoniaAurea_Frame;

public class IsolatedPawnGroupMakerDef : Def
{
    public List<PawnGroupMaker> groupMakers;
    public TraderKindDef traderKind;

    /// <summary>
    /// 尝试获取指定类型的随机PawnGroupMaker。
    /// </summary>
    public bool TryGetRandomPawnGroupMaker(PawnGroupKindDef pawnGroupKindDef, out PawnGroupMaker pawnGroupMaker)
    {
        if (groupMakers is null)
        {
            pawnGroupMaker = null;
            return false;
        }
        return groupMakers.Where(g => g.kindDef == pawnGroupKindDef).TryRandomElementByWeight(g => g.commonality, out pawnGroupMaker);
    }

    /// <summary>
    /// 尝试获取符合参数条件的可用PawnGroupMaker。
    /// </summary>
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

    /// <summary>
    /// 获取随机组的生成选项（忽略标签）。
    /// </summary>
    public IReadOnlyList<PawnGenOption> GetRandomGroupOptionsIgnoreTag() => groups?.RandomElementWithFallback(null)?.options;

    /// <summary>
    /// 获取指定标签的随机PawnGenOption生成组。
    /// </summary>
    public IReadOnlyList<PawnGenOption> GetRandomGroupOptionsWithTag(string tag)
    {
        if (groups.NullOrEmpty() || string.IsNullOrEmpty(tag))
        {
            return null;
        }

        return groups.Where(g => g.tag == tag).RandomElementWithFallback(null)?.options;
    }

    /// <summary>
    /// 获取指定标签的首个PawnGenOption生成组。
    /// </summary>
    public IReadOnlyList<PawnGenOption> GetFirstGroupOptionsWithTag(string tag)
    {
        if (groups.NullOrEmpty() || string.IsNullOrEmpty(tag))
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
        tag = ParseHelper.FromString<string>(xmlRoot.Name);
        options = DirectXmlToObject.ObjectFromXml<List<PawnGenOption>>(xmlRoot, doPostLoad: true);
    }
}