public static class Base32Utility
{
    private const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
    private const uint Base = 32;

    public static string ToBase32(uint value)
    {
        if (value == 0)
            return Alphabet[0].ToString();

        char[] buffer = new char[13]; // uint32 최대 길이 (32진수 기준)
        int index = buffer.Length;

        while (value > 0)
        {
            uint remainder = value % Base;
            buffer[--index] = Alphabet[(int)remainder];
            value /= Base;
        }

        return new string(buffer, index, buffer.Length - index);
    }

    public static string FormatCode(string code)
    {
        if (code == null) throw new System.ArgumentNullException(nameof(code));

        // 대시 제거하고 대문자 통일
        string clean = code.Replace("-", "").ToUpperInvariant();

        const int totalLength = 7; // XXX-XXXX => 7 문자
        if (clean.Length < totalLength)
            clean = clean.PadRight(totalLength, '0');
        else if (clean.Length > totalLength)
            clean = clean.Substring(0, totalLength);

        return $"{clean.Substring(0, 3)}-{clean.Substring(3, 4)}";
    }

    public static string FormatCode(uint seed)
    {
        var code = ToBase32(seed);
        if (code == null) throw new System.ArgumentNullException(nameof(code));

        const int totalLength = 7; // XXX-XXXX => 7 문자
        if (code.Length < totalLength)
            code = code.PadLeft(totalLength, '0');

        return $"{code[..3]}-{code.Substring(3, 4)}";
    }

    public static uint FromBase32(string code)
    {
        uint result = 0;

        foreach (char c in code.Replace("-", ""))
        {
            int index = Alphabet.IndexOf(c);
            if (index < 0)
                throw new System.ArgumentException($"Invalid Base32 character: {c}");

            result = result * Base + (uint)index;
        }

        return result;
    }
}
