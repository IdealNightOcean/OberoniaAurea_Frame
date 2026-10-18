using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 强制颜色组件属性。
/// </summary>
public class CompProperties_ForceColor : CompProperties
{
    /// <summary>
    /// 强制使用的颜色。
    /// </summary>
    public Color color = Color.white;
    /// <summary>
    /// 初始化强制颜色组件属性。
    /// </summary>
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
    /// <summary>
    /// 获取强制颜色的组件属性。
    /// </summary>
    public CompProperties_ForceColor Props => (CompProperties_ForceColor)props;

    /// <inheritdoc/>
    public override Color? ForceColor() => Props.color;
}