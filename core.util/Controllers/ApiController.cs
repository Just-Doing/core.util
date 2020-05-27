using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using core.util.IocTest;
using core.util.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using util.core.Helpers;
using Util.Core.Helpers;
using Util.Dependency;
using WeChatMVC.Common;

namespace core.util.Controllers
{
    public class ApiController : Controller
    {
        // private static List<FunctionTitle> allData = new List<FunctionTitle>();
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult List(string id)
        {

            var allData = Util.Core.Helpers.Json.GetDataFromFile<List<FunctionTitle>>("Config/database.json");
            var currentUtil = string.IsNullOrEmpty(id)? allData[0] : allData.SingleOrDefault(m => m.title == id);

            ViewBag.allData = allData;
            ViewBag.currentUtil = currentUtil;

            var service = Ioc.Create<IClass>();
            var res = service.Calc(1, 2);

            Func<int, int> f = (x) => x + 1;
            var s = f(2);

            var s1 = MachineCode.GetMachineCodeString();
            
            WeChatMVC.Common.JsEncryptHelper jsHelper = new JsEncryptHelper();

           var a = jsHelper.Decrypt("by3XLCAMt+1dAldQJWNvDnRLNGVVdtuGpq+qgbv11I9QwVo9f5hXp5evrRKx32OSpYCAhCGbUgHYLKi5aHMthkG/Q4/uwWjpjFvIpXSbGIFyybf7Sb5rVmRYX+kel3Sw6MhygkgCvAMrS+9xNU0hxuiPO89ZlnWvaChic90LXqw=");

            ViewBag.machinecode = s1;
            return View();
        }
        public IActionResult String()
        {
            return View();
        }
    }
    public class CalcConfig : Module, IConfig
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MyClass2>().As<MyClass2>();
            builder.RegisterType<MyClass>().As<IClass>();
        }
    }
}