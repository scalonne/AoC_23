namespace CollectionsUtils;

public static class MatrixExtensions {
    public static T[][] Rotate<T>(this T[][] matrix, bool clockWise) {
        int rows = matrix.Length;
        int cols = matrix[0].Length;

        T[][] rotatedMatrix = new T[cols][];

        for (int i = 0; i < cols; i++) {
            rotatedMatrix[i] = new T[rows];
        }

        if (clockWise) {
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) {
                    rotatedMatrix[j][rows - 1 - i] = matrix[i][j];
                }
            }
        } else {
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) {
                    rotatedMatrix[cols - 1 - j][i] = matrix[i][j];
                }
            }
        }

        return rotatedMatrix;
    }
     
    public static T[][] AddBorders<T>(this T[][] matrix, T borderValue) {
        var newMatrix = new T[matrix.Length + 2][];

        foreach (var y in Enumerable.Range(0, matrix.Length)) {
            var array = matrix[y];
            var newArray = new T[array.Length + 2];

            Array.Copy(array, 0, newArray, 1, array.Length);
            newArray[0] = borderValue;
            newArray[^1] = borderValue;

            newMatrix[y + 1] = newArray;
        }

        newMatrix[0] = Enumerable.Repeat(borderValue, newMatrix[1].Length).ToArray();
        newMatrix[^1] = newMatrix[0].ToArray();

        return newMatrix;
    }
}