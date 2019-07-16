using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace util.core.rsa_java_csharp
{
    class Rsa_CSharp
    {
        public JObject VerifyToken(string orgData, string tokenStr)
        {
            var dataInfo = DecryptString(tokenStr);
            JObject tokenInfo = JObject.Parse(dataInfo);
            var exprieTime = new DateTimeOffset();
            DateTimeOffset.TryParse(tokenInfo["expire"].ToString(), out exprieTime);
            if (exprieTime < DateTimeOffset.UtcNow)
            {
                throw new Exception("Token have expired ");
            }
            return tokenInfo;
        }

        public string EncryptString(string inputStr)
        {
            var entrData = System.Text.Encoding.UTF8.GetBytes(inputStr);
            //公钥加密
            string fname = "D:\\cert\\taikang_public.crt";
            //string fname = System.Web.HttpContext.Current.Server.MapPath("cert/taikang_public.crt");
            X509Certificate2 pubCert = new X509Certificate2(fname);
            var encry_Rsa = (RSACryptoServiceProvider)pubCert.PublicKey.Key;
            var encryptedData = encry_Rsa.Encrypt(entrData, false);
            var base64Str = System.Convert.ToBase64String(encryptedData);// 需要base64 加密返回

            return base64Str;
        }


        public string DecryptString(string inputStr)
        {
            var decryData = System.Convert.FromBase64String(inputStr);
            string privateFname = "D:\\cert\\taikang_public.pfx";
            //string privateFname = System.Web.HttpContext.Current.Server.MapPath("cert/taikang_public.pfx");
            X509Certificate2 prvcrt = new X509Certificate2(privateFname, "1qaz!QAZ", X509KeyStorageFlags.Exportable);
            RSACryptoServiceProvider decry_Rsa = new RSACryptoServiceProvider();

            decry_Rsa.FromXmlString(prvcrt.PrivateKey.ToXmlString(true));
            var decryRes = decry_Rsa.Decrypt(decryData, false);
            var res = System.Text.Encoding.UTF8.GetString(decryRes);
            return res;
        }
    }
}
