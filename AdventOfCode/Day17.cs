using CollectionsUtils;
using System.Drawing;

namespace AdventOfCode;

public class Day17 : BaseDay
{
    readonly struct Node(int x, int y, Direction direction, int stepsCounter) : IEquatable<Node> {
        public int X => x;
        public int Y => y;
        public Direction Direction => direction;
        public int StepsCounter => stepsCounter;

        public bool Equals(Node other) {
            return X == other.X &&
                   Y == other.Y &&
                   Direction == other.Direction &&
                   StepsCounter == other.StepsCounter;
        }

        public override int GetHashCode()
            =>  HashCode.Combine(X, Y, Direction, StepsCounter);
    }

    enum Direction {
        Up,
        Down,
        Left,
        Right
    }

    static Point STEP_UP = new(0, -1);
    static Point STEP_DOWN = new(0, 1);
    static Point STEP_LEFT = new(-1, 0);
    static Point STEP_RIGHT = new(1, 0);

    static Dictionary<Direction, Point> StepByDirection = new() {
        { Direction.Up,    STEP_UP },
        { Direction.Down,  STEP_DOWN },
        { Direction.Left,  STEP_LEFT },
        { Direction.Right, STEP_RIGHT }
    };

    int[][] Map;
    int Height;
    int Width;

    public Day17() {
        Map = File.ReadLines(InputFilePath)
                  .Select(o => o.Select(p => p - '0')
                                .ToArray())
                  .ToArray()
                  .AddBorders(-1);
        Height = Map.Length;
        Width = Map[0].Length;
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    IEnumerable<Node> GetNextNodes(Node node, int stepMin, int stepMax) {
        // rotate 90°
        if (node.StepsCounter >= stepMin) {
            if (node.Direction == Direction.Up || node.Direction == Direction.Down) {
                yield return new(node.X + STEP_LEFT.X, node.Y + STEP_LEFT.Y, Direction.Left, 1);
                yield return new(node.X + STEP_RIGHT.X, node.Y + STEP_RIGHT.Y, Direction.Right, 1);
            } else {
                yield return new(node.X + STEP_UP.X, node.Y + STEP_UP.Y, Direction.Up, 1);
                yield return new(node.X + STEP_DOWN.X, node.Y + STEP_DOWN.Y, Direction.Down, 1);
            }
        }

        // continue forward
        if (node.StepsCounter < stepMax) {
            var step = StepByDirection[node.Direction];

            yield return new(node.X + step.X, node.Y + step.Y, node.Direction, node.StepsCounter + 1);
        }
    }

    int ComputeGraph(int stepMin, int stepMax) {
        var start1 = new Node(1, 1, Direction.Right, 0);
        var start2 = new Node(1, 1, Direction.Down, 0);
        var endPos = new Point(Width - 2, Height - 2);
        var priorityQueue = new PriorityQueue<Node, int>([(start1, 0), (start2, 0)]);
        var visitedNodes = new Dictionary<Node, int>() { { start1, Int32.MaxValue }, { start2, Int32.MaxValue } };

        while (priorityQueue.TryDequeue(out var node, out var currentHeatLoss)) {
            if (node.X == endPos.X && node.Y == endPos.Y && node.StepsCounter >= stepMin)
                return currentHeatLoss;
            
            foreach (var nextNode in GetNextNodes(node, stepMin, stepMax)) {
                if (Map[nextNode.Y][nextNode.X] < 0)
                    continue;

                var newHeatLoss = currentHeatLoss + Map[nextNode.Y][nextNode.X];

                if (visitedNodes.TryGetValue(nextNode, out var value) && value <= newHeatLoss)
                    continue;

                visitedNodes[nextNode] = newHeatLoss;
                priorityQueue.Enqueue(nextNode, newHeatLoss);
            }
        }

        throw new InvalidOperationException("end not found");
    }

    int Part_1()
        => ComputeGraph(1, 3);

    int Part_2()
        => ComputeGraph(4, 10);
}