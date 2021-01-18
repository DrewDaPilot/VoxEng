using System;
using System.Collections;
using Svelto.ECS;
using Svelto.ECS.Schedulers;

namespace VoxEng.Core.ECS.Schedulers
{
    public class DefaultSubmissionScheduler: EntitiesSubmissionScheduler
    {
        private uint _maxPerFrame;
        private EnginesRoot.EntitiesSubmitter _onTick;
        
        public DefaultSubmissionScheduler(uint maxOpsPerFrame = UInt32.MaxValue)
        {
            _maxPerFrame = maxOpsPerFrame;
        }

        public IEnumerator SubmitEntitiesAsync()
        {
            if (!paused)
            {
                var submitEntities = _onTick.Invoke(_maxPerFrame);

                while (submitEntities.MoveNext())
                    yield return null;
            }
        }

        public IEnumerator SubmitEntitiesAsync(uint maxOps)
        {
            if (!paused)
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
        

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        protected override EnginesRoot.EntitiesSubmitter onTick
        {
            set
            {
                _onTick = value;
            }
        }

        public override bool paused { get; set; }
    }
}