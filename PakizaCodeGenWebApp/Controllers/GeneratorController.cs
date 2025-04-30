using Microsoft.AspNetCore.Mvc;
using PakizaCodeGenWebApp.Models;
using System.IO.Compression;

namespace PakizaCodeGenWebApp.Controllers
{
    public class GeneratorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GenerateCode(string sqlInput)
        {
            var generator = new GeneratorService();
            var results = generator.GenerateAll(sqlInput);
            ViewBag.Results = results;
            ViewBag.SqlInput = sqlInput;
            return View("Index");
        }
    }
}
