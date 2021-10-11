﻿#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;

namespace Loxodon.Framework.Bundles
{
    public class AndroidFileUtil
    {
        private const string ACTIVITY_JAVA_CLASS = "com.unity3d.player.UnityPlayer";

        private static AndroidJavaObject assetManager;

        protected static AndroidJavaObject AssetManager
        {
            get
            {
                if (assetManager != null)
                    return assetManager;

                using (AndroidJavaClass activityClass = new AndroidJavaClass(ACTIVITY_JAVA_CLASS))
                {
                    using (var context = activityClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        assetManager = context.Call<AndroidJavaObject>("getAssets");
                    }
                }
                return assetManager;
            }
        }

        protected static string GetAssetFilePath(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return filename;

            int start = filename.LastIndexOf("!/assets/");
            if (start < 0)
                return filename;

            return filename.Substring(start + 9);
        }

        public static Stream OpenFileStream(string filename)
        {
            return new InputStreamWrapper(AssetManager.Call<AndroidJavaObject>("open", GetAssetFilePath(filename)));
        }

        public static byte[] ReadFile(string filename)
        {
            using (var stream = OpenFileStream(filename))
            {
                if (stream == null)
                    throw new FileNotFoundException();

                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        public static bool Exists(string filename)
        {
            try
            {
                using (AndroidJavaObject fileDescriptor = AssetManager.Call<AndroidJavaObject>("openFd", GetAssetFilePath(filename)))
                {
                    if (fileDescriptor != null)
                        return true;
                }
            }
            catch (Exception) { }

            return false;
        }

        public class InputStreamWrapper : Stream
        {
            private object _lock = new object();
            private long length = 0;
            private long position = 0;
            private AndroidJavaObject inputStream;
            public InputStreamWrapper(AndroidJavaObject inputStream)
            {
                this.inputStream = inputStream;
                this.length = inputStream.Call<int>("available");
            }

            public override bool CanRead { get { return this.position < this.length; } }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return false; }
            }

            public override long Length
            {
                get { return this.length; }
            }

            public override long Position
            {
                get { return this.position; }
                set { throw new NotSupportedException(); }
            }

            public override void Flush()
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                lock (_lock)
                {
                    int ret = 0;
                    IntPtr array = IntPtr.Zero;
                    try
                    {
                        array = AndroidJNI.NewByteArray(count);
                        var method = AndroidJNIHelper.GetMethodID(inputStream.GetRawClass(), "read", "([B)I");
                        ret = AndroidJNI.CallIntMethod(inputStream.GetRawObject(), method, new[] { new jvalue() { l = array } });
                        byte[] data = AndroidJNI.FromByteArray(array);
                        Array.Copy(data, 0, buffer, offset, ret);
                    }
                    finally
                    {
                        if (array != IntPtr.Zero)
                            AndroidJNI.DeleteLocalRef(array);
                    }
                    return ret;
                }
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (inputStream != null)
                {
                    inputStream.Call("close");
                    inputStream.Dispose();
                    inputStream = null;
                }
            }
        }
    }
}
#endif