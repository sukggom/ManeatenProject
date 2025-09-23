/*
 * 
 * 
 */
namespace UG.Framework
{
    public abstract class UActorComponent
    {
        private UActorObject Actor = null;
        internal void SetActor(UActorObject InActor)
        {
            Actor = InActor;

            OnBegin();
        }

        public UActorObject GetActor()
        {
            return Actor;
        }

        public abstract void OnBegin();

        public abstract void OnAlloc();

        public abstract void OnReset();

        public abstract void OnDestory();
    }
}
