using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using Newtonsoft.Json;
using WinSCP;

namespace AWSDownloaderUtility
{
    public class VideoTrimmer
    {
        private readonly object startInfo;

        public int startIndex { get; set; }
        public int endIndex { get; set; }

        public string outputPath { get; set; }
        public string inputPath { get; set; }
        List<ProcessedFile> data = new List<ProcessedFile>();
        private string errorfile = @"C:\Personal\uis\AWSDownloaderUtility\logs\errors.txt";
        string processedfile =  @"C:\Personal\uis\AWSDownloaderUtility\logs\processed.json";

        public VideoTrimmer(int start, int end, string inputfolder, string outputFolder)
        {
            startIndex = start;
            endIndex = end;
            inputPath = inputfolder;
            outputPath = outputFolder;
            JsonSerializer serializer = new JsonSerializer();

            using (StreamReader r = new StreamReader(processedfile))
            {
                string json = r.ReadToEnd();
                data = JsonConvert.DeserializeObject<List<ProcessedFile>>(json);
            }
        }

        public void Trim(string input, string output, int startSecond, int endSecond = 15)
        {
            if (startSecond < 0)
                startSecond = 0;
            if(data == null) data = new List<ProcessedFile>();
            if (data.Any(p => p.Name == output)) return;

            string newFilePath = Path.Combine(outputPath, output);
            if (File.Exists(newFilePath))
            {
                newFilePath = newFilePath + "new";
            }
            var inputFile = new MediaFile { Filename = Path.Combine(inputPath, input) };
            var outputFile = new MediaFile { Filename = newFilePath };
            
            if (!File.Exists(Path.Combine(inputPath, input)))
            {
                using (StreamWriter r = new StreamWriter(errorfile))
                {
                    r.WriteLine($"{System.DateTime.Now}, File not found: {Path.Combine(inputPath, input)} ");
                }
            }
            try
            {
                using (var engine = new Engine())
                {
                    engine.GetMetadata(inputFile);

                    var options = new ConversionOptions();
                    options.CutMedia(TimeSpan.FromSeconds(startSecond), TimeSpan.FromSeconds(15));
                    engine.Convert(inputFile, outputFile, options);
                    
                    data.Add(new ProcessedFile()
                    {
                        Name = output,
                        Folder = inputPath
                    });
                    
                    using (StreamWriter r = new StreamWriter(processedfile))
                    {
                        string obj = JsonConvert.SerializeObject(data);
                        r.Write(obj);
                    }
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter r = new StreamWriter(errorfile))
                {
                    r.WriteLine($"{System.DateTime.Now},{ex.Message}, {output} ");
                }
            }
           
        }
    }

    public class ProcessedFile
    {
        public string Id { get; set; }
        public string Folder { get; set; }
        public string Name { get; set; }
    }
}
