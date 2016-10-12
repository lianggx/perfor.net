using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Perfor.Lib.Cryptography
{
    public class DES3Encrypt
    {
        public static string Decrypt(string Text, string sKey)
        {
            TripleDES provider = TripleDES.Create();
            int num = Text.Length / 2;
            byte[] buffer = new byte[num];
            for (int i = 0; i < num; i++)
            {
                int num3 = Convert.ToInt32(Text.Substring(i * 2, 2), 0x10);
                buffer[i] = (byte)num3;
            }
            provider.Key = Encoding.UTF8.GetBytes(sKey);
            provider.IV = Encoding.UTF8.GetBytes(sKey);

            MemoryStream dencryptStream = new MemoryStream();
            CryptoStream stream = new CryptoStream(dencryptStream, provider.CreateDecryptor(), CryptoStreamMode.Write);
            stream.Write(buffer, 0, buffer.Length);
            stream.Dispose();

            return Convert.ToBase64String(buffer, 0, Convert.ToInt32(dencryptStream.Length, CultureInfo.InvariantCulture));
        }

        public static string Encrypt(string Text, string sKey)
        {
            TripleDES provider = TripleDES.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(Text);
            provider.Key = Encoding.UTF8.GetBytes(sKey);
            provider.IV = Encoding.UTF8.GetBytes(sKey);

            MemoryStream encryptStream = new MemoryStream();
            CryptoStream stream = new CryptoStream(encryptStream, provider.CreateEncryptor(), CryptoStreamMode.Write);
            stream.Write(bytes, 0, bytes.Length);
            stream.FlushFinalBlock();

            StringBuilder builder = new StringBuilder();
            foreach (byte num2 in encryptStream.ToArray())
            {
                builder.AppendFormat("{0:X2}", num2);
            }
            return builder.ToString();
        }        

        public static string Md5(Stream stream)
        {
            return BitConverter.ToString(MD5.Create().ComputeHash(stream)).Replace("-", "");
        }

        public static string Md5(string str)
        {
            return BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(str))).Replace("-", "");
        }

        public static string Md5ToLower(string toCryString)
        {
            MD5 provider = MD5.Create();
            return BitConverter.ToString(provider.ComputeHash(Encoding.UTF8.GetBytes(toCryString))).Replace("-", "").ToLower();
        }
    }
}

