using System.Xml;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 检查 DLC 是否启用的补丁操作。
/// </summary>
public class PatchOperationCheckDLC : PatchOperation
{
    /// <summary>
    /// 是否要求启用皇权（Royalty）DLC。
    /// </summary>
    public bool Royalty;
    /// <summary>
    /// 是否要求启用文化（Ideology）DLC。
    /// </summary>
    public bool Ideology;
    /// <summary>
    /// 是否要求启用生物科技（Biotech）DLC。
    /// </summary>
    public bool Biotech;
    /// <summary>
    /// 是否要求启用异象（Anomaly）DLC。
    /// </summary>
    public bool Anomaly;
    /// <summary>
    /// 是否要求启用远行（Odyssey）DLC。
    /// </summary>
    public bool Odyssey;

    /// <summary>
    /// DLC 条件满足时应用的补丁操作。
    /// </summary>
    public PatchOperation match;

    /// <summary>
    /// DLC 条件不满足时应用的补丁操作。
    /// </summary>
    public PatchOperation nomatch;

    /// <inheritdoc/>
    protected override bool ApplyWorker(XmlDocument xml)
    {
        if (CheckDLC())
        {
            if (match is not null)
            {
                return match.Apply(xml);
            }
        }
        else
        {
            if (nomatch is not null)
            {
                return nomatch.Apply(xml);
            }
        }
        return true;
    }

    /// <summary>
    /// 检查所要求的 DLC 是否均已启用。
    /// </summary>
    protected bool CheckDLC()
    {
        if (Royalty && !ModsConfig.RoyaltyActive)
        {
            return false;
        }
        if (Ideology && !ModsConfig.IdeologyActive)
        {
            return false;
        }
        if (Biotech && !ModsConfig.BiotechActive)
        {
            return false;
        }
        if (Anomaly && !ModsConfig.AnomalyActive)
        {
            return false;
        }
        if (Odyssey && !ModsConfig.OdysseyActive)
        {
            return false;
        }
        return true;
    }
}
