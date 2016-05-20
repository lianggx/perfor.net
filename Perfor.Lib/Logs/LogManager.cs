using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Perfor.Lib.Extension;

namespace Perfor.Lib.Logs
{
    /**
     * @ 日志管理类
     * */
    public partial class LogManager
    {
        #region Identity
        // 日志创建方式
        private static string[] dateFormarts = { "yyyyMMdd", "yyyyMMdd HH", "yyyyMMdd HH.mm" };
        public static LogRecordType logRecordType = LogRecordType.Day;
        public static object RemoteLock = new object();
        // 日志路径，不指定则默认使用该路径
        public static string LogPath = "Logs";
        private static object LocalLockObj = new object();
        // 提供异步写日志的支持
        private delegate void AsyncWrite(string text, LogType type, Exception ex, LogRecordType recordType);
        private delegate void AsyncWriteRemote(string url, string text, LogType type, Exception ex);
        #endregion

        /*
         * @ 以异步方式启动写入日志
         * */
        private static void WriteLocal(string text, LogType type, Exception ex)
        {
            AsyncWrite asyncWrite = new AsyncWrite(Write);
            IAsyncResult result = asyncWrite.BeginInvoke(text, type, ex, logRecordType, null, null);
        }

        /*
         * @ 以异步方式启动写入日志
         * @ url 服务器地址
         * @ text 日志文本
         * @ type 日志类型
         * @ ex 异常信息，如果有
         * */
        public static void WriteRemote(string url, string text, LogType type = LogType.INFO, Exception ex = null)
        {
            if (url.IsNullOrEmpty())
                throw new ArgumentNullException("必须配置日志服务器的完整URL");
            AsyncWriteRemote asyncWrite = new AsyncWriteRemote(WrteToServer);
            IAsyncResult result = asyncWrite.BeginInvoke(url, text, type, ex, null, null);
        }

        /**
         * @ 写入日志到服务器
         * @ url 服务器地址
         * @ text 日志文本
         * @ type 日志类型
         * @ ex 异常信息，如果有
         * */
        private static void WrteToServer(string url, string text, LogType type = LogType.INFO, Exception ex = null)
        {
            lock (RemoteLock)
            {
                try
                {
                    Uri uri = new Uri(url);
                    string Parameter = string.Format("text={0}&type={1}", text, type.ToInt());
                    if (ex != null)
                    {
                        Parameter = string.Format("{0}&exp={1}{2}", Parameter, ex.Message, ex.StackTrace);
                    }
                    byte[] datas = Encoding.UTF8.GetBytes(Parameter);

                    using (WebClient client = new WebClient())
                    {
                        client.UploadData(uri, "POST", datas);
                    }
                }
                catch { }
            }
        }

        /**
         * @ 写入日志
         * */
        private static void Write(string text, LogType type, Exception ex, LogRecordType recordType)
        {
            string dir = string.Format(@"{0}\{1}\{2}\{3}", System.Environment.CurrentDirectory, LogPath, DateTime.Now.ToString("yyyyMMdd"), type.ToString().ToLower());
            if (Directory.Exists(dir) == false)
                Directory.CreateDirectory(dir);
            string df = dateFormarts[recordType.ToInt()];
            string path = string.Format("{0}\\{1}.txt", dir, DateTime.Now.ToString(df));
            try
            {
                lock (LocalLockObj)
                {
                    using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write))
                    {
                        StreamWriter sw = new StreamWriter(fs);
                        sw.WriteLine(DateTime.Now.ToString() + " " + text);
                        if (ex != null)
                        {
                            sw.WriteLine(ex.Message + " " + ex.StackTrace);
                            if (ex.InnerException != null)
                            {
                                sw.WriteLine(ex.InnerException.StackTrace);
                            }
                        }
                        sw.Flush();
                        fs.Flush();
                    }
                }
            }
            catch { }
        }

        /**
         * @ 写入一些信息
         * @ text 信息
         * @ ex 发生的异常
         * */
        public static void Info(string text)
        {
            WriteLocal(text, LogType.INFO, null);
        }

        /**
         * @ 写入错误信息
         * @ text 自定义错误信息
         * @ ex 发生的异常
         * */
        public static void Error(string text, Exception ex = null)
        {
            WriteLocal(text, LogType.ERROR, ex);
        }

        /**
         * @ 写入一些警告信息
         * @ text 警告信息
         * */
        public static void Warning(string text)
        {
            WriteLocal(text, LogType.WARNING, null);

        }

        /**
         * @ 写入调试信息
         * @ text 调试信息
         * @ ex 发生的异常
         * */
        public static void Debug(string text, Exception ex = null)
        {
            WriteLocal(text, LogType.DEBUG, ex);
        }
    }

    /**
     * @ 写入日志的类型
     * */
    public enum LogType
    {
        INFO = 10,
        ERROR = 20,
        WARNING = 30,
        DEBUG = 40
    }

    /**
     * @ 日志文件创建的方式
     **/
    public enum LogRecordType
    {
        /**按天**/
        Day = 0,
        /**按小时**/
        Hour = 1,
        /**按分钟**/
        Minute = 2
    }
}
