using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using VoxEng.Core.ECS.Engines;
using VoxEng.Core.ECS.Scheduler.Interfaces;

namespace VoxEng.Core.ECS.Scheduler
{
    internal class DefaultEngineScheduler: IEngineScheduler
    {

        //The engine dedicated to rendering the scene.
        private IScheduledEngine _renderEngine;
        
        //All engines unrelated to rendering.
        private List<IScheduledEngine> _scheduledEngines;

        //A timing table used for ensuring that engines do not exceed tick-rate.
        private Dictionary<Guid, DateTime> _timingTable;

        //A cached dictionary containing how frequently given engines would like their TickFixed called.
        private Dictionary<Guid, int> _tickRates;

        private SubmissionScheduler _scheduler;
        
        public DefaultEngineScheduler(SubmissionScheduler sched)
        {
            _timingTable = new Dictionary<Guid, DateTime>();
            _tickRates = new Dictionary<Guid, int>();
            
            _scheduler = sched;

            //Start the stopwatch.

            _scheduledEngines = new List<IScheduledEngine>();
        }

        public void Execute()
        {
            foreach (var eng in _scheduledEngines)
            {
                if (_tickRates.ContainsKey(eng.Id))
                {
                    if (_timingTable[eng.Id].AddMilliseconds((float) 1000 / _tickRates[eng.Id]) < DateTime.Now)
                    {
                        eng.TickFixed();
                        _timingTable[eng.Id] = DateTime.Now;
                    }
                }
                
                eng.Tick();
            }
            
            //Render after all other system updates since the other updates might impact data that is going to be rendered.
            _renderEngine.Tick();
        }

        /// <summary>
        /// Registers the scheduled engine for execution.
        /// </summary>
        /// <param name="eng">The engine to register</param>
        public void RegisterEngine(IScheduledEngine eng)
        {
            _scheduledEngines.Add(eng);
            _timingTable[eng.Id] = DateTime.Now;

            var method = eng.GetType().GetMethod("TickFixed");
            var attr = method?.GetCustomAttribute<TickRateAttribute>();
            if (attr is not null && attr.MaxTps != 0)
            {
                _tickRates[eng.Id] = attr.MaxTps;
            }
            else if (method is not null)
            {
                //The method exists but isn't decorated.
                _tickRates[eng.Id] = 60;
            }
            
            //Do nothing.
        }

        public void RegisterGraphicsEngine(IScheduledEngine eng)
        {
            _renderEngine = eng;
        }
    }
}