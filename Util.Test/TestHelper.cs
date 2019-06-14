using Autofac;
using Autofac.Core;
using core.util.IocTest;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using util.core.Helpers;
using Util.Core.Helpers;
using Util.Dependency;
using Xunit;

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
            string value = RSACrypt.encryData();
            //string value = RSACrypt.decryptData(base64StrToken, key, "UTF-8");
            Assert.False(value=="");
        }


        [Fact]
        public void EntryTest()
        {
            var str = "test";
            var key = "MIIBvjCCAWigAwIBAgIQ/tsN7pBT+bFH8H01tmV8czANBgkqhkiG9w0BAQQFADAWMRQwEgYDVQQDEwtSb290IEFnZW5jeTAeFw0xMTA2MDEwMTQzNDZaFw0zOTEyMzEyMzU5NTlaMBkxFzAVBgNVBAMeDk4ATouQGm1Li9WLwU5mMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCwgF + k8qD1bsOReIA + 4NEme9ic73qiFOHC9J3T5MShcDlJ6M4cBC3zmSzrGUnR3BDHbyDrK / wzckS5dQxzwMmwovbKbgNAQoRlDUr9RAoBGBrwMxZ1d1hNNaa3Jxz2gNeY3SucBDtt6lb8Vxv5sjJB0n8bMlQsnUV / kItELPn7jQIDAQABo0swSTBHBgNVHQEEQDA + gBAS5AktBh0dTwCNYSHcFmRjoRgwFjEUMBIGA1UEAxMLUm9vdCBBZ2VuY3mCEAY3bACqAGSKEc + 41KpcNfQwDQYJKoZIhvcNAQEEBQADQQBiNSHpnzyQbbwj5PRUT4T + As1soAUBfMjsl5oRuENhfJ3kJ5t + 6wdlkEDJH9ww48w0yJLmJlsiP0fjMMtKgjMz";
            string value = RSACrypt.encryptData(str, key, "UTF-8");
            Assert.Equal<string>("123", value);
        }

        [Fact]
        public void TestBase64()
        {
            var base64Str = "VVNJRD0xNzQ1NDF8T1JHSUQ9MTE2ODg5fFRJTUU9MjAxOS0wNC0yOVQxNDo1MjowNnxJUD05OS4xMi4zOS4yMjB8U0FQSUQ9ODAxNzQ1NDF8RU1BSUw9aGV5YW5namlhbkBjbWJjaGluYS5jb218TkFNRT262NH0vaN8QlIxPTEwMDAwM3xQQVRIPdfc0NAv0MXPory8yvWyvy/V0NL4zfjC57/GvLwv0dC3otbQ0MQvyv2+3bLWv+K/qreizcW20y+8qNCnudzA7dOm08O/qreiytKjqMnu29qjqXxHUElEPTAxMDAwNDU4VEc=";
            var str = Util.Core.Helpers.String.FromBase64(base64Str);
            Assert.Equal<string>("123", str);
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
