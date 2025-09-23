using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UG.Framework
{
    [Serializable]
    internal class UUIEventMapper
    {
        [Serializable]
        public class UData
        {
            public string Name;
            public UIBehaviour Behaviour;
        }

        [SerializeField]
        private List<UData> EventObjects = new List<UData>();


        private Dictionary<string, UData> EventDictionary = new Dictionary<string, UData>();

        internal void Initialize()
        {
            for (int i = 0; i < EventObjects.Count; ++i)
            {
                EventDictionary.Add(EventObjects[i].Name, EventObjects[i]);
            }
        }
        internal void UnInitialize()
        {
            EventDictionary.Clear();
        }

        internal void BindEvent(string InName, UIBehaviour InBehaviour)
        {
            EventObjects.Add(new UData()
            {
                Name = InName,
                Behaviour = InBehaviour
            });
        }

        internal void ClearEvents()
        {
            EventObjects.Clear();
        }

        public T GetBehaviour<T>(string InPath) where T : UIBehaviour
        {
            UData Data;
            if(EventDictionary.TryGetValue(InPath, out Data))
            {
                return Data.Behaviour as T;
            }

            return null;
        }
    }
}
