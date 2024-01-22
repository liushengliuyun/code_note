//this file is auto created by QuickCode,you can edit it 
//do not need to care initialization of ui widget any more 
//------------------------------------------------------------------------------
/**
* @author :
* date    :
* purpose :
*/
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Linq;
using AppsFlyerSDK;
using Core.Controllers;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Manager;
using Core.Server;
using Core.Services.AudioService.API.Facade;
using Core.Services.PersistService.API.Facade;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using Cysharp.Threading.Tasks;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using iOSCShape;
using Reactive.Bindings;
using Spine.Unity;
using ThinkingAnalytics;
using UnityEngine;
using TMPro;
using UI;

using UnityEngine.UI;
using UnityTimer;
using Utils;

public enum LoginPanel
{
    GMLogin,

    NormalLogin,

    /// <summary>
    /// 断网重连, 废弃
    /// </summary>
    ReConnect,

    /// <summary>
    /// 服务器维护
    /// </summary>
    ServerMaintain
}

public class UILogin : UIBase<UILogin>
{
    #region UI Variable Statement

    [SerializeField] private SkeletonGraphic DogSpine;

    /// <summary>
    /// 用户交互相关的UI
    /// </summary>
    [SerializeField] private MyButton ServiceBtn;

    [SerializeField] private Toggle notNaturalToggle;

    [SerializeField] private Toggle testServerToggle;

    [SerializeField] private Toggle skipBlockToggle;

    /// <summary>
    /// 用户交互界面
    /// </summary>
    [SerializeField] private Transform InterfaceGroup;

    [SerializeField] private InputField inputField;

    [SerializeField] private Slider slider;
    [SerializeField] private Text sliderText;

    [SerializeField] private MyButton playerLoginBtn;

    [SerializeField] private MyButton button_VisterLoginBtn;

    [SerializeField] private Image playerInvisibleIcon;

    [SerializeField] private MyButton termBtn;
    [SerializeField] private MyButton privacyBtn;

    [SerializeField] private Transform iOSText;
    
    [SerializeField] private Transform AndroidBottom;
    
    #endregion

    private string clientUid;
    /// <summary>
    /// 第一个进度条
    /// </summary>
    private TweenerCore<float, float, FloatOptions> tween;
    private TweenerCore<float, float, FloatOptions> tween1;

    public Image GetInvisibleIcon()
    {
        return playerInvisibleIcon;
    }

    private static UILogin inst;
    public static UILogin Inst => inst;

    public bool IsTestServer;

    public override void OnStart()
    {
        //test
        // UserInterfaceSystem.That.ShowUI<UIBingo>();
#if UNITY_IOS
        iOSText.SetActive(true);
#else
              AndroidBottom.SetActive(true);
#endif

        RegisterInterval(0.05f, Interval);
        var panel = GetArgsByIndex<LoginPanel>(0);

#if RTEST && !RELEASE
         InterfaceGroup.SetActive(true);
        #else
        InterfaceGroup.SetActive(panel == LoginPanel.GMLogin);
#endif

        KillTweens();

        var trackEntry = DogSpine.AnimationState.SetAnimation(0, "appear", false);

        this.AttachTimer(trackEntry.AnimationEnd, () =>
        {
            DogSpine.AnimationState.SetAnimation(0, "idea", true);
        });
        switch (panel)
        {
            case LoginPanel.GMLogin:
                slider.SetActive(false);
                break;
            case LoginPanel.NormalLogin:
                StartLoginAnimation();
                break;
            case LoginPanel.ReConnect:
                StartReconnectAnimation();
                AddEventListener(GlobalEvent.ReConnectSuccess, (sender, eventArgs) => { SliderEnd(); });
                break;
            case LoginPanel.ServerMaintain:
                slider.SetActive(false);
                UserInterfaceSystem.That.CloseAllUI(new[] { nameof(UILogin) });
                UserInterfaceSystem.That.ShowUI<UINotice>(Root.Instance.ServerMaintainInfo);
                break;
        }
        
     

#if !RELEASE

        var isSetNoNatural = Root.Instance.IsSetNoNatural();
        notNaturalToggle.isOn = isSetNoNatural;

        YZSDKsController.Shared.AF_ORGANIC = !isSetNoNatural;
            
        notNaturalToggle.onValueChanged.AddListener(arg0 => { Root.Instance.SetNaturalMark(arg0); });
#endif
        
    

        clientUid = PersistSystem.That.GetValue<string>(GlobalEnum.ClientUID) as string;
        if (clientUid.IsNullOrEmpty())
        {
            clientUid = DeviceInfoUtils.Instance.GetEquipmentId();
        }

        inputField.text = clientUid;

        playerLoginBtn.SetClick(SendLogin);
        ServiceBtn.SetClick(() => YZNativeUtil.ContactYZUS(EmailPos.Loading));

        button_VisterLoginBtn.SetClick(() =>
        {
            //清除token 
            var key = Proto.SERVER_URL.Contains("https")
                ? GlobalEnum.AUTHORIZATION_RELAESE
                : GlobalEnum.AUTHORIZATION_DEBUG;
            PersistSystem.That.DeletePrefsValue(key);

            SendLogin();
        });

        ThinkingAnalyticsAPI.Track("loading_start");

        testServerToggle.onValueChanged.AddListener((isOn) =>
        {
            IsTestServer = isOn;
            if (!IsTestServer)
            {
                Proto.SERVER_URL = "https://app-bingo-gp.yatzybrawl.com/";
            }
        });

        skipBlockToggle.onValueChanged.AddListener((isOn) => { YZSDKsController.Shared.IsBlockValid = !isOn; });
        skipBlockToggle.isOn = !YZSDKsController.Shared.IsBlockValid;

        termBtn.SetClick(() => { YZNativeUtil.OpenYZPrivacyServiceUrl(); });
        privacyBtn.SetClick(() => { YZNativeUtil.OpenYZPrivacyPolicyUrl(); });

        inst = this;

        //没有意义
        /*// 给地理位置一个默认的值
        if (YZDataUtil.GetLocaling(YZConstUtil.YZCountryCode).IsNullOrEmpty())
            YZDataUtil.SetYZString(YZConstUtil.YZCountryCode, "US");

        if (YZDataUtil.GetLocaling(YZConstUtil.YZAreaCode).IsNullOrEmpty())
            YZDataUtil.SetYZString(YZConstUtil.YZAreaCode, "NY");*/

        //mark 设置成非自然量,为什么?
        YZServerApiOrganic.Shared.SetOrganic(YZOrganic.YZNONORGANIC);

        YZSDKsController.Shared.Pay_App = YZNativeUtil.GetYZPayApp();
        YZDebug.Log("是否有微信支付宝" + YZSDKsController.Shared.Pay_App);
        
        DeviceInfoUtils.Instance.Init();
        
        this.AttachTimer(0.1f, LocationManager.Shared.RequestLocate);

#if UNITY_IOS && !UNITY_EDITOR
           // mark 如果没有att权限, 去申请
           iOSCShapeTool.Shared.IOSYZRequestATT();
         // AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);
        
         
#if RTEST || RELEASE
        StartCoroutine(LoginCor());
#endif
         
#endif
        // iOSCShapeTool.Shared.IOSYZRequestATT();
    }

