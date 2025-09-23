namespace UG.Framework
{

    struct LogUAssetObjectRefCount : ULogger.ILogCategory
    {
        public bool On()
        {
            return true;
        }
    }
    
    internal class UAssetObject : URefHandle
    {
        private UnityEngine.Object Resource = null;

        private UBundleID BundleID = UBundleID.NullBundleID;
        private UAssetID AssetID = UAssetID.Invalid;
        private UAssetDestroyer DestroyMarker = null;
        public static float MaxLifeTime = 30.0f;

        public UAssetObject(UAssetID InAssetID, UnityEngine.Object InResource, UAssetDestroyer InMarker, UBundleID InBundleID)
        {
            AssetID = InAssetID;
            Resource = InResource;
            DestroyMarker = InMarker;

            BundleID = InBundleID;
        }

        internal override void IncreaseRef()    
        {
            base.IncreaseRef();

            DestroyMarker.CancelDestroyAssetObject(this);

#if DEV_TRACE_REF
            ULogger.Log<LogUAssetObjectRefCount>($"AssetObject IncreaseRef: {AssetID.GetName()}, Count: {Reference}");
#endif
        }
        internal override void DecreaseRef()
        {
            base.DecreaseRef();

            if (IsZeroRef())
            {
                DestroyMarker.Add(this);
            }

#if DEV_TRACE_REF
            ULogger.Log<LogUAssetObjectRefCount>($"AssetObject DecreaseRef: {AssetID.GetName()}, Count: {Reference}");
#endif
        }

        public UAssetID GetAssetID()
        {
            return AssetID;
        }

        public UBundleID GetBundleID()
        {
            return BundleID;
        }

        public UnityEngine.Object Instantiate()
        {
            return UnityEngine.Object.Instantiate(Resource);
        }

        public UnityEngine.Object Instantiate(UnityEngine.Vector3 InCenter)
        {
            return UnityEngine.Object.Instantiate(Resource, InCenter, UnityEngine.Quaternion.identity);
        }

        public UnityEngine.Object Asset()
        {
            return Resource;
        }

        public bool IsBuiltInResource()
        {
            return GetBundleID() == UBundleID.NullBundleID;
        }
    }
}
