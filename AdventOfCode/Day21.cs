namespace AdventOfCode;

public class Day21 : BaseDay
{
    record struct Cell(int X, int Y, char C) {
        public char C { get; set; } = C;
    }

    Cell[][] Map;
    int Height;
    int Width;

    public Day21() {
        Map = File.ReadLines(InputFilePath)
                  .Where(o => !String.IsNullOrEmpty(o))
                  .Select((o, y) => o.Select((c, x) => new Cell(x, y, c))
                                     .ToArray())
                  .ToArray();
        Height = Map.Length;
        Width = Map[0].Length;
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    Cell GetStartingPosition()
        => Map.SelectMany(o => o)
              .First(o => o.C == 'S');

    void Display(HashSet<Cell> pos, int steps) {
        Console.SetCursorPosition(0, 0);
        Console.WriteLine(steps);

        for (var y = 0; y < Height; y++) {
            for (var x = 0; x < Width; x++) {
                Console.Write(Map[y][x].C == '#' ? '#' : ' ');
            }
            Console.WriteLine();
        }

        Console.ForegroundColor = ConsoleColor.DarkRed;
        foreach (var cell in pos) {
            Console.SetCursorPosition(cell.X, cell.Y + 1);
            Console.Write('O');
        }
        Console.ResetColor();
    }

    IEnumerable<Cell> GetNextPositions(Cell[][] map, Cell pos) {
        var x = pos.X;
        var y = pos.Y;

        if (x > 0 && map[y][x - 1].C != '#')
            yield return map[y][x - 1];

        if (x < map[y].Length - 1 && map[y][x + 1].C != '#')
            yield return map[y][x + 1];

        if (y > 0 && map[y - 1][x].C != '#')
            yield return map[y - 1][x];

        if (y < map.Length - 1 && map[y + 1][x].C != '#')
            yield return map[y + 1][x];
    }

    HashSet<Cell> Move(Cell[][] map, HashSet<Cell> currentPositions, int steps) {
        if (steps == 0)
            return new(currentPositions);

        // avoid iterating over all positions again and again
        // - get nexts only for recently added positions
        // - since O and . alternate, jugle w/ two sets accordingly to parity
        var arr = new (HashSet<Cell> positions, HashSet<Cell> newPosBuffer)[2] { (new(currentPositions), new(currentPositions)), (new(), new()) };

        foreach (var i in Enumerable.Range(0, steps)) {
            (var a, var b) = arr[i % 2];
            (var c, var d) = arr[(i + 1) % 2];

            d.Clear();

            foreach (var cell in b) {
                foreach (var next in GetNextPositions(map, cell)) {
                    if (c.Add(next))
                        d.Add(next);
                }
            }

            //Display(c, i + 1);
            //Console.ReadKey();
        }

        return arr[steps % 2].positions;
    }

    int Part_1() {
        var pos = new HashSet<Cell>([GetStartingPosition()]);
        var steps = 64;
        var nextPos = Move(Map, pos, steps);

        return nextPos.Count;
    }

    ulong Part_2() {
        var EXPEND_SIZE = 2;

        // 1. Infinity is simulated by duplicating the matrix into a bigger one
        var newHeight = Height + Height * EXPEND_SIZE * 2;
        var newMap = new Cell[newHeight][];
        var start = GetStartingPosition();

        foreach (var newY in Enumerable.Range(0, newHeight)) {
            newMap[newY] = new Cell[newHeight];

            foreach (var newX in Enumerable.Range(0, newHeight)) {
                var cell = Map[newY % Height][newX % Width];

                newMap[newY][newX] = new Cell(newX, newY, cell.C);
            }
        }
        
        // 2. As for Part_1, compute and store number of steps, repeated (1 + EXPEND_SIZE) times
        var steps = 26_501_365;
        var left = steps % Width;
        var newStart = newMap[start.Y + EXPEND_SIZE * Height][start.X + EXPEND_SIZE * Height];
        var locations = new HashSet<Cell>([newStart]);
        var results = new List<(int steps, ulong count)>(locations.Count);

        // 65: 3_882
        locations = Move(newMap, locations, left);
        results.Add((left, (ulong)locations.Count));

        // 196: 34_441
        // 327: 95_442
        foreach (var i in Enumerable.Range(1, EXPEND_SIZE)) {
            locations = Move(newMap, locations, Width);
            results.Add((left + Width * i, (ulong)locations.Count));

            //Console.WriteLine($"steps: {left + Width*i} count: {locations.Count}");
        }

        // 3. Use day 9's prediction algorithm to predict the next 202_298 values
        // 458: ?
        // ...: ?
        // 202_298: ?
        var predictCounter = (steps - left - Width * EXPEND_SIZE) / Width;
        var predictions = Day09.PredictNextValues(results.Select(o => o.count), predictCounter)
                               .Select((o, i) => (steps: results[^1].steps + Width, count: o));

        results.AddRange(predictions);

        return results[^1].count;
    }
}