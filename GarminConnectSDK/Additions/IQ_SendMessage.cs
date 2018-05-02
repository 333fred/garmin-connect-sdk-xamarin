using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Com.Garmin.Android.Connectiq
{
    public partial class IQ
    {
        // Cache the listeners, as unlike most of the other callbacks this method is expected to be called
        // many times a second as data is sent to the garmin.
        private ObjectPool<IQSendMessageListenerImpl> _objectPool = null;

        private ObjectPool<IQSendMessageListenerImpl> ObjectPool
        {
            get
            {
                if (_objectPool == null)
                {
                    Interlocked.CompareExchange(ref _objectPool, new ObjectPool<IQSendMessageListenerImpl>(CreateListener), null);
                }
                return _objectPool;
            }
        }

        private IQSendMessageListenerImpl CreateListener()
        {
            Debug.Assert(_objectPool != null);
            return new IQSendMessageListenerImpl(_objectPool);
        }

        public Task<IQSendMessageResult> SendMessageAsync(IQDevice device, IQApp app, Java.Lang.Object message)
        {
            var completionSource = new TaskCompletionSource<IQSendMessageResult>();
            MainThreadHandler.Post(() =>
            {
                IQSendMessageListenerImpl listener = ObjectPool.Allocate();
                listener.CompletionSource = completionSource;
                SendMessage(device, app, message, listener);
            });
            return completionSource.Task;
        }
        private class IQSendMessageListenerImpl : Java.Lang.Object, IQSendMessageListener
        {
            private TaskCompletionSource<IQSendMessageResult> _completionSource = null;
            private readonly ObjectPool<IQSendMessageListenerImpl> _objectPool = null;

            public IQSendMessageListenerImpl(ObjectPool<IQSendMessageListenerImpl> objectPool)
            {
                _objectPool = objectPool;
            }

            public TaskCompletionSource<IQSendMessageResult> CompletionSource
            {
                set
                {
                    Debug.Assert(_completionSource == null);
                    _completionSource = value;
                }
            }

            public void OnMessageStatus(IQDevice paramIQDevice, IQApp paramIQApp, IQMessageStatus paramIQMessageStatus)
            {
                Debug.Assert(_completionSource != null && _objectPool != null);
                _completionSource.SetResult(new IQSendMessageResult(paramIQDevice, paramIQApp, paramIQMessageStatus));
                _completionSource = null;
                _objectPool.Free(this);
            }
        }
        public struct IQSendMessageResult
        {
            public IQSendMessageResult(IQDevice device, IQApp app, IQMessageStatus messageStatus)
            {
                Device = device;
                App = app;
                MessageStatus = messageStatus;
            }
            public IQDevice Device { get; }
            public IQApp App { get; }
            public IQMessageStatus MessageStatus { get; }
        }
    }
}