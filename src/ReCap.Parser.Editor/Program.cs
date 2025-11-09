using System;
using Avalonia;

namespace ReCap.Parser.Editor
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                Console.Error.WriteLine(e.ExceptionObject?.ToString());

            try
            {
                return BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return -1;
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new Win32PlatformOptions
                {
                    RenderingMode = new[]
                    {
                        Win32RenderingMode.AngleEgl,
                        Win32RenderingMode.Software
                    },
                    CompositionMode = new[]
                    {
                        Win32CompositionMode.WinUIComposition,
                        Win32CompositionMode.RedirectionSurface
                    }
                })
                .LogToTrace();
    }
}
