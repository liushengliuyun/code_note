using Core.Extensions.UnityComponent;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIBingoPauseGroup : MonoBehaviour
    {
        public MyButton EndNowBtn;
        public MyButton HowToplayBtn;
        public MyButton ResumeBtn;

        public Toggle MusicToggle;
        public Toggle SoundToggle;
        public Toggle VibrationToggle;

        public Slider MusicSlider;
        public Slider SoundSlider;
        public Slider VibrationSlider;
    }
}