using System;
using System.Collections.Generic;

namespace UG.Framework
{
    public class UDatabase : UManager<UDatabase>
    {
        private Dictionary<string, IDataRecord> Records 
            = new Dictionary<string, IDataRecord>();

        private UJSONLoader JsonLoader = new UJSONLoader();

        private UScriptableLoader ScriptableLoader = new UScriptableLoader();

        private List<IDataRecord> Preloads = new List<IDataRecord>();

        public override void Initialize()
        {
        }

        public override void UnInitialize()
        {
            this.ClearPreloads();
        }

        public override void Update(float InDeltaTime)
        {
        }

        public override void ChangingScene()
        {
        }
        public override void ChangedScene()
        {
        }

        public void Preload<TValue>(UDataRecord<TValue> InTrunk) where TValue : class
        {
            if (Preloads.Contains(InTrunk))
            {
                ULogger.Error($"Preload failed. Path: {InTrunk.GetType().FullName}");

                return;
            }
            Preloads.Add(InTrunk);
        }

        public void ClearPreloads()
        {
            for(int i = 0; i < Preloads.Count; ++i)
            {
                if(Preloads[i].GetRefCount() != 1)
                {
                    ULogger.Error($"Database Preload RefCount Error. {Preloads[i].GetPath()} / {Preloads[i].GetRefCount()}");
                }
                else
                {
                    (Preloads[i] as IDisposable).Dispose();
                }
            }

            Preloads.Clear();
        }

        public UDataRecord<TValue> LoadJsonDatabase<TValue>(string InPath) where TValue : class
        {
            UDataRecord<TValue> Trunk = null;

            IDataRecord TmpTrunk;
            if (Records.TryGetValue(InPath, out TmpTrunk))
            {
                Trunk = TmpTrunk as UDataRecord<TValue>;
                if (null == Trunk)
                {
                    ULogger.Error($"LoadDatabase Typecast Error. Path: {InPath}");

                    return null;
                }
            }
            else
            {
                Trunk = JsonLoader.Load<TValue>(InPath);

                if (null == Trunk)
                {
                    ULogger.Error($"LoadDatabase Load Error. Path: {InPath}");

                    return null;
                }

                Records.Add(InPath, Trunk);
            }

            Trunk.IncreaseRef(UnloadDatabase);

            return Trunk;
        }

        public UDataRecord<TValue> LoadScriptableDatabase<TValue>(string InPath) where TValue : class
        {
            UDataRecord<TValue> Trunk = null;

            IDataRecord TmpTrunk;
            if (Records.TryGetValue(InPath, out TmpTrunk))
            {
                Trunk = TmpTrunk as UDataRecord<TValue>;
                if (null == Trunk)
                {
                    ULogger.Error($"LoadDatabase Typecast Error. Path: {InPath}");

                    return null;
                }
            }
            else
            {
                Trunk = ScriptableLoader.Load<TValue>(InPath);
                
                if (null == Trunk)
                {
                    ULogger.Error($"LoadDatabase Load Error. Path: {InPath}");

                    return null;
                }
                
                Records.Add(InPath, Trunk);
            }

            Trunk.IncreaseRef(UnloadDatabase);
            
            return Trunk;

        }
        private void UnloadDatabase(IDataRecord InTrunk)
        {
            bool IsZeroRef = InTrunk.DecreaseRef();
            if (IsZeroRef)
            {
                InTrunk.DestroyAssetRef(); //unload resource

                Records.Remove(InTrunk.GetPath());
            }
        }
    }
}
