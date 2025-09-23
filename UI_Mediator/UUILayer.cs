using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UG.Framework
{
    public class UUILayer
    {
        public static readonly int Invalid = -1;

        private List<IUIController> Objects = new List<IUIController>();

        private GameObject LayerObject = null;

        private RectTransform LayerTransform = null;

        private Canvas Canvas = null;

        private CanvasScaler CanvasScalar = null;

        private CanvasGroup Group = null;

        private GraphicRaycaster Raycaster = null;

        //private bool UniqueModelLayer = false;

        public int Depth
        {
            get;
            private set;
        }

        public UUILayer(GameObject InRoot, int InDepth)
        {
            Depth = InDepth;

            LayerObject = new GameObject($"UILayer_{InDepth}");
            LayerObject.transform.SetParent(InRoot.transform);
            LayerObject.transform.localPosition = Vector3.zero;
            LayerObject.transform.localRotation = Quaternion.identity;
            LayerObject.transform.localScale = Vector3.one;

            LayerObject.layer = LayerMask.NameToLayer("UI");

            GameObject Rect = new GameObject("LayerRect");

            LayerTransform = Rect.AddComponent<RectTransform>();
            LayerTransform.SetParent(LayerObject.transform);
            LayerTransform.localPosition = Vector3.zero;
            LayerTransform.localRotation = Quaternion.identity;
            LayerTransform.localScale = Vector3.one;
            LayerTransform.anchorMin = new Vector2(0, 0);
            LayerTransform.anchorMax = new Vector2(1, 1);
            LayerTransform.offsetMin = new Vector2(0, 0);
            LayerTransform.offsetMax = new Vector2(0, 0);

            Canvas = LayerObject.AddComponent<Canvas>();
            Canvas.sortingLayerName = $"SortingLayer_{InDepth}";
            Canvas.pixelPerfect = false;
            Canvas.planeDistance = 1.0f;
            Canvas.sortingOrder = InDepth;
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScalar = LayerObject.AddComponent<CanvasScaler>();

            Group = LayerObject.AddComponent<CanvasGroup>();
            Raycaster = LayerObject.AddComponent<GraphicRaycaster>();
        }

        public void SetCanvasScaler(CanvasScaler.ScaleMode InScaleMode, Vector2 InResolution, float InMatchWidthOrHeight)
        {
            if (CanvasScalar != null)
            {
                CanvasScalar.uiScaleMode = InScaleMode;
                CanvasScalar.referenceResolution = InResolution;
                CanvasScalar.matchWidthOrHeight = InMatchWidthOrHeight;
            }
        }

        internal void Add(IUIController InController)
        {
            Objects.Add(InController);

            InController.GetViewBehaviour().AttachParent(LayerTransform);
            InController.GetViewBehaviour().ResetUITransform();
        }

        internal void Remove(IUIController InController)
        {
            Objects.Remove(InController);

            //InBehaviour.DetachParent();//해야하나? 어차피 지워질거 같은데 테스트필요
        }

        internal List<IUIController> GetControllers()
        {
            return Objects;
        }

        internal void ClearBehaviours()
        {
            Objects.Clear();
        }

        public void SetBlock(bool IsBlock)
        {
            Group.interactable = !IsBlock;
        }

        public GameObject GetLayerObject()
        {
            return LayerObject;
        }

        public RectTransform GetRectTransform()
        {
            return LayerTransform;
        }
    }
}
