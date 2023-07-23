using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace CrossLink
{
    public interface IUIButtonData
    {
        void OnEnable(UIButton buttonScript);
        // public void Initialize();
    }
    
    public class UIButton : UIEvent
    {
        public MaskableGraphic image;
        public Text text;
        public GameObject[] enabledObjs;
        public Image selectImage;
        public Color normalColor = Color.white;
        public Color hoverColor = Color.white;
        public Color disableColor = Color.white;        
        public UnityEvent select;
        public event System.Action<UIEvent> SelectEvent;
        public IUIButtonData data;

    }

}