using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using Cysharp.Threading.Tasks;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using iOSCShape;
using Reactive.Bindings;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using CollectionExtensions = Castle.Core.Internal.CollectionExtensions;
using Random = UnityEngine.Random;

namespace UI
{
    public enum SubPlayerInfoPanel
    {
        Register,
        Edit
    }

    public class UISubPlayerInfo : UIBase<UISubPlayerInfo>
    {
        [SerializeField] private Transform maskTans;
        [SerializeField] private ScrollRect iconScrollRect;
        [SerializeField] private Transform Content;
        [SerializeField] private Text BtnTitle;
        [SerializeField] private Text EnterNameDesc;
        [SerializeField] private Transform BottomGroup;
        [SerializeField] private Button CloseBtn;
        [SerializeField] private Button SwitchBtn;
        [SerializeField] private Button SignBtn;
        [SerializeField] private Button GalleryBtn;
        [SerializeField] private Button CameraBtn;
        [SerializeField] private Button RandowNameBtn;
        [SerializeField] private MyButton SubMitBtn;
        [SerializeField] private InputField inputField;

        [SerializeField] private Image showImage;
        [SerializeField] private Transform maleSpine;
        [SerializeField] private Transform famaleSpine;


        private Texture2D tempTexture;

        /// <summary>
        /// 允许使用的标点符号
        /// </summary>
        private char[] whitePuntuation = new[] { '.', '_', '-', '?', '\'' };

        private HashSet<char> BlackPuntuation = new() { '+', '>', '<', '^', '$', '~' };

        public int targetWidth = 128;

        public int targetHeight = 128;

        private int maleWizard = 2;
        private int femaleWizard = 1;

        public override void OnStart()
        {
            var type = GetArgsByIndex<SubPlayerInfoPanel>(0);
            switch (type)
            {
                case SubPlayerInfoPanel.Register:
                    iOSCShapeTool.Shared.IOSYZRequestATT();
                    CloseBtn.SetActive(false);
                    RandowNameBtn.SetActive(true);
                    BottomGroup.SetActive(true);
                    BtnTitle.text = I18N.Get("key_next");
                    break;
                case SubPlayerInfoPanel.Edit:
                    CloseBtn.SetActive(true);
                    RandowNameBtn.SetActive(false);
                    BottomGroup.SetActive(false);
                    BtnTitle.text = I18N.Get("key_apply");
                    EnterNameDesc.text = I18N.Get("key_guest_account_6");

                    break;
            }

            InitIconList();

            for (int i = 0; i < Content.childCount; i++)
            {
                var button = Content.GetChild(i).GetComponent<MyButton>();
                int headIndex = i;
                button.SetClick(() => { vm[vname.headIndex.ToString()].ToIObservable<int>().Value = headIndex; });
            }


            CloseBtn.onClick.AddListener(() =>
            {
                if (IsChangeInfo())
                {
                    UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                    {
                        desc = I18N.Get("key_unsaved_change_desc"),
                        cancleCall = Close,
                        title = I18N.Get("key_unsaved_change_title"),
                        cancelTitle = I18N.Get("key_leave"),
                        confirmTitle = I18N.Get("key_stay"),
                        HideCloseBtn = true
                    });
                }
                else
                {
                    Close();
                }
            });

            GalleryBtn.onClick.AddListener(() => { StartPickImage(); });

            CameraBtn.onClick.AddListener(() => { StartPickImage(); });

            RandowNameBtn.onClick.AddListener(SyncPlayerName);

            SignBtn.onClick.AddListener(() => { UserInterfaceSystem.That.ShowUI<UIMailLogin>(MailLoginPanel.Sign); });

            SubMitBtn.onClick.AddListener(() =>
            {
                if (!IsChangeInfo())
                {
                    Close();
                    return;
                }

                if (tempTexture != null)
                {
                    ImageExt.ClearCache();
                }

                if (SubMitBtn.Gray)
                {
                    CheckNameValid(inputField.text);
                    return;
                }

                MediatorRequest.Instance.SubMitPlayerInfo(inputField.text,
                    tempTexture,
                    vm[vname.headIndex.ToString()].ToIObservable<int>().Value
                );
            });

