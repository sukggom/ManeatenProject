using System.Collections.Generic;

using ActorDictionary = System.Collections.Generic.Dictionary<long, UG.Framework.UActorObject>;
using ActorPoolDictionary = System.Collections.Generic.Dictionary<System.Type, UG.Framework.IPool>;

namespace UG.Framework
{
    public class UActor : UManager<UActor>
    {
        private ActorDictionary Actors = new ActorDictionary();
        private ActorPoolDictionary ActorPool = new ActorPoolDictionary();

        private List<long> ActorsKeyList = new List<long>();

        public override void Initialize()
        {
        }

        public override void UnInitialize()
        {
            ClearActors();
            ClearActorPool();
        }

        public override void Update(float InDeltaTime)
        {
            for (int i = 0; i < ActorsKeyList.Count; ++i)
            {
                Actors[ActorsKeyList[i]].OnUpdate(InDeltaTime);
            }
        }

        public override void GameStop(bool bStop)
        {
            for (int i = 0; i < ActorsKeyList.Count; ++i)
            {
                Actors[ActorsKeyList[i]].GameStop(bStop);
            }
        }

        public override void ChangingScene() 
        {
            ClearActors();
        }


        public override void ChangedScene()
        {
        }

        public _Object CreateActor<_Object, _Behaviour>(long InKey
            , int InBehaviourIndex
            , IActorBehaviourProvider[] InActorProviders
            , UnityEngine.Vector3 InCenter) 
            where _Object : UActorObject 
            where _Behaviour : UActorBehaviour
            //where _Animator : UActorAnimationBehaviour
        {
            if(Actors.ContainsKey(InKey))
            {
                ULogger.Error($"CreateActor Error. Contains Actor Key ");

                return null;
            }

            _Object Actor = PoolAllocActor<_Object>();
            if (null == Actor)
            {
                ULogger.Error($"CreateActor Error. Alloc Error");
                return null;
            }


            for (int i =0; i < InActorProviders.Length; ++i)
            {
                UAssetID BehaviourID = new UAssetID(InActorProviders[i].GetModel());

                _Behaviour ActorBehaviour = UScene.Manager.SpawnObject<_Behaviour>(BehaviourID, InCenter);
                if (null == ActorBehaviour)
                {
                    ULogger.Error($"CreateActor Error. Behaviour Spawn Error");
                    return null;
                }


                if (!string.IsNullOrEmpty(InActorProviders[i].GetAnimation()))
                {
                    UAssetID AnimatorID = new UAssetID(InActorProviders[i].GetAnimation());
                    UAssetRefObject AnimatorRef = UResources.Manager.Asset(AnimatorID);
                    if (null != AnimatorRef)
                    {
                        ActorBehaviour.SetAnimatorRef(AnimatorRef);
                    }
                }


                ActorBehaviour.SetBehaviourIndex(i);
                Actor.AttachBehaviour(i, ActorBehaviour);
            }

            #region Single
            //_Behaviour ActorBehaviour = UScene.Manager.SpawnObject<_Behaviour>(InBehaviourID);
            //if(null == ActorBehaviour)
            //{
            //    ULogger.Error($"CreateActor Error. Behaviour Spawn Error");
            //    return null;
            //}

            ////애니메이션 타입을 강제할까 말까 고민. 번들로 묶는다면 강제 해야할 필요도 있음
            //if (InAnimatorID.IsValid())
            //{
            //    UAssetRefObject AnimatorRef = UResources.Manager.Asset(InAnimatorID);
            //    if (null != AnimatorRef)
            //    {
            //        ActorBehaviour.SetAnimatorRef(AnimatorRef);
            //    }
            //}
            //            else
            //            {
            ////#if UNITY_EDITOR
            ////                ULogger.Warning($"InAniatorID is Null - {InAnimatorID}");
            ////#endif
            //            }




            //_Object Actor = PoolAllocActor<_Object>();
            //if (null == Actor)
            //{
            //    ULogger.Error($"CreateActor Error. Alloc Error");
            //    return null;
            //}


            //ActorBehaviour.SetBehaviourIndex(InBehaviourIndex);
            //Actor.AttachBehaviour(InBehaviourIndex, ActorBehaviour);


            //{
            //    if(InBehaviourID.GetName() == "Monster/Common_Spiky01/Prefab/Common_Spiky01")
            //    {
            //        _Behaviour ActorBehaviour2 = UScene.Manager.SpawnObject<_Behaviour>("Monster/Nurse_Ring01/Prefab/Nurse_Ring01".ToAssetID());
            //        if (null == ActorBehaviour2)
            //        {
            //            ULogger.Error($"CreateActor Error. Behaviour Spawn Error");
            //            return null;
            //        }

            //        //애니메이션 타입을 강제할까 말까 고민. 번들로 묶는다면 강제 해야할 필요도 있음
            //        if (InAnimatorID.IsValid())
            //        {
            //            UAssetRefObject AnimatorRef2 = UResources.Manager.Asset("Monster/Nurse_Ring01/Animations/Nurse_Ring01_AC".ToAssetID());
            //            if (null != AnimatorRef2)
            //            {
            //                ActorBehaviour2.SetAnimatorRef(AnimatorRef2);
            //            }
            //        }

            //        ActorBehaviour2.SetBehaviourIndex(1);
            //        Actor.AttachBehaviour(1, ActorBehaviour2);

            //    }
            //}
            #endregion

            Actor.SetActorID(InKey);


            AddActor(InKey, Actor);
            //Actors.Add(InKey, Actor);
            
            return Actor;
        }
        public void ReleaseActor(long InActorID)
        {
            UActorObject Actor;
            if (!Actors.TryGetValue(InActorID, out Actor))
            {
                ULogger.Error($"ReleaseActor Error. Invalid Actor ID");

                return;
            }

            FreeActor(Actor);

            RemoveActor(InActorID);
        }

