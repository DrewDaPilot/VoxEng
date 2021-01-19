using System;
using System.Collections;
using System.Collections.Generic;
using Svelto.ECS;
using Svelto.ECS.Schedulers;

namespace VoxEng.Core.ECS.Scheduler
{
    internal sealed class SubmissionScheduler: EntitiesSubmissionScheduler
    {
        EnginesRoot.EntitiesSubmitter _onTick;
        readonly uint                 _maxNumberOfOperationsPerFrame;
        public override bool          paused { get; set; }
        
        
        public SubmissionScheduler(uint maxOpsPerTick = UInt32.MaxValue)
        {
            _maxNumberOfOperationsPerFrame = maxOpsPerTick;
        }

        public IEnumerator SubmitEntitiesAsync()
        {
            if (paused == false)
            {
                var submitEntities = _onTick.Invoke(_maxNumberOfOperationsPerFrame);
                
                while (submitEntities.MoveNext())
                    yield return null;
            }
        }

        public IEnumerator SubmitEntitesAsync(uint maxOps)
        {
            if (paused == false)
            {
                var submitEntities = _onTick.Invoke(maxOps);
                
                while (submitEntities.MoveNext())
                    yield return null;
            }
        }
        
        public void SubmitEntities()
        {
            var enumerator = SubmitEntitiesAsync();

            while (enumerator.MoveNext());
        }
        
        protected override EnginesRoot.EntitiesSubmitter onTick
        {
            set
            {
                _onTick = value;
            }
        }

        public override void Dispose() { }
        
    }
}