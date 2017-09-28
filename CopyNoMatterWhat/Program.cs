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
        private static int DELAY = 500;

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs;

            TryAgainDriveIsNotReady:
            try {
                 dirs = dir.GetDirectories();
                // If the destination directory doesn't exist, create it.
                if (!Directory.Exists(destDirName)) {
                    Directory.CreateDirectory(destDirName);
                }
            } catch (Exception ex) {
                Thread.Sleep(DELAY);
                Debug.WriteLine("Get Directories Fail");
                goto TryAgainDriveIsNotReady;
            }

            TryAgainDriveIsNotReady2:
            try {
                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files) {
                    string temppath = Path.Combine(destDirName, file.Name);
                    if (!(new FileInfo(temppath).Exists)) copy(file.FullName, temppath, file.Length);
                }
            } catch (Exception ex) {
                Thread.Sleep(DELAY);
                Debug.WriteLine("Get Directories Fail");
                goto TryAgainDriveIsNotReady2;
            }

            TryAgainDriveIsNotReady3:
            try {
                // If copying subdirectories, copy them and their contents to new location.
                if (copySubDirs) {
                    foreach (DirectoryInfo subdir in dirs) {
                        string temppath = Path.Combine(destDirName, subdir.Name);
                        DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                    }
                }
            } catch (Exception ex) {
                Thread.Sleep(DELAY);
                Debug.WriteLine("Get Directories Fail");
                goto TryAgainDriveIsNotReady3;
            }
            Debug.WriteLine("Done");

        }


        static void Main(string[] args)
        {
            di = new DriveInfo("f:");
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

            DirectoryCopy(@"F:\", "out", true);
        }

        private static void copy(string path, string name, long length)
        {
            byte[] data = null;
            int bufferSize = 1024;
            Console.WriteLine("Читаю " + path);

            FileStream fin;
            long index = 0;

            TryAgain1: try {
                if (di.IsReady)
                    fin = new FileStream(path, FileMode.Open);
                else {
                    Debug.WriteLine("Drive is not ready");
                    Thread.Sleep(DELAY);
                    goto TryAgain1;
                }
            } catch (IOException ex) {
                Debug.WriteLine("TA 1: " + ex.Message);
                Thread.Sleep(DELAY);
                goto TryAgain1;
            }


            using (BinaryReader binReader = new BinaryReader(fin, new ASCIIEncoding())) {
                byte[] chunk;
                TryAgain2: try {
                    binReader.BaseStream.Position = index;
                    chunk = binReader.ReadBytes(bufferSize);
                    if (data == null) data = chunk;
                    index = binReader.BaseStream.Position;
                } catch (IOException ex) {
                    Thread.Sleep(DELAY);

                    if (ex.HResult == -2147023890) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Случилось страшное!");
                        Console.ForegroundColor = ConsoleColor.White;
                        goto TryAgain1;
                    } else {
                        Debug.WriteLine("TA 2: " + ex.HResult);
                        goto TryAgain2;
                    }
                }


                while (chunk.Length > 0) {
                    TryAgain3: try {
                        chunk = binReader.ReadBytes(bufferSize);
                        index = binReader.BaseStream.Position;
                        data = CombineByteArrays(data, chunk);
                        Console.Write("\r{0} / {1}", data.Length, length);
                    } catch (IOException ex) {
                        Thread.Sleep(DELAY);
                        if (!di.IsReady) { fin.Close(); goto TryAgain1; }
                        Debug.WriteLine("TA 3: " + ex.Message + ex.HResult);
                        goto TryAgain3;
                    }
                }
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
