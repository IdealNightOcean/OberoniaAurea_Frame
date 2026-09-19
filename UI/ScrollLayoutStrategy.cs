namespace OberoniaAurea_Frame.UI;

/// <summary>滚动区域布局计算策略</summary>
public enum ScrollLayoutStrategy
{
    /// <summary>使用给定视口，保留条目原始尺寸</summary>
    ViewGiven,
    /// <summary>使用给定视口，适配调整条目尺寸</summary>
    ViewGivenItemAdapt,
    /// <summary>由行列约束推导视口，保留条目原始尺寸</summary>
    ViewDerivedByRowCol
}
