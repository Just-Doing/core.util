using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace core.util.Controllers
{
    public class ApiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Common()
        {
            var s = Util.Core.Helpers.String.PinYin("你好");
            return View();
        }
        public IActionResult String()
        {
            return View();
        }
    }
}