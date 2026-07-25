using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// UI 工具类。
/// </summary>
public static class OAFrame_UIUtility
{
    private const float RgbaByteScale = 1f / 255f;
    /// <summary>
    /// 将 0~255 整型 RGBA 通道转换为 <see cref="UnityEngine"/>.<see cref="Color"/> 浮点色彩对象
    /// </summary>
    /// <param name="r">红色通道 0~255</param>
    /// <param name="g">绿色通道 0~255</param>
    /// <param name="b">蓝色通道 0~255</param>
    /// <param name="a">透明度通道 0~255</param>
    /// <returns><see cref="UnityEngine"/>.<see cref="Color"/> 浮点色彩对象</returns>
    public static Color FromRgba255(int r, int g, int b, int a)
    {
        return new Color(r * RgbaByteScale, g * RgbaByteScale, b * RgbaByteScale, a * RgbaByteScale);
    }
    /// <summary>
    /// 将 0~255 整型 RGBA 通道转换为 <see cref="UnityEngine"/>.<see cref="Color"/> 浮点色彩对象
    /// </summary>
    /// <param name="r">红色通道 0~255</param>
    /// <param name="g">绿色通道 0~255</param>
    /// <param name="b">蓝色通道 0~255</param>
    /// <returns><see cref="UnityEngine"/>.<see cref="Color"/> 浮点色彩对象</returns>
    public static Color FromRgba255(int r, int g, int b)
    {
        return new Color(r * RgbaByteScale, g * RgbaByteScale, b * RgbaByteScale, 1f);
    }

    /// <summary> 获取矩形左上角坐标。 </summary>
    /// <param name="rect">矩形</param>
    /// <returns>左上角坐标</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 TopLeftCorner(this Rect rect) => new(rect.xMin, rect.yMin);

    /// <summary> 获取矩形右上角坐标。 </summary>
    /// <param name="rect">矩形</param>
    /// <returns>右上角坐标</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 TopRightCorner(this Rect rect) => new(rect.xMax, rect.yMin);

    /// <summary> 获取矩形左下角坐标。 </summary>
    /// <param name="rect">矩形</param>
    /// <returns>左下角坐标</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 BottomLeftCorner(this Rect rect) => new(rect.xMin, rect.yMax);

    /// <summary> 获取矩形右下角坐标。 </summary>
    /// <param name="rect">矩形</param>
    /// <returns>右下角坐标</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 BottomRightCorner(this Rect rect) => new(rect.xMax, rect.yMax);

    /// <summary> 获取矩形左段区域。 </summary>
    /// <param name="sourceRect">源矩形</param>
    /// <param name="splitRatio">分割比例</param>
    /// <returns>左段矩形</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect LeftSegment(this Rect sourceRect, float splitRatio = 0.5f)
    => new(sourceRect.x, sourceRect.y, sourceRect.width * splitRatio, sourceRect.height);

    /// <summary> 获取矩形右段区域。 </summary>
    /// <param name="sourceRect">源矩形</param>
    /// <param name="splitRatio">分割比例</param>
    /// <returns>右段矩形</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect RightSegment(this Rect sourceRect, float splitRatio = 0.5f)
    {
        float width = sourceRect.width * splitRatio;
        return new(sourceRect.x, sourceRect.yMax - width, width, sourceRect.height);
    }

    /// <summary> 获取矩形上段区域。 </summary>
    /// <param name="sourceRect">源矩形</param>
    /// <param name="splitRatio">分割比例</param>
    /// <returns>上段矩形</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect TopSegment(this Rect sourceRect, float splitRatio = 0.5f)
    => new(sourceRect.x, sourceRect.y, sourceRect.width, sourceRect.height * splitRatio);

    /// <summary> 获取矩形下段区域。 </summary>
    /// <param name="sourceRect">源矩形</param>
    /// <param name="splitRatio">分割比例</param>
    /// <returns>下段矩形</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect BottomSegment(this Rect sourceRect, float splitRatio = 0.5f)
    {
        float height = sourceRect.height * splitRatio;
        return new(sourceRect.x, sourceRect.yMax - height, sourceRect.width, height);
    }

    /// <summary> 获取矩形中心区域。 </summary>
    /// <param name="sourceRect">源矩形</param>
    /// <param name="xSplitRatio">X轴分割比例</param>
    /// <param name="ySplitRatio">Y轴分割比例</param>
    /// <returns>中心矩形</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect CenterSegment(this Rect sourceRect, float xSplitRatio = 0.5f, float ySplitRatio = 0.5f)
    {
        float width = sourceRect.height * xSplitRatio;
        float height = sourceRect.height * ySplitRatio;
        return new Rect(sourceRect.xMin + (sourceRect.width - width) * 0.5f,
                        sourceRect.yMin + (sourceRect.height - height) * 0.5f,
                        width,
                        height);
    }

    /// <summary> 在 X 轴方向获取矩形中心区域。 </summary>
    /// <param name="sourceRect">源矩形</param>
    /// <param name="splitRatio">分割比例</param>
    /// <returns>中心矩形</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect GetCenterSegmentOnX(this Rect sourceRect, float splitRatio = 0.5f)
    {
        float width = sourceRect.width * splitRatio;
        return new(sourceRect.xMin + (sourceRect.width - width) * 0.5f, sourceRect.yMin, width, sourceRect.height);
    }

    /// <summary> 在 Y 轴方向获取矩形中心区域。 </summary>
    /// <param name="sourceRect">源矩形</param>
    /// <param name="splitRatio">分割比例</param>
    /// <returns>中心矩形</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect GetCenterSegmentOnY(this Rect sourceRect, float splitRatio = 0.5f)
    {
        float height = sourceRect.height * splitRatio;
        return new Rect(sourceRect.xMin, sourceRect.yMin + (sourceRect.height - height) * 0.5f, sourceRect.width, height);
    }

    /// <summary>
    /// 重置文本设置为默认值。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ResetTextStyleToDefault()
    {
        Text.WordWrap = true;
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = Color.white;
    }

    /// <summary>
    /// 获取带遮罩的染色材质。
    /// </summary>
    /// <param name="color">颜色</param>
    /// <param name="maskTex">遮罩纹理</param>
    /// <returns>染色材质</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Material GetTintMaterial(Color color, Texture2D maskTex = null)
    {
        if (maskTex == null)
        {
            return SolidColorMaterials.SimpleSolidColorMaterial(color);
        }
        else
        {
            MaterialRequest req = new()
            {
                shader = ShaderDatabase.GrayscaleGUI,
                color = color,
                maskTex = maskTex
            };

            return MaterialPool.MatFrom(req);
        }
    }
}