using System.Threading.Tasks;

namespace Com.Garmin.Android.Connectiq
{
    public partial class IQ
    {
        public Task<IQApplicationInfoResult> GetApplicationInfoAsync(string appId, IQDevice device)
        {
            var completionSource = new TaskCompletionSource<IQApplicationInfoResult>();
            MainThreadHandler.Post(() => GetApplicationInfo(appId, device, new IQApplicationInfoListenerImpl(completionSource)));
                return completionSource.Task;
        }

        private class IQApplicationInfoListenerImpl : Java.Lang.Object, IQApplicationInfoListener
        {
            private readonly TaskCompletionSource<IQApplicationInfoResult> _completionSource;
            public IQApplicationInfoListenerImpl(TaskCompletionSource<IQApplicationInfoResult> completionSource)
            {
                _completionSource = completionSource;
            }
            public void OnApplicationInfoReceived(IQApp paramIQApp)
            {
                _completionSource.SetResult(new IQApplicationInfoResult(paramIQApp));
            }
            public void OnApplicationNotInstalled(string appId)
            {
                _completionSource.SetResult(new IQApplicationInfoResult(appId));
            }
        }
        public struct IQApplicationInfoResult
        {
            public IQApplicationInfoResult(IQApp app)
            {
                Installed = true;
                AppId = app.ApplicationId;
                App = app;
            }
            public IQApplicationInfoResult(string appId)
            {
                Installed = false;
                App = null;
                AppId = appId;
            }
            public bool Installed { get; }
            public string AppId { get; }
            public IQApp App { get; }
        }
    }
}