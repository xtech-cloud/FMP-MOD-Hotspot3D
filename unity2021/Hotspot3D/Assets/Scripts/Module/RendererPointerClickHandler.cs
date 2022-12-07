using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XTC.FMP.MOD.Hotspot3D.LIB.Unity
{
    public class RendererPointerClickHandler : MonoBehaviour, IPointerClickHandler
    {
        public Action<PointerEventData> OnClick;

        public void OnPointerClick(PointerEventData _eventData)
        {
            OnClick(_eventData);
        }
    }
}
