using LudeonTK;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame.UI;

/// <summary>
/// UI 控件工具类。
/// </summary>
public static class OAFrame_Widgets
{
    /// <summary>
    /// 绘制水平线。
    /// </summary>
    /// <param name="startPos">起点位置</param>
    /// <param name="length">线长度</param>
    /// <param name="color">线颜色</param>
    /// <param name="thickness">线粗细</param>
    public static void DrawLineHorizontal(Vector2 startPos, float length, Color color, int thickness = 1)
    {
        Color oriGuiColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(new Rect(startPos.x, startPos.y, length, thickness), BaseContent.WhiteTex);
        GUI.color = oriGuiColor;
    }

    /// <summary>
    /// 绘制垂直线。
    /// </summary>
    /// <param name="startPos">起点位置</param>
    /// <param name="length">线长度</param>
    /// <param name="color">线颜色</param>
    /// <param name="thickness">线粗细</param>
    public static void DrawLineVertical(Vector2 startPos, float length, Color color, int thickness = 1)
    {
        Color oriGuiColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(new Rect(startPos.x, startPos.y, thickness, length), BaseContent.WhiteTex);
        GUI.color = oriGuiColor;
    }

    /// <summary>
    /// 绘制带颜色的矩形边框。
    /// </summary>
    /// <param name="rect">矩形区域</param>
    /// <param name="color">边框颜色</param>
    /// <param name="thickness">边框粗细</param>
    /// <param name="lineTexture">线条纹理</param>
    public static void DrawBox(Rect rect, Color color, int thickness = 1, Texture2D lineTexture = null)
    {
        Color oriGuiColor = GUI.color;
        GUI.color = color;
        Widgets.DrawBox(rect, thickness, lineTexture);
        GUI.color = oriGuiColor;
    }

    /// <summary>
    /// 使用 <see cref="TextStyle"/> 绘制标签。
    /// </summary>
    /// <param name="rect">绘制区域</param>
    /// <param name="label">标签文本</param>
    /// <param name="textStyle">字体样式</param>
    public static void DrawLabel(Rect rect, string label, TextStyle textStyle)
    {
        Rect position = rect;
        float halfUIScale = Prefs.UIScale * 0.5f;
        if (Prefs.UIScale > 1f && Math.Abs(halfUIScale - Mathf.Floor(halfUIScale)) > float.Epsilon)
        {
            position.xMin = UIScaling.AdjustCoordToUIScalingFloor(rect.xMin);
            position.yMin = UIScaling.AdjustCoordToUIScalingFloor(rect.yMin);
            position.xMax = UIScaling.AdjustCoordToUIScalingCeil(rect.xMax + 1E-05f);
            position.yMax = UIScaling.AdjustCoordToUIScalingCeil(rect.yMax + 1E-05f);
        }
        Color oriColor = GUI.color;
        GUI.color = textStyle.guiColor;
        GUI.Label(position, label, textStyle.FontStyle);
        GUI.color = oriColor;
        textStyle.RestoreTextStyle();

    }

    /// <summary>
    /// 绘制带省略号截断的标签。
    /// </summary>
    /// <param name="rect">绘制区域</param>
    /// <param name="label">标签文本</param>
    /// <param name="textStyle">文本样式</param>
    /// <returns>是否发生截断</returns>
    public static bool DrawLabelEllipses(Rect rect, string label, TextStyle textStyle)
    {
        label = Utility.OAFrame_TextUtility.ClampTextWithEllipsis(rect, label, textStyle, out bool isTextClamped);
        DrawLabel(rect, label, textStyle);
        return isTextClamped;
    }

    /// <summary>
    /// 绘制带有文本和图片的按钮。
    /// </summary>
    /// <param name="butRect">按钮区域</param>
    /// <param name="text">按钮文本</param>
    /// <param name="butTex">按钮图片</param>
    /// <param name="baseButColor">按钮基础颜色</param>
    /// <param name="mouseoverButColor">鼠标悬停时按钮颜色</param>
    /// <param name="baseTextColor">文本基础颜色</param>
    /// <param name="mouseoverTextColor">鼠标悬停时文本颜色</param>
    /// <param name="doMouseoverSound">是否播放悬停音效</param>
    /// <returns>是否点击</returns>
    public static bool TextButtonImageFitted(Rect butRect, string text, Texture2D butTex,
                                             Color baseButColor, Color mouseoverButColor,
                                             Color baseTextColor, Color mouseoverTextColor,
                                             bool doMouseoverSound = true)
    {
        Color oriColor = GUI.color;
        bool isOver = Mouse.IsOver(butRect);
        GUI.color = isOver ? mouseoverButColor : baseButColor;
        Widgets.DrawTextureFitted(butRect, butTex, 1f);

        TextAnchor oriAnchor = Text.Anchor;

        Text.Anchor = TextAnchor.MiddleCenter;
        GUI.color = isOver ? mouseoverTextColor : baseTextColor;
        Widgets.Label(butRect, text);

        Text.Anchor = oriAnchor;
        GUI.color = oriColor;
        return Widgets.ButtonInvisible(butRect, doMouseoverSound: doMouseoverSound);
    }

    /// <summary>
    /// 使用默认颜色绘制带有文本和图片的按钮。
    /// </summary>
    /// <param name="butRect">按钮区域</param>
    /// <param name="text">按钮文本</param>
    /// <param name="butTex">按钮图片</param>
    /// <param name="doMouseoverSound">是否播放悬停音效</param>
    /// <returns>是否点击</returns>
    public static bool DefaultTextButtonImageFitted(Rect butRect, string text, Texture2D butTex, bool doMouseoverSound = true)
    {
        return TextButtonImageFitted(butRect: butRect, text: text, butTex: butTex,
                                     baseButColor: Color.white, mouseoverButColor: Color.gray,
                                     baseTextColor: Color.white, mouseoverTextColor: Color.gray,
                                     doMouseoverSound: doMouseoverSound);
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
    /// <see cref="UnityEngine"/>.<see cref="GUI"/>.<see href="CalculateScaledTextureRects"/> 的实现
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