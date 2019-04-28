using System;

public static class UnityPath {

    public static readonly char DirectorySeparatorChar = '\\';
    public static readonly char AltDirectorySeparatorChar = '/';
    public static readonly char VolumeSeparatorChar = ':';

    public static String Combine(String path1, String path2)
    {
        if (path1 == null || path2 == null)
            throw new ArgumentNullException((path1 == null) ? "path1" : "path2");

        return CombineNoChecks(path1, path2);
    }

    private static String CombineNoChecks(String path1, String path2)
    {
        if (path2.Length == 0)
            return path1;

        if (path1.Length == 0)
            return path2;

        if (IsPathRooted(path2))
            return path2;

        char ch = path1[path1.Length - 1];
        if (ch != DirectorySeparatorChar && ch != AltDirectorySeparatorChar && ch != VolumeSeparatorChar)
            return path1 + AltDirectorySeparatorChar + path2;
        return path1 + path2;
    }

    public static bool IsPathRooted(String path)
    {
        if (path != null)
        {
            int length = path.Length;
            if ((length >= 1 && (path[0] == DirectorySeparatorChar || path[0] == AltDirectorySeparatorChar)) || (length >= 2 && path[1] == VolumeSeparatorChar))
                return true;
        }
        return false;
    }
}
