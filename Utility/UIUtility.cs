using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// UI 工具类。
/// </summary>
public static class OAFrame_UIUtility
{
    /// <summary>
    /// 重置文本设置为默认值。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ResetText()
    {
        Text.WordWrap = true;
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;
    }

    /// <summary>
    /// 以指定颜色绘制纹理。
    /// </summary>
    /// <param name="position">绘制区域</param>
    /// <param name="texture">纹理</param>
    /// <param name="color">颜色</param>
    /// <param name="scaleMode">缩放模式</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DrawTextureWithColor(Rect position, Texture2D texture, Color color, ScaleMode scaleMode = ScaleMode.StretchToFill)
    {
        Color oriColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(position, texture, scaleMode);
        GUI.color = oriColor;
    }

    /// <summary>
    /// 以指定材质绘制纹理。
    /// </summary>
    /// <param name="rect">绘制区域</param>
    /// <param name="texture">纹理</param>
    /// <param name="material">材质</param>
    /// <param name="scaleMode">缩放模式</param>
    public static void DrawTextureWithMaterial(Rect rect, Texture texture, Material material, ScaleMode scaleMode = ScaleMode.StretchToFill)
    {
        if (material == null)
        {
            GUI.DrawTexture(rect, texture, scaleMode);
        }
        else if (Event.current.type == EventType.Repaint)
        {
            Color color = material.shader.SupportsMaskTex() ? GUI.color : new Color(GUI.color.r * 0.5f, GUI.color.g * 0.5f, GUI.color.b * 0.5f, GUI.color.a);
            Rect screenRect = default;
            Rect sorceRect = default;
            float imageAspect = texture.width / (float)texture.height;
            CalculateScaledTextureRects(rect, scaleMode, imageAspect, ref screenRect, ref sorceRect);
            Graphics.DrawTexture(screenRect, texture, sorceRect, 0, 0, 0, 0, color, material);
        }
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
        if (maskTex is null)
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

    /// <summary>
    /// UnityEngine.GUI.CalculateScaledTextureRects的实现
    /// </summary>
    private static bool CalculateScaledTextureRects(Rect position, ScaleMode scaleMode, float imageAspect, ref Rect outScreenRect, ref Rect outSourceRect)
    {
        float positionAspect = position.width / position.height;
        bool result = false;
        switch (scaleMode)
        {
            case ScaleMode.StretchToFill:
                outScreenRect = position;
                outSourceRect = new Rect(0f, 0f, 1f, 1f);
                result = true;
                break;
            case ScaleMode.ScaleAndCrop:
                if (positionAspect > imageAspect)
                {
                    float scaleFactor = imageAspect / positionAspect;
                    outScreenRect = position;
                    outSourceRect = new Rect(0f, (1f - scaleFactor) * 0.5f, 1f, scaleFactor);
                    result = true;
                }
                else
                {
                    float scaleFactor = positionAspect / imageAspect;
                    outScreenRect = position;
                    outSourceRect = new Rect(0.5f - scaleFactor * 0.5f, 0f, scaleFactor, 1f);
                    result = true;
                }
                break;
            case ScaleMode.ScaleToFit:
                if (positionAspect > imageAspect)
                {
                    float scaleFactor = imageAspect / positionAspect;
                    outScreenRect = new Rect(position.xMin + position.width * (1f - scaleFactor) * 0.5f, position.yMin, scaleFactor * position.width, position.height);
                    outSourceRect = new Rect(0f, 0f, 1f, 1f);
                    result = true;
                }
                else
                {
                    float scaleFactor = positionAspect / imageAspect;
                    outScreenRect = new Rect(position.xMin, position.yMin + position.height * (1f - scaleFactor) * 0.5f, position.width, scaleFactor * position.height);
                    outSourceRect = new Rect(0f, 0f, 1f, 1f);
                    result = true;
                }
                break;
        }
        return result;
    }
}