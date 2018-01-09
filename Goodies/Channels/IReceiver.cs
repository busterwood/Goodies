using System.Threading.Tasks;

namespace BusterWood.Channels
{
    public interface IReceiver<T>
    {
        /// <summary>Synchronously receives a value, waiting for a sender is one is not ready</summary>
        /// <returns>The value that was sent</returns>
        /// <exception cref="System.OperationCanceledException">thrown when the channel <see cref="Channel{T}.IsClosed"/> and there are no waiting senders</exception>
        T Receive();

        /// <summary>Asynchronously receives a value, waiting for a sender is one is not ready</summary>
        /// <returns>A task that completes with a result when a sender is ready.  The task may also be cancelled is the channel is closed and there are no waiting senders</returns>
        Task<T> ReceiveAsync();

        /// <summary>Tries to receive a value from a waiting sender.</summary>
        /// <param name="value">the value that was received, or default(T) when no sender is ready</param>
        /// <returns>TRUE if a sender was ready and <paramref name="value"/> is set, otherwise returns FALSE</returns>
        bool TryReceive(out T value);
    }
}