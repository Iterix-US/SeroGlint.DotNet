using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Common.Abstractions;
using SeroGlint.DotNet.FrameworkWrappers;

namespace SeroGlint.DotNet.FileManagement
{
    public class ConfigurationLoader
    {
        private readonly IDirectoryManagement _fileManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationLoader"/> class.
        /// </summary>
        /// <param name="fileManager"></param>
        /// <param name="logger"></param>
        public ConfigurationLoader(IDirectoryManagement fileManager = null, ILogger logger = null)
        {
            _fileManager = fileManager ?? new DirectoryManagementWrapper(logger);
        }

        /// <summary>
        /// Loads a JSON configuration file from the specified file path and deserializes it into an object of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public T LoadJsonConfiguration<T>(string filePath) where T : class
        {
            var fileExists = _fileManager.Exists(filePath);

            if (!fileExists)
            {
                return Activator.CreateInstance<T>();
            }

            var json = _fileManager.ReadAllText(filePath) ?? File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json) ?? Activator.CreateInstance<T>();
        }

        /// <summary>
        /// Saves the specified configuration object as a JSON file at the given file path.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public void SaveJsonConfiguration<T>(string filePath, T configuration, ILogger logger = null)
        {
            var json = JsonSerializer.Serialize(configuration, new JsonSerializerOptions { WriteIndented = true });

            try
            {
                var file = _fileManager.GetFullPath(filePath) ?? Path.GetFullPath(filePath);
                var path = _fileManager.GetDirectoryName(filePath) ?? Path.GetDirectoryName(file);

                SafeguardDirectory(path);
                SafeguardFile(file);

                _fileManager.WriteAllText(file, json);
            }
            catch (Exception ex)
            {
                logger?.LogError(message:"Configuration file could not be created", exception: ex);
                throw new FileNotFoundException("Configuration file could not be created", ex);
            }
        }

        /// <summary>
        /// Safeguards the existence of a file by checking if it exists, and if not, creates it.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="logger"></param>
        /// <exception cref="FileNotFoundException"></exception>
        [ExcludeFromCodeCoverage]
        private void SafeguardFile(string file, ILogger logger = null)
        {
            var fileExists = _fileManager.Exists(file);

            if (fileExists && _fileManager == null)
            {
                return;
            }

            var fileStream = _fileManager.Create(file);

            if (fileStream == null)
            {
                logger?.LogError($"The file {file} could not be created.");
                throw new FileNotFoundException($"The file {file} could not be created.");
            }

            fileStream.Dispose();
        }

        /// <summary>
        /// Safeguards the existence of a directory by checking if it exists, and if not, creates it.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="logger"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        [ExcludeFromCodeCoverage]
        private void SafeguardDirectory(string path, ILogger logger = null)
        {
            var directoryExists = _fileManager.Exists(path);

            if (directoryExists)
            {
                return;
            }

            var directoryInfo = _fileManager.CreateDirectory(path);

            if (directoryInfo != null)
            {
                return;
            }

            logger?.LogError($"The directory {path} could not be created.");
            throw new DirectoryNotFoundException($"The directory {path} could not be created.");
        }
    }
}
