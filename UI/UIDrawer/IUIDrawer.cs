using UnityEngine;

namespace OberoniaAurea_Frame.UI;

/// <summary>
/// UI 绘制器接口，描述绘制元素的尺寸、边框厚度与文本样式
/// </summary>
public interface IUIDrawer
{
    /// <summary>
    /// 边框线条厚度（像素）
    /// </summary>
    int OutlineThickness { get; }

    /// <summary>
    /// 绘制区域尺寸
    /// </summary>
    Vector2 DrawSize { get; }

    /// <summary>
    /// 绘制时使用的文本样式
    /// </summary>
    TextStyle TextStyle { get; }
}
