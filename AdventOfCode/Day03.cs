namespace AdventOfCode;

public class Day03 : BaseDay
{
    readonly Cell[][] Map;
    int Height { get; }
    int Width { get; }

    public Day03() {
        var rawMap = File.ReadAllLines(InputFilePath)
                         .Select(o => o.ToArray())
                         .ToArray();
        Height = rawMap.Length;
        Width = rawMap[0].Length;
        Map = new Cell[Height][];

        foreach (var y in Enumerable.Range(0, Height)) {
            Map[y] = new Cell[Width];

            foreach (var x in Enumerable.Range(0, Width)) {
                if (Map[y][x] != null || rawMap[y][x] == '.')
                    continue;

                if (!Char.IsDigit(rawMap[y][x])) {
                    Map[y][x] = new Cell(x, y) { Gear = rawMap[y][x] };
                    continue;
                }

                var numberCell = new Cell(x, y);
                var i = x;

                Map[y][i++] = numberCell;
                while (i < Width && Char.IsDigit(rawMap[y][i]))
                    Map[y][i++] = numberCell;

                numberCell.Number = Int32.Parse(rawMap[y][x..i]);
            }
        }
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    int Part_1()
            => Map.SelectMany(cell => cell.Where(o => o?.IsGear == true))
                  .Select(gearCell => Enumerable.Range(-1, 3)
                                                .SelectMany(x => Enumerable.Range(-1, 3)
                                                                           .Select(y => (X: x + gearCell.X, Y: y + gearCell.Y)))
                                                .Where(o => o.X >= 0 && o.X < Width &&
                                                            o.Y >= 0 && o.Y < Height &&
                                                            Map[o.Y][o.X]?.IsNumber == true)
                                                .Select(o => Map[o.Y][o.X])
                                                .Distinct()
                                                .Sum(o => o.Number.Value))
                   .Sum();

    /// <summary>
    /// A gear is any * symbol that is adjacent to exactly two part numbers.
    /// Its gear ratio is the result of multiplying those two numbers together.
    /// </summary>
    int Part_2()
        => Map.SelectMany(o => o.Where(o => o?.Gear == '*'))
              .Select(gearCell => Enumerable.Range(-1, 3)
                                            .SelectMany(x => Enumerable.Range(-1, 3)
                                                                       .Select(y => (X: x + gearCell.X, Y: y + gearCell.Y)))
                                            .Where(o => o.X >= 0 && o.X < Width &&
                                                        o.Y >= 0 && o.Y < Height &&
                                                        Map[o.Y][o.X]?.IsNumber == true)
                                            .Select(o => Map[o.Y][o.X])
                                            .Distinct())
              .Where(o => o.Count() == 2)
              .Select(o => o.First().Number.Value * o.Last().Number.Value)
              .Sum();

    record Cell(int X, int Y)
    {
        public char Gear { get; init; }
        public int? Number { get; set; }
        public bool IsNumber => Number.HasValue;
        public bool IsGear => !IsNumber;
    }
}