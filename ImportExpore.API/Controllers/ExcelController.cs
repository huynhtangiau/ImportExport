using DocumentFormat.OpenXml.Office2010.ExcelAc;
using ImportExport.CrossCutting.Utils.Helpers;
using ImportExport.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ImportExport.API.Controllers
{
    public class Person
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Age { get; set; }
    }
    [ApiController]
    [Route("[controller]")]
    public class ExcelController : ControllerBase
    {
        private IEnumerable<Person> Generate(int size)
        {
            for (var i = 0; i < size; i++)
            {
                yield return new Person()
                {
                    Id = i.ToString(),
                    Name = $"Name {i.ToString()}",
                    Age = $"Age {i.ToString()}"
                };
            }
        }
        public ExcelController()
        {
        }

        [HttpGet]
        public ActionResult Test()
        {
            var peopleE = Generate(1000000)
                .ToList();
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Logs");
                ws.Cells["A1"].LoadFromCollection(peopleE, true);
                pck.SaveAs("C:\\Users\\giau.huynh.STS\\Giau\\Support.xlsx");
            }
            return Ok();
        }

    }
}
