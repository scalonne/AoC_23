namespace AdventOfCode;

public class Day20 : BaseDay {
    [Flags]
    enum Pulse {
        Low = 0,
        High = 1
    }

    abstract class Module(string id) {
        public string Id { get; } = id;
        public HashSet<Module> Sources { get; } = new();
        public HashSet<Module> Targets { get; } = new();
        protected Pulse? EmitPulseBuffer { get; set; }

        public virtual void AddTarget(Module module) {
            Targets.Add(module);
            module.Sources.Add(this);
        }

        public virtual void Init() {
            EmitPulseBuffer = null;
        }

        public abstract void Receive(Module source, Pulse pulse);

        public virtual IEnumerable<(Module m, Pulse p)> Emit() {
            if (EmitPulseBuffer == null)
                return Enumerable.Empty<(Module m, Pulse p)>();

            var p = EmitPulseBuffer.Value;
            EmitPulseBuffer = null;

            return Targets.Select(o => (o, p));
        }
    }

    class FlipFlop(string id) : Module(id) {
        bool IsOn { get; set; }

        public override void Init() {
            base.Init();

            IsOn = false;
        }

        public override void Receive(Module source, Pulse pulse) {
            if (pulse == Pulse.Low) {
                IsOn = !IsOn;
                EmitPulseBuffer = IsOn ? Pulse.High : Pulse.Low;
            }
        }
    }

    class Conjunction(string id) : Module(id) {
        Dictionary<Module, Pulse> Inputs { get; set; }

        public override void Init() {
            base.Init();

            Inputs = Sources.ToDictionary(o => o, o => Pulse.Low);
        }

        public override void Receive(Module source, Pulse pulse) {
            Inputs[source] = pulse;

             if (Inputs.Values.All(o => o == Pulse.High))
                EmitPulseBuffer = Pulse.Low;
             else
                EmitPulseBuffer = Pulse.High;
        }
    }

    class Broadcast(string id) : Module(id) {
        public override void Receive(Module _, Pulse pulse) {
            EmitPulseBuffer = pulse;
        }
    }

    class Button(string id) : Broadcast(id);

    class Output(string id) : Module(id) {
        public override void Receive(Module source, Pulse pulse) {
        }
    }

    Dictionary<string, Module> ModuleById;

    Module ToModule(string id)
        => id switch {
            "broadcaster" => new Broadcast(id),
            "output" or "rx" => new Output(id),
            var str when str[0] == '%' => new FlipFlop(id[1..]),
            var str when str[0] == '&' => new Conjunction(id[1..]),
            _ => throw new NotImplementedException()
        };

    public Day20() {
        var ids = File.ReadLines(InputFilePath)
                      .Where(o => !String.IsNullOrWhiteSpace(o))
                      .Select(o => o.Split(" -> "))
                      .Select(o => (module: ToModule(o[0]), targets: o[1].Split(", ")))
                      .ToList();

        ModuleById = ids.Select(o => o.module)
                        .ToDictionary(o => o.Id, o => o);
        ModuleById["output"] = new Output("output");
        ModuleById["button"] = new Button("button");
        ModuleById["button"].AddTarget(ModuleById["broadcaster"]);

        foreach ((var module, var targetIds) in ids)
            foreach (var targetId in targetIds) {
                // to handle rx as Output
                if (!ModuleById.TryGetValue(targetId, out var target))
                    ModuleById[targetId] = target = ToModule(targetId);
                
                module.AddTarget(target);
            }
    }

    public override ValueTask<string> Solve_1() => new(Part_1().ToString());

    public override ValueTask<string> Solve_2() => new(Part_2().ToString());

    void Reset() {
        foreach (var module in ModuleById.Values)
            module.Init();
    }

    ulong Part_1() {
        Reset();

        var high = 0ul;
        var low = 0ul;

        foreach (var i in Enumerable.Range(1, 1_000)) {
            var emitQueue = new Queue<(Module from, Module to, Pulse pulse)>([(null, ModuleById["button"], Pulse.Low)]);

            while (emitQueue.TryDequeue(out var emit)) {
                (var from, var to, var pulse) = emit;

                to.Receive(from, pulse);

                foreach ((var nextTarget, var nextPulse) in to.Emit()) {
                    if (nextPulse == Pulse.High)
                        high++;
                    else
                        low++;

                    //Console.WriteLine($"{to.Id} -{nextPulse}- {nextTarget.Id}");
                    emitQueue.Enqueue((to, nextTarget, nextPulse));
                }
            }
        }
        
        //Console.WriteLine($"H: {high} L: {low}");
        return high * low;
    }

    ulong Part_2() {
        Reset();

        var kl = ModuleById["rx"].Sources.First();
        var conjunctions = kl.Sources.Where(o => o is Conjunction);
        var monitoredIds = new HashSet<string>(conjunctions.Select(o => o.Id));
        var monitoredCycles = new List<int>(monitoredIds.Count);
        var i = 1;

        while (true) {
            var emitQueue = new Queue<(Module from, Module to, Pulse pulse)>([(null, ModuleById["button"], Pulse.Low)]);

            while (emitQueue.TryDequeue(out var emit)) {
                (var from, var to, var pulse) = emit;

                if (pulse == Pulse.High && monitoredIds.Contains(from.Id)) {
                    monitoredCycles.Add(i);
                    monitoredIds.Remove(from.Id);

                    if (monitoredIds.Count == 0)
                        return monitoredCycles.Select(Convert.ToUInt64)
                                              .Lcm();
                }

                to.Receive(from, pulse);

                foreach ((var nextTarget, var nextPulse) in to.Emit())
                    emitQueue.Enqueue((to, nextTarget, nextPulse));
            }

            i++;
        }
    }
}