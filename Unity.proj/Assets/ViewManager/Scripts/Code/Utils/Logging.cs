namespace Copper.ViewManager.Code.Utils
{
    using UnityEngine;

    /// <summary>
    /// This delegate defines a logging method for use by the ViewManager. This allows for you to hook up your own logging system should you desire.
    /// </summary>
    /// <param name="msg">The message to print.</param>
    public delegate void LogMethod(string msg);

    internal class Logging
    {
        private static LogMethod internalLog;
        private static LogMethod internalLogWarning;
        private static LogMethod internalLogError;

        public static void SetLogMethods(LogMethod log, LogMethod logWarning, LogMethod logError)
        {
            internalLog = log;
            internalLogWarning = logWarning;
            internalLogError = logError;
        }

        public static void Log(string msg)
        {
            if (internalLog != null)
            {
                internalLog.Invoke(msg);
            }
            else
            {
                Debug.Log(msg);
            }
        }

        public static void LogWarning(string msg)
        {
            if (internalLogWarning != null)
            {
                internalLogWarning.Invoke(msg);
            }
            else
            {
                Debug.LogWarning(msg);
            }
        }

        public static void LogError(string msg)
        {
            if (internalLogError != null)
            {
                internalLogError.Invoke(msg);
            }
            else
            {
                Debug.LogError(msg);
            }
        }
    }
}