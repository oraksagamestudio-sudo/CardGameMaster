using System;

public sealed class XorShift32 : IRng
{
    private uint _state;
    public XorShift32(uint seed) { _state = seed == 0 ? 2463534242u : seed; }

    // 원시 32비트
    private uint NextU()
    {
        uint x = _state;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        _state = x;
        return x;
    }

    public int Next()
    {
        return (int)(NextU() & 0x7FFFFFFF);
    }

    public int Next(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive) throw new ArgumentOutOfRangeException(nameof(maxExclusive));
        uint range = (uint)(maxExclusive - minInclusive);
        // 거절 샘플링: 범위로 나누어떨어지지 않는 상단 구간 버림
        uint limit = uint.MaxValue - (uint.MaxValue % range);
        uint r;
        do { r = NextU(); } while (r >= limit);
        return minInclusive + (int)(r % range);
    }
}