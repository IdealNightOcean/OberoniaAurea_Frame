using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 基于字体大小的文本样式。
/// </summary>
public struct TextStyle_FontSize
{
    /// <summary> 
    /// 是否自动换行。 
    /// </summary>
    public bool wordWrap = true;

    /// <summary> 
    /// 字体大小。 
    /// </summary>
    public readonly int fontSize = 14;

    /// <summary> 
    /// 字体样式。 
    /// </summary>
    public readonly GUIStyle fontStyle;

    /// <summary> 
    /// 文本对齐方式。 
    /// </summary>
    public TextAnchor anchor = TextAnchor.UpperLeft;

    /// <summary> 
    /// <see cref="GUI"/> 颜色。
    /// </summary>
    public Color guiColor = Color.white;

    /// <summary> 
    /// 默认样式。 
    /// </summary>
    public static TextStyle_FontSize DefaultStyle => new(fontSize: 14, anchor: TextAnchor.UpperLeft, wordWrap: true);

    /// <summary>
    /// 创建文本样式。
    /// </summary>
    /// <param name="fontSize">字体大小</param>
    /// <param name="anchor">对齐方式</param>
    /// <param name="wordWrap">是否换行</param>
    public TextStyle_FontSize(int fontSize = 14, TextAnchor anchor = TextAnchor.UpperLeft, bool wordWrap = true)
    {
        this.fontSize = fontSize;
        this.anchor = anchor;
        this.wordWrap = wordWrap;
        fontStyle = fontSize switch
        {
            <= 14 => Text.fontStyles[0],
            <= 20 => Text.fontStyles[1],
            _ => Text.fontStyles[2],
        };
    }

    /// <summary>
    /// 创建文本样式。
    /// </summary>
    /// <param name="guiColor"><see cref="GUI"/> 颜色</param>
    /// <param name="fontSize">字体大小</param>
    /// <param name="anchor">对齐方式</param>
    /// <param name="wordWrap">是否换行</param>
    public TextStyle_FontSize(Color guiColor, int fontSize = 14, TextAnchor anchor = TextAnchor.UpperLeft, bool wordWrap = true) : this(fontSize, anchor, wordWrap)
    {
        this.guiColor = guiColor;
    }

    /// <summary> 
    /// 应用文本样式到游戏。 
    /// </summary>
    public readonly void SetGameTextStyle()
    {
        Text.WordWrap = wordWrap;
        Text.Anchor = anchor;
        GUI.color = guiColor;
        fontStyle.fontSize = fontSize;
    }

    /// <summary> 
    /// 重置 <see cref="FontStyle"/> 字体大小。 
    /// </summary>
    public readonly void ResetFontStyleFontSize()
    {
        if (fontStyle is not null)
            fontStyle.fontSize = 0;
    }
}