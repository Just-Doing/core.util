using Autofac;
using Autofac.Core;
using core.util.IocTest;
using Newtonsoft.Json;
using Sharewinfo.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using util.core.Helpers;
using Util.Core.Helpers;
using Util.Dependency;
using Util.Webs.Clients;
using Xunit;

namespace Util.Test
{
    public class Import
    {
        [Fact]
        public void ImportTest()
        {
            var ds = ExcelUtils.ExcelToDataTableFromStream(new FileStream("C:/Users/yanglin/Downloads/评估计划模板.xlsx", FileMode.Open, FileAccess.ReadWrite), ExcelType.Excel2007, true);
        }
    }
}
