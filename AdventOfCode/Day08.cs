namespace AdventOfCode;

public class Day08 : BaseDay
{
    Queue<char> Path;
    Dictionary<string, (string left, string right)> NodeDic;

    public Day08() {
        var lines  = File.ReadAllLines(InputFilePath);

        Path = new Queue<char>(lines[0]);
        NodeDic = lines.Skip(2)
                       .Select(o => o.Split(" = "))
                       .Select(o => (key: o[0], lr: o[1].Substring(1, o[1].Length - 2)
                                                        .Split(", ")))
                       .ToDictionary(o => o.key, o => (left: o.lr[0], right: o.lr[1]));
    }

    public override ValueTask<string> Solve_1() => new(Part_1("AAA", node => node == "ZZZ").ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    int Part_1(string node, Func<string, bool> stoppingFunc) {
        var steps = 0;

        while (true) {  
            var path = new Queue<char>(Path);

            while (path.TryDequeue(out var dir)) {
                node = dir == 'L' ? NodeDic[node].left : NodeDic[node].right;

                steps++;

                if (stoppingFunc(node))
                    return steps;
            }
        }
    }

    ulong Part_2()
        => NodeDic.Keys.Where(o => o.Last() == 'A')
                       .Select(o => Convert.ToUInt64(Part_1(o, node => node.Last() == 'Z')))
                       .Lcm();
}