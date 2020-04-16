using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using ElectricityArchiveWebApp.Logger;
using ElectricityArchiveWebApp.Services.IServices;
using Microsoft.Extensions.Logging;

namespace ElectricityArchiveWebApp.Services
{
    public class DataService : IDataService
    {
        private readonly NetworkCredential credentials = new NetworkCredential("admin", "wago");
        private const string url = "ftp://192.168.100.50/PLC/Archive/";
        private const string filePath = @"C:\PLC\Archive";

        public DataService(ILoggerFactory loggerFactory)
        {
            var loggerPath = Path.Combine(Directory.GetCurrentDirectory(), "Log");
            if (!Directory.Exists(loggerPath))
            {
                Directory.CreateDirectory(loggerPath);
            }

            loggerFactory.AddFile(Path.Combine(loggerPath, "Logger.txt"));
            var logger = loggerFactory.CreateLogger("DataServiceLogger");

            Task.Run(async () =>
            {
                logger.LogInformation("------------Start server!------------");

                while (true)
                {
                    try
                    {
                        DownloadFtpDirectory(url, credentials, filePath);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"{ex.Message} {url}");
                    }
                    finally
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1));
                    }
                }
            });
        }

        void DownloadFtpDirectory(string url, NetworkCredential credentials, string localPath)
        {
            var listRequest = (FtpWebRequest) WebRequest.Create(url);
            listRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            listRequest.Credentials = credentials;

            var lines = new List<string>();

            using (var listResponse = (FtpWebResponse) listRequest.GetResponse())
            using (var listStream = listResponse.GetResponseStream())
            using (var listReader = new StreamReader(listStream))
            {
                while (!listReader.EndOfStream)
                {
                    lines.Add(listReader.ReadLine());
                }
            }

            foreach (var line in lines)
            {
                var tokens =
                    line.Split(new[] {' '}, 9, StringSplitOptions.RemoveEmptyEntries);
                var name = tokens[8];
                var permissions = tokens[0];
                var size = Convert.ToInt64(tokens[4]);

                var localFilePath = Path.Combine(localPath, name);
                var fileUrl = url + name;

                if (permissions[0] == 'd')
                {
                    if (!Directory.Exists(localFilePath))
                    {
                        Directory.CreateDirectory(localFilePath);
                    }

                    DownloadFtpDirectory(fileUrl + "/", credentials, localFilePath);
                }
                else
                {
                    //Проверка существования файла
                    var fileExists = File.Exists(localFilePath);
                    //Проверка размера файла
                    long fileSize = 0;
                    if (fileExists)
                    {
                        fileSize = new FileInfo(localFilePath).Length;
                    }
                    //Если файл не существует или
                    //размер файла в ПЛК не совпадает с размером локального файла
                    if (!fileExists || size != fileSize)
                    {
                        var downloadRequest = (FtpWebRequest) WebRequest.Create(fileUrl);
                        downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                        downloadRequest.Credentials = credentials;

                        using (var downloadResponse = (FtpWebResponse) downloadRequest.GetResponse())
                        using (var sourceStream = downloadResponse.GetResponseStream())
                        using (Stream targetStream = File.Create(localFilePath))
                        {
                            var buffer = new byte[10240];
                            int read;
                            while ((read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                targetStream.Write(buffer, 0, read);
                            }
                        }
                    }
                }
            }
        }

        public void ProcessDirectory(string targetDirectory) 
        {
            // Process the list of files found in the directory.
            var fileEntries = Directory.GetFiles(targetDirectory);
            foreach(var fileName in fileEntries)
                ProcessFile(fileName);

            // Recurse into subdirectories of this directory.
            var subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach(var subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }
        
        // Insert logic for processing found files here.
        public void ProcessFile(string path) 
        {
            Console.WriteLine("Processed file '{0}'.", path);
        }
    }
}
