using System;
using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Silky.Core
{
    public interface ISilkyFileProvider : IFileProvider
    {
        string Combine(params string[] paths);

        void CreateDirectory(string path);

        void CreateFile(string path);

        void DeleteDirectory(string path);

        void DeleteFile(string filePath);

        bool DirectoryExists(string path);

        void DirectoryMove(string sourceDirName, string destDirName);

        IEnumerable<string> EnumerateFiles(string directoryPath, string searchPattern, bool topDirectoryOnly = true);

        void FileCopy(string sourceFileName, string destFileName, bool overwrite = false);

        bool FileExists(string filePath);

        long FileLength(string path);

        void FileMove(string sourceFileName, string destFileName);

        string GetAbsolutePath(params string[] paths);

        // DirectorySecurity GetAccessControl(string path);

        DateTime GetCreationTime(string path);

        string[] GetDirectories(string path, string searchPattern = "", bool topDirectoryOnly = true);

        string GetDirectoryName(string path);

        string GetDirectoryNameOnly(string path);

        string GetFileExtension(string filePath);

        string GetFileName(string path);

        string GetFileNameWithoutExtension(string filePath);

        string[] GetFiles(string directoryPath, string searchPattern = "", bool topDirectoryOnly = true);

        DateTime GetLastAccessTime(string path);

        DateTime GetLastWriteTime(string path);

        DateTime GetLastWriteTimeUtc(string path);

        string GetParentDirectory(string directoryPath);

        string GetVirtualPath(string path);

        bool IsDirectory(string path);

        string MapPath(string path);

        Task<byte[]> ReadAllBytesAsync(string filePath);

        Task<string> ReadAllTextAsync(string path, Encoding encoding);

        string ReadAllText(string path, Encoding encoding);

        Task WriteAllBytesAsync(string filePath, byte[] bytes);

        Task WriteAllTextAsync(string path, string contents, Encoding encoding);

        void WriteAllText(string path, string contents, Encoding encoding);
    }
}