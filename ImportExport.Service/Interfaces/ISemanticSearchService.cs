using ImportExport.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImportExport.Service.Interfaces
{
    public interface ISemanticSearchService
    {
        Task<List<SearchFileModel>> SearchSimilarFilesAsync(string productName, List<FileModel> files, double threshold = 0.7);
        Task<float[]> GenerateEmbeddingAsync(string text);
        double CalculateSimilarity(float[] embedding1, float[] embedding2);
    }
}