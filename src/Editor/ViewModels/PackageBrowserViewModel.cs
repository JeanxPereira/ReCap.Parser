using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReCap.Parser;

namespace ReCap.Parser.Editor.ViewModels;

/// <summary>
/// Represents an entry in the DBPF package browser (from Catalog).
/// </summary>
public partial class PackageEntryViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;
    
    [ObservableProperty]
    private string _type = string.Empty;
    
    [ObservableProperty]
    private long _compileTime;
    
    [ObservableProperty]
    private int _version;
    
    [ObservableProperty]
    private uint _typeCrc;
    
    [ObservableProperty]
    private uint _dataCrc;
    
    [ObservableProperty]
    private string _sourceFile = string.Empty;
    
    /// <summary>Full virtual name (name.type).</summary>
    public string FullName => string.IsNullOrEmpty(Type) ? Name : $"{Name}.{Type}";
    
    /// <summary>Formatted compile time.</summary>
    public string CompileTimeFormatted => DateTimeOffset.FromUnixTimeSeconds(CompileTime).ToString("yyyy-MM-dd HH:mm:ss");
}

/// <summary>
/// ViewModel for the DBPF Package Browser window.
/// Uses Catalog for optimized asset listing.
/// </summary>
public partial class PackageBrowserViewModel : ObservableObject
{
    private static readonly Regex CatalogPattern = new(@"^catalog_\d+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    
    private DbpfReader? _dbpf;
    private readonly AssetService _assetService;
    
    /// <summary>All entries in the package (from Catalog).</summary>
    public ObservableCollection<PackageEntryViewModel> AllEntries { get; } = [];
    
    /// <summary>Filtered entries based on search/type filter.</summary>
    public ObservableCollection<PackageEntryViewModel> FilteredEntries { get; } = [];
    
    /// <summary>Available type filters.</summary>
    public ObservableCollection<string> TypeFilters { get; } = [];
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPackageOpen))]
    private string _packagePath = string.Empty;
    
    [ObservableProperty]
    private string _packageName = string.Empty;
    
    [ObservableProperty]
    private int _totalEntries;
    
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [ObservableProperty]
    private string _selectedTypeFilter = "All";
    
    [ObservableProperty]
    private PackageEntryViewModel? _selectedEntry;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _statusText = "No package loaded";
    
    public bool IsPackageOpen => _dbpf != null;
    
    /// <summary>Event raised when user wants to open an asset.</summary>
    public event Action<AssetNode, string>? AssetOpened;
    
    public PackageBrowserViewModel() : this(new AssetService()) { }
    
    public PackageBrowserViewModel(AssetService assetService)
    {
        _assetService = assetService;
    }
    
    /// <summary>
    /// Open a DBPF package file and load its catalog.
    /// </summary>
    public async Task OpenPackageAsync(string path)
    {
        if (!File.Exists(path)) return;
        
        IsLoading = true;
        StatusText = "Loading package...";
        
        try
        {
            // Close previous
            _dbpf?.Dispose();
            AllEntries.Clear();
            FilteredEntries.Clear();
            TypeFilters.Clear();
            
            // Open new package
            _dbpf = await Task.Run(() => new DbpfReader(path));
            PackagePath = path;
            PackageName = Path.GetFileName(path);
            
            // Try to load registries from same directory
            var registryDir = Path.Combine(Path.GetDirectoryName(path) ?? "", "registries");
            if (Directory.Exists(registryDir))
                _dbpf.LoadRegistries(registryDir);
            
            StatusText = "Loading catalog...";
            
            // Find and load catalog
            var entries = await LoadCatalogAsync();
            
            if (entries == null || entries.Count == 0)
            {
                StatusText = "Error: Could not find or parse catalog_*.bin";
                return;
            }
            
            // Add entries on UI thread
            foreach (var entry in entries)
                AllEntries.Add(entry);
            
            TotalEntries = AllEntries.Count;
            
            // Build type filters from loaded entries
            var types = AllEntries
                .Select(e => e.Type)
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t);
            
            TypeFilters.Add("All");
            foreach (var type in types)
                TypeFilters.Add(type);
            
            SelectedTypeFilter = "All";
            ApplyFilter();
            
            StatusText = $"Loaded {TotalEntries:N0} assets from catalog";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Find and load the catalog_*.bin file from the package.
    /// </summary>
    private async Task<List<PackageEntryViewModel>?> LoadCatalogAsync()
    {
        if (_dbpf == null) return null;
        
        // Try direct hash lookup for common catalog names
        byte[]? catalogData = null;
        
        for (int i = 131; i <= 150; i++)
        {
            var testName = $"catalog_{i}.bin";
            var data = _dbpf.GetAsset(testName);
            if (data != null && data.Length > 0)
            {
                catalogData = data;
                break;
            }
        }
        
        // Also try catalog_0
        if (catalogData == null)
        {
            var data = _dbpf.GetAsset("catalog_0.bin");
            if (data != null && data.Length > 0)
                catalogData = data;
        }
        
        if (catalogData == null)
            return null;
        
        // Parse catalog on background thread
        return await Task.Run(() => ParseCatalog(catalogData));
    }
    
    /// <summary>
    /// Parse catalog binary data and return list of entries.
    /// </summary>
    private List<PackageEntryViewModel>? ParseCatalog(byte[] data)
    {
        try
        {
            var catalogRoot = _assetService.Parser.Parse(data, "Catalog", 8);
            
            // Find entries array
            var entriesArray = catalogRoot.Children
                .OfType<ArrayNode>()
                .FirstOrDefault(n => n.Name == "entries");
            
            if (entriesArray == null)
                return null;
            
            var entries = new List<PackageEntryViewModel>();
            
            foreach (var entryNode in entriesArray.Children.OfType<StructNode>())
            {
                var vm = new PackageEntryViewModel();
                
                foreach (var field in entryNode.Children)
                {
                    switch (field.Name)
                    {
                        case "assetNameWType" when field is StringNode sn:
                            // Parse "name.type" format
                            var parts = sn.Value.Split('.', 2);
                            vm.Name = parts[0];
                            vm.Type = parts.Length > 1 ? parts[1] : "";
                            break;
                            
                        case "compileTime" when field is NumberNode nn:
                            vm.CompileTime = (long)nn.Value;
                            break;
                            
                        case "version" when field is NumberNode nn:
                            vm.Version = (int)nn.Value;
                            break;
                            
                        case "typeCrc" when field is NumberNode nn:
                            vm.TypeCrc = (uint)nn.Value;
                            break;
                            
                        case "dataCrc" when field is NumberNode nn:
                            vm.DataCrc = (uint)nn.Value;
                            break;
                            
                        case "sourceFileNameWType" when field is StringNode sn:
                            vm.SourceFile = sn.Value;
                            break;
                    }
                }
                
                // Only add if we have a valid name
                if (!string.IsNullOrEmpty(vm.Name))
                    entries.Add(vm);
            }
            
            return entries;
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Open the selected asset for editing.
    /// </summary>
    [RelayCommand]
    public async Task OpenSelectedAssetAsync()
    {
        if (_dbpf == null || SelectedEntry == null) return;
        
        IsLoading = true;
        StatusText = $"Loading {SelectedEntry.FullName}...";
        
        try
        {
            var data = _dbpf.GetAsset(SelectedEntry.FullName);
            if (data == null)
            {
                StatusText = $"Failed to read asset: {SelectedEntry.FullName}";
                return;
            }
            
            var fileType = _assetService.GetFileType(SelectedEntry.Type);
            if (fileType == null)
            {
                StatusText = $"Unknown asset type: {SelectedEntry.Type}";
                return;
            }
            
            var root = await Task.Run(() => 
                _assetService.Parser.Parse(data, fileType.RootStruct, fileType.HeaderSize));
            
            AssetOpened?.Invoke(root, SelectedEntry.FullName);
            StatusText = $"Opened: {SelectedEntry.FullName}";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Apply search and type filter.
    /// </summary>
    private void ApplyFilter()
    {
        FilteredEntries.Clear();
        
        var query = AllEntries.AsEnumerable();
        
        // Type filter
        if (SelectedTypeFilter != "All")
            query = query.Where(e => e.Type.Equals(SelectedTypeFilter, StringComparison.OrdinalIgnoreCase));
        
        // Search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
            query = query.Where(e => e.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                     e.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        
        foreach (var entry in query.OrderBy(e => e.Name))
            FilteredEntries.Add(entry);
        
        StatusText = $"Showing {FilteredEntries.Count:N0} of {TotalEntries:N0} assets";
    }
    
    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnSelectedTypeFilterChanged(string value) => ApplyFilter();
    
    public void Dispose()
    {
        _dbpf?.Dispose();
    }
}
