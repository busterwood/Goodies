using System;

namespace BusterWood.Linq
{
    public interface IBatcher<T>
    {
        int BatchSize { get; }
        ArraySegment<T> NextBatch();
    }


}
