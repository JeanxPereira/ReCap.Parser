using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ReCap.Parser.Editor.Services
{
    public static class CoreXmlBridge
    {
        public static async Task<XDocument?> ParseToXmlAsync(string inputPath)
        {
            var exe = FindCoreExe();
            if (exe == null) throw new InvalidOperationException("ReCap.Parser.Core.exe não encontrado.");

            var temp = Path.Combine(Path.GetTempPath(), "recap_editor_xml");
            Directory.CreateDirectory(temp);
            var outFile = Path.Combine(temp, Path.GetFileNameWithoutExtension(inputPath) + ".xml");

            var psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = $"\"{inputPath}\" --xml -o \"{temp}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var p = Process.Start(psi);
            if (p == null) throw new InvalidOperationException("Falha ao iniciar o Core CLI.");
            await p.WaitForExitAsync();

            if (p.ExitCode != 0)
            {
                var err = await p.StandardError.ReadToEndAsync();
                throw new InvalidOperationException("Core retornou erro: " + err);
            }

            if (!File.Exists(outFile)) throw new FileNotFoundException("XML não gerado pelo Core.", outFile);

            using var fs = File.OpenRead(outFile);
            return XDocument.Load(fs);
        }

        static string? FindCoreExe()
        {
            var baseDir = AppContext.BaseDirectory;
            var direct = Path.Combine(baseDir, "ReCap.Parser.Core.exe");
            if (File.Exists(direct)) return direct;

            var root = FindSolutionRoot(baseDir);
            if (root != null)
            {
                var cand = new[]
                {
                    Path.Combine(root, "src","ReCap.Parser.Core","bin","Debug","net9.0","ReCap.Parser.Core.exe"),
                    Path.Combine(root, "src","ReCap.Parser.Core","bin","Release","net9.0","ReCap.Parser.Core.exe"),
                };
                foreach (var c in cand) if (File.Exists(c)) return c;
            }

            return null;
        }

        static string? FindSolutionRoot(string start)
        {
            var d = new DirectoryInfo(start);
            while (d != null)
            {
                if (d.GetFiles("*.sln").Any()) return d.FullName;
                d = d.Parent!;
            }
            return null;
        }
    }
}
