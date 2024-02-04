using System.Drawing;

namespace AdventOfCode;

public class Day14 : BaseDay {
    const char ROCK = 'O';
    const char EMPTY = '.';
    const char WALL = '#';

    char[][] Map;
    int Height;
    int Width;

    enum Direction {
        North,
        East,
        South,
        West
    }

    Dictionary<Direction, Point> DirectionMoves = new() {
        { Direction.North, new Point(0, -1) },
        { Direction.East, new Point(1, 0) },
        { Direction.South, new Point(0, 1) },
        { Direction.West, new Point(-1, 0) }
    };

    public Day14() {
        Map = File.ReadLines(InputFilePath)
                  .Where(o => !String.IsNullOrWhiteSpace(o))
                  .Select(o => o.ToCharArray())
                  .ToArray();
        Height = Map.Length;
        Width = Map[0].Length;
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    int Part_1() {
        var rocks = Map.SelectMany((o, y) => o.Select((c, x) => (c, x))
                                      .Where(o => o.c == ROCK)
                                      .Select(o => new Point(o.x, y)));
        var res = 0;

        foreach (var p in rocks) {
            var x = p.X;
            var y = p.Y;
            var n = 0;

            while (y - 1 >= 0 && Map[y - 1][x] != WALL) {
                y--;

                if (Map[y][x] == ROCK)
                    n++;
            }

            res += Height - y - n;
        }

        return res;
    }

    int Part_2() {
        var rocks = Map.SelectMany((o, y) => o.Select((c, x) => (c, x))
                                              .Where(o => o.c == ROCK)
                                              .Select(o => new Point(o.x, y)))
                       .ToList();
        var cycle = new[] { Direction.North, Direction.West, Direction.South, Direction.East };
        var hashList = new List<(int hash, List<Point> rocks)>() { (GetHash(rocks), rocks) };

        while (true) {
            foreach (var direction in cycle) {
                foreach (var i in Enumerable.Range(0, rocks.Count)) {
                    var newX = rocks[i].X;
                    var newY = rocks[i].Y;
                    var move = DirectionMoves[direction];
                    var x = rocks[i].X + move.X;
                    var y = rocks[i].Y + move.Y;

                    while (x >= 0 && x < Width && y >= 0 && y < Height && Map[y][x] != WALL) {
                        if (Map[y][x] == EMPTY) {
                            newX = x;
                            newY = y;
                        }
                        x += move.X;
                        y += move.Y;
                    }

                    Map[rocks[i].Y][rocks[i].X] = EMPTY;
                    rocks[i] = new Point(newX, newY);
                    Map[newY][newX] = ROCK;
                }
            }

            var hash = GetHash(rocks);
            var hashIndex = hashList.FindIndex(o => o.hash == hash);

            if (hashIndex < 0) {
                hashList.Add((hash, rocks.ToList()));
                continue;
            }

            var cyclesLeft = (1_000_000_000 - hashIndex) % (hashList.Count - hashIndex);

            return hashList[hashIndex + cyclesLeft].rocks.Sum(o => Height - o.Y);
        }

        int GetHash(List<Point> points)
            => points.Aggregate(0, (hash, point) => hash ^ point.GetHashCode());
    }
}