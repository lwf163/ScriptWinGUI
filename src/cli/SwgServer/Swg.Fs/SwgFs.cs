using System.Diagnostics;
using System.Text;

namespace Swg.Fs;

/// <summary>
/// 文件系统能力静态入口（Windows 文件系统操作 + 快捷方式解析）。
/// 用于 <see cref="FsController"/> 暴露为 REST（表征状态转移）；不负责路由与 HTTP（超文本传输协议）映射。
/// </summary>
public static class SwgFs
{
    public static string ReadText(string filePath, string? encodingName)
    {
        string full = NormalizePathInternal(filePath);
        if (!File.Exists(full))
            throw new InvalidOperationException($"文件不存在：{full}");

        Encoding enc = ParseEncodingOrThrow(encodingName);
        try
        {
            return File.ReadAllText(full, enc);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            throw new InvalidOperationException($"读取文件失败：{full}", ex);
        }
    }

    public static void WriteText(string filePath, string text, string? encodingName)
    {
        string full = NormalizePathInternal(filePath);
        Encoding enc = ParseEncodingOrThrow(encodingName);
        try
        {
            EnsureParentDirectoryExists(full);
            File.WriteAllText(full, text, enc); // 覆盖写入（docs：覆盖文件）
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            throw new InvalidOperationException($"写入文件失败：{full}", ex);
        }
    }

    public static void AppendText(string filePath, string text, string? encodingName)
    {
        string full = NormalizePathInternal(filePath);
        Encoding enc = ParseEncodingOrThrow(encodingName);
        try
        {
            EnsureParentDirectoryExists(full);
            using var fs = new FileStream(full, FileMode.Append, FileAccess.Write, FileShare.Read);
            using var sw = new StreamWriter(fs, enc);
            sw.Write(text);
            sw.Flush();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            throw new InvalidOperationException($"追加写入文件失败：{full}", ex);
        }
    }

