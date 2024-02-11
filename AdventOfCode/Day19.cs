namespace AdventOfCode;

public class Day19 : BaseDay
{
    record RatingExpression(int id, char op, int v) {
        public int LeftSideRatingIndex = id;
        public char BinaryOperator = op;
        public int RightSideValue = v;

        /// <summary>
        /// Part 1
        /// Int comparison
        /// </summary>
        public bool Evaluate(int[] ratings) {
            var left = ratings[LeftSideRatingIndex];

            return BinaryOperator switch {
                '>' => left > RightSideValue,
                '<' => left < RightSideValue,
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Part 2
        /// Range comparison
        /// </summary>
        public (Range? ok, Range? ko) Evaluate(Range left) {
            var s = left.Start.Value;
            var e = left.End.Value;

            if (BinaryOperator == '<') {
                if (s < RightSideValue) {
                    // end< [1..5]  < 10   [1..5][]
                    // end= [1..10] < 10   [1..9][10]
                    // end> [1..20] < 10   [1..9][10..20]
                    if (e < RightSideValue)
                        return (left, default);

                    if (e == RightSideValue)
                        return (new(s, e - 1), new(e, e));

                    if (e > RightSideValue)
                        return (new(s, RightSideValue - 1), new(RightSideValue, e));
                }

                if (left.Start.Value >= RightSideValue) {
                    // end= [10..10] < 10   [][10]
                    // end= [11..11] < 10   [][11]
                    // end> [10..20] < 10   [][10..20]
                    // end> [11..20] < 10   [][11..20]
                    return (default, left);
                }
            }

            if (BinaryOperator == '>') {
                if (s < RightSideValue) {
                    // end<  [1..9]  > 10   [][1..9]
                    // end=  [1..10] > 10   [][1..10]
                    // end>  [1..20] > 10   [11..20][1..10]
                    if (e <= RightSideValue)
                        return (default, left);

                    if (e > RightSideValue)
                        return (new(RightSideValue + 1, e),  new(s, RightSideValue));
                }

                if (s == RightSideValue) {
                    // end= [10..10] > 10   [][10]
                    // end> [10..20] > 10   [11..20][10]
                    if (e == s)
                        return (default, new(e, e));

                    if (e > s)
                        return (new(s + 1, e), new(s, s));
                }

                if (s > RightSideValue) {
                    // end= [11..11] > 10
                    // end> [11..20] > 10
                    return (left, default);
                }
            }

            throw new InvalidOperationException();
        }
    }

    record Rule {
        public RatingExpression? Expression;
        public string FinallyAction;

        /// <summary>
        /// Part 1
        /// </summary>
        public bool TryEvaluate(int[] ratings, out string actionId) {
            if (Expression == null || Expression.Evaluate(ratings)) {
                actionId = FinallyAction;
                return true;
            }

            actionId = null;
            return false;
        }

        /// <summary>
        /// Part 2
        /// </summary>
        public (Range[] okRange, string okActionId, Range[] koRange) Evaluate(Range[] ratings) {
            if (Expression == null)
                return (ratings, FinallyAction, default);

            (var okRange, var koRange) = Expression.Evaluate(ratings[Expression.LeftSideRatingIndex]);
            Range[] okArr = null;
            Range[] koArr = null;

            if (okRange != null) {
                okArr = ratings.ToArray();
                okArr[Expression.LeftSideRatingIndex] = okRange.Value;
            }

            if (koRange != null) {
                koArr = ratings.ToArray();
                koArr[Expression.LeftSideRatingIndex] = koRange.Value;
            }

            return (okArr, FinallyAction, koArr);
        }

        public Rule(string str) {
            var finallyIdx = str.IndexOf(':');

            if (finallyIdx < 0) {
                FinallyAction = str;
                return;
            }

            FinallyAction = str[(finallyIdx + 1)..];

            var i = 0;
            while (Char.IsLetter(str[i]))
                i++;

            // xmas converted as array index 0..3
            Expression = new RatingExpression(str[..i][0] switch { 'x' => 0, 'm' => 1, 'a' => 2, _ => 3 },
                                              str[i],
                                              Int32.Parse(str[(i + 1)..finallyIdx]));
        }
    }

    Dictionary<string, List<Rule>> Workflows;
    List<int[]> Ratings;

    public Day19() {
        var blocks = File.ReadAllText(InputFilePath)
                         .Split($"{Environment.NewLine}{Environment.NewLine}")
                         .ToArray();

        Workflows = blocks[0].Split(Environment.NewLine)
                             .Select(o => (str: o, idx: o.IndexOf('{')))
                             .Select(o => (id: o.str[..o.idx], rules: o.str[(o.idx + 1)..^1].Split(',')
                                                                                            .Select(r => new Rule(r))
                                                                                            .ToList()))
                             .ToDictionary(o => o.id, o => o.rules);

        Ratings = blocks[1].Split(Environment.NewLine)
                           .Select(o => o[1..^1].Split(',')
                                                .Select(o => Int32.Parse(o[2..]))
                                                .ToArray())
                           .ToList();
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    int ComputeRating(string workflowId, int[] ratings) {
        var result = 0;

        while (workflowId != null) {
            var rules = Workflows[workflowId];

            workflowId = null;

            foreach (var rule in rules) {
                if (rule.TryEvaluate(ratings, out var actionId)) {
                    if (actionId == "A")
                        result += ratings.Sum();
                    else if (actionId != "R")
                        workflowId = actionId;
                    break;
                }
            }
        }

        return result;
    }

    int Part_1()
        => Ratings.Sum(o => ComputeRating("in", o));

    ulong ComputeRating(string workflowId, Range[] ratings) {
        var result = 0ul;

        while (workflowId != null) {
            var rules = Workflows[workflowId];

            workflowId = null;

            foreach (var rule in rules) {
                (var okRange, var okActionId, var split) = rule.Evaluate(ratings);

                if (okRange != null) {
                    if (okActionId == "A")
                        result += Enumerable.Range(0, 4).Select(i => Convert.ToUInt64(okRange[i].End.Value - okRange[i].Start.Value + 1))
                                                        .Aggregate((a, b) => a * b);
                    else if (okActionId != "R")
                        result += ComputeRating(okActionId, okRange);

                    if (split == null)
                        return result;
                }

                if (split != null)
                    ratings = split;
            }
        }

        return result;
    }

    ulong Part_2()
        => ComputeRating("in", Enumerable.Repeat(new Range(1, 4000), 4).ToArray());
}