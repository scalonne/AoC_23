public static partial class EnumerableHelper {
    public static IEnumerable<int> Sequence(int start, int count, int steps) {
        if (count == 0)
            yield break;

        while (count-- > 0) {
            yield return start;
            start += steps;
        }
    }
}