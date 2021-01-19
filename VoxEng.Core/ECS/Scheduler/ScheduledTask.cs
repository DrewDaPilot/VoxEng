using System;
using System.Threading.Tasks;

namespace VoxEng.Core.ECS.Scheduler
{
    public class ScheduledTask
    {
        private long? _freq;
        private long _lastTick;
        
        private readonly Task _task;

        /// <summary>
        /// How frequently, in number of times per second, to execute this task.
        /// </summary>
        /// <param name="freq"></param>
        public ScheduledTask(Task t, long freq = default)
        {
            _freq = freq;
            _task = t;
            _lastTick = DateTime.Now.Ticks;
        }

        public async Task TickAsync(long elapsedTicks)
        {
            if (_freq.HasValue)
            {
                var remaining = elapsedTicks - _lastTick;
                if (remaining >=  _freq.Value)
                {
                    await Task.Delay(TimeSpan.FromTicks(remaining - (long) _freq));
                }

                await _task.ConfigureAwait(false);

                _lastTick = elapsedTicks;

                return;
            }

            _lastTick = elapsedTicks;
            await _task;
        }
    }
}