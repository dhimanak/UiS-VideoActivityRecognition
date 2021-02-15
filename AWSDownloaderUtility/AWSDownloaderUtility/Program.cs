using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WinSCP;

namespace AWSDownloaderUtility
{
    class Program
    {
        static int Main(string[] args)
        {
            var data_files = LoadJson();
            foreach (var f in data_files)
            {
                if (f.VideoFiles != null && f.VideoFolder.Equals("CMC"))
                {
                    if(f.SessionID == "400")
                        Console.WriteLine("Test");
                    string[] annotations = f.Annotations.Split(',');
                    string[] videofiles = f.VideoFiles.Split(',');
                    int count = 1;
                    foreach (var annotation in annotations)
                    {
                        var resultString = Regex.Match(annotation, @"\d+").Value;
                        if (string.IsNullOrEmpty(resultString)) continue;
                        var startSec = Convert.ToInt16(resultString);
                        VideoTrimmer trimmer = new VideoTrimmer(0, 0, @"C:\Personal\dataset\" + f.VideoFolder,
                            @"C:\Personal\uis\AWSDownloaderUtility\" + f.VideoFolder);

                        foreach (var videofile in videofiles)
                        {
                            string[] parts = videofile.Split('.');
                            trimmer.Trim(videofile, $"{parts[0]}_{count}.{parts[1]}", startSec - 5, startSec + 5);

                        }
                        count++;
                    }
                }


                Console.WriteLine($"SessionID: {f.SessionID}");

            }

            //try
            //{

            //    // Example #2
            //    // Read each line of the file into a string array. Each element
            //    // of the array is one line of the file.
            //    string[] lines = System.IO.File.ReadAllLines(@"C:\Personal\dataset\downloaded.txt");


            //    var data_files = LoadJson();
            //    // Setup session options
            //    SessionOptions sessionOptions = new SessionOptions
            //    {
            //        Protocol = Protocol.Sftp,
            //        HostName = "s-8ae24b9fe6e44bb18.server.transfer.us-east-1.amazonaws.com",
            //        UserName = "uis",
            //        SshPrivateKeyPath = Environment.CurrentDirectory + "/Key/anil_uis.ppk",
            //        SshHostKeyFingerprint = "ssh-rsa 2048 x4apKkpy1vvjENR50ervPmCX2uMLG+wxzNgUkO/Pyi0="
            //    };
            //    int count = 0;
            //    using (Session session = new Session())
            //    {
            //        // Connect
            //        session.Open(sessionOptions);

            //        // Download files
            //        TransferOptions transferOptions = new TransferOptions();
            //        transferOptions.TransferMode = TransferMode.Automatic;

            //        foreach (var data_file in data_files)
            //        {
            //            if (data_file.VideoFiles != null && data_file.VideoFolder.Equals("CMC"))
            //            {
            //                var filepaths = data_file.VideoFiles.Split(',');
            //                var dest = Path.Combine("C:\\Personal\\dataset\\", data_file.VideoFolder);
            //                foreach (var filepath in filepaths)
            //                {
            //                    count++;
            //                    var path = data_file.VideoFolder + "/" + filepath;
            //                    //if (File.Exists(Path.Combine(dest, filepath))) continue;
            //                    if (lines.Any(p => p.EndsWith(filepath))) continue;
            //                    TransferOperationResult transferResult;
            //                    transferResult = session.GetFiles(path, dest + @"\", false, transferOptions);
            //                    transferResult.Check();

            //                    // This text is always added, making the file longer over time
            //                    // if it is not deleted.
            //                    using (StreamWriter sw = File.AppendText(@"C:\Personal\dataset\downloaded.txt"))
            //                    {
            //                        sw.WriteLine(filepath);

            //                    }


            //                    foreach (TransferEventArgs transfer in transferResult.Transfers)
            //                    {
            //                        Console.WriteLine("Download of {0} succeeded", transfer.FileName);
            //                    }
            //                }
            //            }

            //        };

            //    }
            //    Console.WriteLine($"Count:{count}");
            //    Console.WriteLine("Finished !!");
            //    Console.ReadLine();
            //    return 0;
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("Error: {0}", e);
            //    return 1;
            //}
            return 0;
        }

        public static IEnumerable<Sheet1> LoadJson()
        {
            using (StreamReader r = new StreamReader(Environment.CurrentDirectory + "/Files/reference.json"))
            {
                string json = r.ReadToEnd();
                Rootobject root = JsonConvert.DeserializeObject<Rootobject>(json);
                return root.Sheet1;
            }
        }

        public class Rootobject
        {
            public Sheet1[] Sheet1 { get; set; }
        }

        public class Sheet1
        {
            public string SessionID { get; set; }
            public string VideoFolder { get; set; }
            public string VideoFiles { get; set; }
            public string Annotations { get; set; }
        }

    }
}
