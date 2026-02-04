<div align="center">
  <img src=".github/icon.png" alt="AssetData.Parser Logo" width="256" />
</div>

<h1 align="center">AssetData Parser</h1>
<p align="center">A Binary Parser to Darkspore binary files.</p>

## Architecture

```mermaid
flowchart TB
    %% Paleta Refinada (Inspirada na Logo e CommonUI)
    %% Accent: #378FB6 | Glow: #5BDEF3 | Dark: #0A121D | Border: #40FFFFFF

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

    AssetSvc -- "Direct API Call" --> Logic

    %% Estilização Premium para Dark Mode
    %% Editor: Azul Profundo (SystemAccentColorDark2)
    style Editor fill:#1F536B,stroke:#5BDEF3,stroke-width:2px,color:#FFFFFF
    
    %% Core: Fundo ListBoxItem (Escuro/Sóbrio)
    style Core fill:#0A121D,stroke:#378FB6,stroke-width:2px,color:#FFFFFF
    
    %% Camadas Internas: Tons Médios para contraste
    style TopLayer fill:#2B7291,stroke:#93F5FF,color:#FFFFFF
    style Logic fill:#151819,stroke:#378FB6,color:#FFFFFF

    %% Links e Textos
    linkStyle default stroke:#5BDEF3,stroke-width:1px
```

## Key Changes

### Before (Legacy Process)
```mermaid
flowchart LR
    Bin[Binary Asset] --> CLI["Core CLI<br/>(Process)"]
    CLI --> XML[XML File]
    XML --> Ed[Editor Reads XML]
    Ed --> Nodes[Nodes]
    
    %% Estilo "Desativado" usando as cores de TextBox/Disabled do seu código
    style CLI fill:#252829,stroke:#51FFFFFF,stroke-dasharray: 5 5,color:#99999A
    style XML fill:#252829,stroke:#51FFFFFF,stroke-dasharray: 5 5,color:#99999A
    linkStyle 1,2,3 stroke:#2F3031,stroke-width:1px
```

### After (Optimized API)
```mermaid
flowchart LR
    Bin[Binary Asset] --> API["Core API<br/>(Direct)"]
    API --> Nodes["AssetNode<br/>(Direct Binding)"]
    
    %% Estilo "Ativo/Glow" baseado na Logo
    style API fill:#378FB6,stroke:#93F5FF,stroke-width:3px,color:#FFFFFF
    style Nodes fill:#378FB6,stroke:#93F5FF,stroke-width:3px,color:#FFFFFF
    linkStyle 0,1 stroke:#5BDEF3,stroke-width:2px
```
## Technologies

- **.NET 9** - Latest stable version
- **Avalonia 11.3** - Cross-platform UI
- **CommunityToolkit.Mvvm 8.4** - Source generators for clean MVVM
- **Span<byte>** - Optimized binary reading
- **Dependency Injection** - Microsoft.Extensions.DependencyInjection
- **ReCap.CommonUI** - using native UI of ReCap Project made by Splitwirez

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
- [ ] Asset diff/compare functionality
