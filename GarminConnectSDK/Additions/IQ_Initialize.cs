using System.Threading.Tasks;
using Android.Content;

namespace Com.Garmin.Android.Connectiq
{
    public partial class IQ
    {
        public Task<IQConnectStatusResult> InitializeAsync(Context context, bool autoUI)
        {
            var completionSource = new TaskCompletionSource<IQConnectStatusResult>();
            MainThreadHandler.Post(() => Initialize(context, autoUI, new IQInitializeCallback(completionSource)));
            return completionSource.Task;
        }

        private class IQInitializeCallback : Java.Lang.Object, IConnectIQListener
        {
            private readonly TaskCompletionSource<IQConnectStatusResult> _completionSource;

            public IQInitializeCallback(TaskCompletionSource<IQConnectStatusResult> completionSource)
            {
                _completionSource = completionSource;
            }

            public void OnInitializeError(IQSdkErrorStatus paramIQSdkErrorStatus)
            {
                _completionSource.SetResult(new IQConnectStatusResult(paramIQSdkErrorStatus));
            }

            public void OnSdkReady()
            {
                _completionSource.SetResult(new IQConnectStatusResult());
            }

            public void OnSdkShutDown()
            {
                _completionSource.SetResult(new IQConnectStatusResult(shutdown: true));
            }
        }

        public struct IQConnectStatusResult
        {
            public IQConnectStatusResult(IQSdkErrorStatus status)
            {
                HasError = true;
                ErrorStatus = status;
                Shutdown = false;
            }
            public IQConnectStatusResult(bool shutdown)
            {
                Shutdown = shutdown;
                HasError = false;
                ErrorStatus = null;
            }
            public bool HasError { get; }
            public bool Shutdown { get; }
            public IQSdkErrorStatus ErrorStatus { get; }
        }
    }
}