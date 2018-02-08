namespace BusterWood.Goodies
{
    public static class Swapper
    {
        public static void Swap<T>(ref T left, ref T right)
        {
            T temp = left;
            left = right;
            right = temp;
        }
    }
}