    public static void CopyFile(string sourcePath, string targetPath, bool overwrite)
    {
        string sourceFull = NormalizePathInternal(sourcePath);
        string targetFull = NormalizePathInternal(targetPath);
        if (!File.Exists(sourceFull))
            throw new InvalidOperationException($"源文件不存在：{sourceFull}");

        if (File.Exists(targetFull))
        {
            if (!overwrite)
                throw new InvalidOperationException($"目标文件已存在且未允许覆盖：{targetFull}");
            // 覆盖：先移除再复制，确保不会因为只读属性导致失败。
            DeleteFileInternal(targetFull);
        }

        try
        {
            EnsureParentDirectoryExists(targetFull);
            CopyFileWithAttributesAndTimes(sourceFull, targetFull);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"复制文件失败：{sourceFull} -> {targetFull}", ex);
        }
    }

    public static void MoveFile(string sourcePath, string targetPath, bool overwrite)
    {
        string sourceFull = NormalizePathInternal(sourcePath);
        string targetFull = NormalizePathInternal(targetPath);
        if (!File.Exists(sourceFull))
            throw new InvalidOperationException($"源文件不存在：{sourceFull}");

        if (File.Exists(targetFull) || Directory.Exists(targetFull))
        {
            if (!overwrite)
                throw new InvalidOperationException($"目标已存在且未允许覆盖：{targetFull}");
            DeleteFileSystemItemInternal(targetFull, expectFile: true);
        }

        // “最优方式”：同驱动器用 Move；跨驱动器用 Copy+Delete（以减少元数据丢失风险）。
        if (IsSameVolume(sourceFull, targetFull))
        {
            MoveFileSameVolume(sourceFull, targetFull);
            return;
        }

        // Cross-volume：复制 + 删除源文件
        try
        {
            EnsureParentDirectoryExists(targetFull);
            CopyFileWithAttributesAndTimes(sourceFull, targetFull);
            DeleteFileInternal(sourceFull);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"移动文件失败（跨驱动器）：{sourceFull} -> {targetFull}", ex);
        }
    }

    public static void DeleteFile(string filePath)
    {
        string full = NormalizePathInternal(filePath);
        if (!File.Exists(full))
            throw new InvalidOperationException($"文件不存在：{full}");

        DeleteFileInternal(full);
    }

    public static bool Exists(string path)
    {
        string full = NormalizePathInternal(path);
        return File.Exists(full) || Directory.Exists(full);
    }

    public static GetItemInfoResult GetItemInfo(string path)
    {
        string full = NormalizePathInternal(path);
        if (File.Exists(full))
        {
            var fi = new FileInfo(full);
            return new GetItemInfoResult(
                name: fi.Name,
                fullPath: fi.FullName,
                isDirectory: false,
                sizeBytes: fi.Length,
                creationTimeUtc: fi.CreationTimeUtc,
                lastWriteTimeUtc: fi.LastWriteTimeUtc,
                lastAccessTimeUtc: fi.LastAccessTimeUtc,
                attributes: fi.Attributes.ToString());
        }

        if (Directory.Exists(full))
        {
            var di = new DirectoryInfo(full);
            return new GetItemInfoResult(
                name: di.Name,
                fullPath: di.FullName,
                isDirectory: true,
                sizeBytes: null,
                creationTimeUtc: di.CreationTimeUtc,
                lastWriteTimeUtc: di.LastWriteTimeUtc,
                lastAccessTimeUtc: di.LastAccessTimeUtc,
                attributes: di.Attributes.ToString());
        }

        throw new InvalidOperationException($"路径不存在：{full}");
    }

    public static void CreateDirectory(string directoryPath)
    {
        string full = NormalizePathInternal(directoryPath);
        try
        {
            Directory.CreateDirectory(full); // recursive create：Directory.CreateDirectory 本身即递归
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"创建目录失败：{full}", ex);
        }
    }

    public static void DeleteDirectory(string directoryPath)
    {
        string full = NormalizePathInternal(directoryPath);
        if (!Directory.Exists(full))
            throw new InvalidOperationException($"目录不存在：{full}");

        DeleteDirectoryInternal(full);
    }

    public static IReadOnlyList<string> ListDirectory(string directoryPath, string? pattern)
    {
        string full = NormalizePathInternal(directoryPath);
        if (!Directory.Exists(full))
            throw new InvalidOperationException($"目录不存在：{full}");

        string p = string.IsNullOrWhiteSpace(pattern) ? "*" : pattern.Trim();
        try
        {
            var list = new List<string>();
            foreach (string entry in Directory.EnumerateFileSystemEntries(full, p, SearchOption.TopDirectoryOnly))
                list.Add(Path.GetFileName(entry));
            return list;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"列出目录失败：{full} pattern={p}", ex);
        }
    }

    public static void MoveDirectory(string sourceDirectoryPath, string targetDirectoryPath, bool overwrite)
    {
        string sourceFull = NormalizePathInternal(sourceDirectoryPath);
        string targetFull = NormalizePathInternal(targetDirectoryPath);
        if (!Directory.Exists(sourceFull))
            throw new InvalidOperationException($"源目录不存在：{sourceFull}");

        if (File.Exists(targetFull) || Directory.Exists(targetFull))
        {
            if (!overwrite)
                throw new InvalidOperationException($"目标已存在且未允许覆盖：{targetFull}");
            DeleteFileSystemItemInternal(targetFull, expectFile: File.Exists(targetFull));
        }

        if (IsSameVolume(sourceFull, targetFull))
        {
            MoveDirectorySameVolume(sourceFull, targetFull);
            return;
        }

        // Cross-volume：copy + delete
        try
        {
            CopyDirectoryWithMetadata(sourceFull, targetFull, overwrite: true);
            DeleteDirectoryInternal(sourceFull);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"移动目录失败（跨驱动器）：{sourceFull} -> {targetFull}", ex);
        }
    }

    public static IReadOnlyList<string> SearchFiles(string rootDirectoryPath, string? pattern)
    {
        string full = NormalizePathInternal(rootDirectoryPath);
        if (!Directory.Exists(full))
            throw new InvalidOperationException($"根目录不存在：{full}");

        string p = string.IsNullOrWhiteSpace(pattern) ? "*" : pattern.Trim();
        try
        {
            return Directory.EnumerateFiles(full, p, SearchOption.AllDirectories).ToList();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"递归搜索文件失败：{full} pattern={p}", ex);
        }
    }

    public static IReadOnlyList<string> FindExeShortcuts(string exeName)
    {
        if (string.IsNullOrWhiteSpace(exeName))
            throw new ArgumentException("ExeName 必填。");

        string targetExeFileName = Path.GetFileName(exeName.Trim());
        if (string.IsNullOrWhiteSpace(targetExeFileName))
            throw new ArgumentException("ExeName 格式无效。");

        var roots = new List<string>();
        AddFolderIfExists(roots, Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        AddFolderIfExists(roots, Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory));
        AddFolderIfExists(roots, Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
        AddFolderIfExists(roots, Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));

        if (roots.Count == 0)
            return Array.Empty<string>();

        // 解析 .lnk：使用 Windows 脚本对象（WScript.Shell）动态调用，不额外引 COM（组件对象模型）引用程序集。
        object shellObj;
        try
        {
            shellObj = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell")!)
                       ?? throw new InvalidOperationException("无法创建 WScript.Shell COM（组件对象模型）对象。");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("快捷方式解析（WScript.Shell）失败。", ex);
        }

        var shell = shellObj;
        var matched = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string root in roots)
        {
            foreach (string lnk in Directory.EnumerateFiles(root, "*.lnk", SearchOption.AllDirectories))
            {
                try
                {
                    // dynamic：shortcut.TargetPath
                    dynamic dynShell = shell;
                    dynamic shortcut = dynShell.CreateShortcut(lnk);
                    string? targetPath = shortcut?.TargetPath as string;
                    if (string.IsNullOrWhiteSpace(targetPath))
                        continue;

                    string fileName = Path.GetFileName(targetPath);
                    if (string.Equals(fileName, targetExeFileName, StringComparison.OrdinalIgnoreCase))
                        matched.Add(targetPath);
                }
                catch
                {
                    // 单个快捷方式损坏/权限不足时：跳过，不中断整体搜索。
                }
            }
        }

        return matched.ToList();

        static void AddFolderIfExists(List<string> list, string folder)
        {
            if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
                list.Add(folder);
        }
    }

    public sealed record GetItemInfoResult(
        string name,
        string fullPath,
        bool isDirectory,
        long? sizeBytes,
        DateTime creationTimeUtc,
        DateTime lastWriteTimeUtc,
        DateTime lastAccessTimeUtc,
        string attributes);


    private static Encoding ParseEncodingOrThrow(string? encodingName)
    {
        if (string.IsNullOrWhiteSpace(encodingName))
            return Encoding.UTF8;

        string s = encodingName.Trim();
        if (s.Equals("utf8", StringComparison.OrdinalIgnoreCase) ||
            s.Equals("utf-8", StringComparison.OrdinalIgnoreCase) ||
            s.Equals("utf_8", StringComparison.OrdinalIgnoreCase))
            return Encoding.UTF8;

        if (s.Equals("ascii", StringComparison.OrdinalIgnoreCase))
            return Encoding.ASCII;

        if (s.Equals("gbk", StringComparison.OrdinalIgnoreCase) ||
            s.Equals("gb2312", StringComparison.OrdinalIgnoreCase) ||
            s.Equals("gb-2312", StringComparison.OrdinalIgnoreCase) ||
            s.Equals("gb-18030", StringComparison.OrdinalIgnoreCase))
        {
            // Windows 上一般支持 GBK/GB2312。
            try
            {
                return Encoding.GetEncoding("GBK");
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"不支持的编码：{encodingName}（GBK）。", ex);
            }
        }

        // 允许用户传入 .NET 支持的其他编码名
        try
        {
            return Encoding.GetEncoding(s);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"不支持的编码：{encodingName}。", ex);
        }
    }

    private static string NormalizePathInternal(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path 不能为空。");

        string trimmed = path.Trim();
        string full = Path.GetFullPath(trimmed);

        // 统一分隔符
        full = full.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

        // 去掉尾部分隔符（除非是根目录，如 C:\）
        bool isRoot = string.Equals(full, Path.GetPathRoot(full), StringComparison.OrdinalIgnoreCase);
        if (!isRoot)
            full = full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return full;
    }

    private static void EnsureParentDirectoryExists(string fileFullPath)
    {
        string? dir = Path.GetDirectoryName(fileFullPath);
        if (string.IsNullOrWhiteSpace(dir))
            return;
        Directory.CreateDirectory(dir);
    }

    private static bool IsSameVolume(string fullPathA, string fullPathB)
    {
        string rootA = Path.GetPathRoot(fullPathA) ?? string.Empty;
        string rootB = Path.GetPathRoot(fullPathB) ?? string.Empty;
        return string.Equals(rootA, rootB, StringComparison.OrdinalIgnoreCase);
    }

    private static void CopyFileWithAttributesAndTimes(string sourceFull, string targetFull)
    {
        var src = new FileInfo(sourceFull);
        if (!src.Exists)
            throw new InvalidOperationException($"源文件不存在：{sourceFull}");

        File.Copy(sourceFull, targetFull, overwrite: true);
        File.SetAttributes(targetFull, src.Attributes);
        File.SetCreationTimeUtc(targetFull, src.CreationTimeUtc);
        File.SetLastWriteTimeUtc(targetFull, src.LastWriteTimeUtc);
        File.SetLastAccessTimeUtc(targetFull, src.LastAccessTimeUtc);
    }

    private static void MoveFileSameVolume(string sourceFull, string targetFull)
    {
        var src = new FileInfo(sourceFull);
        if (!src.Exists)
            throw new InvalidOperationException($"源文件不存在：{sourceFull}");

        try
        {
            EnsureParentDirectoryExists(targetFull);
            File.Move(sourceFull, targetFull, overwrite: true); // Overwrite 已在外层处理
            File.SetAttributes(targetFull, src.Attributes);
            File.SetCreationTimeUtc(targetFull, src.CreationTimeUtc);
            File.SetLastWriteTimeUtc(targetFull, src.LastWriteTimeUtc);
            File.SetLastAccessTimeUtc(targetFull, src.LastAccessTimeUtc);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"移动文件失败：{sourceFull} -> {targetFull}", ex);
        }
    }

    private static void DeleteFileInternal(string fullPath)
    {
        try
        {
            var attrs = File.GetAttributes(fullPath);
            if ((attrs & FileAttributes.ReadOnly) != 0)
                File.SetAttributes(fullPath, attrs & ~FileAttributes.ReadOnly);

            File.Delete(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"删除文件失败：{fullPath}", ex);
        }
    }

    private static void DeleteDirectoryInternal(string fullPath)
    {
        try
        {
            RemoveReadOnlyRecursively(fullPath);
            Directory.Delete(fullPath, recursive: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"删除目录失败：{fullPath}", ex);
        }
    }

    private static void DeleteFileSystemItemInternal(string fullPath, bool expectFile)
    {
        // expectFile 仅用于提示日志/异常；实际以 File/Directory.Exists 为准。
        if (File.Exists(fullPath))
            DeleteFileInternal(fullPath);
        else if (Directory.Exists(fullPath))
            DeleteDirectoryInternal(fullPath);
        else
            return;
    }

    private static void RemoveReadOnlyRecursively(string directoryFullPath)
    {
        // 删除时，ReadOnly 文件/目录必须先移除只读属性。
        try
        {
            foreach (string file in Directory.EnumerateFiles(directoryFullPath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    var attrs = File.GetAttributes(file);
                    if ((attrs & FileAttributes.ReadOnly) != 0)
                        File.SetAttributes(file, attrs & ~FileAttributes.ReadOnly);
                }
                catch
                {
                    // 忽略单个文件的失败，让 Directory.Delete 负责最终抛错。
                }
            }

            foreach (string dir in Directory.EnumerateDirectories(directoryFullPath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    var attrs = File.GetAttributes(dir);
                    if ((attrs & FileAttributes.ReadOnly) != 0)
                        File.SetAttributes(dir, attrs & ~FileAttributes.ReadOnly);
                }
                catch
                {
                    // 忽略单个目录失败。
                }
            }

            // 根目录也检查一次
            try
            {
                var attrs = File.GetAttributes(directoryFullPath);
                if ((attrs & FileAttributes.ReadOnly) != 0)
                    File.SetAttributes(directoryFullPath, attrs & ~FileAttributes.ReadOnly);
            }
            catch
            {
                // 忽略。
            }
        }
        catch
        {
            // 交给上层捕获。
        }
    }

    private static void MoveDirectorySameVolume(string sourceFull, string targetFull)
    {
        var srcInfo = new DirectoryInfo(sourceFull);
        if (!srcInfo.Exists)
            throw new InvalidOperationException($"源目录不存在：{sourceFull}");

        try
        {
            Directory.Move(sourceFull, targetFull);
            // 重新应用属性/时间，确保 move 不丢失。
            var destInfo = new DirectoryInfo(targetFull);
            if (!destInfo.Exists)
                return;

            File.SetAttributes(targetFull, srcInfo.Attributes);
            Directory.SetCreationTimeUtc(targetFull, srcInfo.CreationTimeUtc);
            Directory.SetLastWriteTimeUtc(targetFull, srcInfo.LastWriteTimeUtc);
            Directory.SetLastAccessTimeUtc(targetFull, srcInfo.LastAccessTimeUtc);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"移动目录失败：{sourceFull} -> {targetFull}", ex);
        }
    }

    private static void CopyDirectoryWithMetadata(string sourceFull, string targetFull, bool overwrite)
    {
        if (overwrite && (File.Exists(targetFull) || Directory.Exists(targetFull)))
        {
            DeleteFileSystemItemInternal(targetFull, expectFile: File.Exists(targetFull));
        }

        if (!Directory.Exists(sourceFull))
            throw new InvalidOperationException($"源目录不存在：{sourceFull}");

        CopyDirectoryRecursive(sourceFull, targetFull);
    }

    private static void CopyDirectoryRecursive(string sourceFull, string targetFull)
    {
        var srcInfo = new DirectoryInfo(sourceFull);
        if (!srcInfo.Exists)
            throw new InvalidOperationException($"源目录不存在：{sourceFull}");

        Directory.CreateDirectory(targetFull);
        // 先设一次，后续递归创建子项可能会改变目录“最后写入时间”，因此最终还会再设一次。
        ApplyDirectoryMetadata(targetFull, srcInfo);

        foreach (string file in Directory.EnumerateFiles(sourceFull, "*", SearchOption.TopDirectoryOnly))
        {
            string fileName = Path.GetFileName(file);
            string targetFile = Path.Combine(targetFull, fileName);
            CopyFileWithAttributesAndTimes(file, targetFile);
        }

        foreach (string subDir in Directory.EnumerateDirectories(sourceFull, "*", SearchOption.TopDirectoryOnly))
        {
            string subName = Path.GetFileName(subDir);
            string targetSub = Path.Combine(targetFull, subName);
            CopyDirectoryRecursive(subDir, targetSub);
        }

        // 最终再应用一次，尽量锁住最后写入/访问时间。
        ApplyDirectoryMetadata(targetFull, srcInfo);
    }

    private static void ApplyDirectoryMetadata(string directoryFullPath, DirectoryInfo srcInfo)
    {
        File.SetAttributes(directoryFullPath, srcInfo.Attributes);
        Directory.SetCreationTimeUtc(directoryFullPath, srcInfo.CreationTimeUtc);
        Directory.SetLastWriteTimeUtc(directoryFullPath, srcInfo.LastWriteTimeUtc);
        Directory.SetLastAccessTimeUtc(directoryFullPath, srcInfo.LastAccessTimeUtc);
    }
}

