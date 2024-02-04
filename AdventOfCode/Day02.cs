namespace AdventOfCode;

public class Day02 : BaseDay
{
    Dictionary<int, IEnumerable<Subset>> Games;

    public Day02()
    {
        // Game 1: 7 blue, 4 red, 11 green; 2 red, 2 blue, 7 green; 2 red, 13 blue, 8 green; 18 blue, 7 green, 5 red
        Games = File.ReadLines(InputFilePath)
                    .Select((o, i) => (id: i + 1, subsets: o.Substring(o.IndexOf(':') + 2)
                                                            .Split("; ")
                                                            .Select(o => new Subset(o.Split(", ")))))
                    .ToDictionary(o => o.id, o => o.subsets);
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    /// <summary>
    /// The Elf would first like to know which games would have been possible
    /// if the bag contained only 12 red cubes, 13 green cubes, and 14 blue cubes?
    /// </summary>
    int Part_1()
        => Games.Where(kvp => kvp.Value.All(o => o.Red <= 12 && o.Green <= 13 && o.Blue <= 14))
                .Sum(o => o.Key);

    int Part_2()
        => Games.Values.Select(o => o.Max(o => o.Red) * o.Max(o => o.Green) * o.Max(o => o.Blue))
                       .Sum();

    record Subset
    {
        public int Red { get; }
        public int Green { get; }
        public int Blue { get; }

        // [ "7 blue", "4 red", "11 green" ]
        internal Subset(string[] colors) {
            foreach ((var quantity, var color) in colors.Select(o => o.Split(" "))
                                                        .Select(o => (Int32.Parse(o[0]), o[1])))
                switch (color) {
                    case "red":
                        Red = quantity;
                        break;
                    case "green":
                        Green = quantity;
                        break;
                    case "blue":
                        Blue = quantity;
                        break;
                }
        }
    }
}