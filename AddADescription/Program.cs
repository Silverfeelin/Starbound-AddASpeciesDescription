using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddADescription
{
    class Program
    {

        delegate void FileCallback(FileInfo file);
        static JArray result;
        static string basePath;
        static string species;
        static DirectoryInfo diOutput;
        static JArray patch;
        static int count = 0;

        ///string[] extensions = ".activeitem,.object,.codex,.head,.chest,.legs,.back,.augment,.coinitem,.item,.consumable,.unlock,.instrument,.liqitem,.matitem,.thrownitem,.harvestingtool,.flashlight,.grapplinghook,.painttool,.wiretool,.beamaxe,.tillingtool,.miningtool,.techitem".Split(',');
        static string[] extensions = ".object".Split(',');

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Expected args: <asset_folder> <output_file>");
                return;
            }

            patch = JArray.Parse("[{'op':'add','path':'/testDescription','value':''}]");
            Console.WriteLine("Busy. Depending on when the files in this folder were last indexed, this may go really fast or really slow.");

            basePath = args[0];
            string outputPath = args[2];
            species = args[1];

            patch[0]["path"] = "/" + species + "Description";

            if (basePath.LastIndexOf("\\") == basePath.Length - 1)
                basePath = basePath.Substring(0, basePath.Length - 1);

            if (outputPath.LastIndexOf("\\") == outputPath.Length - 1)
                outputPath = outputPath.Substring(0, outputPath.Length - 1);
            diOutput = new DirectoryInfo(outputPath);

            List<string> subPaths = new List<string>() { "items", "objects" };
            DirectoryInfo diPath = new DirectoryInfo(basePath);
            DirectoryInfo[] diSubs = diPath.GetDirectories().Where(d => subPaths.Contains(d.Name)).ToArray();
            
            FileCallback fc = new FileCallback(CreatePatch);

            foreach (DirectoryInfo di in diSubs)
            {
                ScanDirectories(di.FullName, extensions, fc);
            }

            Console.WriteLine("Done creating patches!\nPress any key to exit...");
            Console.ReadKey();
        }

        static void CreatePatch(FileInfo file)
        {
            count++;
            if (count % 50 == 1)
                Console.WriteLine("Scanned {0} files.", count);

            string relativeFilePath = file.FullName.Replace(basePath, "").Replace("\\", "/");
            string relativeFolderPath = (Path.GetDirectoryName(file.FullName)).Replace(basePath, "").Replace("\\", "/");

            JObject item = null;
            try
            {
                item = JObject.Parse(File.ReadAllText(file.FullName));
            }
            catch (Exception exc)
            {
                Console.WriteLine("Skipped '" + file.FullName + "', as it could not be parsed as a valid JSON file.");
                return;
            }

            JToken tDesc = item.SelectToken("humanDescription");
            if (tDesc != null && tDesc.Type == JTokenType.String)
            {
                string desc = tDesc.Value<string>();
                JToken obj = patch.DeepClone();
                obj[0]["value"] = desc;
                Directory.CreateDirectory(diOutput.FullName + relativeFolderPath);
                File.WriteAllText(diOutput.FullName + relativeFilePath + ".patch", obj.ToString(Newtonsoft.Json.Formatting.Indented));
            }
            else
            {
                Console.WriteLine("Skipped '" + file.FullName + "', as it does not have a `humanDescription` set.");
                return;
            }
        }

        static void ScanDirectories(string basePath, string[] extensions, FileCallback callback)
        {
            foreach (var file in Directory.EnumerateFiles(basePath, "*", SearchOption.AllDirectories))
            {
                FileInfo fi = new FileInfo(file);
                if (extensions.Contains(fi.Extension.ToLower()))
                    callback(fi);
            }
        }
    }
}
