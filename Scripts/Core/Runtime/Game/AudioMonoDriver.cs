using System;
using System.Collections;
using System.Collections.Generic;
using Core.Extensions;
using Core.Services.ResourceService.API.Facade;
using UnityEngine;
using UnityEngine.Audio;
using YooAsset;

#region enum SoundPack

public enum SoundPack
{
    Button_Click_valid,
    Button_Click_invalid,
    _4x1,
    _6x1,

    //双倍道具使用时
    Double_Use,

    //双倍时间持续时
    Double_Scoreing,

    //匹配中
    Match_Count_Down,

    //匹配成功
    Match_Success,

    Game_Complecte,

    Game_Win,

    Game_Fail,

    //通用奖励弹窗时
    Get_Reward,

    //点击领取奖励按钮
    Claim_Reward,

    Wheel,

    //转盘指定奖品时
    Wheel_END_Reward,

    Dice_Roll,

    //施法动作
    Casting_Action,

    Yatzy,

    None,

    /// <summary>
    /// 道具使用限时
    /// </summary>
    Prop_Use_Count_Down,

    Section_Bonus,

    Get_Score,

    Not_Get_Score,

    Switch_Sound,
    
    Res_Change_Sound,
    
    /// <summary>
    /// 十字消道具
    /// </summary>
    Cross,
    
    /// <summary>
    /// bingo获得前3名的音效
    /// </summary>
    Bingo_Game_Win,
    
    /// <summary>
    /// 多个bingo
    /// </summary>
    Bingo_Mul_Count,
    
    /// <summary>
    /// 多个fullHouse
    /// </summary>
    Bingo_FullHouse,
    
    /// <summary>
    /// 达成普通Bingo
    /// </summary>
    Bingo_Normal
}

#endregion

public class AudioMonoDriver : MonoBehaviour
{
    static float[] volume =
    {
        -80, -78, -75, -71, -65, -63, -62, -61.8f, -61.1f, -60.5f, -60, -59, -58, -56, -55, -53, -51, -49, -47, -44,
        -42, -41, -40, -39.5f, -39, -38.5f, -38, -37.5f, -37, -36.5f, -36, -35, -34, -33, -32, -31, -30, -29, -28, -26,
        -25, -24.5f, -24, -23, -22, -21, -20, -19, -18, -17, -16, -15, -14, -13, -12, -11.8f, -11.3f, -11, -10.9f,
        -10.6f, -10.3f, -10.1f, -9.8f, -9.5f, -9.3f, -9, -8.9f, -8.6f, -8.4f, -8.2f, -8.1f, -7.8f, -7.5f, -7.2f, -7.0f,
        -6.8f, -6.5f, -6.3f, -6, -5.9f, -5.8f, -5.5f, -5.0f, -4.5f, -4.0f, -3.8f, -3.5f, -3.2f, -3, -2.7f, -2.5f, -2.3f,
        -2.2f, -2.1f, -2, -1.8f, -1.5f, -1, -0.5f, 0
    };

    class AudioHandler
    {
        public readonly AudioSource AudioSource;
        public float BeginTime;
        public float EndTime;
        public float Duration;
        public SoundPack? SoundPack;
        public AssetOperationHandle AssetOperationHandle; 

        public float Length
        {
            get
            {
                float endPoint = EndTime > 0 ? Math.Min(EndTime, AudioSource.clip.length) : AudioSource.clip.length;

                var diff = endPoint - BeginTime;

                if (diff > 0)
                {
                    return diff;
                }
                else
                {
                    return AudioSource.clip.length;
                }
            }
        }

        public int LoopCount;

        public bool IsPlaying => AudioSource.isPlaying;

        private bool isPause;

        public bool IsPause
        {
            set
            {
                if (value)
                {
                    AudioSource.Pause();
                }

                isPause = value;
            }
            get => isPause;
        }

        public AudioHandler()
        {
        }

        ~AudioHandler()
        {
            AssetOperationHandle?.Release();
            AssetOperationHandle = null;
        }
        
        public AudioClip Clip
        {
            set => AudioSource.clip = value;
            get => AudioSource.clip;
        }

        private bool Kill { get; set; }
        public float Delay { get; set; }

        public Coroutine Coroutine { get; set; }

        public AudioHandler(AudioSource audioSource)
        {
            this.AudioSource = audioSource;
        }

        public void Resume()
        {
            isPause = false;
            if (killed)
            {
                return;
            }

            AudioSource.Play();
        }

        public void Pause()
        {
            AudioSource.Pause();
        }

        public void Reset()
        {
            AudioSource.time = 0;
            isPause = false;
            Kill = false;
            BeginTime = 0;
            EndTime = 0;
            Duration = 0;
            SoundPack = null;
            LoopCount = 0;
        }

