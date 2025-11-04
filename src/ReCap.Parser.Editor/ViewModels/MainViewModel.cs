using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ReCap.Parser.Editor.Models;
using ReCap.Parser.Editor.Services;

namespace ReCap.Parser.Editor.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Node> Roots { get; } = new();
        public ObservableCollection<string> KeyAssetSuggestions { get; } = new();

        Node? _selected;
        public Node? Selected
        {
            get => _selected;
            set { if (_selected != value) { _selected = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsSelectedReadOnly)); UpdateConvertState(); } }
        }

        string _convertTarget = "string";
        public string ConvertTarget
        {
            get => _convertTarget;
            set { if (_convertTarget != value) { _convertTarget = value; OnPropertyChanged(); UpdateConvertState(); } }
        }

        bool _canConvert;
        public bool CanConvert
        {
            get => _canConvert;
            private set { if (_canConvert != value) { _canConvert = value; OnPropertyChanged(); } }
        }

        public bool IsSelectedReadOnly => Selected is ArrayNode or StructNode;

        string _findText = "";
        public string FindText { get => _findText; set { if (_findText != value) { _findText = value; OnPropertyChanged(); _flatCache = null; } } }

        string _replaceText = "";
        public string ReplaceText { get => _replaceText; set { if (_replaceText != value) { _replaceText = value; OnPropertyChanged(); } } }

        bool _searchInKeys = true;
        public bool SearchInKeys { get => _searchInKeys; set { if (_searchInKeys != value) { _searchInKeys = value; OnPropertyChanged(); _flatCache = null; } } }

        bool _searchInValues = true;
        public bool SearchInValues { get => _searchInValues; set { if (_searchInValues != value) { _searchInValues = value; OnPropertyChanged(); _flatCache = null; } } }

        bool _caseSensitive;
        public bool CaseSensitive { get => _caseSensitive; set { if (_caseSensitive != value) { _caseSensitive = value; OnPropertyChanged(); _flatCache = null; } } }

        public ICommand ConvertSelectedCommand { get; }
        public ICommand FindNextCommand { get; }
        public ICommand FindPrevCommand { get; }
        public ICommand ReplaceCommand { get; }
        public ICommand ReplaceAllCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        readonly UndoRedoService _undo = new();
        readonly Dictionary<Node, Snapshot> _snap = new();
        readonly HashSet<Node> _subscribed = new();

        public bool IsApplyingUndoRedo { get; set; }

        public MainViewModel()
        {
            ConvertSelectedCommand = new RelayCommand(_ => ConvertSelected(), _ => CanConvert);
            FindNextCommand = new RelayCommand(_ => FindNext());
            FindPrevCommand = new RelayCommand(_ => FindPrev());
            ReplaceCommand = new RelayCommand(_ => ReplaceOne());
            ReplaceAllCommand = new RelayCommand(_ => ReplaceAll());
            UndoCommand = new RelayCommand(_ => _undo.Undo(this));
            RedoCommand = new RelayCommand(_ => _undo.Redo(this));
            _undo.Changed += () =>
            {
                (UndoCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RedoCommand as RelayCommand)?.RaiseCanExecuteChanged();
            };
        }

        public void SetRoots(IEnumerable<Node> nodes)
        {
            Roots.Clear();
            foreach (var n in nodes) Roots.Add(n);
            _flatCache = null;
            _undo.Clear();
            UnsubscribeAll();
            _snap.Clear();
            foreach (var r in Roots) SubscribeTree(r);
        }

        public void SetSuggestions(IEnumerable<string> suggestions)
        {
            KeyAssetSuggestions.Clear();
            foreach (var s in suggestions) KeyAssetSuggestions.Add(s);
        }

        public void ConvertSelected()
        {
            if (Selected == null) return;
            if (Selected is StructNode || Selected is ArrayNode) return;

            if (ConvertTarget == "string")
            {
                if (Selected is StringValueNode) return;
                var replacement = new StringValueNode { Key = Selected.Key, Value = ReadAsString(Selected) };
                ReplaceInParent(Selected, replacement);
                Selected = replacement;
                return;
            }

            if (ConvertTarget == "number")
            {
                var ok = TryReadAsNumber(Selected, out var d);
                if (!ok) return;
                var replacement = new NumberValueNode { Key = Selected.Key, Value = d };
                ReplaceInParent(Selected, replacement);
                Selected = replacement;
                return;
            }

            if (ConvertTarget == "bool")
            {
                var ok = TryReadAsBool(Selected, out var b);
                if (!ok) return;
                var replacement = new BoolValueNode { Key = Selected.Key, Value = b };
                ReplaceInParent(Selected, replacement);
                Selected = replacement;
                return;
            }
        }


        public void ReplaceNodeRaw(Node? parent, int index, Node oldNode, Node newNode)
        {
            if (parent == null)
            {
                Roots[index] = newNode;
                newNode.Parent = null;
            }
            else
            {
                parent.Children[index] = newNode;
                newNode.Parent = parent;
            }
            UnsubscribeNode(oldNode);
            SubscribeTree(newNode);
            _flatCache = null;
        }

        void ReplaceInParent(Node oldNode, Node newNode)
        {
            Node? parent = oldNode.Parent;
            if (parent == null)
            {
                var idx = Roots.IndexOf(oldNode);
                if (idx >= 0)
                {
                    var act = new ReplaceNodeAction(null, idx, oldNode, newNode);
                    _undo.Push(act);
                    ReplaceNodeRaw(null, idx, oldNode, newNode);
                }
                return;
            }
            var idx2 = parent.Children.IndexOf(oldNode);
            if (idx2 >= 0)
            {
                var act = new ReplaceNodeAction(parent, idx2, oldNode, newNode);
                _undo.Push(act);
                ReplaceNodeRaw(parent, idx2, oldNode, newNode);
            }
        }

        bool TryReadAsNumber(Node node, out double value)
        {
            if (node is NumberValueNode nv) { value = nv.Value; return true; }
            if (node is BoolValueNode bv) { value = bv.Value ? 1d : 0d; return true; }
            if (node is StringValueNode sv) return double.TryParse(sv.Value ?? "", System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value);
            value = 0;
            return false;
        }

        bool TryReadAsBool(Node node, out bool value)
        {
            if (node is BoolValueNode bv) { value = bv.Value; return true; }
            if (node is NumberValueNode nv) { value = Math.Abs(nv.Value) > double.Epsilon; return true; }
            if (node is StringValueNode sv)
            {
                var s = (sv.Value ?? "").Trim();
                if (bool.TryParse(s, out value)) return true;
                if (s == "1") { value = true; return true; }
                if (s == "0") { value = false; return true; }
            }
            value = false;
            return false;
        }

        string ReadAsString(Node node)
        {
            if (node is StringValueNode sv) return sv.Value ?? "";
            if (node is BoolValueNode bv) return bv.Value ? "true" : "false";
            if (node is NumberValueNode nv) return nv.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return "";
        }

        void UpdateConvertState()
        {
            if (Selected == null) { CanConvert = false; return; }
            if (Selected is StructNode || Selected is ArrayNode) { CanConvert = false; return; }
            if (ConvertTarget == "string") { CanConvert = Selected is not StringValueNode; return; }
            if (ConvertTarget == "number") { CanConvert = TryReadAsNumber(Selected, out _); return; }
            if (ConvertTarget == "bool") { CanConvert = TryReadAsBool(Selected, out _); return; }
            CanConvert = false;
        }

        List<Node>? _flatCache;
        int _flatIndex = -1;

        void EnsureFlat()
        {
            if (_flatCache != null) return;
            _flatCache = new List<Node>();
            foreach (var r in Roots) Flatten(r, _flatCache);
            if (_flatIndex >= _flatCache.Count) _flatIndex = -1;
        }

        void Flatten(Node n, List<Node> list)
        {
            list.Add(n);
            foreach (var c in n.Children) Flatten(c, list);
        }

        bool MatchNode(Node n, string term)
        {
            var comp = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            var hit = false;
            if (SearchInKeys)
            {
                if (!string.IsNullOrEmpty(n.Key) && n.Key.IndexOf(term, comp) >= 0) hit = true;
            }
            if (!hit && SearchInValues)
            {
                if (n is StringValueNode sv)
                {
                    var v = sv.Value ?? "";
                    if (v.IndexOf(term, comp) >= 0) hit = true;
                }
                else if (n is NumberValueNode nv)
                {
                    var v = nv.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    if (v.IndexOf(term, comp) >= 0) hit = true;
                }
                else if (n is BoolValueNode bv)
                {
                    var v = bv.Value ? "true" : "false";
                    if (v.IndexOf(term, comp) >= 0) hit = true;
                }
            }
            return hit;
        }

        public void FindNext()
        {
            if (string.IsNullOrEmpty(FindText)) return;
            EnsureFlat();
            if (_flatCache == null || _flatCache.Count == 0) return;
            var start = (_flatIndex + 1 + _flatCache.Count) % _flatCache.Count;
            var i = start;
            do
            {
                var n = _flatCache[i];
                if (MatchNode(n, FindText))
                {
                    _flatIndex = i;
                    Selected = n;
                    return;
                }
                i = (i + 1) % _flatCache.Count;
            } while (i != start);
        }

        public void FindPrev()
        {
            if (string.IsNullOrEmpty(FindText)) return;
            EnsureFlat();
            if (_flatCache == null || _flatCache.Count == 0) return;
            var start = (_flatIndex - 1 + _flatCache.Count) % _flatCache.Count;
            var i = start;
            do
            {
                var n = _flatCache[i];
                if (MatchNode(n, FindText))
                {
                    _flatIndex = i;
                    Selected = n;
                    return;
                }
                i = (i - 1 + _flatCache.Count) % _flatCache.Count;
            } while (i != start);
        }

        public void ReplaceOne()
        {
            if (Selected == null || string.IsNullOrEmpty(FindText)) return;
            var comp = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            if (SearchInValues)
            {
                if (Selected is StringValueNode sv && !string.IsNullOrEmpty(sv.Value))
                    sv.Value = ReplaceOnce(sv.Value, FindText, ReplaceText, comp);
                else if (Selected is NumberValueNode nv)
                {
                    var s = nv.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    var r = ReplaceOnce(s, FindText, ReplaceText, comp);
                    if (double.TryParse(r, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d)) nv.Value = d;
                }
                else if (Selected is BoolValueNode bv)
                {
                    var s = bv.Value ? "true" : "false";
                    var r = ReplaceOnce(s, FindText, ReplaceText, comp).Trim();
                    if (bool.TryParse(r, out var b)) bv.Value = b;
                }
            }

            if (SearchInKeys && !string.IsNullOrEmpty(Selected.Key))
            {
                Selected.Key = ReplaceOnce(Selected.Key, FindText, ReplaceText, comp);
            }
        }

        public void ReplaceAll()
        {
            if (string.IsNullOrEmpty(FindText)) return;
            EnsureFlat();
            if (_flatCache == null) return;
            var comp = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            foreach (var n in _flatCache)
            {
                if (SearchInValues)
                {
                    if (n is StringValueNode sv && !string.IsNullOrEmpty(sv.Value))
                        sv.Value = ReplaceAllInternal(sv.Value, FindText, ReplaceText, comp);
                    else if (n is NumberValueNode nv)
                    {
                        var s = nv.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        var r = ReplaceAllInternal(s, FindText, ReplaceText, comp);
                        if (double.TryParse(r, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d)) nv.Value = d;
                    }
                    else if (n is BoolValueNode bv)
                    {
                        var s = bv.Value ? "true" : "false";
                        var r = ReplaceAllInternal(s, FindText, ReplaceText, comp).Trim();
                        if (bool.TryParse(r, out var b)) bv.Value = b;
                    }
                }
                if (SearchInKeys && !string.IsNullOrEmpty(n.Key))
                {
                    n.Key = ReplaceAllInternal(n.Key, FindText, ReplaceText, comp);
                }
            }
        }

        string ReplaceOnce(string src, string find, string repl, StringComparison cmp)
        {
            var idx = src.IndexOf(find, cmp);
            if (idx < 0) return src;
            return src.Substring(0, idx) + repl + src.Substring(idx + find.Length);
        }

        string ReplaceAllInternal(string src, string find, string repl, StringComparison cmp)
        {
            if (string.IsNullOrEmpty(find)) return src;
            var start = 0;
            while (true)
            {
                var idx = src.IndexOf(find, start, cmp);
                if (idx < 0) break;
                src = src.Substring(0, idx) + repl + src.Substring(idx + find.Length);
                start = idx + repl.Length;
            }
            return src;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        struct Snapshot
        {
            public string Key;
            public string StringValue;
            public double NumberValue;
            public bool BoolValue;
        }

        void SubscribeTree(Node n)
        {
            if (_subscribed.Contains(n)) return;
            _subscribed.Add(n);
            _snap[n] = Capture(n);
            n.PropertyChanged += OnNodeChanged;
            foreach (var c in n.Children) SubscribeTree(c);
        }

        void UnsubscribeNode(Node n)
        {
            if (!_subscribed.Contains(n)) return;
            _subscribed.Remove(n);
            n.PropertyChanged -= OnNodeChanged;
            _snap.Remove(n);
            foreach (var c in n.Children) UnsubscribeNode(c);
        }

        void UnsubscribeAll()
        {
            foreach (var n in new List<Node>(_subscribed)) UnsubscribeNode(n);
            _subscribed.Clear();
            _snap.Clear();
        }

        Snapshot Capture(Node n)
        {
            var s = new Snapshot
            {
                Key = n.Key,
                StringValue = n is StringValueNode sv ? sv.Value ?? "" : "",
                NumberValue = n is NumberValueNode nv ? nv.Value : 0d,
                BoolValue = n is BoolValueNode bv && bv.Value
            };
            return s;
        }

        void OnNodeChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (IsApplyingUndoRedo) return;
            if (sender is not Node n) return;
            if (!_snap.TryGetValue(n, out var snap)) snap = Capture(n);

            if (e.PropertyName == "Key")
            {
                var oldV = snap.Key;
                var newV = n.Key;
                if (!string.Equals(oldV, newV, StringComparison.Ordinal))
                {
                    _undo.Push(new PropertyChangeAction(n, "Key", oldV, newV));
                    snap.Key = newV;
                    _snap[n] = snap;
                }
                return;
            }

            if (e.PropertyName == "Value")
            {
                if (n is StringValueNode sv)
                {
                    var oldV = snap.StringValue;
                    var newV = sv.Value ?? "";
                    if (!string.Equals(oldV, newV, StringComparison.Ordinal))
                    {
                        _undo.Push(new PropertyChangeAction(n, "Value", oldV, newV));
                        snap.StringValue = newV;
                        _snap[n] = snap;
                    }
                }
                else if (n is NumberValueNode nv)
                {
                    var oldV = snap.NumberValue;
                    var newV = nv.Value;
                    if (Math.Abs(oldV - newV) > double.Epsilon)
                    {
                        _undo.Push(new PropertyChangeAction(n, "Value", oldV, newV));
                        snap.NumberValue = newV;
                        _snap[n] = snap;
                    }
                }
                else if (n is BoolValueNode bv)
                {
                    var oldV = snap.BoolValue;
                    var newV = bv.Value;
                    if (oldV != newV)
                    {
                        _undo.Push(new PropertyChangeAction(n, "Value", oldV, newV));
                        snap.BoolValue = newV;
                        _snap[n] = snap;
                    }
                }
                return;
            }
        }

        public void UpdateSnapshot(Node n, string prop, object? value)
        {
            if (!_snap.TryGetValue(n, out var s)) s = Capture(n);
            if (prop == "Key") s.Key = value as string ?? "";
            else if (prop == "Value")
            {
                if (n is StringValueNode) s.StringValue = value as string ?? "";
                else if (n is NumberValueNode) s.NumberValue = value is double d ? d : 0d;
                else if (n is BoolValueNode) s.BoolValue = value is bool b && b;
            }
            _snap[n] = s;
        }
    }

    public sealed class RelayCommand : ICommand
    {
        readonly Action<object?> _exec;
        readonly Predicate<object?>? _can;
        public RelayCommand(Action<object?> exec, Predicate<object?>? can = null) { _exec = exec; _can = can; }
        public bool CanExecute(object? parameter) => _can?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _exec(parameter);
        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
