using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 强制颜色组件属性。
/// </summary>
public class CompProperties_ForceColor : CompProperties
{
    public Color color = Color.white;
    public CompProperties_ForceColor()
    {
        compClass = typeof(CompForceColor);
    }
}

/// <summary>
/// 强制颜色的组件。
/// </summary>
public class CompForceColor : ThingComp
{
    public CompProperties_ForceColor Props => (CompProperties_ForceColor)props;

    public override Color? ForceColor() => Props.color;
}