using System.Collections.Frozen;

namespace AdventOfCode;

public class Day16 : BaseDay
{
    char[][] Map;
    int Height;
    int Width;

    [Flags]
    enum Direction {
        Up,
        Down,
        Left,
        Right,
        Vertical = Up | Down, // avoid going trough a cell twice when coming from the opposite direction
        Horizontal = Left | Right
    }

    static FrozenDictionary<Direction, Point> PointByDirection = new Dictionary<Direction, Point>() {
        { Direction.Up,     new Point(0, -1) },
        { Direction.Down,   new Point(0, 1) },
        { Direction.Left,   new Point(-1, 0) },
        { Direction.Right,  new Point(1, 0) }
    }.ToFrozenDictionary();

    static FrozenDictionary<char, Dictionary<Direction, (Direction[] directions, Direction axis)>> DirectionsByCell = new Dictionary<char, Dictionary<Direction, (Direction[], Direction)>>() {
        { 
            '.', new() {
                { Direction.Up,    (new[] { Direction.Up    }, Direction.Vertical)   },
                { Direction.Down,  (new[] { Direction.Down  }, Direction.Vertical)   },
                { Direction.Left,  (new[] { Direction.Left  }, Direction.Horizontal) },
                { Direction.Right, (new[] { Direction.Right }, Direction.Horizontal) }
            }
        },
        {
            '-', new() {
                { Direction.Up,    (new[] { Direction.Left, Direction.Right }, Direction.Vertical)   },
                { Direction.Down,  (new[] { Direction.Left, Direction.Right }, Direction.Vertical)   },
                { Direction.Left,  (new[] { Direction.Left  },                 Direction.Horizontal) },
                { Direction.Right, (new[] { Direction.Right },                 Direction.Horizontal) }
            }
        },
        {
            '|', new() {
                { Direction.Up,    (new[] { Direction.Up   },               Direction.Vertical) },
                { Direction.Down,  (new[] { Direction.Down },               Direction.Vertical) },
                { Direction.Left,  (new[] { Direction.Up, Direction.Down }, Direction.Horizontal) },
                { Direction.Right, (new[] { Direction.Up, Direction.Down }, Direction.Horizontal) }
            }
        },
        {
            '/', new() {
                { Direction.Up,    (new[] { Direction.Right }, Direction.Right) },
                { Direction.Down,  (new[] { Direction.Left  }, Direction.Left)  },
                { Direction.Left,  (new[] { Direction.Down  }, Direction.Down)  },
                { Direction.Right, (new[] { Direction.Up    }, Direction.Up)    }
            }
        },
                {
            '\\', new() {
                { Direction.Up,    (new[] { Direction.Left  }, Direction.Left)  },
                { Direction.Down,  (new[] { Direction.Right }, Direction.Right) },
                { Direction.Left,  (new[] { Direction.Up    }, Direction.Up)    },
                { Direction.Right, (new[] { Direction.Down  }, Direction.Down)  }
            }
        }
    }.ToFrozenDictionary();

    public Day16() {
        Map = File.ReadLines(InputFilePath)
                  .Select(o => o.ToCharArray())
                  .ToArray();
        Height = Map.Length;
        Width = Map[0].Length;
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    int ComputePathLength(Point startPos, Direction startDir) {
        var beams = new List<(Point, Direction)>() { (startPos, startDir) };
        var path = new HashSet<(int x, int y, Direction direction)>();

        while (beams.Count > 0) {
            for (var i = beams.Count - 1; i >= 0; i--) {
                (var beam, var direction) = beams[i];
                var directionPoint = PointByDirection[direction];
                var x = beam.X + directionPoint.X;
                var y = beam.Y + directionPoint.Y;

                // out of bounds or already visited
                if (x < 0 || x >= Width || y < 0 || y >= Height || !path.Add((x, y, DirectionsByCell[Map[y][x]][direction].axis))) {
                    beams.RemoveAt(i);
                    continue;
                }

                // success
                beam.X = x;
                beam.Y = y;

                var nextDirections = DirectionsByCell[Map[y][x]][direction].directions;

                // change direction
                if (nextDirections[0] != direction)
                    beams[i] = (beam, nextDirections[0]);

                // split
                if (nextDirections.Length > 1)
                    beams.Add((new Point(x, y), nextDirections[1]));
            }
        }

        return path.Select(o => (o.x, o.y))
                   .Distinct()
                   .Count();
    }

    int Part_1()
        => ComputePathLength(new Point(-1, 0), Direction.Right);

    int Part_2() {
        var max = 0;

        // top to bottom
        foreach (var x in Enumerable.Range(0, Width))
            max = Math.Max(max, ComputePathLength(new Point(x, -1), Direction.Down));

        // bottom to top
        foreach (var x in Enumerable.Range(0, Width))
            max = Math.Max(max, ComputePathLength(new Point(x, Height), Direction.Up));

        // left to right
        foreach (var y in Enumerable.Range(0, Height))
            max = Math.Max(max, ComputePathLength(new Point(-1, y), Direction.Right));

        // right to left
        foreach (var y in Enumerable.Range(0, Height))
            max = Math.Max(max, ComputePathLength(new Point(Width, y), Direction.Left));

        return max;
    }

    class Point(int X, int Y) {
        public int X { get; set; } = X;
        public int Y { get; set; } = Y;
    }
}