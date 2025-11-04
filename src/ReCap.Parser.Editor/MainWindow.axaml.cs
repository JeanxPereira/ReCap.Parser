// src/ReCap.Parser.Editor/MainWindow.axaml.cs
using System;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.VisualTree;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Controls.Generators;
using ReCap.Parser.Editor.Services;
using ReCap.Parser.Editor.ViewModels;
using ReCap.Parser.Editor.Models;

namespace ReCap.Parser.Editor
{
    public partial class MainWindow : Window
    {
        public static readonly StyledProperty<GridLength> KeyColWidthProperty =
            AvaloniaProperty.Register<MainWindow, GridLength>(nameof(KeyColWidth), new GridLength(280));
        public static readonly StyledProperty<GridLength> TypeColWidthProperty =
            AvaloniaProperty.Register<MainWindow, GridLength>(nameof(TypeColWidth), new GridLength(160));

        public GridLength KeyColWidth
        {
            get => GetValue(KeyColWidthProperty);
            set => SetValue(KeyColWidthProperty, value);
        }

        public GridLength TypeColWidth
        {
            get => GetValue(TypeColWidthProperty);
            set => SetValue(TypeColWidthProperty, value);
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        void OnDataContextChanged(object? sender, EventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.PropertyChanged -= OnVmPropertyChanged;
                vm.PropertyChanged += OnVmPropertyChanged;
            }
        }

