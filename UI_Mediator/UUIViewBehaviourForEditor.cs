using System;
using System.Reflection;
using UnityEngine;

namespace UG.Framework
{
    public partial class UUIViewBehaviour : UManagedBehaviour
    {
#if UNITY_EDITOR
        internal void EventMappingForEditor()
        {
            EventMap.ClearEvents();

            var UIBehaviours = GetComponentsInChildren<UnityEngine.EventSystems.UIBehaviour>();

            for(int i = 0; i < UIBehaviours.Length; ++i)
            {
                if(UUnityHelper.IsEventSystemClass(UIBehaviours[i].GetType()))
                {
                    string Path = UUnityHelper.GetHierachyPath(UIBehaviours[i].gameObject, false);

                    EventMap.BindEvent(Path, UIBehaviours[i]);
                }
            }
                
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);

        }

        /// <summary>
        /// Binding Variables
        /// </summary>

        [ContextMenu("Binding")]
        internal void AutoBindingForEditor()
        {
            ProcessForEditor(transform);

            UnityEditor.EditorUtility.SetDirty(this);
        }

        internal void ProcessForEditor(Transform InTrans)
        {
            BindingFlags BindingFlag = BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.DeclaredOnly;

            for (int i = 0; i < InTrans.childCount; ++i)
            {
                Transform Child = InTrans.GetChild(i);
                int FindIndex = Child.name.IndexOf("this.");
                if (-1 == FindIndex)
                {
                    continue;
                }

                FieldInfo Info = null;
                object SetObject = this;
                Type SetObjectType = GetType();

                string[] Splits = Child.name.Split(".");
                for(int j = 1; j < Splits.Length; ++j)
                {
                    int ArrIndex;
                    string VariableName;
                    if(HasArrayForEditor(Splits[j], out VariableName, out ArrIndex))
                    {
                        Info = SetObjectType.GetField(VariableName, BindingFlag);
                        if(null == Info)
                        {
                            ULogger.Error($"SearchField Error. Name: {VariableName}, Index: {ArrIndex}");
                            break;
                        }
                        if (Info.FieldType.IsArray)
                        {
                            var ArrayObject = (System.Array)Info.GetValue(this);
                            int ResizeIndex = ArrIndex + 1;
                            Type ElemType = Info.FieldType.GetElementType();

                            if (ResizeIndex >= ArrayObject.Length )
                            {
                                var NewArray = Array.CreateInstance(ElemType, ResizeIndex);

                                ArrayObject.CopyTo(NewArray, 0);
                              
                                Info.SetValue(this, NewArray);

                                ArrayObject = NewArray;
                            }

                            if (UUnityHelper.IsUnityClass(ElemType))
                            {
                                ArrayObject.SetValue(Child.GetComponent(ElemType), ArrIndex);

                                Info = null;

                                break;
                                //여기선 더이상 추적할수가 없음.
                                //해당컴포넌트를 생성하는게 아니라서 어차피 UnityObject 가 종착지
                            }
                            else
                            {
                                //일반 객체라면 클래스던 뭐던 생성해서 안쪽의 필드정보로 갱신
                                if(null == ArrayObject.GetValue(ArrIndex))
                                {
                                    SetObject = Activator.CreateInstance(ElemType);
                                    ArrayObject.SetValue(SetObject, ArrIndex);
                                }
                                else
                                {
                                    SetObject = ArrayObject.GetValue(ArrIndex);
                                }

                                SetObjectType = Info.FieldType.GetElementType();
                            }
                        }
                        else
                        {
                            ULogger.Error($"ArrayType Error");

                            break;
                        }
                    }
                    else
                    {
                        Info = SetObjectType.GetField(Splits[j], BindingFlag);

                        //클래스 이거나, 스트럭쳐이거나, 유니티 클래스가 아니면 다음 클래스 정보를 위해 갱신
                        if (UCSharpHelper.IsClassOrStruct(Info.FieldType)//struct
                        && !UUnityHelper.IsUnityClass(Info.FieldType)) 
                        {
                            SetObject = Info.GetValue(SetObject);
                            SetObjectType = Info.FieldType;
                        }
                    }
                }

                if (null != Info)
                {
                    if (Info.FieldType == typeof(UnityEngine.GameObject))
                    {
                        Info.SetValue(SetObject, Child.gameObject);
                    }
                    else
                    {
                        Info.SetValue(SetObject, Child.GetComponent(Info.FieldType));
                    }
                }

                ProcessForEditor(Child);
            }
        }

        internal bool HasArrayForEditor(string InPath, out string OutVariableName, out int OutArrIndex)
        {
            OutArrIndex = -1;
            OutVariableName = string.Empty;

            int BeginIndex = -1;
            int EndIndex = -1;
            for (int i = 0; i < InPath.Length; ++i)
            {
                if (InPath[i] == '[')
                {
                    BeginIndex = i;
                }

                if (InPath[i] == ']')
                {
                    EndIndex = i;
                }
            }

            int Result = -1;

            if (BeginIndex != -1 && EndIndex != -1)
            {
                int StartIndex = BeginIndex + 1;
                string ArrayIndex = InPath.Substring(StartIndex, EndIndex - StartIndex);
                if(int.TryParse(ArrayIndex, out Result))
                {
                    OutArrIndex = Result;

                    OutVariableName = InPath.Substring(0, BeginIndex);

                    return true;
                }
            }

            return false;
        }

#endif
    }
}
