using UnityEngine;

namespace ScMap
{
    public class MULight : TriggerTarget
    {
        public override void OnReset()
        {
            SwitchGoActive(false);
            SwitchGoActive(true);
        }
        public override void OnOpen()
        {
            SwitchGoActive(true);
        }
        public override void OnClose()
        {
            SwitchGoActive(false);
        }
        public override void OnSwitch()
        {
            SwitchGoActive(!isOn);
        }

        void SwitchGoActive(bool _switch)
        {
            isOn = _switch;
            for (int i = 0; i < goList.Length; i++)
            {
                goList[i].SetActive(_switch);
            }
        }
        public GameObject[] goList;
        private bool isOn = false;
    }
}
