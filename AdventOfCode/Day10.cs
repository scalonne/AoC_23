using System.Collections.Specialized;

namespace AdventOfCode;

public class Day10 : BaseDay
{
    record struct Tile(char Char, int X, int Y);

    Dictionary<int, Dictionary<int, Tile>> Map;
    int Height;
    int Width;

    public Day10() {
        Map = File.ReadLines(InputFilePath)
                  .SelectMany((o, y) => o.Select((c, x) => new Tile(c, x, y)))
                  .GroupBy(tiles => tiles.Y)
                  .ToDictionary(o => o.Key, o => o.ToDictionary(o => o.X, o => o));
        Height = Map.Count;
        Width = Map[0].Count;
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    HashSet<char> ToNorthChars = new() { '|', '7', 'F' };
    HashSet<char> ToSouthChars = new() { '|', 'L', 'J' };
    HashSet<char> ToEastChars =  new() { '-', 'J', '7' };
    HashSet<char> ToWestChars =  new() { '-', 'L', 'F' };

    Tile? GoNorth(Tile from)
        => from.Y > 0 && 
           ToNorthChars.Contains(Map[from.Y - 1][from.X].Char) ? Map[from.Y - 1][from.X] : null;

    Tile? GoSouth(Tile from)
        => from.Y < Height - 1 &&
           ToSouthChars.Contains(Map[from.Y + 1][from.X].Char) ? Map[from.Y + 1][from.X] : null;

    Tile? GoWest(Tile from)
        => from.X > 0 &&
           ToWestChars.Contains(Map[from.Y][from.X - 1].Char) ? Map[from.Y][from.X - 1] : null;

    Tile? GoEast(Tile from)
        => from.X < Width - 1 &&
           ToEastChars.Contains(Map[from.Y][from.X + 1].Char) ? Map[from.Y][from.X + 1] : null;

    Tile? GetNextTile(Tile current, Tile previous)
        => current.Char switch {
            '|' => previous.Y < current.Y ? GoSouth(current) : GoNorth(current),
            '7' => previous.X < current.X ? GoSouth(current) : GoWest(current),
            'F' => previous.X > current.X ? GoSouth(current) : GoEast(current),
            '-' => previous.X < current.X ? GoEast(current) : GoWest(current),
            'L' => previous.X > current.X ? GoNorth(current) : GoEast(current),
            'J' => previous.X < current.X ? GoNorth(current) : GoWest(current),
            _ => throw new Exception()
        };

    int Part_1() {
        var start = Map.SelectMany(o => o.Value.Values)
                       .First(o => o.Char == 'S');
        var nexts = new[] { GoNorth, GoSouth, GoWest, GoEast }.Select(o => o(start))
                                                              .Where(o => o != null)
                                                              .Select(o => (current: o.Value, previous: start))
                                                              .ToList();
        var path = new HashSet<Tile>(nexts.Select(o => o.current)) {
            start
        };
        var depth = 0;

        while (true) {
            for (var i = nexts.Count - 1; i >= 0; i--) {
                var nextTile = GetNextTile(nexts[i].current, nexts[i].previous);

                if (nextTile.HasValue && !path.Contains(nextTile.Value)) {
                    nexts[i] = (nextTile.Value, nexts[i].current);
                    path.Add(nextTile.Value);
                } else {
                    nexts.RemoveAt(i);
                }
            }

            depth++;

            if (nexts.Count == 0)
                return depth;
        }
    }

    /// <summary>
    /// Shoelace formula + Pick's theorem
    /// </summary>
    int Part_2() {
        var previous = Map.SelectMany(o => o.Value.Values)
                          .First(o => o.Char == 'S');
        var current = new[] { GoNorth, GoSouth, GoWest, GoEast }.Select(o => o(previous))
                                                                .First(o => o != null)
                                                                .Value;
        var path = new List<Tile>() {
            previous
        };

        for (Tile? nextTile; (nextTile = GetNextTile(current, previous)) != null;) {
            path.Add(current);
            previous = current;
            current = nextTile.Value;
        }

        // shoelace
        var vertices = path.Where(o => o.Char != '|' && o.Char != '-')
                           .Select(o => (o.X, o.Y))
                           .ToList();

        vertices.Add(vertices[0]);

        var area = Math.Abs(vertices.Take(vertices.Count - 1)
                                    .Select((p, i) => (p.X * vertices[i + 1].Y) - (p.Y * vertices[i + 1].X))
                                    .Sum() / 2);

        // pick
        // i = A - b/2 + 1
        return area - (path.Count + 1) / 2 + 1;
    }
}