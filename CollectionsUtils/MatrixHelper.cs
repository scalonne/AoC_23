namespace CollectionsUtils;

public static class MatrixHelper {
    public static T[][] Rotate<T>(T[][] matrix, bool clockWise) {
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
}