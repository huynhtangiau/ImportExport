using ImportExport.Core.Models;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportExport.CrossCutting.Utils.Helpers
{
    public static class FileHelper
    {
        public static byte[] Compress(this IEnumerable<FileModel> files)
        {
            if (files.Any())
            {
                var ms = new MemoryStream();
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
                {
                    foreach (var file in files)
                    {
                        archive.Add(file);
                    }
                }
                ms.Position = 0;
                return ms.ToArray();
            }
            return null;
        }

        private static ZipArchiveEntry Add(this ZipArchive archive, FileModel file)
        {
            var entry = archive.CreateEntry(file.FileName, CompressionLevel.Fastest);
            using (var stream = entry.Open())
            {
                using var memoryStream = new MemoryStream(file.FileStream);
                memoryStream.CopyTo(stream);
            }
            return entry;
        }
        public static List<FileModel> GetFiles(this string sourceFolderPath)
        {
            var dir = new DirectoryInfo(sourceFolderPath);
            var files = dir.GetFiles("*.pdf", SearchOption.AllDirectories);
            return files.Select(s => new FileModel()
            {
                FileName = s.Name,
                Path = s.FullName,
                Extension = s.Extension,
                CreatedDate = s.CreationTime,
                FileStream = File.ReadAllBytes(s.FullName)
            }).ToList();
        }
    }
}
