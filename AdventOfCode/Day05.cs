namespace AdventOfCode;

public class Day05 : BaseDay
{
    List<ulong> Seeds;
    Dictionary<string, (string toMapId, List<(ulong dest, ulong min, ulong max)> values)> Maps = new();

    public Day05() {
        var inputLines = File.ReadAllText(InputFilePath)
                             .Split($"{Environment.NewLine}{Environment.NewLine}");

        Seeds = inputLines[0].Substring("seeds: ".Length)
                             .Split(' ')
                             .Select(UInt64.Parse)
                             .ToList();

        foreach (var mapData in inputLines.Skip(1)) {
            var mapLines = mapData.Split(Environment.NewLine);
            var fromTo = mapLines[0].Substring(0, mapLines[0].IndexOf(' '))
                                    .Split("-to-");
            var lines = mapLines.Skip(1)
                                .SelectMany(o => o.Split(' ').Select(UInt64.Parse))
                                .Select((o, i) => (idx: i, val: o))
                                .GroupBy(o => o.idx / 3)
                                .Select(grp => grp.Select(o => o.val).ToArray())
                                .Select(o => (dest: o[0], min: o[1], max: o[1] + o[2] - 1))
                                .OrderBy(o => o.max)
                                .ToList();

            Maps[fromTo[0]] = (fromTo[1], lines);
        }
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    /// <summary>
    /// Iterate seeds one by one.
    /// </summary>
    ulong Part_1() {
        ulong minLocation = UInt32.MaxValue;

        foreach (var seed in Seeds) {
            var currentPos = seed;
            var toMapId = "seed";

            while (toMapId != "location") {
                foreach ((var dest, var min, var max) in Maps[toMapId].values) {
                    if (currentPos < min || currentPos > max)
                        continue;

                    currentPos = currentPos - min + dest;
                    break;
                }

                toMapId = Maps[toMapId].toMapId;
            }

            minLocation = Math.Min(minLocation, currentPos);
        }

        return minLocation;
    }

    /// <summary>
    /// A seed is now represented as a range instead of a single value.
    /// </summary>
    ulong Part_2() {
        UpdateMaps();

        var seedRanges = Seeds.Select((o, i) => (val: o, idx: i))
                              .GroupBy(o => o.idx / 2)
                              .Select(grp => grp.Select(o => o).ToArray())
                              .Select(o => (min: o[0].val, max: o[0].val + o[1].val));

        return seedRanges.Select(o => GetMinLocation("seed", UInt64.MaxValue, o.min, o.max))
                         .Min();
    }

    /// <summary>
    /// Remove outer bounds by filling range gaps to facilitate later splitting in <see cref="SplitRanges(string, ulong, ulong)"/>.
    /// </summary>
    void UpdateMaps() {
        foreach (var kvp in Maps) {
            var values = kvp.Value.values;

            foreach (var i in Enumerable.Range(1, values.Count - 1).Reverse().ToArray()) {
                var leftBound = values[i - 1].max;
                var rightBound = values[i].min;

                // in between gap
                if (leftBound + 1 < rightBound)
                    values.Insert(i, (dest: leftBound + 1, min: leftBound + 1, max: rightBound - 1));
            }

            // start gap
            if (values[0].min > 0)
                values.Insert(0, (dest: 0, min: 0, max: values[0].min - 1));

            // eng gap
            var last = values.Last();
            values.Add((dest: last.max + 1, min: last.max + 1, max: UInt64.MaxValue));
        }
    }

    /// <summary>
    /// Recurse through maps to find the min location.
    /// Use ranges to limit the search.
    /// </summary>
    ulong GetMinLocation(string fromMapId, ulong currentMinLocation, ulong rangeMin, ulong rangeMax) {
        if (fromMapId == "location")
            return rangeMin;

        var splittedRanges = SplitRanges(fromMapId, rangeMin, rangeMax);

        return splittedRanges.Select(o => GetMinLocation(Maps[fromMapId].toMapId, currentMinLocation, o.min, o.max))
                             .Min();
    }

    /// <summary>
    /// Subdivide a range if it covers many destination ranges.
    /// </summary>
    IEnumerable<(ulong min, ulong max)> SplitRanges(string fromMapId, ulong rangeMin, ulong rangeMax) {
        foreach ((var dest, var min, var max) in Maps[fromMapId].values) {
            if (rangeMin > max)
                continue;

            yield return (rangeMin - min + dest, UInt64.Min(max, rangeMax) - min + dest);

            rangeMin = max == UInt64.MaxValue ? rangeMax : UInt64.Min(max, rangeMax);

            if (rangeMin >= rangeMax)
                break;
        }
    }
}