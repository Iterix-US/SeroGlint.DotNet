using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Common.Abstractions;
using SeroGlint.DotNet.Common.Extensions;

namespace SeroGlint.DotNet.FrameworkWrappers
{
    [ExcludeFromCodeCoverage]
    public class DirectoryManagementWrapper : IDirectoryManagement
    {
        private readonly ILogger _logger;

        public DirectoryManagementWrapper(ILogger logger = null)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            _logger?.LogInformation("Disposing DirectoryManagementWrapper resources.");
        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public bool Exists(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger?.LogWarning("No file path provided.");
                return false;
            }

            _logger?.LogInformation($"Checking if file exists at path: {filePath}");
            return File.Exists(filePath);
        }

        public string ReadAllText(string filePath)
        {
            if (!Exists(filePath))
            {
                _logger?.LogWarning($"File does not exist at path: {filePath}. Cannot read content.");
                return string.Empty;
            }

            _logger?.LogInformation($"Reading content from file at path: {filePath}");
            return File.ReadAllText(filePath);
        }

        public string WriteAllText(string filePath, string contents)
        {
            if (filePath.IsNullOrWhitespace() || contents.IsNullOrWhitespace())
            {
                _logger?.LogWarning("Path or contents cannot be null or whitespace.");
                return string.Empty;
            }

            File.WriteAllText(filePath, contents);
            return ReadAllText(filePath);
        }

        public string GetFullPath(string path)
        {
            if (path.IsNullOrWhitespace())
            {
                _logger?.LogWarning("No path provided.");
                return string.Empty;
            }

            return Path.GetFullPath(path);
        }

        public string GetDirectoryName(string filePath)
        {
            if (filePath.IsNullOrWhitespace())
            {
                _logger?.LogWarning("No file path provided.");
                return string.Empty;
            }

            return Path.GetDirectoryName(filePath);
        }

        public Stream Create(string filePath, FileMode fileMode = FileMode.Create, FileAccess fileAccess = FileAccess.ReadWrite, FileShare fileShare = FileShare.None)
        {
            if (filePath.IsNullOrWhitespace())
            {
                _logger?.LogWarning("No file path provided.");
                throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
            }

            return new FileStream(filePath, fileMode, fileAccess, fileShare);
        }

        public DirectoryInfo CreateDirectory(string path)
        {
            if (path.IsNullOrWhitespace())
            {
                _logger?.LogWarning("No directory provided");
                return null;
            }

            return Directory.CreateDirectory(path);
        }

        public bool DirectoryExists(string path)
        {
            if (path.IsNullOrWhitespace())
            {
                _logger?.LogWarning("No directory path provided.");
                return false;
            }

            _logger?.LogInformation($"Checking if directory exists at path: {path}");
            return Directory.Exists(path);
        }
    }
}
