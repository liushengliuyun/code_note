using System;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Carbon.Util
{
    public static class CarbonLogger
    {
        public static bool EnableLocalLog = false;

        public static bool EnableLog = true;

        private static readonly LockFreeQueue<string> _logQueue = new LockFreeQueue<string>();
        private static readonly LockFreeQueue<string> _logQueueForServer = new LockFreeQueue<string>();

        private static Thread _writeThread;

        private static StreamWriter _streamWriter;

        //赋值，防止后续子线程里面调用Application类，从而造成报错
        private static readonly string _fileBasePath = Application.persistentDataPath;

        private static string _filePath;

        private static int _fileMaxSize = 2 * 1024 * 1024;

        private static string LogTypeLog = "Log";
        private static string LogTypeWarning = "Warning";
        private static string LogTypeError = "Error";

        public static long Tag = 0;
        private static StringBuilder _logBuilder;
        private static int _buildMaxSize = 100;

        private static int Identifier = UnityEngine.Random.Range(1, 10000);

        public static string GetServerLogs()
        {
            if (_logBuilder == null)
                _logBuilder = new StringBuilder();

            if (_logBuilder.Length == 0)
            {
                var count = 0;
                while (_logQueueForServer.Dequeue(out string log) && count < _buildMaxSize)
                {
                    count++;
                    _logBuilder.AppendLine(log);
                }
            }

            return _logBuilder.Length > 0 ? _logBuilder.ToString() : null;
        }

        public static void ClearLogBuilder()
        {
            _logBuilder.Clear();
        }

        private static void TryAppendLog(string log, string logType)
        {
            if (EnableLocalLog)
            {
                log =
                    $"[{Tag}({Identifier})] [{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second} {logType}] {log}";
                _logQueue.Enqueue(log);
                _logQueueForServer.Enqueue(log);

                bool isRunning = _writeThread != null && _writeThread.IsAlive;
                if (isRunning)
                {
                    return;
                }

                _writeThread?.Abort();
                _writeThread = new Thread(new ThreadStart(WriteLocalLog));
                _writeThread.Start();
            }
        }

        private static void WriteLocalLog()
        {
            Thread.Sleep(100);
            while (_logQueue.Dequeue(out string log))
            {
                if (!string.IsNullOrEmpty(_filePath))
                {
                    if (FileUtils.GetFileSize(_filePath) > _fileMaxSize)
                    {
                        _streamWriter?.Close();
                        _streamWriter?.Dispose();
                        _streamWriter = null;
                        FileUtils.CleanFolder($"{_fileBasePath}/Logs");
                    }
                }

                if (_streamWriter == null)
                {
                    var now = DateTime.Now;
                    var nowStr = $"{now.Year}_{now.Month}_{now.Day}_{now.Hour}_{now.Minute}_{now.Second}";
                    _filePath = $"{_fileBasePath}/Logs/{nowStr}.txt";

                    _streamWriter?.Close();
                    _streamWriter?.Dispose();

                    FileUtils.MakeSureFileDirExists(_filePath);
                    _streamWriter = File.CreateText(_filePath);
                    _streamWriter.AutoFlush = true;
                }

                _streamWriter.WriteLine(log);
            }
        }

        public static void Log(string obj, bool force = false)
        {
            TryAppendLog(obj, LogTypeLog);
            if (!force && !EnableLog)
            {
                return;
            }

            Debug.Log(obj);
        }

        public static void LogWarning(string obj, bool force = false)
        {
            TryAppendLog(obj, LogTypeWarning);
            if (!force && !EnableLog)
            {
                return;
            }

            Debug.LogWarning(obj.ToString());
        }

        public static void LogError(string obj, bool force = false)
        {
            TryAppendLog(obj, LogTypeError);
            if (!force && !EnableLog)
            {
                return;
            }

            Debug.LogError(obj);
        }
    }
}