    IEnumerator LoginCor()
    {
        yield return new WaitUntil(() => YZSDKsController.Shared.LocateHaveResult && !Root.Instance.IP.IsNullOrEmpty());
        // yield return new WaitForSeconds(0.5f);
        
        if (Root.Instance.Role.user_id <= 0)
        {
            MediatorRequest.Instance.PlayerLogin();
        }
    }
    
    private void KillTweens()
    {
        tween?.Kill();
        tween1?.Kill();
    }


    public override void Refresh()
    {
        base.Refresh();
        OnStart();
    }

    enum  vname
    {
        VM_Is_Pause
    }

    public override void InitVm()
    {
        vm[vname.VM_Is_Pause.ToString()] =
            new ReactivePropertySlim<bool>(false, ReactivePropertyMode.DistinctUntilChanged);
    }

    public override void InitBinds()
    {
        vm[vname.VM_Is_Pause.ToString()].ToIObservable<bool>().Subscribe(value =>
        {
            if (value)
            {
                tween?.Pause();
                tween1?.Pause();
            }
            else
            {
                tween?.Play();
                tween1?.Play();
            }
        });
    }

    private bool setOriginicSuccess = true;

    private bool loginSuccess = false;

    public override void InitEvents()
    {
        //账号登陆成功
        AddEventListener(GlobalEvent.Login_Success, (sender, eventArgs) =>
        {
            loginSuccess = true;
            UniTaskGetConfig();
        });

        AddEventListener(GlobalEvent.SEND_GM_SET_ORGINIC, (sender, eventArgs) =>
        {
            YZLog.LogColor("setOriginicSuccess 关闭");
            setOriginicSuccess = false;
        });

        AddEventListener(Proto.GM_SET_ORGANIC, (sender, eventArgs) =>
        {
            YZLog.LogColor("setOriginicSuccess 打开");
            setOriginicSuccess = true;
        });

        var register = false;
        // NetSystem 那边有针对这个的特殊处理，防止正式服卡62%
        AddEventListener(GlobalEvent.FinishConfigDeserialize, (sender, eventArgs) =>
        {
            if (register)
            {
                return;
            }

            bool toShowUImain = false;
            register = true;

            if (Root.Instance.NotPassTheNewPlayerGuide(NewPlayerGuideStep.BACKGROUND_STORY))
            {
                MediatorRequest.Instance.GetRandomName();
            }
            
            RegisterInterval(0.2f, () =>
            {
                if (toShowUImain)
                {
                    return;
                }

                if (Root.Instance.IsSetUp())
                {
                    toShowUImain = true;
                    
                    if (Root.Instance.NotPassTheNewPlayerGuide(NewPlayerGuideStep.BACKGROUND_STORY))
                    {
                        SliderEnd<UISubPlayerInfo>(0.1f);
                    }
                    else
                    {
                        SliderEnd<UIMain>();
                    }
                }
            });
        });
    }

