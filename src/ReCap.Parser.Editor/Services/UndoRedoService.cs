using System;
using System.Collections.Generic;
using ReCap.Parser.Editor.Models;
using ReCap.Parser.Editor.ViewModels;

namespace ReCap.Parser.Editor.Services
{
    public interface IUndoableAction
    {
        void Undo(MainViewModel vm);
        void Redo(MainViewModel vm);
        bool TryCoalesce(IUndoableAction next, TimeSpan window);
        DateTime Timestamp { get; }
    }

    public sealed class UndoRedoService
    {
        readonly Stack<IUndoableAction> _undo = new();
        readonly Stack<IUndoableAction> _redo = new();
        readonly int _capacity;
        readonly TimeSpan _coalesceWindow;
        public event Action? Changed;

        public UndoRedoService(int capacity = 256, int coalesceMs = 700)
        {
            _capacity = capacity;
            _coalesceWindow = TimeSpan.FromMilliseconds(coalesceMs);
        }

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;

        public void Clear()
        {
            _undo.Clear();
            _redo.Clear();
            Changed?.Invoke();
        }

        public void Push(IUndoableAction action)
        {
            if (_undo.Count > 0)
            {
                var top = _undo.Peek();
                if (top.TryCoalesce(action, _coalesceWindow))
                {
                    Changed?.Invoke();
                    return;
                }
            }
            _undo.Push(action);
            while (_undo.Count > _capacity) _undo.TryPop(out _);
            _redo.Clear();
            Changed?.Invoke();
        }

        public void Undo(MainViewModel vm)
        {
            if (_undo.Count == 0) return;
            var a = _undo.Pop();
            vm.IsApplyingUndoRedo = true;
            try { a.Undo(vm); } finally { vm.IsApplyingUndoRedo = false; }
            _redo.Push(a);
            Changed?.Invoke();
        }

        public void Redo(MainViewModel vm)
        {
            if (_redo.Count == 0) return;
            var a = _redo.Pop();
            vm.IsApplyingUndoRedo = true;
            try { a.Redo(vm); } finally { vm.IsApplyingUndoRedo = false; }
            _undo.Push(a);
            Changed?.Invoke();
        }
    }

    public sealed class PropertyChangeAction : IUndoableAction
    {
        public readonly Node Node;
        public readonly string PropertyName;
        public object? OldValue;
        public object? NewValue;
        public DateTime Timestamp { get; private set; }

        public PropertyChangeAction(Node node, string propertyName, object? oldValue, object? newValue)
        {
            Node = node;
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
            Timestamp = DateTime.UtcNow;
        }

        public void Undo(MainViewModel vm)
        {
            Apply(Node, PropertyName, OldValue);
            vm.UpdateSnapshot(Node, PropertyName, OldValue);
        }

        public void Redo(MainViewModel vm)
        {
            Apply(Node, PropertyName, NewValue);
            vm.UpdateSnapshot(Node, PropertyName, NewValue);
        }

        public bool TryCoalesce(IUndoableAction next, TimeSpan window)
        {
            if (next is not PropertyChangeAction p) return false;
            if (!ReferenceEquals(Node, p.Node)) return false;
            if (!string.Equals(PropertyName, p.PropertyName, StringComparison.Ordinal)) return false;
            if (p.Timestamp - Timestamp > window) return false;
            NewValue = p.NewValue;
            Timestamp = p.Timestamp;
            return true;
        }

        static void Apply(Node node, string prop, object? value)
        {
            if (prop == "Key")
            {
                node.Key = value as string ?? "";
                return;
            }
            if (prop == "Value")
            {
                if (node is StringValueNode sv) sv.Value = value as string ?? "";
                else if (node is NumberValueNode nv) nv.Value = value is double d ? d : 0d;
                else if (node is BoolValueNode bv) bv.Value = value is bool b && b;
                return;
            }
        }
    }

    public sealed class ReplaceNodeAction : IUndoableAction
    {
        public readonly Node? Parent;
        public readonly int Index;
        public readonly Node OldNode;
        public readonly Node NewNode;
        public DateTime Timestamp { get; } = DateTime.UtcNow;

        public ReplaceNodeAction(Node? parent, int index, Node oldNode, Node newNode)
        {
            Parent = parent;
            Index = index;
            OldNode = oldNode;
            NewNode = newNode;
        }

        public void Undo(MainViewModel vm)
        {
            vm.ReplaceNodeRaw(Parent, Index, NewNode, OldNode);
        }

        public void Redo(MainViewModel vm)
        {
            vm.ReplaceNodeRaw(Parent, Index, OldNode, NewNode);
        }

        public bool TryCoalesce(IUndoableAction next, TimeSpan window)
        {
            return false;
        }
    }
}
