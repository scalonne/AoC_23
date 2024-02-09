namespace AdventOfCode;

public class Day18 : BaseDay
{
    List<(char, int, string)> Lines;

    public Day18() {
        Lines = File.ReadLines(InputFilePath)
                    .Select(o => o.Split(' '))
                    .Select(o => (o[0][0], Int32.Parse(o[1]), o[2][2..^1]))
                    .ToList();
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    public Dictionary<char, (int x, int y)> DirectionByChar = new() {
        { 'R', (1, 0) },
        { '0', (1, 0) },
        { 'D', (0, 1) },
        { '1', (0, 1) },
        { 'L', (-1, 0) },
        { '2', (-1, 0) },
        { 'U', (0, -1) },
        { '3', (0, -1) }
    };

    /// <summary>
    /// Copied from day10, shoelace + pick.
    /// </summary>
    long GetArea(List<(long x, long y)> vertices, long perimeter) {
        var area = Math.Abs(vertices.Take(vertices.Count - 1)
                                    .Select((p, i) => (p.x * vertices[i + 1].y) - (p.y * vertices[i + 1].x))
                                    .Sum() / 2);

        return area + perimeter / 2 + 1;
    }

    long Part_1() {
        var pos = (x: 0, y: 0);
        var vertices = new List<(long x, long y)>() { pos };
        var perimeter = 0;

        foreach ((var cDir, var n, _) in Lines) {
            (var x, var y) = DirectionByChar[cDir];

            perimeter += n;
            pos = (pos.x + x * n, pos.y + y * n);
            vertices.Add(pos);
        }

        return GetArea(vertices, perimeter);
    }

    long Part_2() {
        var pos = (x: 0, y: 0);
        var vertices = new List<(long x, long y)>() { pos };
        var perimeter = 0;

        foreach ((var _, var _, var hex) in Lines) {
            (var x, var y) = DirectionByChar[hex[^1]];
            var n = Convert.ToInt32(hex[..^1], 16);

            perimeter += n;
            pos = (pos.x + x * n, pos.y + y * n);
            vertices.Add(pos);
        }

        return GetArea(vertices, perimeter);
    }
}