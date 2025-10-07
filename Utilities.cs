namespace GlobalConquest;

public class Utilities
{
    public static T[] FlattenArray<T>(T[,] multiDimensionalArray)
    {
        int rows = multiDimensionalArray.GetLength(0);
        int cols = multiDimensionalArray.GetLength(1);
        T[] singleDimensionArray = new T[rows * cols];

        int k = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                singleDimensionArray[k++] = multiDimensionalArray[i, j];
            }
        }
        return singleDimensionArray;
    }


    public static T[,] UnflattenArray<T>(T[] singleDimensionArray, int rows, int cols)
    {
        if (singleDimensionArray.Length != rows * cols)
        {
            throw new ArgumentException("Single dimension array length does not match specified dimensions.");
        }

        T[,] multiDimensionalArray = new T[rows, cols];
        int k = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                multiDimensionalArray[i, j] = singleDimensionArray[k++];
            }
        }
        return multiDimensionalArray;
    }


}