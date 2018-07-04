namespace BusterWood.Linq
{
    public interface IBatcher<T>
    {
        /// <summary>The requested size of batches</summary>
        int BatchSize { get; }

        /// <summary>
        /// Returns an <see cref="IBatchEnumerator{T}"/> that allows reading this data source in batches
        /// </summary>
        IBatchEnumerator<T> GetBatchEnumerator();
    }

    public interface IBatchEnumerator<T>
    {
        /// <summary>The requested size of batches</summary>
        int BatchSize { get; }

        /// <summary>
        /// Populates the <paramref name="batch"/> array with <paramref name="count"/> items.  
        /// Returns TRUE if the array was populated, FALSE if there is no more data
        /// </summary>
        /// <param name="batch">Must be passed in (not null) and have size of <see cref="BatchSize"/></param>
        /// <param name="count">The number of items copied in to <paramref name="batch"/></param>
        bool NextBatch(T[] batch, out int count);
    }

}
