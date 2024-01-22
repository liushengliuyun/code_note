/*
 * This file is part of the CatLib package.
 *
 * (c) CatLib <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 *
 * Document: http://catlib.io/
 */

using System;
using Carbon.Util;
using CatLib;
using CatLib.Util;
using Core.Runtime.Game.Config;
using UnityEngine;

namespace Core.Runtime.Game
{
    /// <summary>
    /// Main project entrance.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Main : Framework
    {
        public Action onBeforeDestroy;
        public new static Main Instance => Framework.Instance as Main;


        private bool IsAppicationQuit = false;

        /// <inheritdoc />
        protected override void OnStartCompleted(IApplication application, StartCompletedEventArgs args)
        {
            // Application entry, Your code starts writing here
            // called this function after, use App.Make function to get service
            // ex: App.Make<IYourService>().Debug("hello world");
            CarbonLogger.Log("Hello CatLib, Debug Level: " + App.Make<DebugLevel>());
            App.Watch<DebugLevel>(newLevel => { CarbonLogger.Log("Change debug level: " + newLevel); });

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        /// <inheritdoc />
        protected override IBootstrap[] GetBootstraps()
        {
            return Arr.Merge(base.GetBootstraps(), Bootstraps.GetBootstraps(this));
        }

        public void Restart()
        {
            new GameObject { name = "restarting" }.AddComponent<Restart>();
        }

        public override void OnBeforeDestroy()
        {
            base.OnBeforeDestroy();
            onBeforeDestroy?.Invoke();
        }

        public override void OnDestroy()
        {
            if (IsAppicationQuit) return;
            base.OnDestroy();
        }

        private void OnApplicationQuit()
        {
            IsInGame = false;
            IsAppicationQuit = true;
            Framework.Instance.OnBeforeDestroy();
        }

        private bool _enableLog;

        public bool EnableLog
        {
            get => _enableLog;
            set
            {
                Debug.unityLogger.logEnabled = value;
                _enableLog = value;
            }
        }
    }
}