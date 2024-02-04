using System.Data;

namespace AdventOfCode;

public class Day12 : BaseDay
{
    List<(char[] map, int[] mines)> Inputs;
    Dictionary<string, ulong> Cache = new();

    public Day12() {
        Inputs = File.ReadLines(InputFilePath)
                     .Where(o => !String.IsNullOrEmpty(o))
                     .Select(o => o.Split(' '))
                     .Select(o => (map: o[0].ToCharArray(), mines: o[1].Split(',')
                                                                       .Select(Int32.Parse)
                                                                       .ToArray()))
                     .ToList();
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    ulong InsertMines(char[] map, int[] mines) {
        // test completion
        if (mines.Length == 0)
            return map.Any(o => o == '#') ? 0ul : 1ul;

        // trim
        var i = 0;
        while (map[i] == '.')
            i++;

        int TryInsert(char[] cpy, int n) {
            if (n > cpy.Length)
                return -1;

            var i = 0;

            while (i < n) {
                if (cpy[i] == '.')
                    return -1;
                cpy[i++] = '#';
            }

            if (i < cpy.Length) {
                if (cpy[i] == '#')
                    return -1;
                cpy[i] = '.';
            }

            return i;
        }

        // insert
        var cpy = map[i..];
        var n = mines[0];
        var x = TryInsert(cpy, n);

        // success: go next mine
        if (x > 0)
            return ComputeArrangements(cpy[x..], mines[1..]);

        // failure
        return 0;
    }

    ulong ComputeArrangements(char[] map, int[] mines) {
        if (mines.Length == 0)
            return map.Any(o => o == '#') ? 0ul : 1ul;

        var cacheKey = new String(map) + new String(mines.Select(o => (char)(o + '0')).ToArray());
        if (Cache.TryGetValue(cacheKey, out var arrangements))
            return arrangements;

        for (var i = 0; i < map.Length; i++) {
            if (map[i] == '.')
                continue;

            arrangements += InsertMines(map[i..], mines);

            if (map[i] == '#')
                break;
        }

        Cache[cacheKey] = arrangements;
        return arrangements;
    }

    ulong Part_1() {
        var arrangements = 0ul;

        foreach ((var map, var mines) in Inputs)
            arrangements += ComputeArrangements(map, mines);

        return arrangements;
    }

    ulong Part_2() {
        var arrangements = 0ul;

        foreach ((var map, var mines) in Inputs) {
            var mapCpy = new char[map.Length * 5 + 4];
            var minesCpy = new int[mines.Length * 5];

            foreach (var i in Enumerable.Range(0, 5)) {
                Array.Copy(map, 0, mapCpy, (map.Length + 1) * i, map.Length);
                if (i < 4)
                    mapCpy[(map.Length + 1) * i + map.Length] = '?';

                Array.Copy(mines, 0, minesCpy, i * mines.Length, mines.Length);
            }

            arrangements += ComputeArrangements(mapCpy, minesCpy);
        }

        return arrangements;
    }
}