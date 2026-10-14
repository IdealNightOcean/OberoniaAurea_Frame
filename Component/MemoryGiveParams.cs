using RimWorld;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 用于配置给予<see cref="Thought_Memory"/>的参数集合。
/// </summary>
public class MemoryGiveParams : IExposable
{
    /// <summary>
    /// 要给予的<see cref="ThoughtDef"/>。
    /// </summary>
    public ThoughtDef MemoryToGive;

    /// <summary>
    /// 该记忆是否为永久性记忆。
    /// </summary>
    public bool Permanent;

    /// <summary>
    /// 覆盖 <see cref="Thought_Memory"/> 的消失时间（Tick数）。小于等于0时使用 <see cref="Thought_Memory"/> 自身的默认值。
    /// </summary>
    /// <remarks>- 应仅在 <see cref="Permanent"/> 为 <see langword="false"/> 时生效</remarks>
    public int OverrideDisappearTicks = -1;
    private int moodOffset;

    /// <summary>
    /// 心情偏移量的随机范围。当 <see cref="moodOffset"/> 为 <see langword="0"/> 时从此范围中随机取值。
    /// </summary>
    public IntRange MoodOffsetRange = IntRange.Zero;

    /// <summary>
    /// 心情偏移量。若赋值为非零值则作为固定偏移量返回；否则从 <see cref="MoodOffsetRange"/> 中随机取值。
    /// </summary>
    public int MoodOffset
    {
        get
        {
            if (moodOffset != 0)
                return moodOffset;
            if (MoodOffsetRange != IntRange.Zero)
                return MoodOffsetRange.RandomInRange;

            return 0;
        }
        set => moodOffset = value;
    }

    /// <summary>
    /// 序列化/反序列化此对象需持久保存的字段。
    /// </summary>
    public virtual void ExposeData()
    {
        Scribe_Defs.Look(ref MemoryToGive, nameof(MemoryToGive));

        Scribe_Values.Look(ref Permanent, nameof(Permanent), defaultValue: false);
        Scribe_Values.Look(ref moodOffset, nameof(moodOffset), defaultValue: 0);
        Scribe_Values.Look(ref MoodOffsetRange, nameof(MoodOffsetRange), defaultValue: IntRange.Zero);
        Scribe_Values.Look(ref OverrideDisappearTicks, nameof(OverrideDisappearTicks), defaultValue: -1);
    }
}
