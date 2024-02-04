namespace AdventOfCode;

public class Day04 : BaseDay
{
    (List<int> WinningNumbers, List<int> CardNumbers)[] Cards;

    public Day04() {
        // Card 1: 41 48 83 86 17 | 83 86  6 31 17  9 48 53
        Cards = File.ReadLines(InputFilePath)
                    .Select(o => o.Substring(o.IndexOf(':') + 2)
                                  .Split(" | ")
                                  .Select(o => o.Split(' ')
                                                .Where(o => !String.IsNullOrWhiteSpace(o))
                                                .Select(Int32.Parse)))
                    .Select(o => (WinningNumbers: o.First().ToList(), CardNumbers: o.Last().ToList()))
                    .ToArray();
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    int Part_1()
        => Cards.Select(o => o.WinningNumbers.Intersect(o.CardNumbers).Count())
                .Where(o => o > 0)
                .Sum(o => (int)Math.Pow(2, o - 1));

    int Part_2() {
        var decks = Cards.Select(o => (o.WinningNumbers, o.CardNumbers, Quantity: 1)).ToArray();

        foreach (var i in Enumerable.Range(0, decks.Length)) {
            var deck = decks[i];
            var matchingNumbers = deck.WinningNumbers.Intersect(deck.CardNumbers).Count();

            while (matchingNumbers > 0) {
                decks[i + matchingNumbers].Quantity += deck.Quantity;
                matchingNumbers--;
            }
        }

        return decks.Sum(o => o.Quantity);
    }
}