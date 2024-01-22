using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AndroidCShape;
using Core.Controllers;
using Core.Manager;
using DataAccess.Controller;
using Utils;

namespace Core.Server
{
    public class YZOrganic
    {
        public const string YZNONE = "NONE";
        /// <summary>
        /// 自然量
        /// </summary>
        public const string YZORGANIC = "Organic";
        /// <summary>
        /// 非自然量
        /// </summary>
        public const string YZNONORGANIC = "Non-organic";
    }

    public class YZServerApiOrganic : YZBaseController<YZServerApiOrganic>
    {
        private string br_server_organic;
        private string br_appflyer_organic;

        /// 是否显示美金
        public bool IsYZShowMoney()
        {
            if (YZGameUtil.GetPlatform() == YZPlatform.Editor)
            {
                return !IsYZOrganic();
            }

            if (YZGameUtil.GetIsiOS())
            {
                return true;
            }
            else
            {
                return !IsYZOrganic();
            }
        }

        /// <summary>
        /// 怎么和Root.IsNaturalFlow统一起来
        /// </summary>
        /// <param name="organic"></param>
        public void SetOrganic(string organic)
        {
            if (PlayerManager.Shared.User.YZOrganic != YZOrganic.YZNONORGANIC)
            {
                PlayerManager.Shared.User.YZOrganic = organic;
                if (PlayerManager.Shared.User.YZOrganic == YZOrganic.YZNONORGANIC)
                {
#if UNITY_ANDROID || UNITY_EDITOR
                    YZAndroidPushPlugin.Shared.AndroidYZSetTags(null);
#endif
                    // if (YZServerApi.Shared.YZStatus >= LoginStatus.config)
                    // {
                    //     YZMainControl.YZOnClickIndex(YZMainUICtrler.Shared().YZCurrentIndex, true);
                    // }
                }
            }
        }
        
        //mark 是否需要统一判断自然量的接口 
        //mark 是否可以看作iOS的AF归因结果
        /// 是否为自然量（无数据也算为自然量）
        public bool IsYZOrganic()
        {
            if (YZGameUtil.GetPlatform() == YZPlatform.Editor)
            {
                return YZDefineUtil.EidtorIsOgranic;
            }

            string organic = PlayerManager.Shared.User.YZOrganic;
            if (string.IsNullOrEmpty(organic))
            {
                return true;
            }

            return organic.Equals(YZOrganic.YZORGANIC) || organic.Equals(YZOrganic.YZNONE);
        }

        [Obsolete]
        /// 设置用户渠道 1.AppsFlyer 2.服务器
        public void TryToOrganic(int type, string organic, string source, string invite)
        {
            // 归因渠道
            if (!string.IsNullOrEmpty(source))
            {
                PlayerManager.Shared.User.YZMediaSource = source;
            }
        
            // 归因邀请信息
            if (!string.IsNullOrEmpty(invite))
            {
            }
        
            // 不同归因方式
            if (type == 1)
            {
                br_appflyer_organic = organic;
            }
            else
            {
                br_server_organic = organic;
            }
        
            /*// 如果当前归因为自然量，则应用最新归因数据
            SetOrganic(organic);
        
            // 判断服务器归因和AF归因全部完成，回传给服务器，客户端当前的最终归因方式
            if (!string.IsNullOrEmpty(br_appflyer_organic) && !string.IsNullOrEmpty(br_server_organic))
            {
                YZDebug.LogConcat(" aflyer: ", br_appflyer_organic,
                    " server: ", br_server_organic,
                    " source: ", PlayerManager.Shared.User.YZMediaSource);
                YZDebug.LogConcat(" finish: ", PlayerManager.Shared.User.YZOrganic);
                // PostYZAFOrganic();
                // PostYZIDFAAAFID();
                MediatorRequest.Instance.SetOrganic();
                MediatorRequest.Instance.SendAFID();
            }*/
        }
        
        // // 上报是否为自然量
        // public YZServerRequest PostYZAFOrganic()
        // {
        //     YZServerRequest request =
        //         YZServerApi.Shared.CreateYZRequest(YZServerApiPath.YZSetOrganic, YZServerApi.YZMethodPost);
        //     request.AddYZParam("is_organic", IsYZOrganic() ? "1" : "0");
        //     request.AddYZParam("organic_media_source", PlayerManager.Shared.User.YZMediaSource);
        //     request.SendYZ();
        //     return request;
        // }
        
        // // 上报idfa和afid
        // public YZServerRequest PostYZIDFAAAFID()
        // {
        //     // 1. afid 没有值，就算了
        //     string af_id = YZNativeUtil.GetYZAFID();
        //     if (string.IsNullOrEmpty(af_id) || af_id.Equals("NONE"))
        //         return null;
        //
        //     // 2. idfa 没有值，也算了
        //     if (string.IsNullOrEmpty(YZNativeUtil.GetYZIDFA()) || YZNativeUtil.GetYZIDFA().Equals("NONE"))
        //         return null;
        //
        //     
        //     YZServerRequest request =
        //         YZServerApi.Shared.CreateYZRequest(YZServerApiPath.YZSetIdfaAppsflyerId, YZServerApi.YZMethodPost);
        //     request.AddYZParam("idfa", YZNativeUtil.GetYZIDFA());
        //     request.AddYZParam("appsflyer_id", af_id);
        //     request.SendYZ();
        //     
        //     
        //     return request;
        // }
    }
}