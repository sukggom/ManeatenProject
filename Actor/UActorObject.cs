using System;
using System.Collections.Generic;
using UnityEngine;

namespace UG.Framework
{
    public abstract class UActorObject : UActorTransform, IPoolObject , IUPointer
    {
        private long ActorID = 0;
        
        private Dictionary<Type, UActorComponent> Components
            = new Dictionary<Type, UActorComponent>();

        private Dictionary<int, UActorBehaviour> Behaviours 
            = new Dictionary<int, UActorBehaviour>();

        //현재 Behaviour
        private UActorBehaviour CurrentBehaviour = null;
        //이 2개 Collider&Collidee는 ActionSystem이 후에 Package로 올라올때를 대비함
        protected int ColliderLayer = 0;
        protected int CollideeLayer = 0;

        private FXRarityLevel FxRarity = 0;
        
#if  UG_DEBUG_OBJECTS_COUNT
        private UObjectCounter<UActorObject> ObjectCounter = new UObjectCounter<UActorObject>();
#endif 

        UObjectPointer ObjPointer;

        public virtual void OnAlloc()
        {
            var Enumerator = Components.GetEnumerator();
            while (Enumerator.MoveNext())
            {
                Enumerator.Current.Value.OnAlloc();
            }
        }

        public virtual void OnFirstCreate()
        {
        }

        public virtual void OnFree()
        {
            var Enumerator = Components.GetEnumerator();
            while(Enumerator.MoveNext())
            {
                Enumerator.Current.Value.OnReset();
            }

            CurrentBehaviour = null;

            ObjPointer.Clear();
        }

        public virtual void OnLastRelease()
        {
            var Enumerator = Components.GetEnumerator();
            while (Enumerator.MoveNext())
            {
                Enumerator.Current.Value.OnDestory();
            }

            Components.Clear();

            ObjPointer.Clear();
        }


        public abstract int GetColliderLayer();
        public abstract int GetCollideeLayer();

        public virtual void OnChangeBehaviour(int InIndex) { }


        public virtual void OnUpdate(float InDeltaTime)
        {
            CheckVelocity();
        }

        public virtual void GameStop(bool bStop)
        {
            if (bStop)
            {
                CurrentBehaviour.GetController()?.Velocity(Vector3.zero, 0);
                CurrentBehaviour.GetAnimator()?.Pause();
            }
            else
            {
                CurrentBehaviour.GetAnimator()?.Resume();
            }
        }

        public virtual void Stop(bool bStop)
        {
            if (bStop)
            {
                CurrentBehaviour.GetController()?.Stop();
            }
            else
            {
                CurrentBehaviour.GetController()?.ReleaseStop();
            }
        }

        public long GetActorID()
        {
            return ActorID;
        }


        public virtual void CheckVelocity()
        {
            if (CurrentBehaviour != null)
            {
                CurrentBehaviour.CheckVelocity();
            }
        }


        public bool IsCollisionLayer(UActorObject InTarget)
        {
            return (GetCollideeLayer() & InTarget.GetColliderLayer()) > 0;
        }

        public UActorBehaviour GetCurrentBehaviour()
        {
            return CurrentBehaviour;
        }

        public IActorAnimator GetAnimator()
        {
            return CurrentBehaviour?.GetAnimator();
        }

        public IActorController GetController()
        {
            return CurrentBehaviour?.GetController();
        }

        public bool EnableController()
        {
            if (CurrentBehaviour == null)
            {
                return false;
            }
            else
            {
                return CurrentBehaviour.GetController().IsEnable();
            }
        }


        public bool CheckBehaviourIndex(int InIndex)
        {
            return Behaviours.ContainsKey(InIndex);
        }

        public void ChangeBehaviour(int InBehaviourIndex)
        {
            UActorBehaviour Behaviour;
            if (false == Behaviours.TryGetValue(InBehaviourIndex, out Behaviour))
            {
                ULogger.Error($"SelectBehaviour Error. Index: {InBehaviourIndex}");
                return;
            }

            if (null != CurrentBehaviour)
            {
                Behaviour.SetPosition(CurrentBehaviour.GetPosition());
                Behaviour.SetRotation(CurrentBehaviour.GetRotation());
                
                CurrentBehaviour.Hide();
            }

            Behaviour.Show();
            
            CurrentBehaviour = Behaviour;

            base.SetBehaviour(CurrentBehaviour);

            OnChangeBehaviour(InBehaviourIndex);
        }

