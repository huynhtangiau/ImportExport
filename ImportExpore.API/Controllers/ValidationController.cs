using ImportExport.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace ImportExport.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ValidationController : ControllerBase
    {
        private readonly ILogger<ValidationController> _logger;
        private readonly IValidationService _validationService;
        public ValidationController(ILogger<ValidationController> logger,
            IValidationService validationService)
        {
            _logger = logger;
            _validationService = validationService;
        }
        [HttpPost]
        public ActionResult ValidateGrossWeight(string sourceFile = @"C:\Users\giau.huynh.STS\Giau\Support\Validation\GrossWeight.xlsx",
            string sourceFolder = @"C:\Users\giau.huynh.STS\Giau\Support\Validation\Input",
            string outputFolder = @"C:\Users\giau.huynh.STS\Giau\Support\Validation\Output")
        {
            var grossWeights = _validationService.GetMasterData(sourceFile);
            System.IO.File.Copy(sourceFile, System.IO.Path.Combine(
                 System.IO.Path.GetDirectoryName(sourceFile), 
                 $"{System.IO.Path.GetFileNameWithoutExtension(sourceFile)}_{DateTime.Now.ToString("yyyyMMdd")}.{System.IO.Path.GetExtension(sourceFile)}"
                ));
            var success = _validationService.Validate(grossWeights, sourceFolder, outputFolder);
            return Ok(success);
        }
    }
}