        void OnVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not MainViewModel vm) return;
            if (e.PropertyName == nameof(MainViewModel.Selected) && vm.Selected != null)
            {
                SelectAndRevealNode(vm.Selected);
            }
        }

        void SelectAndRevealNode(Node node)
        {
            var tv = this.FindControl<TreeView>("Tree");
            if (tv == null) return;
            ExpandPath(node);
            if (!Equals(tv.SelectedItem, node)) tv.SelectedItem = node;
            var c = FindContainer(tv, node);
            c?.BringIntoView();
        }

        void ExpandPath(Node node)
        {
            var tv = this.FindControl<TreeView>("Tree");
            if (tv == null) return;
            var stack = new Stack<Node>();
            var p = node.Parent;
            while (p != null) { stack.Push(p); p = p.Parent; }
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var container = FindContainer(tv, current);
                if (container != null) container.IsExpanded = true;
            }
        }

        TreeViewItem? FindContainer(ItemsControl root, object item)
        {
            foreach (var c in root.GetRealizedContainers())
            {
                if (c is TreeViewItem tvi)
                {
                    if (Equals(tvi.DataContext, item))
                        return tvi;

                    var child = FindContainer(tvi, item);
                    if (child != null)
                        return child;
                }
            }
            return null;
        }

        void ExpandSubTree(TreeViewItem item)
        {
            item.IsExpanded = true;
            foreach (var c in item.GetRealizedContainers())
                if (c is TreeViewItem tvi)
                    ExpandSubTree(tvi);
        }

        void CollapseSubTree(TreeViewItem item)
        {
            foreach (var c in item.GetRealizedContainers())
                if (c is TreeViewItem tvi)
                    CollapseSubTree(tvi);
            item.IsExpanded = false;
        }

        void CollapseAll()
        {
            var tv = this.FindControl<TreeView>("Tree");
            if (tv == null) return;
            foreach (var c in tv.GetRealizedContainers())
                if (c is TreeViewItem tvi)
                    CollapseSubTree(tvi);
        }

        void ExpandAll()
        {
            var tv = this.FindControl<TreeView>("Tree");
            if (tv == null) return;
            foreach (var c in tv.GetRealizedContainers())
                if (c is TreeViewItem tvi)
                    ExpandSubTree(tvi);
        }

        void OnContextExpandAll(object? sender, RoutedEventArgs e) => ExpandAll();
        void OnContextCollapseAll(object? sender, RoutedEventArgs e) => CollapseAll();

        void OnContextExpandNode(object? sender, RoutedEventArgs e)
        {
            var tvi = (sender as Control)?
                .GetLogicalAncestors()
                .OfType<TreeViewItem>()
                .FirstOrDefault();
            if (tvi != null) ExpandSubTree(tvi);
        }

        void OnContextCollapseNode(object? sender, RoutedEventArgs e)
        {
            var tvi = (sender as Control)?
                .GetLogicalAncestors()
                .OfType<TreeViewItem>()
                .FirstOrDefault();
            if (tvi != null) CollapseSubTree(tvi);
        }

        public void SelectAndReveal(object? item)
        {
            if (item is null) return;
            var tv = this.FindControl<TreeView>("Tree");
            if (tv is null) return;

            ExpandAncestors(item, tv);

            tv.SelectedItem = item;
            tv.ScrollIntoView(item);
        }

        void ExpandAncestors(object item, TreeView tv)
        {
            var stack = new Stack<object>();
            var p = GetParent(item);
            while (p != null)
            {
                stack.Push(p);
                p = GetParent(p);
            }

            ItemsControl current = tv;
            while (stack.Count > 0)
            {
                var ancestor = stack.Pop();
                var container = FindContainer(current, ancestor);
                if (container == null) break;
                container.IsExpanded = true;
                current = container;
            }
        }

        object? GetParent(object obj)
        {
            var pi = obj.GetType().GetProperty("Parent");
            return pi?.GetValue(obj);
        }

        void CollapseBranch()
        {
            var tv = this.FindControl<TreeView>("Tree");
            if (tv == null) return;
            var n = (DataContext as MainViewModel)?.Selected;
            if (n == null) return;
            var c = FindContainer(tv, n);
            if (c != null) tv.CollapseSubTree(c);
        }

        void ExpandBranch()
        {
            var tv = this.FindControl<TreeView>("Tree");
            if (tv == null) return;
            var n = (DataContext as MainViewModel)?.Selected;
            if (n == null) return;
            ExpandPath(n);
            var c = FindContainer(tv, n);
            if (c != null) ExpandSubTree(c);
        }

        private void ToggleFindBar()
        {
            var bar = this.FindControl<Border>("FindBar");
            if (bar != null) bar.IsVisible = !bar.IsVisible;
        }

        private void OnOpened(object? sender, EventArgs e)
        {
            AddHandler(InputElement.KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel);
            var s = SettingsService.Load();
            if (s.WindowWidth > 0 && s.WindowHeight > 0)
            {
                Width = s.WindowWidth;
                Height = s.WindowHeight;
            }
            if (s.IsMaximized) WindowState = WindowState.Maximized;
            if (DataContext is MainViewModel vm)
            {
                vm.PropertyChanged -= OnVmPropertyChanged;
                vm.PropertyChanged += OnVmPropertyChanged;
            }
        }

        private void OnWindowKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.F)
            {
                ToggleFindBar();
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Escape)
            {
                var bar = this.FindControl<Border>("FindBar");
                if (bar != null && bar.IsVisible)
                {
                    bar.IsVisible = false;
                    e.Handled = true;
                }
            }
        }

        private void OnDragRegionPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        }

        private void OnDragRegionDoubleTapped(object? sender, TappedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void OnClosing(object? sender, WindowClosingEventArgs e)
        {
            var s = SettingsService.Load();
            s.IsMaximized = WindowState == WindowState.Maximized;
            if (!s.IsMaximized)
            {
                s.WindowWidth = Bounds.Width;
                s.WindowHeight = Bounds.Height;
            }
            SettingsService.Save(s);
        }

        private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        }

        private void OnTitleBarDoubleTapped(object? sender, TappedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void OnTreeSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm && sender is TreeView tv)
            {
                vm.Selected = tv.SelectedItem as Node;
            }
        }

        private void OnKeyLabelDoubleTapped(object? sender, TappedEventArgs e)
        {
            if (sender is TextBlock lbl)
            {
                var parent = lbl.GetVisualParent<Panel>() ?? lbl.Parent;
                if (parent is null) return;
                if (parent is Panel p)
                {
                    var editor = p.Children.OfType<TextBox>().FirstOrDefault(x => x.Name == "KeyEditor");
                    if (editor != null)
                    {
                        lbl.IsVisible = false;
                        editor.IsVisible = true;
                        editor.Focus();
                        editor.CaretIndex = editor.Text?.Length ?? 0;
                        editor.SelectAll();
                    }
                }
                else if (parent is Grid g)
                {
                    var editor = g.Children.OfType<TextBox>().FirstOrDefault(x => x.Name == "KeyEditor");
                    if (editor != null)
                    {
                        lbl.IsVisible = false;
                        editor.IsVisible = true;
                        editor.Focus();
                        editor.CaretIndex = editor.Text?.Length ?? 0;
                        editor.SelectAll();
                    }
                }
            }
        }

        private void OnKeyEditorCommit(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                var parent = tb.GetVisualParent<Panel>() ?? tb.Parent;
                if (parent is Panel p)
                {
                    var label = p.Children.OfType<TextBlock>().FirstOrDefault(x => x.Name == "KeyLabel");
                    if (label != null)
                    {
                        tb.IsVisible = false;
                        label.IsVisible = true;
                    }
                }
                else if (parent is Grid g)
                {
                    var label = g.Children.OfType<TextBlock>().FirstOrDefault(x => x.Name == "KeyLabel");
                    if (label != null)
                    {
                        tb.IsVisible = false;
                        label.IsVisible = true;
                    }
                }
            }
        }

        private void OnKeyEditorKeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (e.Key == Key.Enter || e.Key == Key.Escape)
                {
                    OnKeyEditorCommit(tb, new RoutedEventArgs());
                    e.Handled = true;
                }
            }
        }

        private void OnKeyCellDoubleTapped(object? sender, TappedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.IsReadOnly = false;
                tb.Focus();
                tb.SelectAll();
            }
        }

        private void OnKeyCellLostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
                tb.IsReadOnly = true;
        }

        private async void OnOpenClick(object? sender, RoutedEventArgs e)
        {
            var s = SettingsService.Load();
            var options = new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = "Open Asset",
                SuggestedStartLocation = s.GetStartFolder(StorageProvider),
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Darkspore Assets"){ Patterns = new[]{ "*.Noun", "*.noun" } },
                    new FilePickerFileType("All Files"){ Patterns = new[]{ "*.*" } }
                }
            };

            var files = await StorageProvider.OpenFilePickerAsync(options);
            if (files == null || files.Count == 0) return;

            var path = files[0].TryGetLocalPath();
            if (string.IsNullOrWhiteSpace(path))
            {
                await MessageBox("Selecione um arquivo local do disco.");
                return;
            }

            s.LastOpenDirectory = Path.GetDirectoryName(path) ?? s.LastOpenDirectory;
            SettingsService.Save(s);

            try
            {
                var xdoc = await CoreXmlBridge.ParseToXmlAsync(path);
                if (xdoc == null)
                {
                    await MessageBox("Falha ao obter XML do Core.");
                    return;
                }

                var nodes = XmlToNodes.FromXDocument(xdoc);
                if (DataContext is MainViewModel vm)
                {
                    vm.SetRoots(nodes);
                    vm.SetSuggestions(EnumerateKeyAssetHints(nodes));
                }
            }
            catch (Exception ex)
            {
                await MessageBox("Falha ao abrir o arquivo:\n" + ex.Message);
            }
        }

        private IEnumerable<string> EnumerateKeyAssetHints(IEnumerable<Node> nodes)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            void Walk(Node n)
            {
                if (n is StringValueNode sv)
                {
                    var v = sv.Value ?? "";
                    if (!string.IsNullOrWhiteSpace(v) && (v.Contains("/") || v.Contains("\\"))) set.Add(v);
                }
                foreach (var c in n.Children) Walk(c);
            }
            foreach (var r in nodes) Walk(r);
            return set.OrderBy(x => x);
        }

        private void OnExitClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void OnSaveAsClick(object? sender, RoutedEventArgs e)
        {
            await MessageBox("Save As ainda não implementado.");
        }

        private async Task MessageBox(string text)
        {
            var panel = new StackPanel
            {
                Spacing = 12,
                Margin = new Thickness(16),
                Children =
                {
                    new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, MaxWidth = 520 },
                    new Button
                    {
                        Content = "OK",
                        HorizontalAlignment = HorizontalAlignment.Right,
                        MinWidth = 80
                    }
                }
            };

            var btn = (Button)panel.Children[1];
            var dlg = new Window
            {
                Title = "Info",
                Width = 600,
                Height = 200,
                CanResize = false,
                Content = panel,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            btn.Click += (_, __) => dlg.Close();
            await dlg.ShowDialog(this);
        }

        private void OnExpandClick(object? sender, RoutedEventArgs e)
        {
            var tv = this.FindControl<TreeView>("Tree");
            if (tv == null) return;
            var n = (DataContext as MainViewModel)?.Selected;
            if (n == null) return;
            var c = FindContainer(tv, n);
            if (c != null) c.IsExpanded = true;
        }

        private void OnCollapseClick(object? sender, RoutedEventArgs e)
        {
            var tv = this.FindControl<TreeView>("Tree");
            if (tv == null) return;
            var n = (DataContext as MainViewModel)?.Selected;
            if (n == null) return;
            var c = FindContainer(tv, n);
            if (c != null) c.IsExpanded = false;
        }

        private void OnExpandBranchClick(object? sender, RoutedEventArgs e) => ExpandBranch();
        private void OnCollapseBranchClick(object? sender, RoutedEventArgs e) => CollapseBranch();
        private void OnExpandAllClick(object? sender, RoutedEventArgs e) => ExpandAll();
        private void OnCollapseAllClick(object? sender, RoutedEventArgs e) => CollapseAll();
    }

    static class CoreXmlBridge
    {
        public static async Task<XDocument?> ParseToXmlAsync(string inputPath)
        {
            var exe = FindCoreExe();
            if (exe == null) throw new InvalidOperationException("ReCap.Parser.Core.exe não encontrado (Debug/Release).");

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
            if (p == null) throw new InvalidOperationException("Não foi possível iniciar o Core CLI.");
            await p.WaitForExitAsync();

            if (p.ExitCode != 0)
            {
                var err = p.StandardError.ReadToEnd();
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
