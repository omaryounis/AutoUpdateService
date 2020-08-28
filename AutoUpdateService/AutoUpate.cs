using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static System.Net.Mime.MediaTypeNames;
using Timer = System.Timers.Timer;

namespace AutoUpdateService
{
    public partial class AutoUpate : ServiceBase
    {
        private static readonly log4net.ILog log =
        log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // Timer _timer = new Timer();
        public static string FTP_SERVER = "ftp://192.168.1.4/";
        public static string USERNAME_FTP_SERVER = "omar";
        public static string PASSWORD_FTP_SERVER = "123456789om";
        public static string SERVICE_NAME = "Service1";//"TcpGTService";//
        public static string EXECUTABLE_FILE_NAME = "MyFirstService.exe";//"TcpGTService.exe";// 

        System.Timers.Timer _timer;
        DateTime _scheduleTime;
        public AutoUpate()
        {
            InitializeComponent();
            _timer = new System.Timers.Timer();
            //  _scheduleTime = DateTime.Today.AddDays(1).AddHours(12); // Schedule to run once a day at 7:00 a.m.
        }

        protected override void OnStart(string[] args)
        {
            System.Diagnostics.Debugger.Launch();
            //_timer.Enabled = true;
            //_timer.Interval = _scheduleTime.Subtract(DateTime.Now).TotalSeconds * 1000;
            //_timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);


            log.Info("Service is started at " + DateTime.Now);
            _timer.Interval = 15 * 60 * 1000;
            _timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            _timer.Enabled = true;
            System.Threading.ThreadPool.QueueUserWorkItem(async (_) => await checkVersion());

        }
        private static async Task checkVersion()
        {
           log.Info("Service is started at beginning " + DateTime.Now);
            try
            {
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(FTP_SERVER);
                ftpRequest.Credentials = new NetworkCredential(USERNAME_FTP_SERVER, PASSWORD_FTP_SERVER);
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                List<string> directories = new List<string>();
                string line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    directories.Add(line);
                    line = streamReader.ReadLine();
                }
                streamReader.Close();


                using (WebClient ftpClient = new WebClient())
                {
                    ftpClient.Credentials = new System.Net.NetworkCredential(USERNAME_FTP_SERVER, PASSWORD_FTP_SERVER);

                    for (int i = 0; i <= directories.Count - 1; i++)
                    {
                        if (directories[i].Contains("."))
                        {

                            string path = FTP_SERVER + directories[i].ToString();
                            string trnsfrpth = @"D:\\ftp\" + directories[i].ToString();
                            ftpClient.DownloadFile(path, trnsfrpth);
                        }
                    }
                }
                try
                {
                    FileVersionInfo ServerVerionInfo = null;
                    string ServerFileVersion = string.Empty;
                    FileVersionInfo ClientVersionInfo = null;
                    string ClientfileVersion = string.Empty;
                    string ServiceePath = null;
                    ServerVerionInfo = FileVersionInfo.GetVersionInfo(@"D:\\ftp\" + EXECUTABLE_FILE_NAME);
                    ServerFileVersion = ServerVerionInfo.FileVersion;
                    var domaininfo = new AppDomainSetup();

                    RegistryKey services = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services");
                    if (services != null)
                    {
                        object pathtoexecutable = services.OpenSubKey(SERVICE_NAME).GetValue("ImagePath");
                        string path = pathtoexecutable.ToString();
                        ServiceePath = RemoveIllegalFilenameCharactersFrom(path);
                        ClientVersionInfo = FileVersionInfo.GetVersionInfo(ServiceePath);
                        ClientfileVersion = ClientVersionInfo.FileVersion;
                    }
                    if (ServerFileVersion != ClientfileVersion)
                    {
                        ServiceController service = new ServiceController(SERVICE_NAME);
                        if (service.Status.Equals(ServiceControllerStatus.Running))
                            log.Info("service will stop here to chnage it's version " + DateTime.Now);
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped);
                        log.Info("service will check");
                        var client = Path.GetDirectoryName(ServiceePath);
                        using (WebClient ftpClient = new WebClient())
                        {
                            ftpClient.Credentials = new System.Net.NetworkCredential(USERNAME_FTP_SERVER, PASSWORD_FTP_SERVER);

                            for (int i = 0; i <= directories.Count - 1; i++)
                            {
                                if (directories[i].Contains("."))
                                {

                                    string path = FTP_SERVER + directories[i].ToString();
                                    string trnsfrpth = client + @"\" + directories[i].ToString();
                                    ftpClient.DownloadFile(path, trnsfrpth);
                                }
                            }
                        }
                        if (service.Status.Equals(ServiceControllerStatus.Stopped))
                            service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running);
                        log.Info("service started after change version at " + DateTime.Now);
                    }
                    else
                    {
                        log.Info("Service is not updated it's the same version" + DateTime.Now);
                    }
                }
                catch (Exception ex)
                {

                    log.Error("Exeption " + ex + "\n" + ex.StackTrace);
                }

            }
            catch (Exception ex)
            {

                log.Error("Exeption " + ex + "\n" + ex.StackTrace);
            }
            // 2.If tick for the first time, reset next run to every 24 hours

        }
        private async static void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            await checkVersion();
            // 1. Process Schedule Task
            // ----------------------------------
            // Add code to Process your task here
            // ----------------------------------

            //WriteToFile("Service is started at beginning " + DateTime.Now);
            //try
            //{
            //    FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(FTP_SERVER);
            //    ftpRequest.Credentials = new NetworkCredential(USERNAME_FTP_SERVER, PASSWORD_FTP_SERVER);
            //    ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            //    FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
            //    StreamReader streamReader = new StreamReader(response.GetResponseStream());
            //    List<string> directories = new List<string>();
            //    string line = streamReader.ReadLine();
            //    while (!string.IsNullOrEmpty(line))
            //    {
            //        directories.Add(line);
            //        line = streamReader.ReadLine();
            //    }
            //    streamReader.Close();


            //    using (WebClient ftpClient = new WebClient())
            //    {
            //        ftpClient.Credentials = new System.Net.NetworkCredential(USERNAME_FTP_SERVER, PASSWORD_FTP_SERVER);

            //        for (int i = 0; i <= directories.Count - 1; i++)
            //        {
            //            if (directories[i].Contains("."))
            //            {

            //                string path = FTP_SERVER + directories[i].ToString();
            //                string trnsfrpth = @"D:\\ftp\" + directories[i].ToString();
            //                ftpClient.DownloadFile(path, trnsfrpth);
            //            }
            //        }
            //    }
            //    try
            //    {
            //        FileVersionInfo ServerVerionInfo = null;
            //        string ServerFileVersion = string.Empty;
            //        FileVersionInfo ClientVersionInfo = null;
            //        string ClientfileVersion = string.Empty;
            //        string ServiceePath = null;
            //        ServerVerionInfo = FileVersionInfo.GetVersionInfo(@"D:\\ftp\" + EXECUTABLE_FILE_NAME);
            //        ServerFileVersion = ServerVerionInfo.FileVersion;
            //        var domaininfo = new AppDomainSetup();

            //        RegistryKey services = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services");
            //        if (services != null)
            //        {
            //            object pathtoexecutable = services.OpenSubKey(SERVICE_NAME).GetValue("ImagePath");
            //            string path = pathtoexecutable.ToString();
            //            ServiceePath = RemoveIllegalFilenameCharactersFrom(path);
            //            ClientVersionInfo = FileVersionInfo.GetVersionInfo(ServiceePath);
            //            ClientfileVersion = ClientVersionInfo.FileVersion;
            //        }
            //        if (ServerFileVersion != ClientfileVersion)
            //        {
            //            ServiceController service = new ServiceController(SERVICE_NAME);
            //            if (service.Status.Equals(ServiceControllerStatus.Running))
            //                WriteToFile("service will stop here to chnage it's version " + DateTime.Now);
            //            service.Stop();
            //            service.WaitForStatus(ServiceControllerStatus.Stopped);
            //            WriteToFile("service will check");
            //            var client = Path.GetDirectoryName(ServiceePath);
            //            using (WebClient ftpClient = new WebClient())
            //            {
            //                ftpClient.Credentials = new System.Net.NetworkCredential(USERNAME_FTP_SERVER, PASSWORD_FTP_SERVER);

            //                for (int i = 0; i <= directories.Count - 1; i++)
            //                {
            //                    if (directories[i].Contains("."))
            //                    {

            //                        string path = FTP_SERVER + directories[i].ToString();
            //                        string trnsfrpth = client + @"\" + directories[i].ToString();
            //                        ftpClient.DownloadFile(path, trnsfrpth);
            //                    }
            //                }
            //            }
            //            if (service.Status.Equals(ServiceControllerStatus.Stopped))
            //                service.Start();
            //            service.WaitForStatus(ServiceControllerStatus.Running);
            //            WriteToFile("service started after change version at " + DateTime.Now);
            //        }
            //        else
            //        {
            //            WriteToFile("Service is not updated it's the same version" + DateTime.Now);
            //        }
            //    }
            //    catch (Exception ex)
            //    {

            //        WriteToFile("Exeption " + ex + "\n" + ex.StackTrace);
            //    }

            //}
            //catch (Exception ex)
            //{

            //    WriteToFile("Exeption " + ex + "\n" + ex.StackTrace);
            //}
            // 2.If tick for the first time, reset next run to every 24 hours

            //if (_timer.Interval != 12 * 60 * 60 * 1000)
            //{
            //    _timer.Interval = 12 * 60 * 60 * 1000;
            //}
        }

        public static string RemoveIllegalFilenameCharactersFrom(string unsafeString)
        {
            const string illegalCharactersClass = @"[\&\<\>\/|" + "\"" + @"\?\*]";

            string replaced = Regex.Replace(unsafeString, illegalCharactersClass, "");

            return replaced;
        }
        protected override void OnStop()
        {
            WriteToFile("Service is stopped at " + DateTime.Now);
        }
        //private void OnElapsedTime(object source, ElapsedEventArgs e)
        //{
        //    WriteToFile("Service is recall at " + DateTime.Now);
        //}
        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}
