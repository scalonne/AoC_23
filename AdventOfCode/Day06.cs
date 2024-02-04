namespace AdventOfCode;

public class Day06 : BaseDay
{
    List<(int time, int distance)> Races = new();

    public Day06() {
        var lines = File.ReadAllLines(InputFilePath)
                        .Select(o => o.Split(' ')
                                      .Skip(1)
                                      .Where(o => !String.IsNullOrWhiteSpace(o))
                                      .Select(o => Int32.Parse(o.Trim()))
                                      .ToArray())
                        .ToArray();

        foreach (var x in Enumerable.Range(0, lines[0].Length))
            Races.Add((time: lines[0][x], distance: lines[1][x]));
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    int Part_1() {
        var r = 1;

        foreach ((var time, var distance) in Races) {
            var z = 0;

            for (var i = 0; i < time; i++) {
                if (i * (time - i) > distance) {
                    z++;
                }
            }   

            r *= z;
        }

        return r;
    }

    ulong Part_2() {
        var flattenedRace = Races.Select(o => (time: o.time.ToString(), distance: o.distance.ToString()))
                                 .Aggregate((o, p) => (o.time + p.time, o.distance + p.distance));
        var race = (time: UInt64.Parse(flattenedRace.time), distance: UInt64.Parse(flattenedRace.distance));
        var start = 0ul;
        var end = 0ul;

        bool CanWin(ulong i)
            => i * (race.time - i) > race.distance;

        for (var i = 0ul; i < race.time; i++) {
            if (CanWin(i)) {
                start = i;
                break;
            }
        }

        for (var i = race.time; i > 0; i--) {
            if (CanWin(i)) {
                end = i;
                break;
            }
        }

        return end - start + 1;
    }
}