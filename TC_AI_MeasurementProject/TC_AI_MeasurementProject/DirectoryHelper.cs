using System;
using System.IO;

namespace Training_Tutorials
{
    public static class DirectoryHelper
    {

        /// <summary>
        /// Deletes the specified directory
        /// </summary>
        /// <param name="target_dir">The target_dir.</param>
        public static void DeleteDirectory(string target_dir)
        {
            if (Directory.Exists(target_dir))
            {
                DeleteDirectoryFiles(target_dir);
                while (Directory.Exists(target_dir))
                {
                    //lock (_lock)
                    {
                        DeleteDirectoryDirs(target_dir);
                    }
                }
            }
        }

        private static void DeleteDirectoryDirs(string target_dir)
        {
            System.Threading.Thread.Sleep(100);

            if (Directory.Exists(target_dir))
            {

                string[] dirs = Directory.GetDirectories(target_dir);

                if (dirs.Length == 0)
                    Directory.Delete(target_dir, false);
                else
                    foreach (string dir in dirs)
                        DeleteDirectoryDirs(dir);
            }
        }

        private static void DeleteDirectoryFiles(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectoryFiles(dir);
            }
        }       

        /// <summary>
        /// Copies a directory from Source to Destination
        /// </summary>
        /// <param name="src">Source Folder</param>
        /// <param name="dst">Destingation Folder</param>
        public static void CopyDirectory(string src, string dst)
        {
            String[] files;

            if (dst[dst.Length - 1] != Path.DirectorySeparatorChar)
                dst += Path.DirectorySeparatorChar;

            if (!Directory.Exists(dst))
                Directory.CreateDirectory(dst);

            files = Directory.GetFileSystemEntries(src);

            foreach (string element in files)
            {
                // Sub directories

                if (Directory.Exists(element))
                    CopyDirectory(element, dst + Path.GetFileName(element));
                // Files in directory

                else
                    File.Copy(element, dst + Path.GetFileName(element), true);
            }
        }
    }
}
