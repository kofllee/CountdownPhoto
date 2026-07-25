using System.Runtime.InteropServices;

namespace _PhotoCountdown.Core.Persistence
{
    public static class WebGlFileSystemSync
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void PhotoCountdownSyncFileSystem();
#endif

        public static void Request()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            PhotoCountdownSyncFileSystem();
#endif
        }
    }
}