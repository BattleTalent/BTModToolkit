using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CrossLink
{

    public class UIOption : MonoBehaviour
    {
        public int selectedIndex = 0;
        protected int selectRange = 10;
        public void UpdateOptionNum(int num) { selectRange = num; }
        public bool cycle = true;

        public UIButton next;
        public UIButton prev;

        public event System.Action<UIOption> OptionUpdateEvent;


    }

}