    bool CanSendGetConfig()
    {
        return loginSuccess && setOriginicSuccess;
    }

    async private UniTask UniTaskGetConfig()
    {
        await UniTask.WaitUntil(() => CanSendGetConfig());

        MediatorRequest.Instance.GetConfigs(isShowUIMain: false);
    }

    private void SliderEnd<T>(float duration = 1f) where T : UIBase<T>
    {
        KillTweens();
        DOTween.To(() => slider.value,
                x => slider.value = x,
                100,
                duration).SetEase(Ease.InQuint)
            .OnUpdate(() =>
            {
                // YZLog.LogColor("slider end " + slider.value);
                sliderText.text = Mathf.Ceil(slider.value) + "%";
            })
            .OnComplete(() =>
            {
                UserInterfaceSystem.That.ShowUI<T>();
                Close();
            });
    }

    private void SliderEnd()
    {
        KillTweens();
        DOTween.To(() => slider.value,
                x => slider.value = x,
                100,
                1f).SetEase(Ease.OutExpo)
            .OnUpdate(() => { sliderText.text = Mathf.Ceil(slider.value) + "%"; })
            .OnComplete(Close);
    }

    //覆盖了UIBase的Update
    // private void Update()
    // {
    //     Interval();
    // }

    private void Interval()
    {
        if (vm == null || !vm.Any())
        {
            return;
        }

        var uiTop = UserInterfaceSystem.That.GetTopNormalUI();
        if (uiTop != null)
        {
            if (tween != null || tween1 != null)
            {
                vm[vname.VM_Is_Pause.ToString()].ToIObservable<bool>().Value = uiTop.uiType != uiType;
            }
        }
    }

    private void StartLoginAnimation()
    {
        if (tween != null) return;

#if RTEST
        playerLoginBtn.SetActive(false);
        button_VisterLoginBtn.SetActive(false);
#else
        InterfaceGroup.SetActive(false);
#endif
       
        slider.SetActive(true);

        I18N.Init();
        tween = DOTween.To(() => slider.value,
                    x => slider.value = x,
                    61.8f,
                    1.5f).SetEase(Ease.OutCubic)
                .OnUpdate(() =>
                    sliderText.text = Math.Round(slider.value, 1) + "%"
                )
                .OnComplete(() =>
                {
                    tween1 = DOTween.To(() => slider.value,
                            x => slider.value = x,
                            99,
                            25f).SetEase(Ease.Linear).OnUpdate(() =>
                        {
                            // YZLog.LogColor("tween1 " + slider.value);
                            sliderText.text = Math.Round(slider.value, 1) + "%";
                        })
                        .OnComplete(() =>
                        {
                            UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                            {
                                Type = UIConfirmData.UIConfirmType.OneBtn,
                                HideCloseBtn = true,
                                IsNetWorkError = true,
                                confirmTitle = I18N.Get("key_relogin"),
                                desc = I18N.Get("key_net_relogin"),
                                confirmCall = () =>
                                {
                                    Close();
                                    MediatorRequest.Instance.LoginWithProcess(false);
                                }
                            });
                        });
                })
            ;
    }

    private void StartReconnectAnimation()
    {
        if (tween != null) return;

        InterfaceGroup.SetActive(false);
        slider.SetActive(true);

        tween = DOTween.To(() => slider.value,
                x => slider.value = x,
                62.5f,
                10f).SetEase(Ease.OutCubic)
            .OnUpdate(() =>
                sliderText.text = Math.Round(slider.value, 1) + "%"
            );
    }

    private void SendLogin()
    {
        if (GetArgsByIndex<LoginPanel>(0) is LoginPanel.GMLogin)
        {
            // MediatorRequest.ResetEnvironment();
        }

        if (inputField.text.IsNullOrEmpty())
        {
            inputField.text = DeviceInfoUtils.Instance.GetEquipmentId();
        }

        if (clientUid != inputField.text)
        {
            clientUid = inputField.text;
            var key = Proto.SERVER_URL.Contains("https")
                ? GlobalEnum.AUTHORIZATION_RELAESE
                : GlobalEnum.AUTHORIZATION_DEBUG;
            PersistSystem.That.DeletePrefsValue(key);
            PersistSystem.That.SaveValue(GlobalEnum.ClientUID, clientUid);
        }

        DeviceInfoUtils.Instance.TestDeviceId = inputField.text;
        // AudioSystem.That.InitVolumeSetting();

        MediatorRequest.Instance.UpdateUdid();
        UserInterfaceSystem.That.CloseAllUI(new[] { "UILogin" });
        //MediatorRequest.Instance.PlayerLogin();

        StartLoginAnimation();

        StartCoroutine(LoginCoroutine());
    }

    IEnumerator LoginCoroutine()
    {
        yield return new WaitForSeconds(1.5f);
        MediatorRequest.Instance.PlayerLogin();
    }

    protected override void OnClose()
    {
        base.OnClose();
        KillTweens();
    }
}