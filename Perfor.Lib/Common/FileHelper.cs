using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Perfor.Lib.Common
{
    /*
     * @ 文件帮助类
     * */
    public class FileHelper : IDisposable
    {
        #region Identity
        private bool m_disposing = false;
        private FileStream fileStream = null;
        private StreamWriter streamWriter = null;
        private StreamReader streamReader = null;
        private BinaryFormatter binFormat = null;

        public FileHelper() { }
        ~FileHelper()
        {
            Dispose(false);
        }
        #endregion

        /**
        * @ 读取文件
        * @ filePath 文件全路径
        * */
        public string ReadFile(string filePath)
        {
            string result = string.Empty;
            try
            {
                CheckExists(filePath);

                using (fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    streamReader = new StreamReader(fileStream);
                    result = streamReader.ReadToEnd();
                    fileStream.Flush();
                }
            }
            finally
            {
                Dispose(false);
            }
            return result;
        }

        /**
         * @ 写入文件
         * @ filePath 文件全路径
         * @ text 要写入的文本
         * */
        public bool WriteFile(string filePath, string text)
        {
            try
            {
                CheckExists(filePath);

                using (fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    streamWriter = new StreamWriter(fileStream);
                    streamWriter.Write(text);
                    streamWriter.Flush();
                }
            }
            finally
            {
                Dispose(false);
            }
            return true;
        }

        /**
         * @ 读取二进制文件并转换成指定对象
         * @ filePath 文件全路径
         * */
        public T ReadFile<T>(string filePath) where T : class,new()
        {
            T obj = null;
            try
            {
                CheckExists(filePath);
                using (fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    byte[] bytes = new byte[(int)fileStream.Length];
                    fileStream.Read(bytes, 0, bytes.Length);
                    MemoryStream ms = new MemoryStream(bytes);
                    ms.Position = 0;
                    binFormat = new BinaryFormatter();
                    obj = (T)binFormat.Deserialize(ms);
                    fileStream.Flush();
                }
            }
            catch { }
            finally
            {
                Dispose(false);
            }
            obj = obj ?? new T();
            return obj;
        }

        /**
         * @ 将对象以二进制方式写入文件
         * @ filePath 文件全路径
         * @ obj 写入的的目标对象
         * */
        public bool WriteFile<T>(string filePath, T obj) where T : class,new()
        {
            try
            {
                CheckExists(filePath);
                using (fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    MemoryStream ms = new MemoryStream();
                    binFormat = new BinaryFormatter();
                    binFormat.Serialize(ms, obj);
                    byte[] bytes = new byte[ms.Length];
                    bytes = ms.ToArray();
                    fileStream.Write(bytes, 0, bytes.Length);
                    fileStream.Flush();
                }
            }
            finally
            {
                Dispose(false);
            }
            return true;
        }

        /**
         * @ 检查文件是否存在，不存在则创建
         * */
        private void CheckExists(string filePath)
        {
            if (File.Exists(filePath))
                return;

            File.Create(filePath);
        }

        /**
         * @ 接口实现
         * */
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /**
         * @ 自定义清理资源
         * */
        private void Dispose(bool disposing)
        {
            if (!this.m_disposing && disposing)
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
                if (streamWriter != null)
                {
                    streamWriter.Close();
                    streamWriter.Dispose();
                }
                if (streamReader != null)
                {
                    streamReader.Close();
                    streamReader.Dispose();
                }
                this.m_disposing = true;
            }
        }
    }
}
