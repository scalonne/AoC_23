using CollectionsUtils;

namespace AdventOfCode;

public class Day13 : BaseDay
{
    List<char[][]> Maps;

    public Day13() {
        Maps = File.ReadAllText(InputFilePath)
                   .Split($"{Environment.NewLine}{Environment.NewLine}")
                   .Select(o => o.Split(Environment.NewLine)
                                 .Select(o => o.ToCharArray())
                                 .ToArray())
                   .ToList();
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    int Compute(Func<char[][], int> getMirrorLocationFunc) {
        var sum = 0;

        foreach (var map in Maps) {
            var y = getMirrorLocationFunc(map);

            if (y >= 0) {
                sum += 100 * (y + 1);
            } else {
                y = getMirrorLocationFunc(map.Rotate(clockWise: true));
                sum += y + 1;
            }
        }

        return sum;
    }

    int Part_1() {
        int GetMirrorLocation(char[][] map) {
            foreach (var y in Enumerable.Range(0, map.Length - 1)) {
                if (!map[y].Select((c, i) => (c, i))
                           .All(o => o.c == map[y + 1][o.i]))
                    continue;

                var equals = true;
                var n = Math.Min(y, (map.Length - 1) - (y + 1));

                while (n > 0 && equals) {
                    foreach (var x in Enumerable.Range(0, map[0].Length)) {
                        if (map[y - n][x] != map[y + 1 + n][x]) {
                            equals = false;
                            break;
                        }
                    }
                    n--;
                }

                if (equals)
                    return y;
            }

            return -1;
        }

        return Compute(GetMirrorLocation);
    }

    int Part_2() {
        int GetMirrorLocation(char[][] map) {
            int sludges;

            foreach (var y in Enumerable.Range(0, map.Length - 1)) {
                sludges = 0;

                foreach (var x in Enumerable.Range(0, map[0].Length)) {
                    if (map[y][x] != map[y + 1][x]) {
                        if (++sludges > 1) {
                            break;
                        }
                    }
                }

                var equals = true;
                var n = Math.Min(y, (map.Length - 1) - (y + 1));

                while (n > 0 && equals) {
                    foreach (var x in Enumerable.Range(0, map[0].Length)) {
                        if (map[y - n][x] != map[y + 1 + n][x]) {
                            if (++sludges > 1) {
                                equals = false;
                                break;
                            }
                        }
                    }
                    n--;
                }

                if (equals && sludges == 1)
                    return y;
            }

            return -1;
        }

        return Compute(GetMirrorLocation);
    }
}