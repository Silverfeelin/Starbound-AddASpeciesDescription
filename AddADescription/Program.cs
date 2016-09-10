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
        static string outputPath;
        static string species;
        static bool skipMissing;

        static bool overwriteAll = false;

        static DirectoryInfo diOutput;
        static JArray patch;
        static int count = 0;

        ///string[] extensions = ".activeitem,.object,.codex,.head,.chest,.legs,.back,.augment,.coinitem,.item,.consumable,.unlock,.instrument,.liqitem,.matitem,.thrownitem,.harvestingtool,.flashlight,.grapplinghook,.painttool,.wiretool,.beamaxe,.tillingtool,.miningtool,.techitem".Split(',');
        static string[] extensions = ".object,.material".Split(',');
        static string patchString = "[{'op':'add','path':'/testDescription','value':''}]";

        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Invalid arguments.\nExpected: AddADescription.exe <asset_folder> <species_name> <output_folder> <skip_missing(true/false)>");
                Setup();
            }
            else
            {
                basePath = args[0];
                outputPath = args[2];
                species = args[1];
                skipMissing = args[3].ToLower() == "true" ? true : false;

                bool baseExists = Directory.Exists(basePath);
                bool outputExists = Directory.Exists(outputPath);
                if (!baseExists || !outputExists)
                {
                    string err = baseExists ? outputPath : outputExists ? basePath : basePath + " and " + outputPath;
                    Console.WriteLine("Invalid path(s): {0}. Starting regular setup.", err);
                    Setup();
                }
            }

            Console.WriteLine("Busy. Depending on when the files in this folder were last indexed, this may go really fast or really slow.");

            patch = JArray.Parse(patchString);
            patch[0]["path"] = string.Format("/{0}Description", species);

            FixPath(basePath);
            FixPath(outputPath);

            diOutput = new DirectoryInfo(outputPath);

            List<string> subPaths = new List<string>() { "objects", "tiles" };
            DirectoryInfo diPath = new DirectoryInfo(basePath);
            DirectoryInfo[] diSubs = diPath.GetDirectories().Where(d => subPaths.Contains(d.Name)).ToArray();
            
            FileCallback fc = new FileCallback(CreatePatch);

            foreach (DirectoryInfo di in diSubs)
                ScanDirectories(di.FullName, extensions, fc);

            Console.WriteLine("Done creating patches!\nPress any key to exit...");
            Console.ReadKey();
        }

        static void Setup()
        {
            // Asset path
            Console.WriteLine("Enter unpacked asset path:");
            while (true)
            {
                string path = FixPath(Console.ReadLine());
                if (Directory.Exists(path))
                {
                    basePath = path;
                    break;
                }
                else
                    Console.WriteLine("The given path is invalid. Please enter a correct path to unpacked game assets.");
            }

            // Species
            Console.WriteLine("Enter species name:");
            species = Console.ReadLine();

            // Output path
            Console.WriteLine("Enter output path (it is recommended to use an empty directory):");
            while (true)
            {
                string path = FixPath(Console.ReadLine());
                if (Directory.Exists(path))
                {
                    outputPath = path;
                    break;
                }
                else
                    Console.WriteLine("The given path is invalid. Please enter a correct path to create patches in.");
            }

            // Skip files that lack a `humanDescription`.
            Console.WriteLine("Skip objects with missing `humanDescription` value? If true, `description` will be used instead. y\\n");
            skipMissing = Console.ReadKey().Key == ConsoleKey.Y;
        }

        static string FixPath(string path)
        {
            path = path.Replace("/", "\\");
            if (path.LastIndexOf("\\") == path.Length - 1)
                path = path.Substring(0, path.Length - 1);
            return path;
        }

        static void CreatePatch(FileInfo file)
        {
            count++;
            if ((count + 1) % 50 == 1)
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
            if (tDesc == null || tDesc.Type != JTokenType.String)
            {
                Console.Write("File '" + file.FullName + "' does not have a `humanDescription` set.");

                if (!skipMissing)
                {
                    Console.WriteLine(" Using `description`.");
                    tDesc = item.SelectToken("description");
                    if (tDesc == null || tDesc.Type != JTokenType.String)
                    {
                        Console.WriteLine(" No description found. Skipped.");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine(" Skipped.");
                    return;
                }
            }

            if (tDesc != null && tDesc.Type == JTokenType.String)
            {
                string desc = tDesc.Value<string>();
                JToken obj = patch.DeepClone();
                obj[0]["value"] = desc;
                Directory.CreateDirectory(diOutput.FullName + relativeFolderPath);

                string filePath = diOutput.FullName + relativeFilePath + ".patch";
                if (ConfirmOverwrite(filePath))
                    File.WriteAllText(filePath, obj.ToString(Newtonsoft.Json.Formatting.Indented));
            }
        }

        static bool ConfirmOverwrite(string path)
        {
            if (!overwriteAll && File.Exists(path))
            {
                Console.WriteLine("The file {0} already exists. Do you want to overwrite it? y\\n\\(a)ll", path);
                ConsoleKey key = Console.ReadKey().Key;
                if (key == ConsoleKey.A)
                {
                    overwriteAll = true;
                    return true;
                }
                else
                    return key == ConsoleKey.Y;
            }
            else
                return true;
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
