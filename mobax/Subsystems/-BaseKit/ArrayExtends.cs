public static class ArrayExtends
{
    public static T GetItem<T>(this T[] array, int index, T defaultVal = default)
    {
        if (index < array.Length)
        {
            return array[index];
        }

        return defaultVal;
    }
}