        /// <summary>
        /// 播放持续的时间
        /// </summary>
        private float playingTime;


        private int loopedCount;
        private bool killed;

        public void Play()
        {
            playingTime = 0;

            loopedCount = 0;

            killed = false;
            isPause = false;

            AudioSource.time = BeginTime;
            if (Delay > 0)
            {
                AudioSource.PlayDelayed(Delay);
            }
            else
            {
                AudioSource.Play();
            }
        }

        public void Stop()
        {
            Kill = true;
        }

        private void InternalStop()
        {
            Kill = false;
            killed = true;
            isPause = false;
            AudioSource.time = 0;
            AudioSource.Stop();
        }

        public void Update(float deltaTime)
        {
            if (Clip == null)
            {
                return;
            }

            if (killed)
            {
                return;
            }

            if (Kill)
            {
                InternalStop();
                return;
            }

            if (LoopCount == 0 && Duration == 0)
            {
                return;
            }

            if (isPause)
            {
                return;
            }

            if (Delay > 0)
            {
                Delay -= deltaTime;
                return;
            }

            //如果Duration = 0 ,不会进入这些逻辑
            if (Duration > 0)
            {
                Duration -= deltaTime;
            }

            if (Duration < 0)
            {
                InternalStop();
            }

            playingTime += deltaTime;

            if (LoopCount > loopedCount)
            {
                if (playingTime > (loopedCount + 1) * Length)
                {
                    loopedCount++;

                    AudioSource.time = BeginTime;
                    AudioSource.Play();
                }
            }
            else
            {
                InternalStop();
            }
        }
    }

    public AudioMixer AudioMixer;
    public AudioMixerGroup SoundMixer;
    public AudioMixerGroup MusicMixer;

    public AudioSource Sound;

    public AudioSource Music;

    [SerializeField] private AudioClip defaultSound;
    [SerializeField] private AudioClip defaultMusic;

    private readonly List<AudioHandler> handlerPool = new();

    private int lastUseIndex;

    void Awake()
    {
        handlerPool.Add(new AudioHandler(Sound));
    }

    AudioHandler GetSoundHandler(SoundPack soundPack)
    {
        AudioHandler handler = null;
        bool findOld = false;
        switch (soundPack)
        {
            case SoundPack.Match_Count_Down:
            case SoundPack.Prop_Use_Count_Down:
                handler = handlerPool.Find(source => source.SoundPack == soundPack);
                if (handler != null)
                {
                    findOld = true;
                }

                break;
        }

        if (handler == null)
        {
            lastUseIndex = handlerPool.FindIndex(source => !source.IsPlaying && !source.IsPause);
            handler = lastUseIndex >= 0 ? handlerPool[lastUseIndex] : null;
        }

        if (handler == null)
        {
            if (handlerPool.Count > 15)
            {
                lastUseIndex = (lastUseIndex + 1) % handlerPool.Count;
                handler = handlerPool[lastUseIndex];
            }
            else
            {
                lastUseIndex = handlerPool.Count;
                handler = new AudioHandler(transform.AddChildGameObject<AudioSource>("Sound"));
                handler.AudioSource.playOnAwake = false;
                handlerPool.Add(handler);
            }
        }

        handler.AudioSource.outputAudioMixerGroup = SoundMixer;


        if (!findOld)
        {
            handler.Reset();
        }

        return handler;
    }

