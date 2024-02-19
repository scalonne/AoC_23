namespace AdventOfCode;

public class Day09 : BaseDay
{
    List<List<int>> Histories;

    public Day09() {
        Histories = File.ReadLines(InputFilePath)
                        .Select(o => o.Split(' ')
                                      .Select(Int32.Parse).ToList())
                        .ToList();
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    static List<List<T>> GenerateDeltas<T>(IEnumerable<T> sequence) where T : struct {
        List<T> GetDelta(List<T> values)
            => Enumerable.Range(0, values.Count - 1)
                         .Select(o => (dynamic)values[o + 1] - values[o])
                         .Cast<T>()
                         .ToList();

        var deltas = new List<List<T>>([sequence.ToList()]);

        while (!deltas[^1].All(o => o.Equals(deltas[^1][0])))
            deltas.Add(GetDelta(deltas[^1]));

        return deltas;
    }

    /// <summary>
    /// Predict n next values of a growing sequence.
    /// https://youtu.be/4AuV93LOPcE?si=nuZ_C3lmvPAeImHg
    /// </summary>
    public static List<T> PredictNextValues<T>(IEnumerable<T> sequence, int n) where T : struct {
        if (n == 0)
            return new();

        var deltas = GenerateDeltas(sequence);
        var baseIncr = deltas[^1][0];

        while (n-- > 0) {
            deltas[^1].Add(baseIncr);

            for (var i = deltas.Count - 2; i >= 0; i--)
                deltas[i].Add((dynamic)deltas[i][^1] + deltas[i + 1][^1]);
        }

        return deltas[0].Skip(sequence.Count())
                        .ToList();
    }

    int Part_1()
        => Enumerable.Range(0, Histories.Count)
                     .Select(i => PredictNextValues(Histories[i], 1)[0])
                     .Sum();

    int Part_2() {
        int PredictPreviousValues(List<int> sequence)
            => GenerateDeltas(sequence).Select(o => o[0])
                                       .Reverse()
                                       .Aggregate((a, b) => b - a);

        return Enumerable.Range(0, Histories.Count)
                         .Select(i => PredictPreviousValues(Histories[i]))
                         .Sum();
    }
}