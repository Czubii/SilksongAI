using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace AIPlugin.Utilities
{
    public static class ThreadSafeLogService
    {
        private static ConcurrentQueue<(string msg, Action<string> logAction)> _queue = new ConcurrentQueue<(string, Action<string>)>();
        public static void Log(string message, Action<string> unityLog = null)
        {
            _queue.Enqueue((message, unityLog ?? Debug.Log));
        }
        public static void Flush()
        {
            while (_queue.TryDequeue(out var item))
            {
                item.logAction(item.msg);
            }
        }
    }
}