    public void PlaySound(string url)
    {
        if (url.IsNullOrEmpty()) return;
        var clip = ResourceSystem.That.LoadAssetSync<AudioClip>(url);
        if (clip == null)
        {
            return;
        }

        Sound.PlayOneShot(clip);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="soundPack"></param>
    /// <param name="duration">最多播放的时长</param>
    /// <param name="delay"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void PlaySound(SoundPack soundPack, float duration = 0, float delay = 0)
    {
        var handler = GetSoundHandler(soundPack);

        handler.Duration = duration;
        handler.Delay = delay;
        handler.SoundPack = soundPack;

        string clipName = null;
        switch (soundPack)
        {
            case SoundPack.Bingo_Normal:
                clipName = "bingo_normal";
                break;
            case SoundPack.Bingo_Game_Win:
                clipName = "bingo_game_win";
                break;
            case SoundPack.Bingo_Mul_Count:
                clipName = "bingo_mul_count";
                break;
            case SoundPack.Bingo_FullHouse:
                clipName = "bingo_fullhouse";
                break;
            case SoundPack.Button_Click_valid:
                clipName = "click_valid";
                break;
            case SoundPack.Button_Click_invalid:
                clipName = "click_invalid";
                break;
            case SoundPack._4x1:
                clipName = "prop_sound_4x1";
                break;
            case SoundPack._6x1:
                clipName = "prop_sound_6x1";
                break;
            case SoundPack.Double_Use:
                clipName = "prop_sound_double";
                break;
            case SoundPack.Double_Scoreing:
                break;
            case SoundPack.Match_Success:
                clipName = "match_success";
                break;
            case SoundPack.Game_Complecte:
                clipName = "complete_game";
                break;
            case SoundPack.Get_Reward:
                clipName = "reward_sound";
                break;
            case SoundPack.Claim_Reward:
                break;
            case SoundPack.Wheel:
                clipName = "wheel_sound";
                break;
            case SoundPack.Wheel_END_Reward:
                break;
            case SoundPack.Game_Win:
                clipName = "game_win";
                break;
            case SoundPack.Game_Fail:
                clipName = "game_fail";
                break;
            case SoundPack.Prop_Use_Count_Down:
                clipName = "prop_count_down";
                break;
            case SoundPack.Match_Count_Down:
                handler.LoopCount = 99999999;
                clipName = "match_count_down";
                break;
            case SoundPack.Dice_Roll:
                clipName = "roll_dice_sound";
                break;
            case SoundPack.Casting_Action:
                clipName = "use_magic_sound";
                break;
            case SoundPack.Yatzy:
                clipName = "yatzy_sound";
                break;
            case SoundPack.Section_Bonus:
                clipName = "section_bonus";
                break;
            case SoundPack.Get_Score:
                clipName = "card_score";
                break;
            case SoundPack.Not_Get_Score:
                clipName = "card_not_score";
                break;
            case SoundPack.Switch_Sound:
                clipName = "switch_sound";
                break;
            case SoundPack.Res_Change_Sound:
                clipName = "get_res_sound";
                break;
        }

        if (!clipName.IsNullOrEmpty())
        {
            var assetOperationHandle = ResourceSystem.That.GetAssetHandle<AudioClip>($"audio/{clipName}");
            handler.AssetOperationHandle?.Release();
            handler.AssetOperationHandle = assetOperationHandle;
            handler.Clip = assetOperationHandle.GetAssetObject<AudioClip>();
            handler.Play();
        }
    }

    public void PauseSound(SoundPack soundPack)
    {
        var handler = handlerPool.Find(audioHandler => audioHandler.SoundPack == soundPack && audioHandler.IsPlaying);
        if (handler == null)
        {
            return;
        }

        handler.IsPause = true;
        handler.Pause();
    }

    public void Resume(SoundPack soundPack)
    {
        var handler = handlerPool.Find(audioHandler => audioHandler.SoundPack == soundPack && audioHandler.IsPause);
        if (handler == null)
        {
            return;
        }

        handler.Resume();
    }

    private AssetOperationHandle musicAssetOperationHandle; 
    
    public void PlayMusic(string url)
    {
        if (!url.IsNullOrEmpty())
        {
            musicAssetOperationHandle?.Release();
            musicAssetOperationHandle = ResourceSystem.That.GetAssetHandle<AudioClip>(url);
            var loadAssetSync = musicAssetOperationHandle.GetAssetObject<AudioClip>();
            if (loadAssetSync == null)
            {
                return;
            }
            Music.clip = loadAssetSync;
        }

        Music.Play();
    }

    public void PlaySound(AudioClip AudioClip = null)
    {
        if (AudioClip != null)
        {
            Sound.PlayOneShot(AudioClip);
        }
        else
        {
            PlaySound(SoundPack.Button_Click_valid);
        }
    }

    public void PlayMusic(AudioClip AudioClip = null)
    {
        if (AudioClip != null)
        {
            Music.clip = AudioClip;
        }
        else
        {
            Music.clip = defaultMusic;
        }

        Music.Play();
    }

    public void SetMaxterVolume(float volume)
    {
        AudioMixer.SetFloat("MasterVolume", GetVolume(volume));
    }

    public void SetMusicVolume(float volume)
    {
        AudioMixer.SetFloat("MusicVolume", GetVolume(volume));
    }

    public void SetSoundVolume(float volume)
    {
        AudioMixer.SetFloat("SoundVolume", GetVolume(volume));
    }

    float GetVolume(float f)
    {
        int index = Math.Clamp((int)f, 0, volume.Length - 1);
        return volume[index];
    }

    public void Stop(SoundPack soundPack)
    {
        var handler = handlerPool.Find(audioHandler => audioHandler.SoundPack == soundPack);
        if (handler == null)
        {
            return;
        }

        handler.Stop();
    }

    private void Update()
    {
        foreach (var handler in handlerPool)
        {
            handler.Update(Time.deltaTime);
        }
    }
}