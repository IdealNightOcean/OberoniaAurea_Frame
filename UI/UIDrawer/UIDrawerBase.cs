using UnityEngine;

namespace OberoniaAurea_Frame.UI;

/// <summary>
/// UI 绘制器基类，提供绘制尺寸、边框厚度与文本样式的基础配置
/// </summary>
public abstract class UIDrawerBase : IUIDrawer
{
    /// <summary>
    /// 绘制区域的总尺寸（含边框），默认 800×600
    /// </summary>
    public Vector2 DrawSize { get; protected set; } = new(800, 600);

    /// <summary>
    /// 扣除四周边框厚度后的有效绘制尺寸
    /// </summary>
    public Vector2 ValidDrawSize => new(DrawSize.x - 2 * OutlineThickness, DrawSize.y - 2 * OutlineThickness);

    /// <summary>
    /// 边框线条厚度（像素），默认 1
    /// </summary>
    public int OutlineThickness { get; protected set; } = 1;

    /// <summary>
    /// 绘制时使用的文本样式，默认为 <see cref="TextStyle.DefaultStyle"/>
    /// </summary>
    public TextStyle TextStyle { get; protected set; } = TextStyle.DefaultStyle;

    /// <summary>
    /// 设置绘制尺寸
    /// </summary>
    /// <param name="size">目标尺寸</param>
    /// <returns>尺寸宽高均为正值时设置成功并返回 <see langword="true"/>，否则返回 <see langword="false"/></returns>
    public bool SetDrawSize(Vector2 size)
    {
        if (size.x > 1e-6f && size.y > 1e-6f)
        {
            DrawSize = size;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 在容器尺寸内按原始宽高比等比适配绘制尺寸（contain 方式）
    /// </summary>
    /// <param name="containerSize">容器尺寸</param>
    /// <returns>适配成功返回 <see langword="true"/>；容器尺寸非法时返回 <see langword="false"/></returns>
    public bool SetDrawSizeAspectFit(Vector2 containerSize)
    {
        if (containerSize.x <= 1e-6f || containerSize.y <= 1e-6f)
            return false;

        float scaledHeight = DrawSize.y * containerSize.x / DrawSize.x;
        if (scaledHeight < containerSize.y)
        {
            return SetDrawSizeByWidth(containerSize.x);
        }
        else
        {
            return SetDrawSizeByHeight(containerSize.y);
        }
    }

    /// <summary>
    /// 固定宽度，按原始宽高比自动计算高度
    /// </summary>
    /// <param name="width">目标宽度</param>
    /// <returns>宽度或当前绘制尺寸非法时返回 <see langword="false"/>，否则返回设置结果</returns>
    public bool SetDrawSizeByWidth(float width)
    {
        if (DrawSize.x < 1e-6f || width < 1e-6f)
            return false;

        return SetDrawSize(new Vector2(width, DrawSize.y * width / DrawSize.x));
    }

    /// <summary>
    /// 固定高度，按原始宽高比自动计算宽度
    /// </summary>
    /// <param name="height">目标高度</param>
    /// <returns>高度或当前绘制尺寸非法时返回 <see langword="false"/>，否则返回设置结果</returns>
    public bool SetDrawSizeByHeight(float height)
    {
        if (DrawSize.x < 1e-6f || height < 1e-6f)
            return false;

        return SetDrawSize(new Vector2(DrawSize.x * height / DrawSize.y, height));
    }

    /// <summary>
    /// 按缩放系数整体缩放DrawSize
    /// </summary>
    /// <param name="scaleFactor">缩放系数</param>
    /// <returns>系数为正时返回缩放后的设置结果，否则返回 <see langword="false"/></returns>
    public bool ScaleDrawSize(float scaleFactor)
    {
        if (scaleFactor > 1e-6f)
        {
            Vector2 newDrawSize = DrawSize * scaleFactor;
            return SetDrawSize(newDrawSize);
        }

        return false;
    }
}
