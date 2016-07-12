using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Perfor.Lib.Extension;
using System.Text.RegularExpressions;
using Perfor.Lib.Common;

namespace Perfor.Lib.Web
{
    /// <summary>
    ///  web 迭代指定目录和文件信息到页面的帮助类
    /// </summary>
    public class WebFileEnumerator
    {
        #region Identity
        private HttpServerUtilityBase mvc_server = null;
        private HttpResponseBase mvc_response = null;
        // asp.net 支持
        private HttpServerUtility asp_server = null;
        private HttpResponse asp_response = null;
        private string filePah = string.Empty;

        /// <summary>
        ///  支持 asp.net mvc 调用的构造函数
        /// </summary>
        /// <param name="server"></param>
        /// <param name="res"></param>
        public WebFileEnumerator(HttpServerUtilityBase server, HttpResponseBase res)
        {
            mvc_server = server;
            mvc_response = res;
        }

        /// <summary>
        ///  支持 asp.net  调用的构造函数
        /// </summary>
        /// <param name="server"></param>
        /// <param name="res"></param>
        public WebFileEnumerator(HttpServerUtility server, HttpResponse res)
        {
            asp_server = server;
            asp_response = res;
        }
        #endregion

        /// <summary>
        ///  获取文件和目录信息
        /// </summary>
        /// <param name="filepath">起始目录</param>
        /// <param name="outputPage">要输出信息的页面，方便组织 a 标签 href 信息</param>
        public void GetFileInfo(string filepath, string outputPage)
        {
            if (filepath.IsNullOrEmpty() || outputPage.IsNullOrEmpty())
                throw new ArgumentNullException("必须指定参数 filePath 和 outputPage 的值");

            filePah = filepath;
            WebParams wp = new WebParams();
            string sourcePath = wp["path"];

            if (sourcePath.IsNullOrEmpty())
            {
                WriteInfo("<div><label>必须指定起始目录</label></div>");
                return;
            }

            int level = wp["level"].ToInt();
            bool isfile = wp["isfile"].ToBoolean();
            if (isfile == false)
            {
                List<FileEntityInfo> list = new List<FileEntityInfo>();
                string path = string.Format(@"[0}\{1}", filePah, sourcePath);
                if (Directory.Exists(path) == false)
                {
                    WriteInfo("<div><label>必须指定起始目录</label></div>");
                    return;
                }
                // 获取子目录
                string[] dirs = Directory.GetDirectories(path);
                if (dirs != null)
                {
                    IOrderedEnumerable<string> orderDirs = dirs.OrderByDescending(f => f);
                    foreach (var item in orderDirs)
                    {
                        DirectoryInfo di = new DirectoryInfo(item);
                        string dirName = string.Format("{0}/{1}", sourcePath, di.Name);
                        list.Add(new FileEntityInfo() { Path = dirName, Level = level + 1, IsFile = false, Name = di.Name });
                    }
                }
                // 获取文件
                string[] files = Directory.GetFiles(path);
                if (files != null)
                {
                    IOrderedEnumerable<string> orderFiles = files.OrderByDescending(f => f);
                    foreach (var item in orderFiles)
                    {
                        FileInfo fi = new FileInfo(item );
                        string fiPath = string.Format("{0}/{1}", sourcePath, fi.Name);
                        list.Add(new FileEntityInfo() { Path = fiPath, Level = level + 1, IsFile = true, Name = fi.Name });
                    }
                }
                // 准备输出到 outputPage
                if (list.Count == 0)
                {
                    WriteInfo("<div><label>该目录不存在子目录或者文件</label></div>");
                    return;
                }
                // 返回上一层
                if (filepath != sourcePath)
                {
                    sourcePath = HttpUtility.UrlEncode(sourcePath.Substring(0, sourcePath.LastIndexOf("/")));
                    WriteInfo(string.Format("<a href='{0}?path={1}&isfile=0&level={2}'>返回上一层</a><br/>", outputPage, sourcePath, --level));
                }
                else
                    WriteInfo("<div><label>已是最上层目录</label></div>");

                foreach (var item in list)
                {
                    string div = string.Format("<div><a href='{0}?path={1}&isfile={2}&level={3}'>{4}</a><div/>", outputPage, HttpUtility.UrlEncode(item.Path), item.IsFile.ToInt(), item.Level, item.Path);
                    WriteInfo(div);
                }
            }
            else {
                // 显示文件
                ShowFile(sourcePath);
            }
        }

        /// <summary>
        ///  显示文件
        /// </summary>
        /// <param name="sourcePath"></param>
        private void ShowFile(string sourcePath)
        {
            Regex regex = new Regex("  ");
            // 显示文件
            string file = string.Format(@"{0}\{1}", filePah, sourcePath);
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                StreamReader sr = new StreamReader(fs, true);
                string line = sr.ReadLine();
                while (line != null)
                {
                    WriteInfo(string.Format("<div>{0}</div>", line));
                    line = sr.ReadLine();
                }
            }
        }

        /// <summary>
        ///  根据传入的层级和目录，获取上级目录全路径
        /// </summary>
        /// <param name="level">层级</param>
        /// <param name="di">目录</param>
        /// <returns></returns>
        private string GetDirParent(int level, DirectoryInfo di)
        {
            if (level == 0)
                return "/";
            string path = string.Format("/{0}/", di.Name, GetDirParent(--level, di.Parent));
            return path;
        }

        /// <summary>
        ///  将文本写入响应流中
        /// </summary>
        /// <param name="text"></param>
        private void WriteInfo(string text)
        {
            if (mvc_response != null)
                mvc_response.Write(text);
            else if (asp_response != null)
                asp_response.Write(text);
        }
    }

    /// <summary>
    ///  文件实体
    /// </summary>
    public class FileEntityInfo
    {
        /// <summary>
        ///  路径
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        ///  名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        ///  是否目录
        /// </summary>
        public bool IsFile { get; set; }
        /// <summary>
        ///  所在层级
        /// </summary>
        public int Level { get; set; }
    }
}