        public void AttachParts<T>(int InPartsIndex, UAssetID InPartsID, int InSocketIndex) where T : UActorPartsBehaviour
        {
            if(null == CurrentBehaviour)
            {
                ULogger.Error($"AttachParts Error. null ActorBehaviour");
                return;
            }

            UActorPartsBehaviour PrevParts = CurrentBehaviour.GetParts(InPartsIndex);
            if (null != PrevParts)
            {
                if(PrevParts.AssetID == InPartsID) //만약 같은 파츠인데 속성이 다른거라면 밖에서 체크
                {
                    return;
                }

                PrevParts = CurrentBehaviour.DetachParts(InPartsIndex);

                PrevParts.DestroyGameObject();
            }

            T PartsBehaviour 
                = UResources.Manager.AssetInstantiate<T>(InPartsID);

            if(null == PartsBehaviour)
            {
                return;
            }

            PartsBehaviour.AssetID = InPartsID;

            CurrentBehaviour.AttachParts(InPartsIndex, PartsBehaviour, InSocketIndex);
        }

        public void DetachParts(int InPartsIndex)
        {
            if (null == CurrentBehaviour)
            {
                ULogger.Error($"AttachParts Error. null ActorBehaviour");
                return;
            }

            UActorPartsBehaviour Parts = CurrentBehaviour.DetachParts(InPartsIndex);
            if(null != Parts)
            {
                Parts.DestroyGameObject();
            }
        }

        public UActorPartsBehaviour GetParts(int InPartsIndex)
        {
            return CurrentBehaviour?.GetParts(InPartsIndex) ?? null;
        }

        public T AddComponent<T>() where T : UActorComponent, new()
        {
            Type ComponentType = typeof(T);

            if(Components.ContainsKey(ComponentType))
            {
                return null;
            }

            T Component = new T();
            Component.SetActor(this);

            Components.Add(ComponentType, Component);

            return Component;
        }

        public T GetComponent<T>() where T : UActorComponent
        {
            UActorComponent Value;

            if (Components.TryGetValue(typeof(T), out Value))
            {
                return Value as T;
            }

            return null;
        }
       
        internal void SetActorID(long InActorID)
        {
            ActorID = InActorID;
        }

        internal void DetachAllBehaviours()
        {
            Behaviours.Clear();

            base.SetBehaviour(InBehaviour: null);
        }

        internal void AttachBehaviour(int InIndex, UActorBehaviour InBehaviour)
        {
            if (Behaviours.ContainsKey(InIndex))
            {
                ULogger.Error($"AttachBehaviour Error. Index: {InIndex} ");

                return;
            }

            Behaviours.Add(InIndex, InBehaviour);

            if (Behaviours.Count <= 1)
            {
                ChangeBehaviour(InIndex);
            }
            else
            {
                InBehaviour.Hide();
            }
        }

        internal Dictionary<int, UActorBehaviour>.ValueCollection GetBehaviours()
        {
            return Behaviours.Values;
        }

        internal void DestroyAllParts()
        {
            if(null == Behaviour)
            {
                return;
            }

            var Enumerator = Behaviours.GetEnumerator();
            while(Enumerator.MoveNext())
            {
                var Parts = Enumerator.Current.Value.GetParts();
                var PartsEnumerator = Parts.GetEnumerator();
                while(PartsEnumerator.MoveNext())
                {
                    PartsEnumerator.Current.Value.DestroyGameObject();
                }

                Parts.Clear();
            }
        }

        public void CurrentBehaviourHide()
        {
            CurrentBehaviour.Hide();
        }
        public void CurrentBehaviourShow()
        {
            CurrentBehaviour.Show();
        }

        public virtual bool IsInValidTarget()
        {
            return CurrentBehaviour == null;
        }

        public void AddObjectPointer(IUObjectPointer InObjectPointer)
        {
            ObjPointer.Add(InObjectPointer);
        }

        public void RemoveObjectPointer(IUObjectPointer InObjectPointer)
        {
            ObjPointer.Remove(InObjectPointer);
        }

        public void SetFxRarity(FXRarityLevel InFxRarity)
        {
            FxRarity = InFxRarity;
        }

        public FXRarityLevel GetFxRarity()
        {
            return FxRarity;
        }
    }
}
