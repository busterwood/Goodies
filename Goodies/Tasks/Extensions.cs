using System.Threading.Tasks;

namespace BusterWood.Tasks
{
    public static class Extensions
    {
        public static void DontWait(this Task t) { }
    }
}
