using System.Threading.Tasks;

namespace Com.Garmin.Android.Connectiq
{
    public partial class IQ
    {
        public Task<IQOpenApplicationResult> OpenApplicationAsync(IQDevice device, IQApp app)
        {
            var completionSource = new TaskCompletionSource<IQOpenApplicationResult>();
            MainThreadHandler.Post(() => OpenApplication(device, app, new IQOpenApplicationListenerImpl(completionSource)));
            return completionSource.Task;
        }

        private class IQOpenApplicationListenerImpl : Java.Lang.Object, IQOpenApplicationListener
        {
            private readonly TaskCompletionSource<IQOpenApplicationResult> _completionSource;

            public IQOpenApplicationListenerImpl(TaskCompletionSource<IQOpenApplicationResult> completionSource)
            {
                _completionSource = completionSource;
            }

            public void OnOpenApplicationResponse(IQDevice paramIQDevice,
                                                  IQApp paramIQApp,
                                                  IQOpenApplicationStatus paramIQOpenApplicationStatus)
            {
                _completionSource.SetResult(new IQOpenApplicationResult(paramIQDevice, paramIQApp, paramIQOpenApplicationStatus));
            }
        }

        public struct IQOpenApplicationResult
        {
            public IQOpenApplicationResult(IQDevice device, IQApp app, IQOpenApplicationStatus status)
            {
                Device = device;
                App = app;
                Status = status;
            }

            public IQDevice Device { get; private set; }
            public IQApp App { get; private set; }
            public IQOpenApplicationStatus Status { get; private set; }
        }
    }
}