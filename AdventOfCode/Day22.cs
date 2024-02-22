namespace AdventOfCode;

public class Day22 : BaseDay
{
    record struct Brick(int X1, int Y1, int Z1, int X2, int Y2, int Z2, char Id) {
        public Brick Fall(int z)
            => new(X1, Y1, z, X2, Y2, z + Z2 - Z1, Id);
    }

    Dictionary<Brick, (HashSet<Brick> top, HashSet<Brick> bottom)> Bricks;

    public Day22() {
        (int x, int y, int z) To3dPoint(string s) {
            var p = s.Split(',');

            return (Int32.Parse(p[0]), Int32.Parse(p[1]), Int32.Parse(p[2]));
        }

        var floatingBricks = File.ReadLines(InputFilePath)
                                 .Where(o => !String.IsNullOrWhiteSpace(o))
                                 .Select(o => o.Split('~'))
                                 .Select((o, i) => (from: To3dPoint(o[0]), to: To3dPoint(o[1]), id: (char)('A' + i)))
                                 .Select(o => new Brick(o.from.x, o.from.y, o.from.z, o.to.x, o.to.y, o.to.z, o.id))
                                 .ToList();

        Bricks = ComputeFalling(floatingBricks);
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    void Display(IEnumerable<Brick> bricks) {
        var reverse = bricks.Reverse().ToList();
        var zOffset = reverse.Max(o => o.Z2) + 10;

        foreach (var brick in reverse) {
            foreach (var x in Enumerable.Range(Math.Min(brick.X1, brick.X2), Math.Abs(brick.X2 - brick.X1) + 1)) {
                Console.SetCursorPosition(x, brick.Z1);
                Console.Write(brick.Id);
            }
            foreach (var z in Enumerable.Range(Math.Min(brick.Z1, brick.Z2), Math.Abs(brick.Z2 - brick.Z1) + 1)) {
                Console.SetCursorPosition(brick.X1, z);
                Console.Write(brick.Id);
            }
        }

        var xOffset = bricks.Max(o => Math.Max(o.X1, o.X2)) + 10;

        foreach (var brick in reverse) {
            foreach (var y in Enumerable.Range(Math.Min(brick.Y1, brick.Y2), Math.Abs(brick.Y2 - brick.Y1) + 1)) {
                Console.SetCursorPosition(y + xOffset, brick.Z1);
                Console.Write(brick.Id);
            }
            foreach (var z in Enumerable.Range(Math.Min(brick.Z1, brick.Z2), Math.Abs(brick.Z2 - brick.Z1) + 1)) {
                Console.SetCursorPosition(brick.Y1 + xOffset, z);
                Console.Write(brick.Id);
            }
        }
    }

    bool Collide(Brick a, Brick b) {
        var minX = Math.Min(b.X1, b.X2);
        var maxX = Math.Max(b.X1, b.X2);
        var minY = Math.Min(b.Y1, b.Y2);
        var maxY = Math.Max(b.Y1, b.Y2);

        return (minX <= a.X1 && a.X1 <= maxX || minX <= a.X2 && a.X2 <= maxX || a.X1 <= minX && a.X2 >= maxX || a.X2 <= minX && a.X1 >= maxX) &&
               (minY <= a.Y1 && a.Y1 <= maxY || minY <= a.Y2 && a.Y2 <= maxY || a.Y1 <= minY && a.Y2 >= maxY || a.Y2 <= minY && a.Y1 >= maxY);
    }

    Dictionary<Brick, (HashSet<Brick> top, HashSet<Brick> bottom)> ComputeFalling(List<Brick> floatingBricks) {
        // bricks are ordered by z desc to facilitate the creation of the falling list
        floatingBricks = floatingBricks.OrderBy(o => o.Z1).ToList();

        // fallingList will be ordered by z asc to optimize the neighbors identification
        var fallingList = new List<Brick>(floatingBricks.Count) { floatingBricks[0].Fall(0) };

        foreach (var a in floatingBricks.Skip(1)) {
            int? newZ = null;

            foreach (var b in fallingList) {
                if (b.Z2 + 1 < (newZ ?? a.Z1) && Collide(a, b)) {
                    newZ = b.Z2 + 1;
                    break;
                }
            }

            foreach (var i in Enumerable.Range(0, fallingList.Count)) {
                var b = a.Fall(newZ ?? 0);

                if (b.Z2 > fallingList[i].Z2) {
                    fallingList.Insert(i, b);
                    break;
                }

                if (i == fallingList.Count - 1)
                    fallingList.Add(b);
            }
        }

        //Display(fallingList);

        var neighborsByBrick = fallingList.ToDictionary(o => o, o => (top: new HashSet<Brick>(), bottom: new HashSet<Brick>()));

        foreach (var i in Enumerable.Range(0, fallingList.Count)) {
            var current = fallingList[i];

            foreach (var j in Enumerable.Range(i + 1, fallingList.Count - (i + 1))) {
                var next = fallingList[j];

                if (next.Z2 < current.Z1 - 1)
                    break;

                if (next.Z2 == current.Z1 - 1 && Collide(current, next)) {
                    neighborsByBrick[current].bottom.Add(next);
                    neighborsByBrick[next].top.Add(current);
                }
            }
        }

        return neighborsByBrick;
    }

    int Part_1()
        => Bricks.Values.Where(o => o.top.Count == 0 || o.top.All(p => Bricks[p].bottom.Count > 1))
                        .Count();

    int Part_2() {
        int GetUpperBricks(Brick brick, HashSet<Brick> path) {
            path.Add(brick);

            foreach (var b in Bricks[brick].top) {
                if (!path.Contains(b) && (Bricks[b].bottom.Count == 1 || Bricks[b].bottom.Where(o => !path.Contains(o)).Count() == 0))
                    GetUpperBricks(b, path);
            }

            return path.Count - 1;
        }

        var res = 0;

        foreach (var brick in Bricks.Keys) {
            if (Bricks[brick].top.Count > 0)
                res += GetUpperBricks(brick, new());
        }

        return res;
    }
}