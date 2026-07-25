using LudeonTK;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// UI 控件工具类。
/// </summary>
public static class OAFrame_Widgets
{

    /// <summary>
    /// 使用 <see cref="TextStyle_FontSize"/> 绘制标签。
    /// </summary>
    /// <param name="textStyle">字体样式</param>
    /// <param name="rect">绘制区域</param>
    /// <param name="label">标签文本</param>
    public static void DrawLabel(this TextStyle_FontSize textStyle, Rect rect, string label)
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

        TextStyle_GameFont oriTextStyle = TextStyle_GameFont.CurTextStyle;

        try
        {
            textStyle.SetGameTextStyle();
            GUI.Label(position, label, textStyle.fontStyle);
        }
        finally
        {
            textStyle.ResetFontStyleFontSize();
            oriTextStyle.SetGameTextStyle();
        }
    }

    /// <summary>
    /// 使用 <see cref="TextStyle_GameFont"/> 绘制标签。
    /// </summary>
    /// <param name="textStyle">字体样式</param>
    /// <param name="rect">绘制区域</param>
    /// <param name="label">标签文本</param>
    public static void DrawLabel(this TextStyle_GameFont textStyle, Rect rect, string label)
    {

        TextStyle_GameFont oriTextStyle = TextStyle_GameFont.CurTextStyle;

        try
        {
            textStyle.SetGameTextStyle();
            Widgets.Label(rect, label);
        }
        finally
        {
            oriTextStyle.SetGameTextStyle();
        }
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
        return Widgets.ButtonInvisible(butRect);
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