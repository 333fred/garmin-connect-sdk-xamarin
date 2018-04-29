using System;
using Android.OS;

namespace Com.Garmin.Android.Connectiq
{
    public partial class IQ
    {
        private readonly Lazy<Handler> _mainThreadHandler = new Lazy<Handler>(() =>
            new Handler(Looper.MainLooper));

        private Handler MainThreadHandler { get => _mainThreadHandler.Value; }
    }
}