using System;
using System.IO;

namespace SeroGlint.DotNet.Common.Abstractions
{
    public interface IDirectoryManagement : IDisposable
    {
        bool Exists(string filePath);
        string GetFileName(string path);
        string ReadAllText(string filePath);
        string WriteAllText(string path, string contents);
        string GetFullPath(string path);
        string GetDirectoryName(string filePath);
        Stream Create(string filePath, FileMode fileMode = FileMode.Create, FileAccess fileAccess = FileAccess.ReadWrite, FileShare fileShare = FileShare.None);
        DirectoryInfo CreateDirectory(string path);
        bool DirectoryExists(string logFilePath);
    }
}
