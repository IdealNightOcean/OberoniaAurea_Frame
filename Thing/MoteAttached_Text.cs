﻿using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

public class MoteAttached_Text : MoteAttached
{
    public string text;

    public Color textColor = Color.white;

    public float overrideTimeBeforeStartFadeout = -1f;

    protected float TimeBeforeStartFadeout
    {
        get
        {
            if (!(overrideTimeBeforeStartFadeout >= 0f))
            {
                return SolidTime;
            }
            return overrideTimeBeforeStartFadeout;
        }
    }

    protected override bool EndOfLife => AgeSecs >= TimeBeforeStartFadeout + def.mote.fadeOutTime;

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
    }

    public override void DrawGUIOverlay()
    {
        float a = 1f - (AgeSecs - TimeBeforeStartFadeout) / def.mote.fadeOutTime;
        OAFrame_MiscUtility.DrawText(new Vector2(exactPosition.x, exactPosition.z), text, new Color(textColor.r, textColor.g, textColor.b, a));
    }


}