            SwitchBtn.onClick.AddListener(() =>
            {
                var wizardType = vm[vname.wizardType.ToString()].ToIObservable<int>().Value;
                if (wizardType == maleWizard)
                {
                    vm[vname.wizardType.ToString()].ToIObservable<int>().Value = femaleWizard;
                }
                else
                {
                    vm[vname.wizardType.ToString()].ToIObservable<int>().Value = maleWizard;
                }
            });

            inputField.onValueChanged.AddListener((data) =>
            {
                vm[vname.playerName.ToString()].ToIObservable<string>().Value = inputField.text;
            });

            //检验 输入不能是中文 ， 以及除了. _之外的标点符号， 输入字符字符长度不能超过20个字符
            inputField.onValidateInput = (text, index, addedChar) =>
            {
                if (addedChar >= 0x4e00 && addedChar <= 0x9fbb)
                {
                    UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("DOES_NOT_SUPPORT_CHINESE_INPUT"));
                    return '\0';
                }

                if (BlackPuntuation.Contains(addedChar) ||
                    !whitePuntuation.Contains(addedChar) && char.IsPunctuation(addedChar))
                {
                    UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("DOES_NOT_SUPPORT_THE_SYMBOLS"));
                    return '\0';
                }

                if (text.Length >= 20)
                {
                    UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("UPPER_THAN_LIMIT"));
                    return '\0';
                }
                
                return addedChar;
            };
        }

        private void InitIconList()
        {
            for (int i = 0; i < iconScrollRect.content.childCount; i++)
            {
                var image = iconScrollRect.content.GetChild(i).Find("Icon").GetComponent<Image>();
                if (image == null) continue;
                if (image.sprite == null)
                {
                    image.sprite = Root.Instance.LoadPlayerIconByIndex(i);
                }
            }
        }

        private bool IsChangeInfo()
        {
            if (GetArgsByIndex<SubPlayerInfoPanel>(0) == SubPlayerInfoPanel.Register)
            {
                return true;
            }

            if (CheckNameValid(inputField.text) && inputField.text != Root.Instance.Role.nickname)
            {
                return true;
            }

            if (tempTexture != null && customHeadChange)
            {
                return true;
            }

            if (vm[vname.headIndex.ToString()].ToIObservable<int>().Value != Root.Instance.Role.head_index)
            {
                return true;
            }

            if (vm[vname.wizardType.ToString()].ToIObservable<int>().Value != Root.Instance.Role.wizard)
            {
                return true;
            }

            return false;
        }

        enum vname
        {
            playerName,

            /// <summary>
            /// 魔法师
            /// </summary>
            wizardType,

            /// <summary>
            /// 系统头像索引
            /// </summary>
            headIndex
        }

        private bool customHeadChange;

        public override void InitVm()
        {
            //默认男角色
            int wizard = Root.Instance.Role.wizard > 0 ? Root.Instance.Role.wizard : maleWizard;

            var type = GetArgsByIndex<SubPlayerInfoPanel>(0);

            int headIndex = 0;
            string playerName = "";
            switch (type)
            {
                case SubPlayerInfoPanel.Register:
                    playerName = MediatorRequest.Instance.GetNameExceptInput();

                    if (CollectionExtensions.IsNullOrEmpty(playerName))
                    {
                        MediatorRequest.Instance.GetRandomName();
                    }

                    headIndex = Random.Range(0, 10);
                    break;
                case SubPlayerInfoPanel.Edit:
                    playerName = Root.Instance.Role.nickname;
                    headIndex = Root.Instance.Role.head_index;
                    Root.Instance.Role.LoadIcon(showImage);

                    if (!Root.Instance.Role.head_url.IsNullOrEmpty())
                    {
                        tempTexture = showImage.sprite.texture;
                    }

                    break;
            }

            //初始为空时， 不进行合法性监测
            vm[vname.playerName.ToString()] = new ReactivePropertySlim<string>(playerName,
                playerName.IsNullOrEmpty() ? ReactivePropertyMode.DistinctUntilChanged : ReactivePropertyMode.Default);

            vm[vname.wizardType.ToString()] = new ReactivePropertySlim<int>(wizard);

            vm[vname.headIndex.ToString()] = new ReactivePropertySlim<int>(headIndex);
        }

        public override void InitBinds()
        {
            vm[vname.playerName.ToString()].ToIObservable<string>().Subscribe(value =>
            {
                SubMitBtn.Gray = !CheckNameValid(value);
                inputField.text = value;
            });

            vm[vname.wizardType.ToString()].ToIObservable<int>().Subscribe(value =>
            {
                famaleSpine.SetActive(value == 1);

                maleSpine.SetActive(value == 2);
            });

            vm[vname.headIndex.ToString()].ToIObservable<int>().Subscribe(value =>
            {
                if (IsInitEnd || GetArgsByIndex<SubPlayerInfoPanel>(0) is SubPlayerInfoPanel.Register)
                {
                    var sprite = Root.Instance.LoadPlayerIconByIndex(value);
                    showImage.sprite = sprite;
                    tempTexture = null;

                    //test
                    // PickerImageFinish(sprite, null);
                }
            });
        }

        private bool CheckNameValid(string value)
        {
            if (value.IsNullOrEmpty())
            {
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_name_need"));
                return false;
            }

            if (value.Length > 20)
            {
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("UPPER_THAN_LIMIT"));
                return false;
            }

            foreach (var c in value)
            {
                if (c >= 0x4e00 && c <= 0x9fbb)
                {
                    UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("DOES_NOT_SUPPORT_CHINESE_INPUT"));
                    return false;
                }

                if (!whitePuntuation.Contains(c) && char.IsPunctuation(c))
                {
                    UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("DOES_NOT_SUPPORT_THE_SYMBOLS"));
                    return false;
                }
            }

            return true;
        }

        private void SyncPlayerName()
        {
            MediatorRequest.Instance.GetRandomName();
        }

        async UniTask SetPlayerName(string name)
        {
            await UniTask.WaitUntil(() => vm != null && vm.Any());
            vm[vname.playerName.ToString()].ToIObservable<string>().Value = name;
        }

        public override void InitEvents()
        {
            var type = GetArgsByIndex<SubPlayerInfoPanel>(0);

            switch (type)
            {
                case SubPlayerInfoPanel.Register:
                    AddEventListener(Proto.USER_INFO_UPLOAD, (sender, eventArgs) =>
                    {
                        if (eventArgs is ProtoEventArgs { Result: ProtoResult.Success })
                        {
                            MediatorRequest.Instance.SendNewPlayerGuideStep(NewPlayerGuideStep.BACKGROUND_STORY);

                            UserInterfaceSystem.That.ShowUI<UIMain>();
                        }
                    });
                    break;
                case SubPlayerInfoPanel.Edit:
                    AddEventListener(Proto.USER_INFO_UPLOAD, (sender, eventArgs) =>
                    {
                        if (eventArgs is ProtoEventArgs { Result: ProtoResult.Success })
                        {
                            Close();
                        }
                    });
                    break;
            }

            AddEventListener(GlobalEvent.Sync_RandomName, (sender, eventArgs) =>
            {
                var name = sender as string;
                if (name == inputField.text)
                {
                    name = MediatorRequest.Instance.GetNameExceptInput(name);
                }

                if (!CollectionExtensions.IsNullOrEmpty(name))
                {
                    vm[vname.playerName.ToString()].ToIObservable<string>().Value = name;
                }
            });

            AddEventListener(GlobalEvent.Pick_Image_Finish, (sender, eventArgs) => maskTans.SetActive(false));

#if UNITY_IOS
            AddEventListener(GlobalEvent.iOS_Click_Camera_Btn, (sender, eventArgs) =>
            {
                YZLog.LogColor("点击了camera");
                iOSPickImage = true;
                checkPermission();
            });
#endif
        }

        private bool iOSPickImage;
        
        void StartPickImage()
        {
#if UNITY_ANDROID
            GameUtils.StartPickImageAndroid(LoadTexture);
#endif

#if UNITY_IOS
            GameUtils.StartPickImageIOS(PickerImageFinish);
#endif
            
#if UNITY_ANDROID
            maskTans.SetActive(true);
#endif
        }

        private void PickerImageFinish(Sprite sprite, string file)
        {
            YZLog.LogColor("iOS加载图片 file = " + file);
            if (sprite != null)
            {
                customHeadChange = true;
                showImage.sprite = sprite;
                tempTexture = sprite.texture;

                // ImageCropper.Instance.Show(tempTexture, (result, originalImage, croppedImage) =>
                // {
                //     if (croppedImage == null)
                //     {
                //         return;
                //     }
                //
                //     var readableTexture = MakeTextureReadable(croppedImage);
                //     var croppedSprite = Sprite.Create(readableTexture, new Rect(0, 0, readableTexture.width, readableTexture.height),
                //         Vector2.zero);
                //     showImage.sprite = croppedSprite;
                //     tempTexture = croppedSprite.texture;
                // });
            }
        }

        private Texture2D MakeTextureReadable(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(256, 256);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        /*private void OpenGallery()
        {
            // 打开用户的图库
            NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
            {
                if (path != null)
                {
                    //确定宽高
                    LoadTexture(path);
                }
            }, "Select photo", "image/*", () => { UserInterfaceSystem.That.ShowUI<UITip>("Failed to open Library"); });

            // GalleryBtn.GetComponent<LoadGalleryPhoto>().PickPhoto();
        }

        private void TakePhoto()
        {
            // 打开摄像机拍照
            NativeCamera.Permission permission = NativeCamera.TakePicture((path) =>
                {
                    if (path != null)
                    {
                        LoadTexture(path);
                    }
                }, 512, true, NativeCamera.PreferredCamera.Default,
                () => { UserInterfaceSystem.That.ShowUI<UITip>("Failed to open Camera"); });
        }*/

        private void LoadTexture(string path)
        {
            customHeadChange = true;
            tempTexture = new Texture2D(2, 2);
            var imageData = File.ReadAllBytes(path);
            YZLog.LogColor("Image byte 长度 " + imageData.Length);
            tempTexture.LoadImage(imageData);
            tempTexture.Apply();
            // tempTexture = ResizeTexture(tempTexture);
            showImage.sprite = Sprite.Create(tempTexture, new Rect(0, 0, tempTexture.width, tempTexture.height),
                Vector2.zero);
        }

        /*/// <summary>
        /// 缩小图片
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        private Texture2D ResizeTexture(Texture2D texture)
        {
            RenderTexture rt = new RenderTexture(targetWidth, targetHeight, 32);
            RenderTexture.active = rt;
            Graphics.Blit(texture, rt);

            Texture2D resizedTexture = new Texture2D(targetWidth, targetHeight);
            resizedTexture.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            resizedTexture.Apply();

            RenderTexture.active = null;
            rt.Release();
            Destroy(rt);

            return resizedTexture;
        }*/

        
        void checkPermission()
        {
            // maskTans.SetActive(false);
#if UNITY_IOS
            if (iOSPickImage)
            {
                iOSPickImage = false;
                PermissionSelf result = PermissionPluginForIOS.CheckPermission(PermissionType.Camera);
                YZLog.LogColor("ios的核查结果：" + result);
     
                if (result == PermissionSelf.Denied)
                {
                    // Timer.Register(0.3f, () =>
                    // {
                        UserInterfaceSystem.That.ShowUI<UITip>("Failed to open Camera");
                        //拒绝后 , 需要跳转到权限页面手动设置
                        // PermissionPluginForIOS.RequestPermission(PermissionType.Camera);
                        string txt_title = I18N.Get("key_ERROR");
                        string txt_tips = I18N.Get("key_the_get_camera", YZNativeUtil.GetYZAppName());
                        string txt_btn = I18N.Get("key_go_to");
                
                        UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                        {
                            Type = UIConfirmData.UIConfirmType.TwoBtn,
                            title = txt_title,
                            desc = txt_tips,
                            confirmTitle = txt_btn,
                            confirmCall = () =>
                            {
                                iOSCShapeTool.Shared.IOSYZGotoSetting();
                            }
                        });
                        // });
                }
            }
#endif
        }
        
        private new void OnDestroy()
        {
            base.OnDestroy();
            GameUtils.StopPickImage();
        }
    }
}