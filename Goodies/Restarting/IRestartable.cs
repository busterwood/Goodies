using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BusterWood.Restarting
{
    /// <summary>A service that may be restarted on failure</summary>
    public interface IRestartable
    {
        /// <summary>The task that is running</summary>
        /// <remarks>needed so we can monitor and re-start on error</remarks>
        Task MonitoredTask { get; }

        /// <summary>The implemeter should pause the requested amout of time</summary>
        Task PauseBeforeRestart(TimeSpan delay);

        /// <summary>Start the service, typically re-loads state from the database</summary>
        /// <returns>A <see cref="Task"/> which completes when the implementor has restarted</returns>
        Task Restart();

        /// <summary>Called by <see cref="RestartMonitoring"/> when the maximum number of re-starts has been reached and no more attempts will be made</summary>
        Task MaxRestartsReached(int attempts);
    }

}