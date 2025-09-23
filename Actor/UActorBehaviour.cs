using System;
using System.Collections.Generic;
using UnityEngine;

namespace UG.Framework
{
    public enum eSocketType
    {
        Pelvis,
        Weapon,
        Head,
    }



    public abstract class UActorBehaviour : UManagedBehaviour
    {
        [Serializable]
        public class USocket
        {
            public int Socket;
            public Transform Parent;
        }

        [SerializeField]
        private USocket[] Sockets = null;

        [SerializeField]
        private Renderer Renderer = null;

        [SerializeField]
        private Transform RootBone;

        [SerializeField]
        private Transform[] Bones = null;

        [SerializeField]
        private UFXBehaviour ManualFXBehaviour;


        private int BehaviourIndex = 0;

        private UAssetRefObject AnimatorRef = null;

        private Dictionary<int, UActorPartsBehaviour> Parts = new Dictionary<int, UActorPartsBehaviour>();
        public abstract IActorAnimator GetAnimator();
        public abstract IActorController GetController();
        public abstract void BindingAnimator(UAssetRefObject InAnimatorRef);

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void UnInitialize()
        {
            DestroyAnimatorRef();
            ManualFXBehaviour?.OnFree();
            base.UnInitialize();
        }

        internal void SetAnimatorRef(UAssetRefObject InAnimatorRef)
        {
            AnimatorRef = InAnimatorRef;

            BindingAnimator(InAnimatorRef);
        }

        internal void DestroyAnimatorRef()
        {
            AnimatorRef?.Destroy();
            AnimatorRef = null;
        }

        public virtual void CheckVelocity()
        {

        }

        public override void DestroyGameObject()
        {
            var FxList = GetComponentsInChildren<UFXBehaviour>();

            if (null != FxList)
            {
                for (int i = 0; i < FxList.Length; ++i)
                {
                    FxList[i].DestroyFX();
                }
            }

            base.DestroyGameObject();
        }


        internal void AttachParts<T>(int InPartsIndex, T InParts, int InSocketIndex) where T : UActorPartsBehaviour
        {
            USocket Socket = GetSocket(InSocketIndex);
            if (null == Socket)
            {
                ULogger.Error($"AttachParts Error. Invalid socketIndex");

                return;
            }

            if (Parts.ContainsKey(InPartsIndex))
            {
                ULogger.Error($"AttachParts Error. Contains Parts Key");

                return;
            }

            Parts.Add(InPartsIndex, InParts);

            InParts.AttachParent(Socket.Parent);
            InParts.ResetLocalTransform();
            InParts.AttachSkinnedMeshRenderer(Socket.Parent);
        }

        public UActorPartsBehaviour DetachParts(int InPartsIndex)
        {
            UActorPartsBehaviour PartsBehaviour;
            if (!Parts.TryGetValue(InPartsIndex, out PartsBehaviour))
            {
                return null;
            }

            Parts.Remove(InPartsIndex);

            return PartsBehaviour;
        }

        internal void SetBehaviourIndex(int InBehaviourIndex)
        {
            BehaviourIndex = InBehaviourIndex;
        }

        internal Dictionary<int, UActorPartsBehaviour> GetParts()
        {
            return Parts;
        }
        public int GetBehaviourIndex()
        {
            return BehaviourIndex;
        }

        public UActorPartsBehaviour GetParts(int InPartsIndex)
        {
            UActorPartsBehaviour PartsBehaviour;
            if (!Parts.TryGetValue(InPartsIndex, out PartsBehaviour))
            {
                return null;
            }

            return PartsBehaviour;
        }

        public USocket GetSocket(int InSocket)
        {
            for (int i = 0; i < Sockets?.Length; ++i)
            {
                if (Sockets[i].Socket == InSocket)
                {
                    return Sockets[i];
                }
            }

            return null;
        }

        public Renderer GetRenderer()
        {
            return Renderer;
        }

        public virtual void SetWeaponModelKey(string InKey)
        {

        }

        public override void ResetTransform()
        {
            base.ResetTransform();

            //Debug.Log("Changing");
            if (Bones != null)
            {
                for(int i =0; i < Bones.Length; ++i)
                {
                    Bones[i].localScale = Vector3.one;
                }
            }
        }
        private void OnValidate()
        {
            if (RootBone != null)
            {
                Bones = RootBone.GetComponentsInChildren<Transform>(false);
            }

            if (Renderer == null)
            {
                Renderer = GetComponentInChildren<Renderer>();
            }

            ManualFXBehaviour = GetComponentInChildren<UFXBehaviour>();

        }
    }
}
