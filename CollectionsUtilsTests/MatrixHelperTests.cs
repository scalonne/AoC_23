using CollectionsUtils;

namespace CollectionsUtilsTests;

[TestClass]
public class MatrixHelperTests {
    [TestMethod]
    public void RotateTest() {
        var matrix = new int[][] {
            [1, 2, 3],
            [4, 5, 6],
            [7, 8, 9]
        };

        var rotatedMatrix = MatrixHelper.Rotate(matrix, clockWise: true);

        Assert.AreEqual(3, rotatedMatrix.Length);
        Assert.AreEqual(3, rotatedMatrix[0].Length);
        Assert.AreEqual(3, rotatedMatrix[1].Length);
        Assert.AreEqual(3, rotatedMatrix[2].Length);

        Assert.AreEqual(7, rotatedMatrix[0][0]);
        Assert.AreEqual(4, rotatedMatrix[0][1]);
        Assert.AreEqual(1, rotatedMatrix[0][2]);
        Assert.AreEqual(8, rotatedMatrix[1][0]);
        Assert.AreEqual(5, rotatedMatrix[1][1]);
        Assert.AreEqual(2, rotatedMatrix[1][2]);
        Assert.AreEqual(9, rotatedMatrix[2][0]);
        Assert.AreEqual(6, rotatedMatrix[2][1]);
        Assert.AreEqual(3, rotatedMatrix[2][2]);

        rotatedMatrix = MatrixHelper.Rotate(matrix, clockWise: false);

        Assert.AreEqual(3, rotatedMatrix.Length);
        Assert.AreEqual(3, rotatedMatrix[0].Length);
        Assert.AreEqual(3, rotatedMatrix[1].Length);
        Assert.AreEqual(3, rotatedMatrix[2].Length);

        Assert.AreEqual(3, rotatedMatrix[0][0]);
        Assert.AreEqual(6, rotatedMatrix[0][1]);
        Assert.AreEqual(9, rotatedMatrix[0][2]);
        Assert.AreEqual(2, rotatedMatrix[1][0]);
        Assert.AreEqual(5, rotatedMatrix[1][1]);
        Assert.AreEqual(8, rotatedMatrix[1][2]);
        Assert.AreEqual(1, rotatedMatrix[2][0]);
        Assert.AreEqual(4, rotatedMatrix[2][1]);
        Assert.AreEqual(7, rotatedMatrix[2][2]);
    }
}