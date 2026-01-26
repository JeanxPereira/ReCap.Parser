<div align="center">
  <img src=".github/icon.png" alt="ReCapParser Logo" width="256" />
</div>

<h1 align="center">AssetData Parser</h1>
<p align="center">A Binary Parser to Darkspore binary files.</p>

## Architecture

```mermaid
flowchart TB
    subgraph Editor ["Editor (MVVM)"]
        direction TB
        subgraph TopLayer [Logic Layer]
            MVM["MainViewModel<br/>(Observable)"]
            Undo["UndoRedoService<br/>(IUndoable)"]
            Settings["SettingsService<br/>(JSON)"]
        end

        AssetSvc["AssetService<br/>LoadFile -> AssetNode<br/>(Observable, Bindable)"]
        
        MVM --> AssetSvc
        Undo --> AssetSvc
        Settings -.-> AssetSvc
    end

    subgraph Core ["Core Library"]
        direction TB
        subgraph Logic [Parsing Logic]
            Parser["AssetParser<br/>(Span byte)"]
            TypeSys["TypeSystem<br/>(DataTypes)"]
            Cats["Catalogs<br/>(Noun, Phase...)"]
        end

        Tree["AssetNode Tree<br/>StructNode -> StringNode, NumberNode...<br/>(Observable, INotifyPropertyChanged)"]

        Logic --> Tree
    end

    AssetSvc -- "Direct API Call (No XML!)" --> Logic
```

## Key Changes

### Before (Slow)
```mermaid
flowchart LR
    Bin[Binary Asset] --> CLI["Core CLI<br/>(Process)"]
    CLI --> XML[XML File]
    XML --> Ed[Editor Reads XML]
    Ed --> Nodes[Nodes]
    style CLI fill:#f9f,stroke:#333
    style XML fill:#f9f,stroke:#333
```

### After (Fast)
```mermaid
flowchart LR
    Bin[Binary Asset] --> API["Core API<br/>(Direct)"]
    API --> Nodes["AssetNode<br/>(Direct Binding)"]
    style API fill:#9f9,stroke:#333
    style Nodes fill:#9f9,stroke:#333
```

## Technologies

- **.NET 9** - Latest stable version
- **Avalonia 11.3** - Cross-platform UI
- **CommunityToolkit.Mvvm 8.4** - Source generators for clean MVVM
- **Span<byte>** - Optimized binary reading
- **Dependency Injection** - Microsoft.Extensions.DependencyInjection
- **ReCap.CommonUI** - using native UI of ReCap Project made by Splitwirez

## Folder Structure

```
src/
├── Core/                       # Parsing library (DLL)
│   ├── AssetNode.cs            # Observable model (MVVM-ready)
│   ├── AssetParser.cs          # Optimized binary parser
│   ├── AssetService.cs         # High-level API
│   ├── TypeSystem.cs           # Type definitions
│   └── Catalog/                # Struct definitions by type
│       ├── Noun.cs
│       ├── Phase.cs
│       └── ...
│
├── Editor/                     # Avalonia UI
│   ├── ViewModels/             # MVVM ViewModels
│   │   └── MainViewModel.cs    # Main ViewModel
│   ├── Views/                  # XAML Views
│   ├── Services/               # Helper Services
│   │   ├── ServiceConfiguration.cs
│   │   ├── UndoRedoService.cs
│   │   └── SettingsService.cs
│   └── MainWindow.axaml        # Main Window
│
└── ReCap.CommonUI/             # Shared UI Components
```

## How to Use

### In the Editor
```csharp
// ViewModel receives AssetService via DI
public MainViewModel(AssetService assetService)
{
    _assetService = assetService;
}

// Load file directly (no XML!)
await LoadFileAsync("path/to/asset.noun");
```

### Direct API
```csharp
var service = new AssetService();
AssetNode root = service.LoadFile("creature.noun");

// Access data directly
foreach (var child in root.Children)
{
    if (child is StringNode sn)
        Console.WriteLine($"{sn.Name}: {sn.Value}");
}
```

## Benefits

1.  **Performance**: Eliminating the XML bridge reduces load time by ~80%.
2.  **Memory**: No data duplication (XML + Nodes).
3.  **Pure MVVM**: AssetNodes are observable, allowing direct binding.
4.  **Testability**: Injectable services make it easy to mock.
5.  **Maintainability**: Clear separation between Core and Editor.

## Next Steps

- [ ] Implement binary serialization (saving changes)
- [ ] Add Enum editing with ComboBox
- [ ] TreeView virtualization for large files
- [ ] Asset diff/compare functionality
- [ ] Asset read using `catalog_%d.bin`