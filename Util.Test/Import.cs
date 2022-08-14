using Autofac;
using Autofac.Core;
using core.util.IocTest;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Util.Core.Helpers;
using Util.Core.Dependency;
using Util.Core.Webs.Clients;
using Xunit;
using Util.Core;

namespace Util.Test
{
    public class TestHelper
    {
        [Fact]
        public void IocTest()
        {
            IConfig[] configs = new IConfig[] {
                new CalcConfig()
            };
            Ioc.Register(configs);
            var service = Ioc.Create<IClass>();
            Assert.Equal<int>(7, service.Calc(1, 2));
        }

        [Fact]
        public void DetryTest()
        {
            //string value = RSACrypt.encryData();
            //string value = RSACrypt.decryptData(base64StrToken, key, "UTF-8");
            //Assert.False(value=="");
        }
        
        [Fact]
        public void EntryTest()
        {
            var entrData = System.Text.Encoding.UTF8.GetBytes("test");

            //公钥加密
            RSACryptoServiceProvider entry_Rsa = new RSACryptoServiceProvider();
            string fname = @"D:\cert\taikang_public.crt";
            X509Certificate2 pubCert = new X509Certificate2(fname);
            var keyPara = pubCert.GetRSAPublicKey();
            var para = keyPara.ExportParameters(false);
            entry_Rsa.ImportParameters(para);
            var encryptedData = entry_Rsa.Encrypt(entrData, false);
            var base64Str = System.Convert.ToBase64String(encryptedData);// 需要base64 加密返回


            //私钥解密
            var encryByt = System.Convert.FromBase64String(base64Str);
            string privateFname = @"D:\cert\taikang_private.pfx";
            X509Certificate2 prvcrt = new X509Certificate2(privateFname, "1qaz!QAZ", X509KeyStorageFlags.Exportable);
            RSA decry_Rsa = prvcrt.GetRSAPrivateKey() ;
            var decryRes = decry_Rsa.Decrypt(encryByt, RSAEncryptionPadding.Pkcs1);
            var res = System.Text.Encoding.UTF8.GetString(decryRes);


            //私钥签名
            var data = decry_Rsa.SignData(entrData, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
            var signRes = System.Convert.ToBase64String(data);
            
            //公钥验签
            byte[] signBytes = System.Convert.FromBase64String(signRes);
            var verify = entry_Rsa.VerifyData(entrData, signBytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

            Assert.Equal<string>("123", "123");
        }
        private bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }
        private int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();
            else
            if (bt == 0x82)
            {
                var highbyte = binr.ReadByte();
                var lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }
        [Fact]
        public void TestBase64()
        {
            var base64Str = "VVNJRD0xNzQ1NDF8T1JHSUQ9MTE2ODg5fFRJTUU9MjAxOS0wNC0yOVQxNDo1MjowNnxJUD05OS4xMi4zOS4yMjB8U0FQSUQ9ODAxNzQ1NDF8RU1BSUw9aGV5YW5namlhbkBjbWJjaGluYS5jb218TkFNRT262NH0vaN8QlIxPTEwMDAwM3xQQVRIPdfc0NAv0MXPory8yvWyvy/V0NL4zfjC57/GvLwv0dC3otbQ0MQvyv2+3bLWv+K/qreizcW20y+8qNCnudzA7dOm08O/qreiytKjqMnu29qjqXxHUElEPTAxMDAwNDU4VEc=";
            var str = Util.Core.Helpers.String.FromBase64(base64Str);
            Assert.Equal<string>("123", str);
        }

        [Fact]
        public async void TestWebClient()
        {
            var url = "https://management.chinacloudapi.cn/subscriptions/545bb669-844a-4a39-9a6b-fcf98e3af40a/resourceGroups/SharewinfoBI/providers//capacities/pbiemb?api-version=2017-10-01&force=true";
            var webClient = new WebClient().Patch(url);
            webClient.Data(new Dictionary<string, object>() {
                { "sku", new Dictionary<string, object>(){
                    {"name", "A1" }
                } }
            });

            var s = await webClient.ResultAsync();

        }


        [Fact]
        public void TestExportExcel()
        {

            var ds = new DataSet();
            var dt = new DataTable("table1");
            dt.Columns.Add(new DataColumn("name"));
            dt.Columns.Add(new DataColumn("age"));
            var dr = dt.NewRow();
            dr["name"] = "nicol";
            dr["age"] = 18;
            var dr1 = dt.NewRow();
            dr1["name"] = "nicol11111111111";
            dr1["age"] = 20;
            dt.Rows.Add(dr);
            dt.Rows.Add(dr1);
            ds.Tables.Add(dt);

            var dt2 = new DataTable("table2");
            dt2.Columns.Add(new DataColumn("name2"));
            dt2.Columns.Add(new DataColumn("age2"));
            var dr3 = dt2.NewRow();
            dr3["name2"] = "nicol333333333";
            dr3["age2"] = 18;
            var dr4 = dt2.NewRow();
            dr4["name2"] = "nico222222";
            dr4["age2"] = 20;
            dt2.Rows.Add(dr3);
            dt2.Rows.Add(dr4);
            ds.Tables.Add(dt2);
            Dictionary<string, List<ExcelColumn>> dc = new Dictionary<string, List<ExcelColumn>>();
            dc.Add("table1", new List<ExcelColumn>() {
                new ExcelColumn("名称", "name", 300),
                new ExcelColumn("年龄", "age", 100)
            });
            dc.Add("table2", new List<ExcelColumn>() {
                new ExcelColumn("名称222", "name2", 300),
                new ExcelColumn("年龄222", "age2", 100)
            });
            var st = new ExcelUtils().GetStreamByData(ds, dc);
            using (FileStream fs = new FileStream("d:/aaa.xlsx", FileMode.Create))
            {
                byte[] buffer = st.ToArray();//转化为byte格式存储
                fs.Write(buffer, 0, buffer.Length);
                fs.Flush();
                buffer = null;
            }
        }
    }
    public class CalcConfig: Module, IConfig
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MyClass2>().As<MyClass2>();
            builder.RegisterType<MyClass>().As<IClass>();
        }
    }
}
