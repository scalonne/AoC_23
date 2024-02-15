public static partial class EnumerableHelper {
    public static IEnumerable<int> Sequence(int start, int count, int steps) {
        if (count == 0)
            yield break;

        while (count-- > 0) {
            yield return start;
            start += steps;
        }
    }

    public static T Lcm<T>(this IEnumerable<T> source) where T : struct {
        T GreatestCommonFactor(dynamic a, dynamic b) {
            while (b != 0) {
                var temp = b;

                b = a % b;
                a = temp;
            }

            return a;
        }

        T LeastCommonMultiple(dynamic a, dynamic b)
            => a / GreatestCommonFactor(a, b) * b;

        var lcm = source.First();

        foreach (var step in source.Skip(1))
            lcm = LeastCommonMultiple(lcm, step);

        return lcm;
    }
}