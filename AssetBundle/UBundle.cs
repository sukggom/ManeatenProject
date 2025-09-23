using System.Collections.Generic;
using UnityEngine;

namespace UG.Framework
{
    struct LogUBundleRefCount : ULogger.ILogCategory
    {
        public bool On()
        {
            return true;
        }
    }

    internal class UBundle : URefHandle
    {
        private AssetBundle Bundle = null;

        private UBundleID BundleID = null;

        private List<URefHandle> BundleDependencies = new List<URefHandle>();

        public static float MaxLifeTime = 120.0f;

        private UBundleDestroyer Destroyer = null;

        public UBundle(UBundleID InBundleID, AssetBundle InBundle, UBundleDestroyer InDestroyer)
        {
            BundleID = InBundleID;
            Bundle = InBundle;
            Destroyer = InDestroyer;
        }

        internal override void IncreaseRef()
        {
            base.IncreaseRef();

#if DEV_TRACE_REF
            ULogger.Log<LogUBundleRefCount>($"AssetBundle IncreaseRef: {BundleID.GetBundleName()}, Count: {Reference}");
#endif
        }
        internal override void DecreaseRef()
        {
            base.DecreaseRef();

#if DEV_TRACE_REF
            ULogger.Log<LogUBundleRefCount>($"AssetBundle DecreaseRef: {BundleID.GetBundleName()}, Count: {Reference}");
#endif

            if (IsZeroRef())
            {
                Destroyer.Add(this);
            }
        }

        public UBundleID GetBundleID()
        {
            return BundleID;
        }

        public void AddBundleDependencies(URefHandle InBundleRefHandle)
        {
            if (InBundleRefHandle != null)
            {
                BundleDependencies.Add(InBundleRefHandle);

                InBundleRefHandle.IncreaseRef();
                //InBundleRefHandle.AddRef(null);
            }
        }

        private void RemoveBundleDependencies()
        {
            for (int BundleIndex = 0;
                BundleIndex < BundleDependencies.Count;
                ++BundleIndex)
            {
                if (BundleDependencies[BundleIndex] != null)
                {
                    BundleDependencies[BundleIndex].DecreaseRef();

                    ULogger.Log<LogUBundleRefCount>("====> Remove Bundle Dependencies : " + BundleID.GetBundleName());
                }
            }

            BundleDependencies.Clear();
        }


        public bool IsValid()
        {
            if (BundleID.IsResourcesType() == false)
            {
                if (Bundle == null)
                {
                    return false;
                }
            }

            return true;
        }

        public UnityEngine.Object LoadBaseAssetObject(UAssetID InAssetID, UAssetDestroyer InDestroyMarker)
        {
            if (UAssetID.CheckValid(InAssetID) == false)
            {
                return null;
            }

            if (IsValid() == false)
            {
                return null;
            }
            
            UnityEngine.Object LoadedAsset = Bundle.LoadAsset(string.Format(UFramework.GetSettings().Bundle_AssetRootPath(),InAssetID.GetName()));
            if (LoadedAsset != null)
            {
                return LoadedAsset;
            }
            else
            {
                ULogger.Error(
                    "Create asset error!: " + InAssetID.ToString());

                if (Bundle != null)
                {
                    string[] AllAssetNames = Bundle.GetAllAssetNames();
                    for (int i = 0; i < AllAssetNames.Length; ++i)
                    {
                        ULogger.Error(
                            "    AssetList!: " + AllAssetNames[i]);
                    }
                }

                return null;
            }
        }

        /**
         * 번들과 할께 의존하는 번들들과, 이 번들에서 로드한 Asset들을 모두 제거한다.
         */
        public void ReleaseBundle()
        {
            RemoveBundleDependencies();

            //var Enumerator = BaseAssetObjects.Values.GetEnumerator();
            //while (Enumerator.MoveNext())
            //{
            //    Enumerator.Current.Clear();
            //}

            //BaseAssetObjects.Clear();

            if (Bundle != null)
            {
                Bundle.Unload(true);
                Bundle = null;
            }
        }

    }

}