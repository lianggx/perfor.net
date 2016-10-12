﻿using Perfor.Lib.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace dywebsdk.Common
{
    /// <summary>
    ///  RSA非对称加密算法实现类
    /// </summary>
    public class RSAEncrypt
    {
        #region Identity        
        /// <summary>
        ///  构造函数
        /// </summary>
        /// <param name="rsap">包含公私钥的对象</param>
        public RSAEncrypt(RSAParameters rsap)
        {
            this.rsap = rsap;
        }

        /// <summary>
        ///  构造函数第一次重载
        /// </summary>
        /// <param name="exponent">公钥Exponent节</param>
        /// <param name="modulus">公钥Modulus节</param>
        public RSAEncrypt(string exponent, string modulus)
        {
            rsap = new RSAParameters();
            rsap.Exponent = Encoding.UTF8.GetBytes(exponent);
            rsap.Modulus = Encoding.UTF8.GetBytes(modulus);
        }

        /// <summary>
        ///  构造函数第二次重载
        /// </summary>
        /// <param name="exponent">公钥Exponent节</param>
        /// <param name="modulus">公钥Modulus节</param>
        /// <param name="d">私钥D节</param>
        /// <param name="dp">私钥DP节</param>
        /// <param name="dq">私钥DQ节</param>
        /// <param name="inverseQ">私钥InverseQ节</param>
        /// <param name="p">私钥P节</param>
        /// <param name="q">私钥Q节</param>
        public RSAEncrypt(string exponent, string modulus, string d, string dp, string dq, string inverseQ, string p, string q)
        {
            rsap = new RSAParameters();
            rsap.Exponent = Encoding.UTF8.GetBytes(exponent);
            rsap.Modulus = Encoding.UTF8.GetBytes(modulus);
            rsap.D = Encoding.UTF8.GetBytes(d);
            rsap.DP = Encoding.UTF8.GetBytes(dp);
            rsap.DQ = Encoding.UTF8.GetBytes(dq);
            rsap.InverseQ = Encoding.UTF8.GetBytes(inverseQ);
            rsap.P = Encoding.UTF8.GetBytes(p);
            rsap.Q = Encoding.UTF8.GetBytes(q);
        }

        /// <summary>
        ///  构造函数第二次重载
        /// </summary>
        /// <param name="rsapJson">RSAParameters对象的序列化json形式</param>
        public RSAEncrypt(string rsapJson)
        {
            rsap = rsapJson.FromJson<RSAParameters>();
        }
        #endregion

        /// <summary>
        ///  加密字符串
        /// </summary>
        /// <param name="cryptText">待加密的内容</param>
        /// <returns></returns>
        public string Encrypt(string cryptText)
        {
            CreateRSAInstance();
            byte[] data = Encoding.UTF8.GetBytes(cryptText);
            byte[] cryptaData = rsa.Encrypt(data, padding);
            return Convert.ToBase64String(cryptaData);
        }

        /// <summary>
        ///  解密字符串
        /// </summary>
        /// <param name="cryptText">带解密的内容</param>
        /// <returns></returns>
        public string Decrypt(string cryptText)
        {
            CreateRSAInstance();
            byte[] cryptaData = Convert.FromBase64String(cryptText);
            byte[] data = rsa.Decrypt(cryptaData, padding);
            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        ///  创建RSA加密算法的实现
        /// </summary>
        /// <returns></returns>
        private void CreateRSAInstance()
        {
            rsa.KeySize = keysize;
            rsa.ImportParameters(rsap);
        }

        /// <summary>
        ///  创建一个随机的RSA加密算法的RSAParameters对象，并转换为 json格式的字符串
        /// </summary>
        /// <param name="includePrivateParameters">是否包含私钥</param>
        /// <param name="keySize">加密 key 的长度</param>
        /// <returns></returns>
        public static string GenerateKey(bool includePrivateParameters = true, int keySize = 1024)
        {
            RSA rsa = RSA.Create();
            rsa.KeySize = keySize;
            RSAParameters paramenter = rsa.ExportParameters(includePrivateParameters);

            return paramenter.ObjToJson();
        }

        /// <summary>
        ///  导出私钥或者公钥的RSAParameters对象，并转换为 json格式的字符串
        /// </summary>
        /// <param name="includePrivateParameters">是否包含私钥</param>
        /// <returns></returns>
        public string ExportParameters(bool includePrivateParameters = false)
        {
            CreateRSAInstance();
             RSAParameters paramenter = rsa.ExportParameters(includePrivateParameters);

            return paramenter.ObjToJson();
        }


        #region Properties
        private RSA rsa = RSA.Create();       
        /// <summary>
        ///  获取或者设置加密算法
        /// </summary>
        public RSA Rsa
        {
            get { return rsa; }
            set { rsa = value; }
        }
        private RSAParameters rsap;
        /// <summary>
        ///  加密算法的密钥
        /// </summary>
        public RSAParameters Paramenter
        {
            get { return rsap; }
            set { rsap = value; }
        }

        private int keysize = 1024;
        /// <summary>
        /// 获取或者设置 RSA加密算法的 key长度
        /// 原系统默认2048，为兼容windows平台，本例设置为默认 1024，该属性应该在调用 Encrypt/Decrypt 方法前调用
        /// </summary>
        public int KeySize
        {
            get { return keysize; }
            set { keysize = value; }
        }

        private RSAEncryptionPadding padding = RSAEncryptionPadding.OaepSHA256;
        /// <summary>
        ///  加密算法的填充强度，默认为 OaepSHA512
        /// </summary>
        public RSAEncryptionPadding Padding
        {
            get { return padding; }
            set { padding = value; }
        }
        #endregion
    }
}
