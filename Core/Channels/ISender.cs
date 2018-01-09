using System.Threading.Tasks;

namespace BusterWood.Channels
{
    public interface ISender<T>
    {
        /// <summary>Synchronously sends a value to receiver, waiting until a receiver is ready to receive</summary>
        /// <param name="value">the value to send</param>
        /// <exception cref="System.OperationCanceledException">thrown when the channel <see cref="Channel{T}.IsClosed"/></exception>
        void Send(T value);

        /// <summary>Asynchronously sends a value to receiver, waiting until a receiver is ready to receive</summary>
        /// <param name="value">the value to send</param>
        /// <returns>A task that completes when the value has been sent to a receiver.  The returned task may be cancelled if the channel is closed</returns>
        Task SendAsync(T value);

        /// <summary>Tries to send a value to a waiting receiver.</summary>
        /// <param name="value">the value to send</param>
        /// <returns>TRUE if the value was sent, FALSE if the channel was closed or there was no waiting receivers</returns>
        bool TrySend(T value);
    }
}