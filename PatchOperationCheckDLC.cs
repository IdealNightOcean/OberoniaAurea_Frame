﻿using System.Xml;
using Verse;

namespace OberoniaAurea_Frame;

public class PatchOperationCheckDLC : PatchOperation
{
    public bool Royalty;
    public bool Ideology;
    public bool Biotech;
    public bool Anomaly;
    public bool Odyssey;

    public PatchOperation match;

    public PatchOperation nomatch;

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
