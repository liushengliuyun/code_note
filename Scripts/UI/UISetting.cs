//this file is auto created by QuickCode,you can edit it 
//do not need to care initialization of ui widget any more 
//------------------------------------------------------------------------------
/**
* @author :
* date    :
* purpose :
*/
//------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using Core.Extensions;
using Core.Services.AudioService.API.Facade;
using Core.Services.PersistService.API.Facade;
using Core.Services.UserInterfaceService.API;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils.Static;
using DG.Tweening;
using TMPro;
//using UIWidgets;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utils;

public class UISetting : UIBase<UISetting>
{
    #region UI Variable Statement

    [SerializeField] private Toggle musicToggle;

    [SerializeField] private Toggle vibrateToggle;

    [SerializeField] private Toggle soundToggle;

    [SerializeField] private Button closeBtn;

    [SerializeField] private Slider soundSlider;

    [SerializeField] private Slider musicSlider;

    [SerializeField] private Slider vibrateSlider;

    #endregion

    #region UI Event Register

    private float soundVolume;

    private float musicVolume;
    
    private float vibrateVolume;
    
    public override UIType uiType { get; set; } = UIType.Window;

    private void AddEvent()
    {
        soundVolume = (float)PersistSystem.That.GetValue<float>(GlobalEnum.SOUND_VOLUME);
        musicVolume = (float)PersistSystem.That.GetValue<float>(GlobalEnum.MUSIC_VOLUME);
        vibrateVolume = (float)PersistSystem.That.GetValue<float>(GlobalEnum.VIBRATION_VOLUME);
        
        soundSlider.value = soundVolume;
        musicSlider.value = musicVolume;
        vibrateSlider.value = vibrateVolume;

        var musicOn = (bool)PersistSystem.That.GetValue<bool>(GlobalEnum.MUTE_MUSIC);
        var soundOn = (bool)PersistSystem.That.GetValue<bool>(GlobalEnum.MUTE_SOUND);
        vibrateToggle.isOn = !Root.Instance.VibrationON;

        //必要 设置初始状态
        musicToggle.isOn = musicOn;
        soundToggle.isOn = soundOn;

        musicToggle.transform.SetAlpha(!musicOn ? 1 : 0.5f);
        soundToggle.transform.SetAlpha(!soundOn ? 1 : 0.5f);
        vibrateToggle.transform.SetAlpha(Root.Instance.VibrationON ? 1 : 0.5f);

        closeBtn.onClick.AddListener(Close);

        soundSlider.onValueChanged.AddListener(OnSoundSliderValueChanged);

        musicSlider.onValueChanged.AddListener(OnMusicSliderValueChanged);

        vibrateSlider.onValueChanged.AddListener(value =>
        {
            YZGameUtil.OnVibrateSliderChanged(value, vibrateToggle);
            vibrateVolume = value;
        });
        
        musicToggle.onValueChanged.AddListener(value =>
        {
            if (value)
            {
                AudioSystem.That.SetMusicVolume(GlobalEnum.MinPreSliderValue);
            }
            else
            {
                AudioSystem.That.SetMusicVolume(musicVolume);
            }

            musicToggle.transform.SetAlpha(!value ? 1 : 0.5f);
            
            PersistSystem.That.SaveValue(GlobalEnum.MUTE_MUSIC, value);
        });

        soundToggle.onValueChanged.AddListener(value =>
        {
            if (value)
            {
                AudioSystem.That.SetSoundVolume(GlobalEnum.MinPreSliderValue);
            }
            else
            {
                AudioSystem.That.SetSoundVolume(soundVolume);
            }

            soundToggle.transform.SetAlpha(!value ? 1 : 0.5f);
            PersistSystem.That.SaveValue(GlobalEnum.MUTE_SOUND, value);
        });

        vibrateToggle.onValueChanged.AddListener(value =>
        {
            vibrateToggle.transform.SetAlpha(!value ? 1 : 0.5f);
            Root.Instance.VibrationON = value;
        });
    }

    public void OnSoundSliderValueChanged(float value)
    {
        PersistSystem.That.SaveValue(GlobalEnum.SOUND_VOLUME, value);

        soundVolume = value;
        if (value > GlobalEnum.MinPreSliderValue)
        {
            soundToggle.isOn = false;
        }

        AudioSystem.That.SetSoundVolume(value);
    }

    private void OnMusicSliderValueChanged(float value)
    {
        PersistSystem.That.SaveValue(GlobalEnum.MUSIC_VOLUME, value);

        //省掉了读取值的
        musicVolume = value;
        if (value > GlobalEnum.MinPreSliderValue)
        {
            musicToggle.isOn = false;
        }

        AudioSystem.That.SetMusicVolume(value);
    }

    #endregion

    public override void InitEvents()
    {
    }

    public override void OnStart()
    {
        AddEvent();
    }

    public override void InitVm()
    {
    }

    public override void InitBinds()
    {
    }

    public override void Close()
    {
        OnAnimationOut();
    }

    protected override void OnAnimationIn()
    {
        transform.GetChild(1).localScale = new Vector3(0.3f, 0.3f, 0.3f);
        transform.GetChild(1).DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    protected override void OnAnimationOut()
    {
        transform.GetChild(1).DOScale(Vector3.one * 0.8f, 0.1f).SetEase(Ease.OutCubic)
            .OnComplete(() => base.Close());
    }
}