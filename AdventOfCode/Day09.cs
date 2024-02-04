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

    int Part_1() {
        int ExtrapolatePrediction(Stack<List<int>> predictions, int currentHistory)
            => predictions.TryPop(out var prediction)
                ? ExtrapolatePrediction(predictions, currentHistory + prediction.Last())
                : currentHistory;

        return Enumerable.Range(0, Histories.Count)
                         .Select(i => ExtrapolatePrediction(BuildHistoryPrediction(Histories[i]), 0))
                         .Sum();
    }

    int Part_2() {
        int ExtrapolatePrediction(Stack<List<int>> predictions, int currentHistory)
            => predictions.TryPop(out var prediction)
                ? ExtrapolatePrediction(predictions, prediction.First() - currentHistory)
                : currentHistory;

        return Enumerable.Range(0, Histories.Count)
                         .Select(i => ExtrapolatePrediction(BuildHistoryPrediction(Histories[i]), 0))
                         .Sum();
    }

    Stack<List<int>> BuildHistoryPrediction(List<int> history) {
        List<int> SubstractHistory(List<int> history)
            => Enumerable.Range(0, history.Count - 1)
                         .Select(o => history[o + 1] - history[o])
                         .ToList();

        var result = new Stack<List<int>>();

        result.Push(history);

        while (!history.All(o => o == 0))
            result.Push(history = SubstractHistory(history));

        return result;
    }
}