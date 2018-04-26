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

        void InitGarmin()
        {
            var commStrategy = IQ.IQConnectType.Wireless;
            _iq = IQ.GetInstance(this, commStrategy);
            _iq.Initialize(this, true, new IQListener(this));
        }

        void SdkReady()
        {
            var knownDevices = _iq.KnownDevices;
            _device = knownDevices.First(dev => dev.FriendlyName == "vivoactive HR");
            _iq.RegisterForDeviceEvents(_device, new IQDeviceEvents(this));
        }

        void ReceivedDeviceInfo(IQDevice device, IQDevice.IQDeviceStatus status)
        {
            if (status != IQDevice.IQDeviceStatus.Connected)
            {
                return;
            }

            _iq.GetApplicationInfo("fc4cdb94-9339-44e4-ad86-2d235312f0e7", device, new IQApplicationListener(this));
        }

        void ReceivedAppInfo(IQApp app)
        {
            _app = app;
            var javaMap = new Java.Util.HashMap();
            javaMap.Put(1, 6);
            _iq.SendMessage(_device, _app, javaMap, new IQMessageListener());
        }

        private class IQListener : Java.Lang.Object, IQ.IConnectIQListener
        {
            private MainActivity _mainPage;
            public IQListener(MainActivity container)
            {
                _mainPage = container;
            }

            public void OnInitializeError(IQ.IQSdkErrorStatus p0)
            {
                throw new Exception($"Received garmin error {p0}");
            }

            public void OnSdkReady()
            {
                _mainPage.SdkReady();
            }

            public void OnSdkShutDown()
            {
                throw new NotImplementedException();
            }
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

        private class IQApplicationListener : Java.Lang.Object, IQ.IQApplicationInfoListener
        {
            private MainActivity _mainPage;
            public IQApplicationListener(MainActivity mainPage)
            {
                _mainPage = mainPage;
            }

            public void OnApplicationInfoReceived(IQApp p0)
            {
                _mainPage.ReceivedAppInfo(p0);
            }

            public void OnApplicationNotInstalled(string p0)
            {
                throw new NotImplementedException();
            }
        }

        private class IQMessageListener : Java.Lang.Object, IQ.IQSendMessageListener
        {
            public void OnMessageStatus(IQDevice p0, IQApp p1, IQ.IQMessageStatus p2)
            {
                Log.Info("BindingTest", $"Received status {p2}");
            }
        }
    }
}

