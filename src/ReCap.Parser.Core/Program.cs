using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ReCap.Parser.Core.Core;

internal static class Program
{
    private sealed class Options
    {
        public string InputPath { get; set; } = "";
        public bool Xml { get; set; }
        public bool Yaml { get; set; }
        public bool Silent { get; set; }
        public bool Debug { get; set; }
        public string OutputDir { get; set; } = "";
        public bool Log { get; set; }
        public bool SortExt { get; set; }
        public string GameVersion { get; set; } = "5.3.0.103";
        public bool Recursive { get; set; }
        public string RecursiveFilter { get; set; } = "";
    }

    private sealed class TeeTextWriter : TextWriter
    {
        private readonly TextWriter _a;
        private readonly TextWriter _b;
        public TeeTextWriter(TextWriter a, TextWriter b) { _a = a; _b = b; }
        public override Encoding Encoding => _a.Encoding;
        public override void Write(char value) { _a.Write(value); _b.Write(value); }
        public override void Write(string? value) { _a.Write(value); _b.Write(value); }
        public override void WriteLine(string? value) { _a.WriteLine(value); _b.WriteLine(value); }
        public override void Flush() { _a.Flush(); _b.Flush(); }
    }

    private static int Main(string[] args)
    {
        var opt = ParseArgs(args);
        if (opt is null) return 1;
        if (string.IsNullOrWhiteSpace(opt.InputPath))
        {
            Console.Error.WriteLine("Error: missing input path");
            return 1;
        }
        if (!File.Exists(opt.InputPath) && !Directory.Exists(opt.InputPath))
        {
            Console.Error.WriteLine($"Error: path not found: {opt.InputPath}");
            return 1;
        }
        if (opt.Xml && opt.Yaml)
        {
            Console.Error.WriteLine("Error: --xml and --yaml are mutually exclusive");
            return 1;
        }

        var exportFormat = "none";
        if (opt.Xml) exportFormat = "xml";
        else if (opt.Yaml) exportFormat = "yaml";

        StreamWriter? logFile = null;
        TextWriter? origOut = null;
        TextWriter? origErr = null;

        try
        {
            if (opt.Log)
            {
                logFile = new StreamWriter("recap_parser.log", false);
                logFile.AutoFlush = true;
                origOut = Console.Out;
                origErr = Console.Error;
                Console.SetOut(new TeeTextWriter(origOut, logFile));
                Console.SetError(new TeeTextWriter(origErr, logFile));
            }

            var failed = new List<string>();
            var exts = ParseExtFilter(opt.RecursiveFilter);

            void ProcessOne(string file)
            {
                var catalog = new Catalog();
                catalog.SetGameVersion(opt.GameVersion);
                try
                {
                    var parser = new Parser(catalog, file, opt.Silent, opt.Debug, exportFormat);
                    if (!parser.Parse())
                    {
                        failed.Add(file);
                        return;
                    }
                    if (!string.Equals(exportFormat, "none", StringComparison.OrdinalIgnoreCase))
                    {
                        var baseOut = string.IsNullOrWhiteSpace(opt.OutputDir) ? Path.GetDirectoryName(file)! : opt.OutputDir;
                        var targetDir = baseOut;
                        if (opt.SortExt)
                        {
                            var e = Path.GetExtension(file).TrimStart('.').ToLowerInvariant();
                            if (string.IsNullOrEmpty(e)) e = "unknown";
                            targetDir = Path.Combine(baseOut, e);
                        }
                        EnsureDir(targetDir);
                        var name = Path.GetFileNameWithoutExtension(file);
                        var outName = exportFormat.Equals("xml", StringComparison.OrdinalIgnoreCase) ? name + ".xml" : name + ".yaml";
                        var outPath = Path.Combine(targetDir, outName);
                        parser.ExportToFile(outPath);
                    }
                }
                catch (Exception e)
                {
                    failed.Add(file);
                    if (!opt.Silent) Console.Error.WriteLine($"Parse failed: {file} : {e.Message}");
                }
            }

            if (File.Exists(opt.InputPath))
            {
                ProcessOne(opt.InputPath);
            }
            else
            {
                if (opt.Recursive)
                {
                    foreach (var file in Directory.EnumerateFiles(opt.InputPath, "*", new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true, AttributesToSkip = 0 }))
                    {
                        if (exts.Count > 0 && !HasAnyExtension(file, exts)) continue;
                        ProcessOne(file);
                    }
                }
                else
                {
                    foreach (var file in Directory.EnumerateFiles(opt.InputPath))
                    {
                        ProcessOne(file);
                    }
                }
            }

            return failed.Count == 0 ? 0 : 1;
        }
        finally
        {
            if (logFile is not null)
            {
                Console.Out.Flush();
                Console.Error.Flush();
                Console.SetOut(origOut ?? Console.Out);
                Console.SetError(origErr ?? Console.Error);
                logFile.Dispose();
            }
        }
    }

    private static Options? ParseArgs(string[] args)
    {
        if (args.Length == 0) return null;

        var opt = new Options();
        var i = 0;
        opt.InputPath = args[i++];
        while (i < args.Length)
        {
            var a = args[i++];
            if (a.Equals("--xml", StringComparison.OrdinalIgnoreCase)) opt.Xml = true;
            else if (a.Equals("--yaml", StringComparison.OrdinalIgnoreCase) || a.Equals("--yml", StringComparison.OrdinalIgnoreCase) || a.Equals("-y", StringComparison.OrdinalIgnoreCase)) opt.Yaml = true;
            else if (a.Equals("--silent", StringComparison.OrdinalIgnoreCase)) opt.Silent = true;
            else if (a.Equals("--debug", StringComparison.OrdinalIgnoreCase) || a.Equals("-d", StringComparison.OrdinalIgnoreCase)) opt.Debug = true;
            else if (a.Equals("--log", StringComparison.OrdinalIgnoreCase) || a.Equals("-l", StringComparison.OrdinalIgnoreCase)) opt.Log = true;
            else if (a.Equals("--sort-ext", StringComparison.OrdinalIgnoreCase) || a.Equals("-s", StringComparison.OrdinalIgnoreCase)) opt.SortExt = true;
            else if (a.Equals("--recursive", StringComparison.OrdinalIgnoreCase) || a.Equals("-r", StringComparison.OrdinalIgnoreCase))
            {
                opt.Recursive = true;
                if (i < args.Length && !args[i].StartsWith("-", StringComparison.Ordinal))
                {
                    opt.RecursiveFilter = args[i++];
                }
            }
            else if (a.Equals("--output", StringComparison.OrdinalIgnoreCase) || a.Equals("-o", StringComparison.OrdinalIgnoreCase))
            {
                if (i >= args.Length) return null;
                opt.OutputDir = args[i++];
            }
            else if (a.Equals("--game-version", StringComparison.OrdinalIgnoreCase) || a.Equals("--gv", StringComparison.OrdinalIgnoreCase))
            {
                if (i >= args.Length) return null;
                opt.GameVersion = args[i++];
            }
            else
            {
                return null;
            }
        }
        return opt;
    }

    private static HashSet<string> ParseExtFilter(string filter)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(filter)) return set;
        var tmp = new List<char>();
        foreach (var c in filter)
        {
            if (c == ',' || c == ';' || char.IsWhiteSpace(c))
            {
                if (tmp.Count > 0)
                {
                    var s = new string(tmp.ToArray());
                    s = s.Trim().TrimStart('.');
                    if (s.Length > 0) set.Add(s);
                    tmp.Clear();
                }
            }
            else if (c == '.')
            {
            }
            else
            {
                tmp.Add(c);
            }
        }
        if (tmp.Count > 0)
        {
            var s = new string(tmp.ToArray());
            s = s.Trim().TrimStart('.');
            if (s.Length > 0) set.Add(s);
        }
        return set;
    }

    private static bool HasAnyExtension(string file, HashSet<string> exts)
    {
        if (exts.Count == 0) return true;
        var e = Path.GetExtension(file).TrimStart('.').ToLowerInvariant();
        return exts.Contains(e);
    }

    private static void EnsureDir(string path)
    {
        Directory.CreateDirectory(path);
    }
}
