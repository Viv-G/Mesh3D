namespace GoogleARCore.Examples.Common
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class TextUpdate : MonoBehaviour
    {
        public Text Text_Offset;
        public Text Text_Rotate;
        public Text Text_conf;
        public static string t_offSet;
        public static string t_roTate;
        public static string t_conf;
        private int _frameTrack;
        // Start is called before the first frame update

        private void Update()
        {
            _frameTrack += 1;
            if (_frameTrack % 3 == 0)
            {
                //   UpdateText(t_offSet);
                Text_Offset.text = t_offSet;
                Text_Rotate.text = t_roTate;
                Text_conf.text = t_conf;
            }
        }


        public void UpdateText(string textOffset)
        {
            Text_Offset.text = textOffset;
        }
    }
}
