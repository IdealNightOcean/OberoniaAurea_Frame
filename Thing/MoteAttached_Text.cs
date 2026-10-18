using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 附着文本粒子。
/// </summary>
public class MoteAttached_Text : MoteAttached
{
    /// <summary>
    /// 要显示的文字内容。
    /// </summary>
    public string text;

    /// <summary>
    /// 文字显示颜色。
    /// </summary>
    public Color textColor = Color.white;

    /// <summary>
    /// 覆盖开始淡出前的生存时间，负值表示不覆盖。
    /// </summary>
    public float overrideTimeBeforeStartFadeout = -1f;

    /// <summary>
    /// 获取开始淡出前的生存时间。
    /// </summary>
    protected float TimeBeforeStartFadeout
    {
        get
        {
            if (!(overrideTimeBeforeStartFadeout >= 0f))
            {
                return SolidTime;
            }
            return overrideTimeBeforeStartFadeout;
        }
    }

    /// <inheritdoc/>
    protected override bool EndOfLife => AgeSecs >= TimeBeforeStartFadeout + def.mote.fadeOutTime;

    /// <inheritdoc/>
    protected override void DrawAt(Vector3 drawLoc, bool flip = false) { }

    /// <inheritdoc/>
    public override void DrawGUIOverlay()
    {
        float a = 1f - (AgeSecs - TimeBeforeStartFadeout) / def.mote.fadeOutTime;
        DrawText(new Vector2(exactPosition.x, exactPosition.z), text, new Color(textColor.r, textColor.g, textColor.b, a));
    }

    private static void DrawText(Vector2 worldPos, string text, Color textColor)
    {
        Vector3 position = new(worldPos.x, 0f, worldPos.y);
        Vector2 vector = Find.Camera.WorldToScreenPoint(position) / Prefs.UIScale;
        vector.y = Verse.UI.screenHeight - vector.y;
        Text.Font = GameFont.Tiny;
        float rectY = vector.y;

        float textWidth = Text.CalcSize(text).x;
        float textX = vector.x - textWidth / 2f;

        GUI.DrawTexture(new Rect(textX - 4f, rectY, textWidth + 8f, 16f), TexUI.GrayTextBG);
        GUI.color = textColor;
        Text.Anchor = TextAnchor.UpperCenter;
        Widgets.Label(new Rect(textX, rectY - 2f, textWidth, 128f), text);
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;
    }
}