        public ObjectT GetActor<ObjectT>(long InKey) where ObjectT : UActorObject
        {
            UActorObject ActorObject;
            if(!Actors.TryGetValue(InKey, out ActorObject) )
            {
                return null;
            }

            ObjectT Actor = ActorObject as ObjectT;
            if(null == Actor )
            {
                ULogger.Error($"GetActor Typecast Error. {InKey}");

                return null;
            }

            return Actor;
        }

        //관리 개선이 필요
        public IEnumerator<ObjectT> Acquire<ObjectT>() where ObjectT : UActorObject
        {
            for (int i = 0; i < ActorsKeyList.Count; ++i)
            {
                if (Actors[ActorsKeyList[i]] is ObjectT)
                {
                    yield return Actors[ActorsKeyList[i]] as ObjectT;
                }
            }
        }

        public ActorDictionary GetActorsList()
        {
            return Actors;
        }

        public IEnumerator<Target> AcquireTarget<Target>(UActorObject MyObject) where Target : UActorObject
        {
            for (int i = 0; i < ActorsKeyList.Count; ++i)
            {
                if (Actors[ActorsKeyList[i]] is Target)
                {
                    var target = Actors[ActorsKeyList[i]];

                    if(MyObject == target)
                    {
                        continue;
                    }

                    if (target.IsInValidTarget())
                    {
                        continue;
                    }

                    if (!MyObject.IsCollisionLayer(target))
                    {
                        continue;
                    }

                    if (null == target.GetController())
                    {
                        continue;
                    }

                    yield return target as Target;
                }
            }
        }

        private T PoolAllocActor<T>() where T : UActorObject
        {
            IPool Pool;
            if(!ActorPool.TryGetValue(typeof(T), out Pool))
            {
                Pool = new UObjectPool<T>();

                ActorPool.Add(typeof(T), Pool);
            }
            
            return (Pool as UObjectPool<T>).Alloc();
        }

        private void PoolFreeActor(UActorObject InActor)
        {
            IPool Pool;
            if (!ActorPool.TryGetValue(InActor.GetType(), out Pool))
            {
                ULogger.Error($"FreeActor Error. ActorType: {InActor.GetType().FullName}");

                return;
            }

            Pool.Free(InActor);
        }

        private void FreeActor(UActorObject InActor)
        {
            var Behaviours = InActor.GetBehaviours().GetEnumerator();
            while (Behaviours.MoveNext())
            {
                //Parts Release
                InActor.DestroyAllParts();

                //Animator Release
                Behaviours.Current.DestroyAnimatorRef();

                Behaviours.Current.ResetTransform();
                //Remove Scene
                UScene.Manager.RemoveObject(Behaviours.Current);
            }
            
            //Clear Behaviours
            InActor.DetachAllBehaviours();

            PoolFreeActor(InActor);
        }

        private void ClearActors()
        {
            for (int i =0; i < ActorsKeyList.Count; ++i)
            {
                FreeActor(Actors[ActorsKeyList[i]]);
            }

            Actors.Clear();

            ActorsKeyList.Clear();

            //var ActorEnumerator = Actors.Values.GetEnumerator();
            //while(ActorEnumerator.MoveNext())
            //{
            //    FreeActor(ActorEnumerator.Current);
            //}

            //Actors.Clear();
        }

        private void ClearActorPool()
        {
            var PoolEnumerator = ActorPool.Values.GetEnumerator();
            while(PoolEnumerator.MoveNext())
            {
                PoolEnumerator.Current.Release();
            }

            ActorPool.Clear();
        }


        private void AddActor(long InKey, UActorObject InActor)
        {
            Actors.Add(InKey, InActor);
            ActorsKeyList.Add(InKey);
        }

        private void RemoveActor(long InKey)
        {
            Actors.Remove(InKey);
            ActorsKeyList.Remove(InKey);
        }
    }
}
