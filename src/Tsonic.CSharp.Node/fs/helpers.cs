using System.Text;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    // Helper to parse encoding strings
    private static Encoding ParseEncoding(string encoding)
    {
        return encoding.ToLowerInvariant() switch
        {
            "utf-8" or "utf8" => Utf8NoBom,
            "ascii" => Encoding.ASCII,
            "utf-16" or "utf16" => Encoding.Unicode,
            "utf-32" or "utf32" => Encoding.UTF32,
            _ => Utf8NoBom
        };
    }

    // Helper method for recursive directory copy
    private static void CopyDirectory(string sourceDir, string destDir)
    {
        // Create destination directory if it doesn't exist
        Directory.CreateDirectory(destDir);

        // Copy files
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(destDir, fileName);
            File.Copy(file, destFile, overwrite: true);
        }

        // Copy subdirectories
        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(subDir);
            var destSubDir = Path.Combine(destDir, dirName);
            CopyDirectory(subDir, destSubDir);
        }
    }
}
