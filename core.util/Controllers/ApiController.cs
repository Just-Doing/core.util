using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using core.util.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
            return View();
        }
        public IActionResult String()
        {
            return View();
        }
    }
}