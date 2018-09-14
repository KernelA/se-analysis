namespace SEA
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Data;

    using NLog;

    using CommandLine;
    using CommandLine.Text;



    class Options
    {
        private static readonly Example[] _examples = new Example[2]
        {
            new Example("Example", new UnParserSettings() { UseEqualToken = true }, new Options { InputDir = "Path_to_input_dir"}),
            new Example("Constrain size of files", new Options { InputDir = "Path_to_input_dir", MaxFilesSize = 512 })
        };

        [Value(0, MetaName = "Input dir.", HelpText = "A path to a directory which contains all directories of sections Stack Exchange.", Required = true)]
        public string InputDir { get; set; }

        [Option('s', "size", HelpText = "A maximum size of all files for loading to RAM (in megabytes) in the directory. If files have a size is greater than maximum then it is skipped.", Default = 1024)]
        public long MaxFilesSize { get; set; }

        [Usage]
        public static IEnumerable<Example> Examples => _examples;
    }


    internal class Program
    {
        private static readonly string[] _acceptedNames =
        {
            "Posts",
            "Users",
            "Tags",
            "Comments"
        };

        private static Logger _log = LogManager.GetLogger("Main");

        private static DataSet LoadDataSet(IEnumerable<FileInfo> Files, string DataSetName)
        {
            DataSet set = SETables.CreateDataSet(DataSetName);

            _log.Info("Filling:");

            foreach (FileInfo file in Files)
            {
                string tableName = Path.GetFileNameWithoutExtension(file.Name);

                _log.Info($"\t{tableName}");

                Filler filler = Filler.GetFillerByTableName(tableName);
                DataTable table = set.Tables[tableName];
                filler.FillFromXml(file.FullName, table);
            }

            return set;
        }

        private static int Run(Options Options)
        {
            long ConvertMBToBytes(long Megabytes)
            {
                return Megabytes * 1024 * 1024;
            }

            string pathToDir = Options.InputDir;

            long maxFilesSizeInBytes = ConvertMBToBytes(Options.MaxFilesSize);

            if (!Directory.Exists(pathToDir))
            {
                Console.WriteLine($"\n\tThe directory '{pathToDir}' does not exist.");
                return 1;
            }

            string resDir = "Stat";

            if(Directory.Exists(resDir))
            {
                bool isEmptyDir = Directory.EnumerateFileSystemEntries(resDir).Any();

                char answer = 'n';

                if (isEmptyDir)
                {
                    Console.WriteLine($"The directory '{Path.Combine(".", resDir)}' is not empty. Need to clear '{resDir}'.\nDo you want to delete all subdirectories and files?");

                    do
                    {
                        Console.WriteLine("\nEnter 'Y/y' or 'N/n'.");

                        answer = Console.ReadKey().KeyChar;

                        answer = char.ToLowerInvariant(answer);

                    } while (answer != 'y' && answer != 'n');

                    Console.WriteLine("\n");

                    if (answer == 'n')
                    {
                        Console.WriteLine($"The program need to clear '{resDir}' for further work. Save all files from '{resDir}' and rerun program.");
                        return 0;
                    }
                    else if (answer == 'y')
                    {
                        Directory.Delete(resDir, true);
                        Directory.CreateDirectory(resDir);
                    }
                }          
            }
            else
            {
                Directory.CreateDirectory(resDir);
            }

            DirectoryInfo datasetsDir = new DirectoryInfo(pathToDir);

            foreach (DirectoryInfo datasetDir in datasetsDir.EnumerateDirectories())
            {
                _log.Info($"Filling tables from '{datasetDir.Name}'.");

                FileInfo[] files = datasetDir.EnumerateFiles("*.xml").Where(file => _acceptedNames.Contains(Path.GetFileNameWithoutExtension(file.Name.ToLower()))).ToArray();

                long totalSize = files.Select(file => file.Length).Sum();

                if (totalSize > maxFilesSizeInBytes)
                {
                    _log.Info("\tExceeded maximum size of files.\n\tThe directory skipped.");
                    break;
                }
                else
                {
                    DataSet dataSet = LoadDataSet(files, datasetDir.Name);
                    Stat.CollectStat(dataSet, resDir);
                }
            }

            _log.Info($"\nAll files are saved in '{Path.Combine(".", resDir)}'.");

            return 0;
        }

        private static int Main(string[] args)
        {
            ParserResult<Options> result = Parser.Default.ParseArguments<Options>(args);

            return result.MapResult(options => Run(options), _ => 1);
        }
    }
}