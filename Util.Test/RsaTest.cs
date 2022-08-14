using Autofac;
using Autofac.Core;
using CMCloud.SaaS;
using core.util.IocTest;
using System;
using util.core.rsa_java_csharp;
using Util.Core.Helpers;
using Util.Core.Dependency;
using Xunit;

namespace Util.Test
{
    public class RsaTest
    {
        [Fact]
        public void DectryTest()
        {
            var pulicKey = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQC8HMr2CBpoZPm3t9tCVlrKtTmI4jNJc7/HhxjIEiDjC8czP4PV+44LjXvLYcSV0fwi6nE4LH2c5PBPEnPfqp0g8TZeX+bYGvd70cXee9d8wHgBqi4k0J0X33c0ZnW7JruftPyvJo9OelYSofBXQTcwI+3uIl/YvrgQRv6A5mW01QIDAQAB";
            var privateKey = "MIICdwIBADANBgkqhkiG9w0BAQEFAASCAmEwggJdAgEAAoGBALwcyvYIGmhk+be320JWWsq1OYjiM0lzv8eHGMgSIOMLxzM/g9X7jguNe8thxJXR/CLqcTgsfZzk8E8Sc9+qnSDxNl5f5tga93vRxd5713zAeAGqLiTQnRffdzRmdbsmu5+0/K8mj056VhKh8FdBNzAj7e4iX9i+uBBG/oDmZbTVAgMBAAECgYEAmgNU5NTDkj9B+Pnt6UU8doSjw3+3j+bV2K2yS3QUOvAUus/Ax7x6ktjWxzCXvDY9IfUil2RNv9vtKEAqYLCWjc+lf8PV/yH1b7NEgyeAPBXtAJRoOnmYL2bdPW92kP9KgxJruF6Dz/C5AmMOncsvq8ABD+9Darn4p8dwj2ZC4O0CQQDf/AHmZsQokEItfCy4mHS9UbxbfIhEUv1ApPh/+Sr7NkJkHWYCtBQo+8jKO6zurAZQgWBPD1XX2UE4R+VIiZazAkEA1wAqtMvGhccyRZr+6kpkpDIa8+9jOE+nGUzqTDvgCID6as8AzOONFVVK6m/UUqkhcJ8Qu1pF36BGojy5BX2KVwJBAJSFpbji0hXXupowqfLp3RcgmNbNWAp+QUJZYhJx5cdYbmO2fssyH+AhPT6knYJR/YnqkDM8hv6vKCkqu2YDHjMCQAOA8TE5EOclM+CGghj3VWSHnIDVKdzFD4gOBNNxNlltIKeU8AJmwunSFgJ0CBXAw9a+ANvMwM7AIeaK7sj0HskCQAvxfDCq7gaNx+pfu0FHG8Gix08A/A6foggBl1fVu+L9sr9ZuOQ3HbXnl28F9ewuB9xdjnLUDjp7W7U0pB+vKoQ=";
            var rsa = new RSACryptoService(privateKey, pulicKey);
            var str = rsa.Decrypt("gq0UvCi2fEGBL1StV5reO4fLsEwC1CUhSNdrneiX2Zbli4lj//QvgRykU4tSKLInHzTS4bkRCV4VimKghqRUsafzFy71P/J09Jow+kOHJzbDzyuHlUvlWZGWSUx3FBbA87E7GDb2sQsvsCUY1ad0JZCktznuUWhEZr9O8No5+ks=");

            Assert.Equal<int>(7, 8);

        }
[Fact]
        public void DectryTest1()
        {
            var s = new Rsa_CSharp().EncryptString("test");
            var s1 = new Rsa_CSharp().DecryptString(s);
            Assert.Equal<int>(7, 8);

        }
    }
    
}
