using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Extensions;
using DataAccess.Model;
using Firebase.Analytics;

namespace Core.Controllers
{
    public class YZFirebaseController : YZBaseController<YZFirebaseController>
    {
        public Firebase.FirebaseApp FirebaseApp { get; private set; } = null;
        public Firebase.Auth.FirebaseAuth FirebaseAuth { get; private set; } = null;
        public Firebase.Auth.FirebaseUser FirebaseUser { get; private set; } = null;
        public bool IsFirebaseInitSuccess = false;
        public string FirebaseEmail = string.Empty;
        public string FirebasePhoneNumber = string.Empty;

        public override void InitController()
        {
            base.InitController();
        }

        public void Init(Action<bool> callback)
        {
#if !UNITY_EDITOR
            YZLog.LogColor("Firebase initialize");
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    FirebaseApp = Firebase.FirebaseApp.DefaultInstance;
                    Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

                    YZLog.LogColor("Firebase initialized Successfully!");
                    IsFirebaseInitSuccess = true;
                }
                else
                {
                    YZLog.LogColor(System.String.Format("Firebase Could not resolve all dependencies: {0}", dependencyStatus));
                }
                if (callback != null)
                {
                    callback(IsFirebaseInitSuccess);
                }
            });
#endif
        }

        // 设置id
        public void FireBaseSetUserId(string userId)
        {
#if !UNITY_EDITOR
            YZLog.LogColor("Firebase Set User Id : " + userId);
            FirebaseAnalytics.SetUserId(userId);
#endif
        }

        public void FireBaseAuth()
        {
#if !UNITY_EDITOR
            YZLog.LogColor("Firebase Auth");
            FirebaseAuth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            FirebaseAuth.StateChanged += AuthStateChanged;
            AuthStateChanged(this, null);
#endif
        }

        private void AuthStateChanged(object sender, System.EventArgs eventArgs)
        {
            if (FirebaseAuth.CurrentUser != FirebaseUser)
            {
                bool signedIn = FirebaseUser != FirebaseAuth.CurrentUser && FirebaseAuth.CurrentUser != null;
                FirebaseUser = FirebaseAuth.CurrentUser;
                if (signedIn)
                {
                    // Get the user's email address
                    FirebaseEmail = FirebaseUser.Email;
                    // or get their phone number
                    FirebasePhoneNumber = FirebaseUser.PhoneNumber;
                    // ...

                    if (!FirebaseEmail.Equals(string.Empty))
                    {
                        YZLog.LogColor("Firebase initiate on device conversion measurement with email address : " + FirebaseEmail);
                        FirebaseAnalytics.InitiateOnDeviceConversionMeasurementWithEmailAddress(FirebaseEmail);
                    }
                    else if (!FirebasePhoneNumber.Equals(string.Empty))
                    {
                        YZLog.LogColor("Firebase initiate on device conversion measurement with phone number : " + FirebasePhoneNumber);
                        FirebaseAnalytics.InitiateOnDeviceConversionMeasurementWithPhoneNumber(FirebasePhoneNumber);
                    }
                    else
                    {

                    }
                }
            }
        }

        // 玩家注册
        public void FireBaseTrackRegisterEvent()
        {
#if !UNITY_EDITOR
            YZLog.LogColor("Firebase track register event");
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventSignUp);
#endif
        }

        // 用户登录
        public void FireBaseTrackLoginEvent()
        {
#if !UNITY_EDITOR
            YZLog.LogColor("Firebase track login event");
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLogin);
#endif
        }

        public void TrackPurchaseEvent(int type, double revenue)
        {
#if !UNITY_EDITOR
            if (FirebaseApp != null)
            {
                YZLog.LogColor("Firebase track purchase event");
                string userId = Root.Instance.UserId.ToString();
                FirebaseAnalytics.SetUserId(userId);
                Parameter[] parematers = new Parameter[2];
                parematers[0] = new Parameter(FirebaseAnalytics.ParameterCurrency, "USD");
                parematers[1] = new Parameter(FirebaseAnalytics.ParameterValue, revenue);
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, parematers);
            }
#endif
        }














    }
}













