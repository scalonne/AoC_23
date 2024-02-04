using System.Drawing;

namespace AdventOfCode;

public class Day11 : BaseDay
{
    List<string> RawMap;

    public Day11() {
        RawMap = File.ReadLines(InputFilePath)
                     .ToList();
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    List<string> ExpandMap() {
        var copy = RawMap.ToList();

        foreach (var y in EnumerableHelper.Sequence(copy.Count - 1, copy.Count, -1)) {
            if (copy[y].All(o => o == '.')) {
                copy.Insert(y, copy[y]);
            }
        }

        foreach (var x in EnumerableHelper.Sequence(copy[0].Length - 1, copy[0].Length, -1)) {
            if (copy.All(o => o[x] == '.')) {
                foreach (var y in Enumerable.Range(0, copy.Count))
                    copy[y] = copy[y].Insert(x, ".");
            }
        }

        return copy;
    }

    int Part_1() {
        var map = ExpandMap().Select(o => o.ToCharArray())
                             .ToArray();
        var galaxies = map.SelectMany((o, y) => o.Select((c, x) => (c, coords: new Point(x, y))))
                          .Where(o => o.c == '#')
                          .Select(o => o.coords)
                          .ToList();
        var sum = 0;

        foreach (var i in Enumerable.Range(0, galaxies.Count)) {
            foreach (var j in Enumerable.Range(i + 1, galaxies.Count - (i + 1))) {
                var manhattan = Math.Abs(galaxies[i].X - galaxies[j].X) + Math.Abs(galaxies[i].Y - galaxies[j].Y);

                sum += manhattan;
            }
        }

        return sum;
    }

    List<Point> GetExpandLocations() {
        var expandPoints = new List<Point>();

        foreach (var y in EnumerableHelper.Sequence(RawMap.Count - 1, RawMap.Count, -1)) {
            if (RawMap[y].All(o => o == '.')) {
                expandPoints.Add(new Point(-1, y));
            }
        }

        foreach (var x in EnumerableHelper.Sequence(RawMap[0].Length - 1, RawMap[0].Length, -1)) {
            if (RawMap.All(o => o[x] == '.')) {
                expandPoints.Add(new Point(x, -1));
            }
        }

        return expandPoints;
    }

    /// <summary>
    /// No longer update map but save expand rows/columns and check overlaps.
    /// </summary>
    long Part_2() {
        var map = RawMap.Select(o => o.ToCharArray())
                        .ToArray();
        var expandLocations = GetExpandLocations();
        var galaxies = map.SelectMany((o, y) => o.Select((c, x) => (c, coords: new Point(x, y))))
                          .Where(o => o.c == '#')
                          .Select(o => o.coords)
                          .ToList();
        var sum = 0L;

        foreach (var i in Enumerable.Range(0, galaxies.Count)) {
            foreach (var j in Enumerable.Range(i + 1, galaxies.Count - (i + 1))) {
                var p1 = galaxies[i];
                var p2 = galaxies[j];
                var manhattan = Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);

                foreach (var expandLocation in expandLocations) {
                    if (expandLocation.X > Math.Min(p1.X, p2.X) && expandLocation.X < Math.Max(p1.X, p2.X) ||
                        expandLocation.Y > Math.Min(p1.Y, p2.Y) && expandLocation.Y < Math.Max(p1.Y, p2.Y)) {
                        sum += 1_000_000 - 1;
                    }
                }

                sum += manhattan;
            }
        }

        return sum;
    }
}