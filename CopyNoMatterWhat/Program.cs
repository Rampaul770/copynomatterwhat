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
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            //TryAgain:
            //if (!(new DriveInfo("f:").IsReady)) {
            //    //throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            //    Thread.Sleep(200);
            //    goto TryAgain;
            //}

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName)) {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files) {
                string temppath = Path.Combine(destDirName, file.Name);
                copy(file.FullName, temppath);
                Console.WriteLine(file.Name + "...OK");
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs) {
                foreach (DirectoryInfo subdir in dirs) {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }


        static void Main(string[] args)
        {
            /*  string driveName = Console.ReadLine();
              DriveInfo di = new DriveInfo(driveName);
              (new DirectoryInfo("out")).Create();

              foreach (FileInfo fi in new DirectoryInfo(driveName).GetFiles("*.*", SearchOption.AllDirectories)) {
                  if (di.IsReady) {
                      copy(fi.FullName, fi.Name);
                  } else {
                      Thread.Sleep(500);
                  }
              }

      */

            DirectoryCopy(@"D:\_Images\Мотивация\", "out", true);
        }

        static int currentByte = 0;
        private static void copy(string path, string name)
        {
            FileStream fin;
            int index = 0;

            const int arrayLength = 1000;
            byte[] dataArray = new byte[arrayLength];
            byte[] verifyArray = new byte[arrayLength];

            //        TryAgain1: try {
            fin = new FileStream(path, FileMode.Open);
            byte[] data;
            using (BinaryReader binReader = new BinaryReader(fin, new ASCIIEncoding())) {
                binReader.BaseStream.Position = index;

                byte[] chunk = binReader.ReadBytes(1024);
                data = chunk;
                while (chunk.Length > 0) {
                    chunk = binReader.ReadBytes(1024);
                    data = CombineByteArrays(data, chunk);
                }
            }


            File.WriteAllBytes(name, data);


            //} catch (Exception ex) {
            //    Debug.WriteLine(ex.Message);
            //    Thread.Sleep(200);
            //    goto TryAgain1;
            //}
            currentByte = 0;
            fin.Close();
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
