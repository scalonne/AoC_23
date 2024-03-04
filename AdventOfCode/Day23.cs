using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace AdventOfCode;

public class Day23 : BaseDay {
    record Cell(int X, int Y, char C) {
        public int X { get; } = X;
        public int Y { get; } = Y;
        public char C { get; } = C;
    }

    Cell[][] Map;
    List<Cell> Nodes;
    int Height => Map.Length;
    int Width => Map[0].Length;
    Cell Start => Nodes[0];
    Cell End => Nodes[^1];

#if DEBUG
    const int THREAD_DELAY_MS = 0;
#else
    const int THREAD_DELAY_MS = 0;
#endif


    public Day23() {
        Map = File.ReadLines(InputFilePath)
                  .Where(o => !String.IsNullOrWhiteSpace(o))
                  .Select((o, y) => o.Select((c, x) => new Cell(x, y, c))
                                     .ToArray())
                  .ToArray();

        Nodes = Map.SelectMany(o => o)
                   .Where(o => o.C != '#')
                   .ToList();
    }

    Dictionary<Cell, List<(Cell node, int steps)>> BuildGraph(Func<Cell, IEnumerable<Cell>> getNextPathsFunc) {
        var nodes = new Dictionary<Cell, List<(Cell node, int steps)>>();
        var graphQueue = new Queue<Cell>([Start]);
        var visited = new HashSet<Cell>([Start]);
        var buffer = new List<Cell>(3);
        Cell pos;

        List<Cell> GetNextPaths(Cell pos, Cell previous = null) {
            buffer.Clear();

            foreach (var next in getNextPathsFunc(pos)) {
                if (next != previous)
                    buffer.Add(next);
            }

            return buffer;
        }

        // build graph
        while (graphQueue.TryDequeue(out var node)) {
            nodes[node] = new();

            var nexts = GetNextPaths(node);

            foreach (var next in nexts.ToList()) {
                var previous = node;
                var steps = 1;

                pos = next;

                while ((nexts = GetNextPaths(pos, previous)).Count == 1) {
                    previous = pos;
                    pos = nexts[0];
                    steps++;
                }

                if (pos == Start)
                    continue;

                nodes[node].Add((pos, steps));

                if (visited.Add(pos))
                    graphQueue.Enqueue(pos);
            }
        }

        // fuse intermediate nodes between start and branch
        pos = Start;
        while (nodes[pos].Count == 1) {
            var skip = nodes[pos][0];
            var nexts = nodes[skip.node];

            nodes[pos].Clear();
            nodes.Remove(skip.node);

            foreach (var (next, steps) in nexts) {
                nodes[pos].Add((next, steps + skip.steps));

                var idx = nodes[next].FindIndex(o => o.node == skip.node);

                if (idx >= 0)
                    nodes[next].RemoveAt(idx);
            }
        }

        // fuse intermediate nodes between last branch and end
        pos = End;
        while (nodes[pos].Count == 1) {
            var skip = nodes[pos][0];
            var nexts = nodes[skip.node];

            if (nexts.Count == 1)
                break;

            nodes[pos].Clear();
            nodes.Remove(skip.node);

            foreach (var (next, steps) in nexts) {
                if (next == pos)
                    continue;

                nodes[pos].Add((next, steps + skip.steps));

                var idx = nodes[next].FindIndex(o => o.node == skip.node);

                nodes[next].RemoveAt(idx);
                nodes[next].Add((pos, steps + skip.steps));
            }
        }

        // remove dead ends from graph if any
        var deadEndsQueue = new Queue<Cell>(nodes.Keys.Where(o => nodes[o].Count == 1 && o != Start && o != End));

        while (deadEndsQueue.TryDequeue(out var deadEnd)) {
            foreach (var (next, _) in nodes[deadEnd]) {
                if (next == Start || next == End)
                    continue;

                var removeIndex = nodes[next].FindIndex(o => o.node == deadEnd);

                if (removeIndex >= 0)
                    nodes[next].RemoveAt(removeIndex);
        
                if (nodes[next].Count == 0)
                    deadEndsQueue.Enqueue(next);
            }

            nodes.Remove(deadEnd);
        }

        // clean edges: cannot go backward from an edge (cannot go up from left|right and cannot go left from top|bottom)
        var edgesStack = new Stack<Cell>(nodes[Start].Select(o => o.node));

        while (edgesStack.TryPop(out var edge)) {
            var edges = nodes[edge].Select(o => o.node)
                                   .Where(o => nodes[o].Count == 3);

            if (edges.Count() != 1)
                continue;

            var nextEdge = edges.First();
            var idx = nodes[nextEdge].FindIndex(o => o.node == edge);

            nodes[nextEdge].RemoveAt(idx);
            edgesStack.Push(nextEdge);
        }

        return nodes;
     }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());


    [DllImport("kernel32.dll", EntryPoint = "GetConsoleWindow", SetLastError = true)]
    static extern IntPtr GetConsoleHandle();

    [DllImport("user32.dll")]
    static extern int GetWindowRect(IntPtr hwnd, out Rectangle rect);

    GraphicsPath GraphicWalls, GraphicLines, GraphicPos;

    List<(Cell node, int steps)> previousPath;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    void Display(Dictionary<Cell, List<(Cell node, int steps)>> graph, List<(Cell node, int steps)> path) {
        var size = 5;
        var offset = (x: 100, y: 10);

        using var gfx = Graphics.FromHwnd(GetConsoleHandle());

        var lineSize = 2;
        var linePathPen = new Pen(Color.DarkSlateGray, lineSize + 4);
        var lineGreen2Pen = new Pen(Color.Red, lineSize);
        var lineGreenPen = new Pen(Color.DarkSlateGray, lineSize + 4);
        var lineRedPen = new Pen(Color.Yellow, lineSize);

        // init graphics
        if (GraphicWalls == null) {
            Console.CursorVisible = false;
            Console.Clear();
            Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            Console.SetWindowPosition(0, 0);
            Console.Title = "aoc23 - day23";

            gfx.SmoothingMode = SmoothingMode.AntiAlias;
            gfx.Clear(Color.Transparent);

            // 1. walls
            {
                GraphicWalls = new GraphicsPath();

                GraphicWalls.FillMode = FillMode.Winding;
                foreach (var wall in Map.SelectMany(o => o).Where(o => o.C == '#'))
                    GraphicWalls.AddRectangle(new Rectangle(wall.X * size + offset.x, wall.Y * size + offset.y, size, size));

                var pen = new Pen(Color.DarkSlateGray);
                pen.LineJoin = LineJoin.Round;
                gfx.DrawPath(pen, GraphicWalls);
            }

            GraphicLines = new GraphicsPath();

            foreach (var kvp in graph) {
                var c1 = kvp.Key;
                var nexts = kvp.Value;

                foreach (var (c2, _) in nexts)
                    gfx.DrawLine(lineGreenPen, c1.X * size + offset.x + size / 2, c1.Y * size + offset.y + size / 2, c2.X * size + offset.x + size / 2, c2.Y * size + offset.y + size / 2);
            }

            foreach (var i in Enumerable.Range(0, path.Count - 1)) {
                var c1 = path[i].node;
                var c2 = path[i + 1].node;

                gfx.DrawLine(lineRedPen, c1.X * size + offset.x + size / 2, c1.Y * size + offset.y + size / 2, c2.X * size + offset.x + size / 2, c2.Y * size + offset.y + size / 2);
            }

            // pos
            {
                GraphicPos = new GraphicsPath();

                foreach (var node in graph.Keys)
                    GraphicPos.AddString($"{node.X},{node.Y}", FontFamily.GenericSerif, 0, 22f, new PointF(node.X * size + offset.x, node.Y * size + offset.y - 10), null);

                var nodeSize = size * 1;
                var nodePen = new Pen(Color.Black, 2);
                gfx.DrawPath(nodePen, GraphicPos);
                gfx.FillPath(Brushes.Orange, GraphicPos);
            }
        }

        // 3. path lines
        if (previousPath != null) {
            var z = -1;

            foreach (var i in Enumerable.Range(0, previousPath.Count - 1)) {
                var c1 = previousPath[i].node;
                var c2 = previousPath[i + 1].node;

                if (i <= path.Count - 2 && c1 == path[i].node && c2 == path[i + 1].node) {
                    gfx.DrawLine(lineGreen2Pen, c1.X * size + offset.x + size / 2, c1.Y * size + offset.y + size / 2, c2.X * size + offset.x + size / 2, c2.Y * size + offset.y + size / 2);
                } else {
                    break;
                }
            }

            foreach (var i in Enumerable.Range(0, previousPath.Count - 1)) {
                var c1 = previousPath[i].node;
                var c2 = previousPath[i + 1].node;

                if (i >= path.Count - 2 || c1 != path[i].node || c2 != path[i + 1].node) {
                    gfx.DrawLine(linePathPen, c1.X * size + offset.x + size / 2, c1.Y * size + offset.y + size / 2, c2.X * size + offset.x + size / 2, c2.Y * size + offset.y + size / 2);

                    if (z < 0)
                        z = i;
                }
            }

            foreach (var i in Enumerable.Range(z, path.Count - 1 - z)) {
                var c1 = path[i].node;
                var c2 = path[i + 1].node;

                gfx.DrawLine(lineRedPen, c1.X * size + offset.x + size / 2, c1.Y * size + offset.y + size / 2, c2.X * size + offset.x + size / 2, c2.Y * size + offset.y + size / 2);
            }
        }

        previousPath = path.ToList();
        //Thread.Sleep(25);
        //Console.ReadKey();
    }

    Image BaseImage;
    MemoryStream GraphicsGridMs;


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    void Display2(Dictionary<Cell, List<(Cell node, int steps)>> graph, Dictionary<byte, Cell> cellById, byte[][] threadPaths) {
        var size = 5;
        var offset = 20;
        var lineSize = 4;
        var hwnd = GetConsoleHandle();

        if (GraphicsGridMs == null) {
            Console.Clear();
            Console.CursorVisible = false;
            Console.Title = "aoc23 - day23";
            Console.SetCursorPosition(0, 450);

            GetWindowRect(hwnd, out var rect);

            var bitmap = new Bitmap(800, 800, PixelFormat.Format32bppPArgb);
            var graphics = Graphics.FromImage(bitmap);

            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // walls
            var wallPen = new Pen(Color.DarkSlateGray);

            foreach (var wall in Map.SelectMany(o => o).Where(o => o.C == '#'))
                graphics.DrawRectangle(wallPen, new Rectangle(wall.X * size + offset, wall.Y * size + offset, size, size));

            // links
            var linksPen = new Pen(Color.FromArgb(100, Color.DarkSlateGray), lineSize);

            foreach (var (c1, c2) in graph.SelectMany(o => o.Value.SelectMany(p => new[] { (c1: o.Key, c2: p.node), (c1: p.node, c2: o.Key) })).Distinct())
                graphics.DrawLine(linksPen, c1.X * size + offset + size / 2, c1.Y * size + offset + size / 2, c2.X * size + offset + size / 2, c2.Y * size + offset + size / 2);

            // pos x,y
            var graphicsPos = new GraphicsPath();
            var posPen = new Pen(Color.Black, 2);

            graphicsPos.AddString($"{Start.X},{Start.Y}", FontFamily.GenericSerif, 0, 22f, new PointF(10, -5), null);
            graphicsPos.AddString($"{End.X},{End.Y}", FontFamily.GenericSerif, 0, 22f, new PointF(End.X * size - 10, End.Y * size + offset + 5), null);

            //foreach (var node in graph.Keys)
            //    graphicsPos.AddString($"{node.X},{node.Y}", FontFamily.GenericSerif, 0, 22f, new PointF(node.X * size + offset, node.Y * size + offset - 10), null);
            graphics.DrawPath(posPen, graphicsPos);
            graphics.FillPath(Brushes.Orange, graphicsPos);
            GraphicsGridMs = new MemoryStream();
            bitmap.Save(GraphicsGridMs, ImageFormat.Bmp);
        }

        var img = Image.FromStream(GraphicsGridMs);
        using var lineGraphics = Graphics.FromImage(img);
        var colors = new[] { Color.Red, Color.Yellow, Color.Cyan, Color.Blue, Color.Fuchsia, Color.Teal };

        lineGraphics.SmoothingMode = SmoothingMode.AntiAlias;

        for (var i = 0; i < threadPaths.Length; i++) {
            var path = threadPaths[i % colors.Length].ToArray();
            var color = colors[i % colors.Length];
            var pen = new Pen(Color.FromArgb(240, color), 3);

            for (var j = 0; j < path.Length && path[j + 1] > 0 && path[j + 1] < Byte.MaxValue; j++) {
                var c1 = cellById[path[j]];
                var c2 = cellById[path[j + 1]];

                var x1 = c1.X * size + offset + size / 2 + i*5f - threadPaths.Length;
                var y1 = c1.Y * size + offset + size / 2 + i*5f - threadPaths.Length;
                var x2 = c2.X * size + offset + size / 2 + i*5f - threadPaths.Length;
                var y2 = c2.Y * size + offset + size / 2 + i*5f - threadPaths.Length;
                
                lineGraphics.DrawLine(pen, x1, y1, x2, y2);

                //var p1 = new Point(x1, y1); //starting point
                ////var p2 = new Point((Math.Min(x1, x2) + Math.Abs(x1 - x2)) / 3 + i * 2, (Math.Min(y1, y2) + Math.Abs(y1 - y2)));
                ////var p3 = new Point((Math.Max(x1, x2) - Math.Abs(x1 - x2)) / 2 * 3 + i * 2, (Math.Max(y1, y2) - Math.Abs(y1 - y2)));
                ////var p3 = new Point(Math.Min(x1, x2) + Math.Abs(x1 - x2) * 2 / 3, Math.Min(y1, y2) + Math.Abs(y1 - y2) * 2 / 3);
                ////var p2 = new Point(Math.Min(x1, x2) + Math.Abs(c1.X - c2.X) + j * 4, Math.Min(y1, y2) + Math.Abs(c1.Y - c2.Y)); //first control point
                //var p2 = new Point(Math.Min(x1, x2) + Math.Abs(c1.X - c2.X) + i*3, Math.Min(y1, y2) + Math.Abs(c1.Y - c2.Y) + i*3); //first control point
                //var p3 = new Point(Math.Max(x1, x2) - Math.Abs(c1.X - c2.X) + i*4, Math.Max(y1, y2) - Math.Abs(c1.Y - c2.Y) + i*4); //first control point
                //var p4 = new Point(x2, y2); //ending point


                //int ComputedRotationAngleA = -20, ComputedRotationAngleB = -20;
                //int ControlPointDistance = i*5;
                //var p1 = new Point(x1, y1);
                //var p2 = new Point(x1 + ControlPointDistance, y1 + ControlPointDistance);
                //p2.Offset(ComputedRotationAngleA, ComputedRotationAngleA);
                //var p3 = new Point(x2 - ControlPointDistance, y2 - ControlPointDistance);
                //p3.Offset(ComputedRotationAngleB, ComputedRotationAngleB);
                //var p4 = new Point(x2, y2);
                ////
                //lineGraphics.DrawBezier(pen, p1, p2, p3, p4);

            }
        }

        using var console = Graphics.FromHwnd(hwnd);

        console.SmoothingMode = SmoothingMode.AntiAlias;
        console.DrawImage(img, offset, offset);
    }

    int Travel(Dictionary<Cell, List<(Cell node, int steps)>> graph, List<(Cell node, int steps)> path, HashSet<Cell> visited) {
        var current = path[^1].node;
        var nexts = graph[current];

        if (current == End)
            return  path.Sum(o => o.steps);

        var res = 0;

        foreach (var next in nexts) {
            if (!visited.Add(next.node))
                continue;

            path.Add(next);

            res = Math.Max(res, Travel(graph, path, visited));

            path.Remove(next);
            visited.Remove(next.node);
        }

        return res;
    }

    int Travel_Part2(Dictionary<byte, Dictionary<byte, int>> graph, byte endId, byte[] path, int pathIndex) {
        var currentId = path[pathIndex];

        if (currentId == endId) {
            var sum = 0;

            for (var i = 0; i < pathIndex; i++)
                sum += graph[path[i]][path[i + 1]];

#if DEBUG
            Thread.Sleep(THREAD_DELAY_MS);
#endif

            return sum;
        }

        var res = 0;

        foreach (var next in graph[currentId].Keys) {
            var contains = false;

            for (var i = 0; i < pathIndex; i++)
                if (next == path[i]) {
                    contains = true;
                    break;
                }

            if (contains)
                continue;

            path[pathIndex + 2] = Byte.MaxValue; // not needed, a safety net when displaying
            path[pathIndex + 1] = next;

            res = Math.Max(res, Travel_Part2(graph, endId, path, pathIndex + 1));
        }

        return res;
    }

    int Part_1() {
        IEnumerable<Cell> GetNextLocations(Cell c) {
            bool leftFunc()
                => c.X > 0 && Map[c.Y][c.X - 1].C != '#' && Map[c.Y][c.X - 1].C != '>';
            bool rightFunc()
                => c.X < Width - 1 && Map[c.Y][c.X + 1].C != '#' && Map[c.Y][c.X + 1].C != '<';
            bool upFunc()
                => c.Y > 0 && Map[c.Y - 1][c.X].C != '#' && Map[c.Y - 1][c.X].C != 'v';
            bool downFunc()
                => c.Y < Height - 1 && Map[c.Y + 1][c.X].C != '#' && Map[c.Y + 1][c.X].C != '^';

            switch (c.C) {
                case '<':
                    if (leftFunc())
                        yield return Map[c.Y][c.X - 1];
                    yield break;
                case '>':
                    if (rightFunc())
                        yield return Map[c.Y][c.X + 1];
                    yield break;
                case '^':
                    if (upFunc())
                        yield return Map[c.Y - 1][c.X];
                    yield break;
                case 'v':
                    if (downFunc())
                        yield return Map[c.Y + 1][c.X];
                    yield break;
                default:
                    if (leftFunc())
                        yield return Map[c.Y][c.X - 1];
                    if (rightFunc())
                        yield return Map[c.Y][c.X + 1];
                    if (upFunc())
                        yield return Map[c.Y - 1][c.X];
                    if (downFunc())
                        yield return Map[c.Y + 1][c.X];
                    yield break;
            }
        }

        var graph = BuildGraph(GetNextLocations);

        return Travel(graph, new([(Start, 0)]), new([Start]));
    }

    int Part_2() {
        IEnumerable<Cell> GetNextLocations(Cell c) {
            if (c.X > 0 && Map[c.Y][c.X - 1].C != '#')
                yield return Map[c.Y][c.X - 1];

            if (c.X < Width - 1 && Map[c.Y][c.X + 1].C != '#')
                yield return Map[c.Y][c.X + 1];

            if (c.Y > 0 && Map[c.Y - 1][c.X].C != '#')
                yield return Map[c.Y - 1][c.X];

            if (c.Y < Height - 1 && Map[c.Y + 1][c.X].C != '#')
                yield return Map[c.Y + 1][c.X];
        }

        var graph = BuildGraph(GetNextLocations);
        var idByCell = graph.Select((o, i) => (cell: o.Key, id: Convert.ToByte(i)))
                            .ToDictionary(o => o.cell,
                                          o => o.id);
        var convertedGraph = graph.ToDictionary(o => idByCell[o.Key],
                                                o => o.Value.ToDictionary(p => idByCell[p.node],
                                                                          p => p.steps));
        var nodeCount = graph.Count;
        var threadCount = 5;
        var threadStarts = new Queue<(byte[] path, byte pathIndex)>();
        var startArray = new byte[graph.Count + 1];

        startArray[0] = idByCell[Start];
        threadStarts.Enqueue((startArray, 0));

        while (--threadCount > 0) {
            var (posArray, posIndex) = threadStarts.Dequeue();

            foreach (var (nextId, _) in convertedGraph[posArray[posIndex]]) {
                var arrayCopy = posArray.ToArray();

                arrayCopy[posIndex + 1] = nextId;
                arrayCopy[posIndex + 2] = Byte.MaxValue;

                threadStarts.Enqueue((arrayCopy, posIndex));
            }
        }

        var tokenSource = new CancellationTokenSource();

#if DEBUG

        var displayTask = new Task(() => {
            while (!tokenSource.Token.IsCancellationRequested) {
                var pathReferences = threadStarts.Select(o => o.path)
                                                 .ToArray();
                Display2(graph, idByCell.ToDictionary(o => o.Value, o => o.Key), pathReferences);
                Thread.Sleep(THREAD_DELAY_MS);
            }
        }, tokenSource.Token);

        displayTask.Start();

#endif

        var tasks = threadStarts.Select(o => Task.Run(() => Travel_Part2(convertedGraph, idByCell[End], o.path, o.pathIndex))).ToArray();

        Task.WaitAll(tasks);

        tokenSource.Cancel();

        //Console.ReadKey();

        return tasks.Max(o => o.Result);
    }
}
