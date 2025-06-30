using ImportExport.Core.Models;
using ImportExport.Service.Interfaces;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics.Tensors;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ImportExport.Service.Services
{
    public class SemanticSearchService : ISemanticSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly string _modelPath;
        private InferenceSession _session;

        public SemanticSearchService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _modelPath = "all-MiniLM-L6-v2.onnx"; // You'll need to download this model
            InitializeModel();
        }

        private void InitializeModel()
        {
            try
            {
                if (File.Exists(_modelPath))
                {
                    _session = new InferenceSession(_modelPath);
                }
            }
            catch (Exception ex)
            {
                // Log error - model not available, will use fallback method
                _session = null;
            }
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            if (_session == null)
            {
                // Fallback to simple text processing if ONNX model not available
                return GenerateSimpleEmbedding(text);
            }

            try
            {
                // Tokenize text (simplified - in production you'd use proper tokenizer)
                var tokens = TokenizeText(text);
                var inputTensor = new DenseTensor<long>(tokens, new[] { 1, tokens.Length });
                
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input_ids", inputTensor)
                };

                using var results = _session.Run(inputs);
                var embedding = results.FirstOrDefault()?.AsTensor<float>();
                
                if (embedding != null)
                {
                    return embedding.ToArray();
                }
            }
            catch (Exception)
            {
                // Fall back to simple embedding if ONNX fails
            }

            return GenerateSimpleEmbedding(text);
        }

        private float[] GenerateSimpleEmbedding(string text)
        {
            // Simple embedding based on character frequencies and n-grams
            var embedding = new float[384]; // Standard embedding size
            var normalized = text.ToLowerInvariant();
            
            // Character frequency features
            for (int i = 0; i < Math.Min(normalized.Length, 100); i++)
            {
                if (i < embedding.Length)
                {
                    embedding[i] = (float)(normalized[i] % 256) / 256.0f;
                }
            }

            // Word-based features
            var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < Math.Min(words.Length, 50); i++)
            {
                if (100 + i < embedding.Length)
                {
                    embedding[100 + i] = (float)(words[i].GetHashCode() % 1000) / 1000.0f;
                }
            }

            // Normalize the embedding
            var norm = (float)Math.Sqrt(embedding.Sum(x => x * x));
            if (norm > 0)
            {
                for (int i = 0; i < embedding.Length; i++)
                {
                    embedding[i] /= norm;
                }
            }

            return embedding;
        }

        private long[] TokenizeText(string text)
        {
            // Simplified tokenization - in production use proper tokenizer
            var words = text.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return words.Select(w => (long)(w.GetHashCode() % 30000 + 1)).ToArray();
        }

        public double CalculateSimilarity(float[] embedding1, float[] embedding2)
        {
            if (embedding1.Length != embedding2.Length)
                return 0.0;

            var dotProduct = 0.0;
            var norm1 = 0.0;
            var norm2 = 0.0;

            for (int i = 0; i < embedding1.Length; i++)
            {
                dotProduct += embedding1[i] * embedding2[i];
                norm1 += embedding1[i] * embedding1[i];
                norm2 += embedding2[i] * embedding2[i];
            }

            if (norm1 == 0.0 || norm2 == 0.0)
                return 0.0;

            return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }


        public async Task<List<SearchFileModel>> SearchSimilarFilesAsync(string productName, List<FileModel> files, double threshold = 0.7)
        {
            var results = new List<SearchFileModel>();
            var productEmbedding = await GenerateEmbeddingAsync(productName);

            var tasks = files.Select(async file =>
            {
                try
                {
                    // Only use filename for semantic matching
                    var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    
                    var fileEmbedding = await GenerateEmbeddingAsync(fileName);
                    var similarity = CalculateSimilarity(productEmbedding, fileEmbedding);

                    return new SearchFileModel
                    {
                        File = file,
                        SimilarityScore = similarity,
                        ResultKey = similarity >= threshold ? SearchResultKey.Found : SearchResultKey.LowConfidence
                    };
                }
                catch (Exception)
                {
                    return new SearchFileModel
                    {
                        File = file,
                        SimilarityScore = 0.0,
                        ResultKey = SearchResultKey.LowConfidence
                    };
                }
            });

            var searchResults = await Task.WhenAll(tasks);
            
            return searchResults
                .Where(r => r.SimilarityScore >= threshold * 0.5) // Include low confidence matches
                .OrderByDescending(r => r.SimilarityScore)
                .ToList();
        }

        public void Dispose()
        {
            _session?.Dispose();
        }
    }
}