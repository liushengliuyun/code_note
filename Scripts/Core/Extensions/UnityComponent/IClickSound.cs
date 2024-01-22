using UnityEngine;

namespace Core.Extensions.UnityComponent
{
    public interface IClickSound
    {
        public bool ClickSound { get; set; }

        public AudioClip AudioClip { get; set; }

        public SoundPack SoundPack { get; set; }
    }
}