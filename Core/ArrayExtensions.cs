namespace BusterWood
{
    public static class ArrayExtensions
    {
        public static T[] Copy<T>(this T[] source)
        {
            var result = new T[source.Length];
            source.CopyTo(result, 0);
            return result;
        }
    }
}