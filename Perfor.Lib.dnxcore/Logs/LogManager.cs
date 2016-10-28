using System;
using System.IO;
using System.Net.Http;
using System.Text;

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
        private string[] dateFormarts = { "yyyyMMdd", "yyyyMMdd HH", "yyyyMMdd HH.mm" };
        public LogRecordType logRecordType = LogRecordType.Day;
        public object RemoteLock = new object();
        // 日志路径，不指定则默认使用该路径
        private string logpath = string.Empty;
        private object LocalLockObj = new object();
        // 提供异步写日志的支持
        private delegate void AsyncWrite(string text, LogType type, Exception ex, LogRecordType recordType);
        private delegate void AsyncWriteRemote(string url, string text, LogType type, Exception ex);
        public LogManager(string logPath)
        {
            logpath = logPath;
        }
        #endregion

        /// <summary>
        ///  创建日志管理对象
        /// </summary>
        /// <param name="logPath"></param>
        /// <returns></returns>
        public static LogManager CreateLogManager(string logPath)
        {
            return new LogManager(logPath);
        }

        /*
         * @ 以异步方式启动写入日志
         * */
        private void WriteLocal(string text, LogType type, Exception ex)
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
        public void WriteRemote(string url, string text, LogType type = LogType.INFO, Exception ex = null)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("必须配置日志服务器的完整URL");
            AsyncWriteRemote asyncWrite = new AsyncWriteRemote(WriteToServer);
            IAsyncResult result = asyncWrite.BeginInvoke(url, text, type, ex, null, null);
        }

        /**
         * @ 写入日志到服务器
         * @ url 服务器地址
         * @ text 日志文本
         * @ type 日志类型
         * @ ex 异常信息，如果有
         * */
        private void WriteToServer(string url, string text, LogType type = LogType.INFO, Exception ex = null)
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

                    HttpClient client = new HttpClient();
                    ByteArrayContent content = new ByteArrayContent(datas, 0, datas.Length);
                    client.PostAsync(url, content);
                }
                catch { }
            }
        }

        /**
         * @ 写入日志
         * */
        private void Write(string text, LogType type, Exception ex, LogRecordType recordType)
        {
            if (string.IsNullOrEmpty(logpath))
                throw new ArgumentNullException("必须设置属性LOGPATH的值，即日志文件夹的名称");

            string dir = string.Format(@"{0}\{1}\{2}\{3}", Directory.GetCurrentDirectory(), logpath, DateTime.Now.ToString("yyyyMMdd"), type.ToString().ToLower());
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
        public void Info(string text)
        {
            WriteLocal(text, LogType.INFO, null);
        }

        /**
         * @ 写入错误信息
         * @ text 自定义错误信息
         * @ ex 发生的异常
         * */
        public void Error(string text, Exception ex = null)
        {
            WriteLocal(text, LogType.ERROR, ex);
        }

        /**
         * @ 写入一些警告信息
         * @ text 警告信息
         * */
        public void Warning(string text)
        {
            WriteLocal(text, LogType.WARNING, null);

        }

        /**
         * @ 写入调试信息
         * @ text 调试信息
         * @ ex 发生的异常
         * */
        public void Debug(string text, Exception ex = null)
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
