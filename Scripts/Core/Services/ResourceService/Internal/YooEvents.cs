using CatLib;

namespace Core.Services.ResourceService.Internal
{
    public static class YooEvents
    {
        public static readonly string OnUserTryInitialize = $"{BaseEventArgs}.OnUserTryInitialize";
        public static readonly string OnPatchStatesChange = $"{BaseEventArgs}.OnPatchStatesChange";
        public static readonly string OnInitializeFailed = $"{BaseEventArgs}.OnInitializeFailed";
        public static readonly string OnInitializeSuccess = $"{BaseEventArgs}.InitializeSuccess";

        private const string BaseEventArgs = "EventArgs.YooEventArgs";
    }

    /// <summary>
    /// It indicates that the bootstrap will be bootstrapped.
    /// </summary>
    public class UserTryInitializeEventArgs : ApplicationEventArgs
    {
        public UserTryInitializeEventArgs() : base(App.That)
        {
        }
    }

    /// <summary>
    /// 补丁流程步骤改变
    /// </summary>
    public class PatchStatesChangeEventArgs : ApplicationEventArgs
    {
        public string State;

        public PatchStatesChangeEventArgs(string state) : base(App.That)
        {
            State = state;
        }
    }


    public class InitializeSuccessEventArgs : ApplicationEventArgs
    {
        public InitializeSuccessEventArgs() : base(App.That)
        {
        }
    }

    /// <summary>
    /// 补丁包初始化失败
    /// </summary>
    public class InitializeFailedEventArgs : ApplicationEventArgs
    {
        public InitializeFailedEventArgs() : base(App.That)
        {
        }
    }
}