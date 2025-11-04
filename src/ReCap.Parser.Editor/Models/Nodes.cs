using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReCap.Parser.Editor.Models
{
    public abstract class Node : INotifyPropertyChanged
    {
        string _key = "";
        public string Key { get => _key; set { if (_key != value) { _key = value; OnPropertyChanged(); } } }
        public string Kind { get; internal set; } = "";
        public Node? Parent { get; internal set; }
        public ObservableCollection<Node> Children { get; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        public Node()
        {
            Children.CollectionChanged += OnChildrenChanged;
        }

        void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null) foreach (var it in e.OldItems) if (it is Node n) n.Parent = null;
            if (e.NewItems != null) foreach (var it in e.NewItems) if (it is Node n) n.Parent = this;
        }
    }

    public sealed class StructNode : Node
    {
        public StructNode() { Kind = "struct"; }
    }

    public sealed class ArrayNode : Node
    {
        public ArrayNode() { Kind = "array"; }
    }

    public sealed class StringValueNode : Node
    {
        string _value = "";
        public string Value { get => _value; set { if (_value != value) { _value = value; OnPropertyChanged(); } } }
        public StringValueNode() { Kind = "string"; }
    }

    public sealed class NumberValueNode : Node
    {
        double _value;
        public double Value { get => _value; set { if (_value != value) { _value = value; OnPropertyChanged(); } } }
        public NumberValueNode() { Kind = "number"; }
    }

    public sealed class BoolValueNode : Node
    {
        bool _value;
        public bool Value { get => _value; set { if (_value != value) { _value = value; OnPropertyChanged(); } } }
        public BoolValueNode() { Kind = "bool"; }
    }
}
