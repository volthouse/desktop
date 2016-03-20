using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using System.Threading;
using System.Diagnostics.Tracing;
using System.Diagnostics;

namespace ACastShared
{
    public class DebugService
    {
        
        private static readonly Lazy<DebugService> lazy = new Lazy<DebugService>(() => new DebugService());

        public static DebugService Instance { get { return lazy.Value; } }

        private DebugService()
        {
            //Deserialize();
        }

        private bool serialize = false;

        public List<string> DebugMessages = new List<string>();

        public static void Add(string message)
        {
            Instance.DebugMessages.Add(DateTime.Now.ToString("yyyymmdd hh:mm:ss") + ":" + message);

            if (Instance.serialize)
                Instance.Serialize();
        }

        public static void Clear()
        {
            Instance.DebugMessages.Clear();
            Instance.Serialize();
        }

        public void Serialize()
        {
            //var mutex = new AsyncLock();
            //using(await mutex.LockAsync())
            //{
            //    var serializeStream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync("Debug.dat", CreationCollisionOption.ReplaceExisting);
            //    XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
            //    serializer.Serialize(serializeStream, Instance.DebugMessages);
            //    serializeStream.Flush();
            //    serializeStream.Dispose();
            //}

            StringWriter writer = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
            serializer.Serialize(writer, DebugMessages);
            ApplicationSettingsHelper.SaveSettingsValue("Debug", writer.ToString());
        }

        public void Deserialize()
        {
            //var mutex = new AsyncLock();
            //using (await mutex.LockAsync())
            //{
            //    if (await FileExtensions.FileExist2(ApplicationData.Current.LocalFolder, "Debug.dat"))
            //    {
            //        StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("Debug.dat");

            //        var deserializeStream = await file.OpenStreamForReadAsync();
            //        XmlSerializer deserializer = new XmlSerializer(typeof(List<string>));
            //        Instance.DebugMessages = (List<string>)deserializer.Deserialize(deserializeStream);
            //        deserializeStream.Dispose();
            //    }
            //}


            var messages = ApplicationSettingsHelper.ReadResetSettingsValue("Debug");
            if (messages != null)
            {
                StringReader reader = new StringReader(messages.ToString());
                XmlSerializer deserializer = new XmlSerializer(typeof(List<string>));
                DebugMessages = (List<string>)deserializer.Deserialize(reader);
            }
        }
    }

    public sealed class AsyncLock
    {
        private readonly SemaphoreSlim m_semaphore = new SemaphoreSlim(1, 1);
        private readonly Task<IDisposable> m_releaser;

        public AsyncLock()
        {
            m_releaser = Task.FromResult((IDisposable)new Releaser(this));
        }

        public Task<IDisposable> LockAsync()
        {
            var wait = m_semaphore.WaitAsync();
            return wait.IsCompleted ?
                        m_releaser :
                        wait.ContinueWith((_, state) => (IDisposable)state,
                            m_releaser.Result, CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLock m_toRelease;
            internal Releaser(AsyncLock toRelease) { m_toRelease = toRelease; }
            public void Dispose() { m_toRelease.m_semaphore.Release(); }
        }
    }

    
}
