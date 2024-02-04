namespace AdventOfCode;

public class Day07 : BaseDay {
    enum HandValues {
        HighCard,
        OnePair,
        TwoPairs,
        ThreeOfAKind,
        FullHouse,
        FourOfAKind,
        FiveOfAKind
    }

    List<(string cards, int bid)> Games;

    public Day07() {
        Games = File.ReadLines(InputFilePath)
                    .Select(o => o.Split(' ').ToArray())
                    .Select(o => (cards: o[0], bid: Int32.Parse(o[1])))
                    .ToList();
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    HandValues GetHandValue(List<int> cardCounts)
        => cardCounts switch {
               [5] => HandValues.FiveOfAKind,
               [4, _] => HandValues.FourOfAKind,
               [3, 2] => HandValues.FullHouse,
               [3, .. _] => HandValues.ThreeOfAKind,
               [2, 2, _] => HandValues.TwoPairs,
               [2, ..] => HandValues.OnePair,
               _ => HandValues.HighCard
           };

    int Part_1() {
        Dictionary<char, char> valueByCard = new() {
            { '2', '2' },
            { '3', '3' },
            { '4', '4' },
            { '5', '5' },
            { '6', '6' },
            { '7', '7' },
            { '8', '8' },
            { '9', '9' },
            { 'T', 'A' },
            { 'J', 'B' },
            { 'Q', 'C' },
            { 'K', 'D' },
            { 'A', 'E' }
        };

        return Games.Select(o => (cards: new String(o.cards.Select(o => valueByCard[o]).ToArray()), o.bid))
                    .Select(o => (o.cards,
                                  o.bid,
                                  handValue: GetHandValue(o.cards.Distinct()
                                                                 .Select(card => o.cards.Count(c => c == card))
                                                                 .OrderByDescending(o => o)
                                                                 .ToList())))
                    .OrderBy(o => o.handValue)
                    .ThenBy(o => o.cards)
                    .Select((o, i) => o.bid * (i + 1))
                    .Sum();
    }

    int Part_2() {
        const char JokerValue = '1';
        Dictionary<char, char> valueByCard = new() {
            { '2', '2' },
            { '3', '3' },
            { '4', '4' },
            { '5', '5' },
            { '6', '6' },
            { '7', '7' },
            { '8', '8' },
            { '9', '9' },
            { 'T', 'A' },
            { 'J', JokerValue },
            { 'Q', 'C' },
            { 'K', 'D' },
            { 'A', 'E' }
        };

        HandValues JokerHandValue(string cards) {
            var cardCounts = cards.Distinct()
                                  .Select(o => (card: o, count: cards.Count(c => c == o)))
                                  .OrderByDescending(o => o.count)
                                  .ToList();

            var jokerIndex = cardCounts.FindIndex(o => o.card == JokerValue);
            if (jokerIndex >= 0 && cardCounts.Count > 1) {
                var jokerCount = cardCounts[jokerIndex].count;
                var i = jokerIndex == 0 ? 1 : 0;

                cardCounts[i] = (cardCounts[i].card, cardCounts[i].count + jokerCount);
                cardCounts.RemoveAt(jokerIndex);
            }

            return GetHandValue(cardCounts.Select(o => o.count).ToList());
        }

        return Games.Select(o => (cards: new String(o.cards.Select(o => valueByCard[o]).ToArray()), o.bid))
                    .Select(o => (o.cards, o.bid, handValue: JokerHandValue(o.cards)))
                    .OrderBy(o => o.handValue)
                    .ThenBy(o => o.cards)
                    .Select((o, i) => o.bid * (i + 1))
                    .Sum();
    }
}