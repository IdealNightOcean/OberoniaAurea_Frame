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