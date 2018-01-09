using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BusterWood.Restarting
{
    public class RestartMonitoring
    {
        readonly IEnumerator<TimeSpan> _delays;

        public RestartMonitoring(IEnumerable<TimeSpan> delays)
        {
            if (delays == null) throw new ArgumentNullException(nameof(delays));
            _delays = delays.GetEnumerator();
        }

        public void Monitor(IRestartable restartable, int attempt = 0)
        {
            if (restartable.MonitoredTask == null) throw new ArgumentException("restartable.Running is null");
            restartable.MonitoredTask.ContinueWith(t => { AttemptRestart(restartable, attempt, t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
        }

        async Task AttemptRestart(IRestartable restartable, int attempt, Exception error)
        {
            attempt += 1;

            // quite if there are no more delays in the sequence
            if (!_delays.MoveNext())
            {
                await restartable.MaxRestartsReached(attempt);
                return;
            }

            // attempt another restart
            TimeSpan delay = _delays.Current;
            Debug.Assert(delay > TimeSpan.Zero);
            await restartable.PauseBeforeRestart(delay);
            try
            {
                await restartable.Restart();
            }
            catch (Exception ex)
            {
                Logging.Log.Warning("Failed to restart: " + ex);
                return;
            }
            Monitor(restartable, attempt);
        }

    }

}