using System.Collections.Generic;
using UnityEngine;

namespace UG.Framework
{
    public class UAssetPool
    {
        private UObjectPool<IPoolObject> Pool = new UObjectPool<IPoolObject>();

        public UAssetPool(UAssetID InAssetID, UResources InResource, int InCapacity)
        {
            Pool.SetAllocator(new UResources.UAllocator(InAssetID, InResource));
            Pool.AddCount(InCapacity);
        }

        public UManagedBehaviour Alloc(Vector3 InCenter)
        {
            UManagedBehaviour Object = Pool.Alloc() as UManagedBehaviour;
            if (null != Object)
            {
                Object.SetPosition(InCenter);
                Object.GetGameObject().SetActive(true);
            }

            return Object;
        }


        public UManagedBehaviour Alloc()
        {
            UManagedBehaviour Object = Pool.Alloc() as UManagedBehaviour;
            if (null != Object)
            {
                Object.GetGameObject().SetActive(true);
            }

            return Object;
        }

        public void AddSize(int InCount)
        {
            Pool.AddCount(InCount);
        }

        public void Free(UManagedBehaviour InObject)
        {
            InObject.DetachParent();
            InObject.GetGameObject().SetActive(false);

            IPoolObject PoolObject = InObject as IPoolObject;
            if (null == PoolObject)
            {
                ULogger.Error($"Invalid PoolObject Type. {InObject.name} {InObject.GetType().FullName}");

                return;
            }

            Pool.Free(PoolObject);
        }

        public void Release()
        {
            var Enumerator = Pool.GetEnumerator();
            while (Enumerator.MoveNext())
            {
                UResources.DestroyObject(Enumerator.Current as UManagedBehaviour);
            }

            Pool.Release();
        }

    }
}
