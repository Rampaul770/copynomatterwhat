using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace CopyNoMatterWhat
{
    public class Program
    {
        private static DriveInfo di;
        private static int DELAY = 250;

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs;

            TryAgain:
            try {
                dirs = dir.GetDirectories();
                // If the destination directory doesn't exist, create it.
                if (!Directory.Exists(destDirName)) {
                    Directory.CreateDirectory(destDirName);
                }
  
                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files) {
                    string temppath = Path.Combine(destDirName, file.Name);
                    if (!(new FileInfo(temppath).Exists)) copy(file.FullName, temppath, file.Length);
                }
          
                // If copying subdirectories, copy them and their contents to new location.
                if (copySubDirs) {
                    foreach (DirectoryInfo subdir in dirs) {
                        string temppath = Path.Combine(destDirName, subdir.Name);
                        DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                    }
                }

            } catch (Exception ex) {
                Thread.Sleep(DELAY);
                Debug.WriteLine("Get Files Fail");
                goto TryAgain;
            }
            Debug.WriteLine("Done");
        }


        static void Main(string[] args)
        {
            di = new DriveInfo("f:");
            DirectoryCopy(@"F:\", "out", true);
        }

        private static void copy(string path, string name, long fileLength)
        {
            byte[] data = null;
            byte[] chunk = null;
            int bufferSize = 1024;


            Console.WriteLine("Copying " + path);

            FileStream fin;
            long index = 0;

            TryAgain1: try {
                fin = new FileStream(path, FileMode.Open);
                using (BinaryReader binReader = new BinaryReader(fin, new ASCIIEncoding())) {
                    while (data == null || (data.Length < fileLength)) {
                        binReader.BaseStream.Position = index;
                        chunk = binReader.ReadBytes(bufferSize);
                        if (data == null) data = chunk; else data = CombineByteArrays(data, chunk);
                        index = binReader.BaseStream.Position;
                        Console.Write("\r{0} / {1}", data.Length, fileLength);

                    }
                }
            } catch (IOException ex) {
                Debug.WriteLine("TA 1: " + ex.Message);
                Thread.Sleep(DELAY);
                goto TryAgain1;
            }

            File.WriteAllBytes(name, data);

            fin.Close();
            Console.Write(" ... OK");
            Console.WriteLine();
        }


        public static byte[] CombineByteArrays(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }
    }
}
