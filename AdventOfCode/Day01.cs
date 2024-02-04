namespace AdventOfCode;

public class Day01 : BaseDay
{
    readonly char[][] RawInputLines;

    public Day01()
    {
        RawInputLines = File.ReadLines(InputFilePath)
                            .Select(o => o.ToArray())
                            .ToArray();
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    int Part_1()
        => RawInputLines.Sum(o => o.GetFirstDigit() * 10 + o.Reverse().GetFirstDigit());

    /// <summary>
    /// Testing list pattern matching w/ recursion
    /// </summary>
    int Part_1(string[] lines, int sum)
        => lines switch {
            [var head, .. var tail] => Part_1(tail, sum + head.GetFirstDigit() * 10 + head.Reverse().GetFirstDigit()),
            _ => sum
        };

    int Part_2() {
        var spelledDigits = new Dictionary<int, string>() {
            { 1, "one" },
            { 2, "two" },
            { 3, "three" },
            { 4, "four" },
            { 5, "five" },
            { 6, "six" },
            { 7, "seven" },
            { 8, "eight" },
            { 9, "nine" }
        };
        var spelledDigitsReversed = spelledDigits.ToDictionary(kvp => kvp.Key,
                                                               kvp => new String(kvp.Value.Reverse().ToArray()));

        return RawInputLines.Sum(o => o.GetFirstDigitOrSpelledDigit(spelledDigits) * 10 + o.Reverse().ToArray().GetFirstDigitOrSpelledDigit(spelledDigitsReversed));
    }
}

file static class Extensions {
    public static int GetFirstDigit(this IEnumerable<char> source)
        => source.First(Char.IsDigit) - '0';

    public static int GetFirstDigitOrSpelledDigit(this char[] source, Dictionary<int, string> spelledDigits) {
        foreach (var i in Enumerable.Range(0, source.Length)) {
            if (Char.IsDigit(source[i]))
                return source[i] - '0';

            foreach (var kvp in spelledDigits)
                if (source.FindAt(i, kvp.Value))
                    return kvp.Key;
        }

        throw new InvalidOperationException($"No digit found: {new String(source)}");
    }

    public static bool FindAt(this char[] source, int pos, string value) {
        if (pos + value.Length > source.Length)
            return false;

        foreach (var i in Enumerable.Range(0, value.Length))
            if (source[pos + i] != value[i])
                return false;

        return true;
    }
}