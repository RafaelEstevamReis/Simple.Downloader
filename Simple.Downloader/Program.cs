using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Simple.Downloader
{
    class Program
    {
        static WebClient wc;
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: Supply one or more list of files");
                return;
            }

            Console.CursorVisible = false;
            wc = new WebClient();
            wc.DownloadProgressChanged += wc_DownloadProgressChanged;
            wc.DownloadFileCompleted += wc_DownloadFileCompleted;
            foreach (var item in args)
            {
                processFile(item);
            }
        }


        private static void processFile(string item)
        {
            Console.WriteLine($"Processing file: {item}");

            var lines = File.ReadAllLines(item);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length == 0) continue;

                try
                {
                    var uri = new Uri(lines[i]);

                    processUri(i, lines.Length, uri);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing line {i}: {lines[i]}");
                    Console.WriteLine($"{ex.Message}");
                    Console.WriteLine($"{ex.InnerException}");
                    Console.WriteLine("---------------------------------");
                }
            }

        }

        private static void processUri(int i, int length, Uri uri)
        {
            Console.WriteLine($"Item {i + 1}/{length}: {uri}");

            var fileName = $"item_{i + 1}of{length}_{DateTime.Now:yyyyMMddHHmmss}";
            if (uri.Segments.Length > 0)
            {
                if (uri.Segments[^1].Length > 0) fileName = uri.Segments[^1];
            }
            if (File.Exists(fileName))
            {
                Console.WriteLine("  > Skipped");
                return;
            }

            Console.Write($">             0%");

            downloading = true;
            percent = -1;

            wc.DownloadFileAsync(uri, fileName + ".tmp");
            while (downloading)
            {
                Thread.Sleep(100);

                Console.CursorLeft = 1;
                for (int p = 0; p < 10; p++)
                {
                    if (p * 10 < percent) Console.Write('#');
                    else Console.Write('_');
                }
                Console.Write($" {percent}% {kbytesReceived / 1024.0:N1} MB/{kbytesTotal / 1024:N1} MB       ");
            }

            File.Move(fileName + ".tmp", fileName);
            Console.WriteLine();
        }
        static bool downloading;
        static int percent;
        static int kbytesReceived;
        static int kbytesTotal;

        private static void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            percent = e.ProgressPercentage;
            kbytesReceived = (int)(e.BytesReceived / 1024);
            kbytesTotal = (int)(e.TotalBytesToReceive / 1024);
        }
        private static void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            downloading = false;
        }

    }
}
