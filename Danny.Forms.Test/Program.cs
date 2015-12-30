﻿using Danny.Lib.Cacheing;
using Danny.Forms.Test.example;
using Danny.Lib.Xml.PListXml;
using Danny.Lib.Enums;
using Danny.Lib.Extension;
using Danny.Lib.Helpers;
using Danny.Lib.Logs;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Danny.Lib.Web;
using Danny.Lib.Common;
using Danny.Lib.Helpers.Mssql;
using Danny.Lib.Xml;

namespace Danny.Forms.Test
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //string id = Guid.NewGuid().ToString("N");

            TestCryptor();
            Console.WriteLine("succeed");
            Console.ReadKey();

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm());
        }

        #region Mssql Test
        static void TestParams()
        {
            WebParams wp = new WebParams();
            int count = wp["count1"].ToInt();
            Console.WriteLine(count);
        }

        static void InsertTest()
        {

            MssqlInsert insert = new MssqlInsert();
            string[] fields = { "ID", "UserName", "Gender", "Phone", "LastModifyTime" };
            insert.TableName = "Customers";
            DataTable table = new DataTable();
            table.Columns.Add("ID");
            table.Columns.Add("UserName");
            table.Columns.Add("Gender");
            table.Columns.Add("Phone");
            table.Columns.Add("LastModifyTime");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < 10; i++)
            {
                //object[] values = { Guid.NewGuid().ToString("N"), "李进", 1, "13856986236", DateTime.Now };
                //insert.InsertObject(fields, values);
                //Customers cus = new Customers();
                //cus.ID = Guid.NewGuid().ToString("N");
                //cus.UserName = "测试人员";
                //cus.Phone = null;
                //cus.Gender = 0;
                //cus.LastModifyTime = DateTime.Now;
                //insert.InsertObject<Customers>(cus);
                //table.Clear();
                //for (int j = 0; j < 100000; j++)
                //{
                //    DataRow dr = table.NewRow();
                //    dr["ID"] = Guid.NewGuid().ToString("N");
                //    dr["UserName"] = "李进";
                //    dr["Gender"] = 1;
                //    dr["Phone"] = "13800138000";
                //    dr["LastModifyTime"] = DateTime.Now;

                //    table.Rows.Add(dr);
                //} 
                //insert.InsertObject(table);
            }
            insert.SaveChange();
            watch.Stop();
            Console.WriteLine(watch.Elapsed.Milliseconds);
        }

        static void UpdateTest()
        {
            MssqlUpdate update = new MssqlUpdate();
            Customers cus = new Customers();
            cus.ID = "0000006eb74e49e8a6504cabf402d08d";
            cus.UserName = "李宁7";
            cus.Phone = "1685425632";
            cus.LastModifyTime = DateTime.Now;
            update.UpdateObject<Customers>(cus);
            update.SaveChange();
        }

        static void DeleteTest()
        {
            MssqlDelete delete = new MssqlDelete();
            Customers cus = new Customers();
            cus.ID = "00000db144494225b9482d6cf9cac5f0";
            delete.DeleteObject<Customers>(cus);
            delete.SaveChange();
        }

        static void SelectTest()
        {
            // 多表连接，分组，排序，按条件
            /***
             * 
             * SELECT A.ID,A.CreateTime,A.Total,B.Count,C.Price,C.Name,D.UserName,D.Phone,D.ID AS Cus_ID FROM [dbo].[OrderHeaders] AS A
LEFT JOIN [dbo].[OrderDetails] AS B ON A.ID=B.OH_ID
LEFT JOIN [dbo].[Goods] AS C ON B.G_ID=C.ID
LEFT JOIN [dbo].[Customers] AS D ON A.Cus_ID=D.ID
WHERE D.ID='00000ddb92044fad8be0913b68697318'
             * */
            MssqlReadPager read = new MssqlReadPager();
            read.TableName = "OrderHeaders";
            read.TableAlias = "A";
            string[] fields = { "A.ID", "A.CreateTime", "A.Total", "B.Count", "C.Price", "C.Name", "D.UserName", "D.Phone", "D.ID AS Cus_ID" };
            string leftJoin = @"LEFT JOIN [dbo].[OrderDetails] AS B ON A.ID=B.OH_ID
LEFT JOIN [dbo].[Goods] AS C ON B.G_ID=C.ID
LEFT JOIN [dbo].[Customers] AS D ON A.Cus_ID=D.ID ";
            read.SetOrderBy("A.CreateTime", SQLExpression.Order.DESC);
            //  read.SetGroupBy(fields);
            read.AddWhere("D.ID", "00000ddb92044fad8be0913b68697318");
            //read.AddBracketLeft();
            //read.AddWhere("A.UserName", LdfSQLExpression.ExprOperator.Eq, "李进", LdfSQLExpression.JoinType.OR);
            //read.AddWhere("A.UserName", "李进");
            //read.AddBracketRight();
            int rowCount = 0;
            IEnumerable<SQLDataResult> list = read.Select("ID", fields, leftJoin, 1, 20, out rowCount);
            foreach (var item in list)
            {
                Console.WriteLine("ID:{0}", item["ID"]);
                Console.WriteLine("CreateTime:{0}", item["CreateTime"]);
                Console.WriteLine("Total:{0}", item["Total"]);
                Console.WriteLine("Count:{0}", item["Count"]);
                Console.WriteLine("Price:{0}", item["Price"]);
                Console.WriteLine("Name:{0}", item["Name"]);
                Console.WriteLine("UserName:{0}", item["UserName"]);
                Console.WriteLine("Phone:{0}", item["Phone"]);
                Console.WriteLine("Cus_ID:{0}", item["Cus_ID"]);
            }
            /*
                       // 单表查询
                       LdfSQLReadPager read = new LdfSQLReadPager();
                       read.TableName = "Customers";
                       read.TableAlias = "";
                       string[] fields = { "ID", "UserName", "Gender", "Phone", "LastModifyTime" };
                       string leftJoin = "";
                       read.SetOrderBy("ID", LdfSQLExpression.Order.DESC);
                       read.SetGroupBy(fields);
                       //read.AddWhere("UserName", "李进");
                       IEnumerable<Customers> list = read.Select<Customers>("ID", fields, leftJoin, 1, 20); 
             */
            //MssqlReadPager read = new MssqlReadPager();
            //int rowCount = 0;
            //List<Customers> list = read.Select<Customers>(1, 20, out rowCount);
            //foreach (var item in list)
            //{
            //    Console.WriteLine("{0}：{1}", item.ID, item.UserName);
            //}
        }

        static void CustomCmdTest()
        {
            MssqlCustomCmd cmd = new MssqlCustomCmd();
            cmd.SQLCmdText = string.Format("SELECT * FROM Customers WHERE ID=@ID");
            cmd.AddParams("ID", "0000112b783a4a3a86387d281fa1c3a4");
            cmd.ExecuteReader();
        }
        #endregion

        /**
         * @ RedisCache Test
         * */
        static void RedisCacheTest()
        {
            List<string> writeHost = new List<string>() { "123456@127.0.0.1:9706" };
            List<string> readHost = new List<string>() { "123456@127.0.0.1:9706" };
            RedisCacheExpiration redisCache = new RedisCacheExpiration(writeHost, readHost);
            redisCache.Set("userid", "lgx", DateTime.Now.AddDays(2));
        }

        /***
         * @ 日志测试
         * */
        static void LogManagerTest()
        {
            while (true)
            {
                string msg = Console.ReadLine();
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect("www.baidu.com", 80);
                if (client.Connected)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(msg);
                    client.Send(bytes);
                    int b = 0;
                    int len = 100;
                    StringBuilder resString = new StringBuilder();
                    while (true)
                    {
                        byte[] result = new byte[len];
                        b = client.Receive(result, 0, len, SocketFlags.None);
                        if (b <= 0)
                            break;
                        string r = Encoding.UTF8.GetString(result);
                        resString.Append(r);
                    }
                    Console.WriteLine(resString.ToString());
                }
            }
        }

        #region PList Xml Test
        static void TestPList()
        {
            PListDict ht = new PListDict();
            ht["user"] = new PListDict("lgx");
            ht["password"] = new PListDict("123456");
            decimal gender = (decimal)25.6;
            double g2 = 33.4;
            float g3 = 45.2f;
            int age = 18;
            bool istrue = true;
            bool isfalse = false;
            ht["gender"] = new PListDict(gender);
            ht["g2"] = new PListDict(g2);
            ht["g3"] = new PListDict(g3);

            PListDict ht2 = new PListDict();
            ht2["age"] = new PListDict(age);
            ht2["istrue"] = new PListDict(istrue);
            ht2["isfalse"] = new PListDict(isfalse);

            ht["ht2"] = ht2;

            PListArray pa = new PListArray();

            PListArray array = new PListArray();
            array.Add(new PListDict(1));
            array.Add(new PListDict("234"));
            array.Add(new PListDict(23.4));
            array.Add(new PListDict(DateTime.Now));

            PListDict ht3 = new PListDict();
            ht3["age"] = new PListDict(age);
            ht3["istrue"] = new PListDict(istrue);
            ht3["isfalse"] = new PListDict(isfalse);

            array.Add(ht3);
            ht["array"] = array;
            string result = ht.ToPListString();
            PListDict node = new PListDict();
            node.FromString(result);
        }

        static void TestPListParse()
        {
            PListDict dict = new PListDict();
            dict.FromFile("新建文本文档.txt");

            string result = dict.ToPListString();
            Console.WriteLine(result);
        }

        static void TestNormalXml()
        {
            XmlDict xd = new XmlDict();
            xd.Load("XMLFile1.xml");

            string v1 = xd["recipename"].Value.ToString();
            XmlDict l3 = xd["ingredlist"]["listitem 3"];
            string v2 = l3.ToXml();
            Console.WriteLine("{0},{1},{2}", l3.Tag, l3.Order, l3.Value);

            Console.WriteLine(v1);
            Console.WriteLine(v2);
        }
        #endregion


        /**
         * @ 对称加密算法测试
         * */
        static void TestCryptor()
        {
            List<Customers> list = new List<Customers>();
            for (int i = 0; i < 10; i++)
            {
                Customers cus = new Customers();
                cus.Enabled = true;
                cus.Gender = 1;
                cus.ID = Guid.NewGuid().ToString("N");
                cus.LastModifyTime = DateTime.Now.AddDays(10);
                cus.Phone = "13800138000";
                cus.SQLOption = SQLOption.CREATE;
                cus.UserName = "李小唐";
                list.Add(cus);
            }
            string json = list.ObjToJson();
            AesFactory encrypt = new AesFactory();
            string result = encrypt.Encrypt(json);

            Console.WriteLine(result);

            AesFactory decrypt = new AesFactory();
            result = decrypt.Decrypt(result);

            Console.WriteLine(result);
        }
    }
}