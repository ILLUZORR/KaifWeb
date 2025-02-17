using System.Collections;
using System.Linq;

namespace Content.Client._ViewportGui.ViewportUserInterface.UI;

public class HUDControl : IDisposable
{
    public readonly List<HUDControl> OrderedChildren = new();

    public Vector2i Position { get; set; }
    public Vector2i Size { get; set; }

    public event Action<HUDControlChildMovedEventArgs>? OnChildMoved;
    public event Action<HUDControl>? OnChildAdded;
    public event Action<HUDControl>? OnChildRemoved;

    public HUDControl? Parent { get; private set; }

    public HUDOrderedChildCollection Children { get; }

    public int ChildCount => OrderedChildren.Count;

    public string? Name { get; set; }

    public HUDControl()
    {
        Children = new HUDOrderedChildCollection(this);
    }

    public HUDControl GetChild(int index)
    {
        return OrderedChildren[index];
    }

    protected virtual void ChildMoved(HUDControl child, int oldIndex, int newIndex)
    {
        OnChildMoved?.Invoke(new HUDControlChildMovedEventArgs(child, oldIndex, newIndex));
    }

    protected virtual void Deparented()
    {
    }

    public int GetPositionInParent()
    {
        if (Parent == null)
        {
            throw new InvalidOperationException("This control has no parent!");
        }

        return Parent.OrderedChildren.IndexOf(this);
    }

    public void AddChild(HUDControl child)
    {
        if (child.Parent != null)
        {
            throw new InvalidOperationException("This component is still parented. Deparent it before adding it.");
        }

        if (child == this)
        {
            throw new InvalidOperationException("You can't parent something to itself!");
        }

        // Ensure this control isn't a parent of ours.
        // Doesn't need to happen if the control has no children of course.
        if (child.ChildCount != 0)
        {
            for (var parent = Parent; parent != null; parent = parent.Parent)
            {
                if (parent == child)
                {
                    throw new ArgumentException("This control is one of our parents!", nameof(child));
                }
            }
        }

        child.Parent = this;
        OrderedChildren.Add(child);

        ChildAdded(child);
    }

    public void RemoveChild(HUDControl child)
    {
        if (child.Parent != this)
        {
            throw new InvalidOperationException("The provided control is not a direct child of this control.");
        }

        var childIndex = OrderedChildren.IndexOf(child);
        RemoveChild(childIndex);
    }

    public void RemoveChild(int childIndex)
    {
        var child = OrderedChildren[childIndex];
        OrderedChildren.RemoveAt(childIndex);

        child.Parent = null;

        child.Deparented();

        ChildRemoved(child);
    }

    protected virtual void ChildRemoved(HUDControl child)
    {
        OnChildRemoved?.Invoke(child);
    }

    protected virtual void ChildAdded(HUDControl newChild)
    {
        OnChildAdded?.Invoke(newChild);
    }

    public void DisposeAllChildren()
    {
        // Cache because the children modify the dictionary.
        var children = new List<HUDControl>(Children);
        foreach (var child in children)
        {
            child.Dispose();
        }
    }

    public void RemoveAllChildren()
    {
        foreach (var child in Children.ToArray())
        {
            // This checks fails in some obscure cases like using the element inspector in the dev window.
            // Why? Well I could probably spend 15 minutes in a debugger to find out,
            // but I'd probably still end up with this fix.
            if (child.Parent == this)
                RemoveChild(child);
        }
    }

    /// <summary>
    ///     Make this child an orphan. i.e. remove it from its parent if it has one.
    /// </summary>
    public void Orphan()
    {
        Parent?.RemoveChild(this);
    }

    public virtual void Draw(in ViewportUIDrawArgs args)
    {
        foreach (var child in Children.ToArray())
        {
            child.Draw(args);
        }
    }

    public virtual void Dispose()
    {
    }
}

public sealed class HUDOrderedChildCollection : ICollection<HUDControl>, IReadOnlyCollection<HUDControl>
{
    private readonly HUDControl Owner;

    public HUDOrderedChildCollection(HUDControl owner)
    {
        Owner = owner;
    }

    public Enumerator GetEnumerator()
    {
        return new(Owner);
    }

    IEnumerator<HUDControl> IEnumerable<HUDControl>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(HUDControl item)
    {
        Owner.AddChild(item);
    }

    public void Clear()
    {
        Owner.RemoveAllChildren();
    }

    public bool Contains(HUDControl item)
    {
        return item?.Parent == Owner;
    }

    public void CopyTo(HUDControl[] array, int arrayIndex)
    {
        Owner.OrderedChildren.CopyTo(array, arrayIndex);
    }

    public bool Remove(HUDControl item)
    {
        if (item?.Parent != Owner)
        {
            return false;
        }

        Owner.RemoveChild(item);

        return true;
    }

    int ICollection<HUDControl>.Count => Owner.ChildCount;
    int IReadOnlyCollection<HUDControl>.Count => Owner.ChildCount;

    public bool IsReadOnly => false;


    public struct Enumerator : IEnumerator<HUDControl>
    {
        private List<HUDControl>.Enumerator _enumerator;

        internal Enumerator(HUDControl control)
        {
            _enumerator = control.OrderedChildren.GetEnumerator();
        }

        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
            ((IEnumerator) _enumerator).Reset();
        }

        public HUDControl Current => _enumerator.Current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }
}

public readonly struct HUDControlChildMovedEventArgs
{
    public HUDControlChildMovedEventArgs(HUDControl control, int oldIndex, int newIndex)
    {
        Control = control;
        OldIndex = oldIndex;
        NewIndex = newIndex;
    }

    public readonly HUDControl Control;
    public readonly int OldIndex;
    public readonly int NewIndex;
}
