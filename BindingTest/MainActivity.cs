using System;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Com.Garmin.Android.Connectiq;
using System.Linq;
using Android.Util;

namespace BindingTest
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;
            InitGarmin();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        private IQ _iq;
        private IQDevice _device;
        private IQApp _app;

        async void InitGarmin()
        {
            var commStrategy = IQ.IQConnectType.Wireless;
            _iq = IQ.GetInstance(this, commStrategy);
            var initStatus = await _iq.InitializeAsync(this, autoUI: true);
            if (initStatus.HasError)
            {
                Log.Error("BindingTest", $"Received garmin SDK error {initStatus.ErrorStatus}");
                return;
            }

            var knownDevices = _iq.KnownDevices;
            _device = knownDevices.First(dev => dev.FriendlyName == "vivoactive HR");
            IQDevice.IQDeviceStatus status = _iq.GetDeviceStatus(_device);
            if (status != IQDevice.IQDeviceStatus.Connected)
            {
                return;
            }

            var appInfo = await _iq.GetApplicationInfoAsync("fc4cdb94-9339-44e4-ad86-2d235312f0e7", _device);
            if (!appInfo.Installed)
            {
                Log.Error("BindingTest", "Application not installed");
                return;
            }

            _app = appInfo.App;
            var javaMap = new Java.Util.HashMap();
            javaMap.Put(1, 6);
            var messageStatus = await _iq.SendMessageAsync(_device, _app, javaMap);
            Log.Info("BindingTest", $"Message status {messageStatus.MessageStatus}");
        }

        private class IQDeviceEvents : Java.Lang.Object, IQ.IQDeviceEventListener
        {
            private MainActivity _mainPage;
            public IQDeviceEvents(MainActivity mainPage)
            {
                _mainPage = mainPage;
            }
            public void OnDeviceStatusChanged(IQDevice p0, IQDevice.IQDeviceStatus p1)
            {
                _mainPage.ReceivedDeviceInfo(p0, p1);
            }
        }
    }
}

