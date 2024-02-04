using System.Reflection.Metadata;

namespace AdventOfCode;

public class Day15 : BaseDay
{
    string[] Steps;

    public Day15() {
        Steps = File.ReadAllText(InputFilePath)
                    .Split(',');
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    int GetBoxHash(string label) {
        var hash = 0;

        foreach (var c in label) {
            hash += c;
            hash *= 17;
            hash %= 256;
        }

        return hash;
    }

    int Part_1()
        => Steps.Sum(GetBoxHash);

    int Part_2() {
        var dic = new SortedDictionary<int, List<(string label, int focal)>>();

        foreach (var box in Steps) {
            var i = 0;
            while (box[i] != '=' && box[i] != '-')
                i++;

            var label = box.Substring(0, i);
            var operation = box[i];
            var lense = operation == '=' ? Int32.Parse(box.Substring(i + 1)) : (int?)null;
            var hash = GetBoxHash(label);

            if (!dic.TryGetValue(hash, out var list)) {
                if (operation == '-')
                    continue;

                list = new() { (label, lense.Value) };
                dic[hash] = list;
            }

            var idx = list.FindIndex(o => o.label == label);
            if (idx < 0) {
                if (operation == '=')
                    list.Add((label, lense.Value));
            } else {
                if (operation == '=')
                    list[idx] = (label, lense.Value);
                else
                    list.RemoveAt(idx);
            }
        }

        return dic.Select(kvp => kvp.Value.Select((o, i) => (o.focal, i: i + 1))
                                              .Sum(o => (kvp.Key + 1) * o.i * o.focal))
                  .Sum();
    }
}