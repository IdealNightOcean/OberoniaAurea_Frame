using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

public class CompProperties_ForceColor : CompProperties
{
    public Color color = Color.white;
    public CompProperties_ForceColor()
    {
        compClass = typeof(CompForceColor);
    }
}

public class CompForceColor : ThingComp
{
    public CompProperties_ForceColor Props => (CompProperties_ForceColor)props;

    public override Color? ForceColor() => Props.color;
}