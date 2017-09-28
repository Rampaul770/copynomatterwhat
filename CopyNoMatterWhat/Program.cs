using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CopyNoMatterWhat
{
    public class Program
    {
        [DllImport("User32.dll")]
        public static extern Int32 FindWindow(String lpClassName, String lpWindowName);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(int hWnd, int msg, int wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        private const int WM_CLOSE = 16;
        private const int BN_CLICKED = 245;
        static int hwnd = 0;
        static IntPtr hwndChild = IntPtr.Zero;

        private static int DELAY = 1000;
        private static int bufferSize = 4096;
        private static BackgroundWorker worker = new BackgroundWorker();

        static void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine(e.ProgressPercentage.ToString());
        }
        static private void Intercept()
        {
            //Get a handle for the "1" button
            string btnContinueText = "&Continue";
            string btnTryAgainText = "&Try Again";
            hwndChild = FindWindowEx((IntPtr)hwnd, IntPtr.Zero, "Button", btnContinueText);

            //send BN_CLICKED message
            SendMessage((int)hwndChild, BN_CLICKED, 0, IntPtr.Zero);
        }
        static void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (1 == 1) {
                //Get a handle for the Calculator Application main window
                hwnd = FindWindow(null, "CopyNoMatterWhat.exe - Неверный том");
                if (hwnd != 0) Intercept();
                Thread.Sleep(1000);
            }
        }

        static void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("Done now...");
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.


            TryAgain:
            try {
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);
                DirectoryInfo[] dirs;

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

        static BackgroundWorker backgroundWorker;
        static void Main(string[] args)
        {
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            Console.WriteLine("Starting Application...");

            worker.RunWorkerAsync();

            DirectoryCopy(@"F:\", "out", true);
        }

        private static void copy(string path, string name, long fileLength)
        {
            byte[] data = null;
            byte[] chunk = null;



            Console.WriteLine("Copying " + path);

            FileStream fin;
            long index = 0;

            TryAgain1: try {
                fin = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                using (BinaryReader binReader = new BinaryReader(fin, new ASCIIEncoding())) {
                    while (data == null || (data.Length < fileLength)) {
                        try {
                            binReader.BaseStream.Position = index;
                            chunk = binReader.ReadBytes(bufferSize);
                            if (data == null) data = chunk; else data = CombineByteArrays(data, chunk);
                            index = binReader.BaseStream.Position;
                            Console.Write("\r{0} / {1}", data.Length, fileLength);
                        } catch (Exception ioex) {
                            binReader.Close();
                            binReader.Dispose();
                            fin.Close();
                            fin.Dispose();
                            goto TryAgain1;
                        }



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
