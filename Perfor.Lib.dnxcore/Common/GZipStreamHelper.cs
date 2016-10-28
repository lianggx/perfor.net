using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace dywebsdk.Common
{
    public class GZipStreamHelper
    {
        /// <summary>
        ///  压缩数据
        /// </summary>
        /// <param name="data">待压缩的数据流</param>
        /// <param name="level">指定表明是否压缩操作重点介绍速度或压缩大小的值。</param>
        public static byte[] Compress(byte[] data, CompressionLevel level)
        {
            MemoryStream baseStream = new MemoryStream();
            using (GZipStream compressstream = new GZipStream(baseStream, level))
            {
                compressstream.Write(data, 0, data.Length);
                compressstream.Flush();
                baseStream.Position = 0;
                data = baseStream.ToArray();
            }

            return data;
        }

        /// <summary>
        ///  压缩数据
        /// </summary>
        /// <param name="data">待解压缩的数据流</param>
        public static byte[] Decompress(byte[] data)
        {
            byte[] deData = null;
            using (MemoryStream baseStream = new MemoryStream(data))
            {
                baseStream.Position = 0;
                GZipStream decompress = new GZipStream(baseStream, CompressionMode.Decompress, true);
                deData = ReadByteStream(decompress);
            }
            return deData;
        }

        private static byte[] ReadByteStream(Stream stream)
        {
            byte[] data = null;
            MemoryStream outStream = new MemoryStream();
            int b = stream.ReadByte();
            while (b > 0)
            {
                outStream.WriteByte((byte)b);
                b = stream.ReadByte();
            }
            outStream.Position = 0;
            data = outStream.ToArray();

            return data;
        }

        /// <summary>
        ///  从压缩包里面读取指定文件存档。
        /// </summary>
        /// <param name="archiveFileName">要打开的存档的路径，指定为相对路径或绝对路径。相对路径是指相对于当前工作目录的路径。</param>
        /// <param name="fileName">要解压的文件名，压缩包内全路径名</param>
        /// <returns></returns>
        public static byte[] ExtractFile(string archiveFileName, string fileName)
        {
            byte[] data = null;
            // 打开压缩包
            using (ZipArchive zfa = ZipFile.Open(archiveFileName, ZipArchiveMode.Read))
            {
                // 获取压缩包内容
                ZipArchiveEntry entry = zfa.GetEntry(fileName);
                if (entry == null)
                    throw new FileNotFoundException(fileName);
                Stream fileStream = entry.Open();
                data = ReadByteStream(fileStream);
            }

            return data;
        }
    }
}
