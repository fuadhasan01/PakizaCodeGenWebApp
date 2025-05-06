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
            if (string.IsNullOrWhiteSpace(sqlInput))
            {
                ViewBag.Error = "⚠️ Please paste a valid CREATE TABLE SQL before generating code.";
                return View("Index");
            }
            var generator = new GeneratorService();
            var results = generator.GenerateAll(sqlInput);
            if (results == null || results.Count == 0 || results.Values.Any(v => v.StartsWith("Invalid SQL")))
            {
                ViewBag.Error = "⚠️ Your SQL is invalid. Please ensure it's a proper CREATE TABLE script.";
                ViewBag.SqlInput = sqlInput;
                return View("Index");
            }
            ViewBag.Results = results;
            ViewBag.SqlInput = sqlInput;
            return View("Index");
        }
    }
}
