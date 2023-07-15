using ImportExport.Core.Models.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportExport.Service.Interfaces
{
    public interface IValidationService
    {
        IEnumerable<GrossWeightModel> GetMasterData(string sourceFile);
        bool Validate(IEnumerable<GrossWeightModel> grossWeights, string inputPath, string outputPath);
    }
}
