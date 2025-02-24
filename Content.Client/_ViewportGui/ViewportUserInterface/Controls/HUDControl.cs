using System.Collections;
using System.Linq;
using Robust.Client.UserInterface;
using Robust.Shared.Timing;

namespace Content.Client._ViewportGui.ViewportUserInterface.UI;

public class HUDControl : IDisposable
{
    private Vector2i _globalPosition = Vector2i.Zero;
    private Vector2i _position = Vector2i.Zero;
    private Vector2i _size = Vector2i.Zero;

    private bool _visible = true;

    /// <summary>
    /// Do not use it! It works like Control._orderedChildren. It is public, because there is some CSharp's limits.
    /// </summary>
    public readonly List<HUDControl> OrderedChildren = new();

    /// <summary>
    /// Global position in Viewport's UI.
    /// </summary>
    public Vector2i GlobalPosition
    {
        get => _globalPosition;
        internal set
        {
            _globalPosition = value;
        }
    }

    /// <summary>
    /// Local position in his parent's position.
    /// </summary>
    public Vector2i Position
    {
        get => _position;
        set
        {
            _position = value;

            if (Parent is not null)
                ChildPositionChanged(this);

            ValidateGlobalPosition();
        }
    }

    /// <summary>
    /// Control size.
    /// </summary>
    public Vector2i Size
    {
        get => _size;
        set
        {
            _size = value;
        }
    }

    /// <summary>
    /// Set visibility of control. Might be useful, if you don't wanna recreate element each time.
    /// </summary>
    public bool Visible
    {
        get => _visible;
        set
        {
            _visible = value;
        }
    }

    /// <summary>
    /// Check, if control can be visible in the tree.
    /// </summary>
    public bool VisibleInTree
    {
        get
        {
            for (var parent = this; parent != null; parent = parent.Parent)
            {
                if (!parent.Visible)
                {
                    return false;
                }

                if (parent is HUDRoot)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public bool Disposed { get; private set; }

    public event Action<HUDControlChildMovedEventArgs>? OnChildMoved;
    public event Action<HUDControl>? OnChildAdded;
    public event Action<HUDControl>? OnChildRemoved;

    public event Action<GUIBoundKeyEventArgs>? OnKeyBindDown;
    public event Action<GUIBoundKeyEventArgs>? OnKeyBindUp;

    public HUDControl? Parent { get; private set; }
    public HUDOrderedChildCollection Children { get; }
    public int ChildCount => OrderedChildren.Count;

    public string? Name { get; set; }

    public HUDMouseFilterMode MouseFilter { get; set; } = HUDMouseFilterMode.Ignore;

    public HUDControl()
    {
        Children = new HUDOrderedChildCollection(this);
        Disposed = false;
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

    /// <summary>
    ///     Sets the index of this control in the parent.
    ///     This pretty much corresponds to layout and drawing order in relation to its siblings.
    /// </summary>
    /// <param name="position"></param>
    /// <exception cref="InvalidOperationException">This control has no parent.</exception>
    public void SetPositionInParent(int position)
    {
        if (Parent == null)
        {
            throw new InvalidOperationException("No parent to change position in.");
        }

        var posInParent = GetPositionInParent();
        if (posInParent == position)
        {
            return;
        }

        // If it was at the top index and we re-add it there then don't throw.
        Parent.OrderedChildren.RemoveAt(posInParent);

        if (position == Parent.OrderedChildren.Count)
        {
            Parent.OrderedChildren.Add(this);
        }
        else
        {
            Parent.OrderedChildren.Insert(position, this);
        }

        Parent.ChildMoved(this, posInParent, position);
    }

    /// <summary>
    ///     Makes this the first control among its siblings,
    ///     So that it's first in things such as drawing order.
    /// </summary>
    /// <exception cref="InvalidOperationException">This control has no parent.</exception>
    public void SetPositionFirst()
    {
        SetPositionInParent(0);
    }

    /// <summary>
    ///     Makes this the last control among its siblings,
    ///     So that it's last in things such as drawing order.
    /// </summary>
    /// <exception cref="InvalidOperationException">This control has no parent.</exception>
    public void SetPositionLast()
    {
        if (Parent == null)
        {
            throw new InvalidOperationException("No parent to change position in.");
        }

        SetPositionInParent(Parent.ChildCount - 1);
    }

    /// <summary>
    ///     Gets the "index" in the parent.
    ///     This index is used for ordering of actions like input and drawing among siblings.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if this control has no parent.
    /// </exception>
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
        // Update GlobalPosition for childs
        child.ValidateGlobalPosition();

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

    public virtual void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        if (VisibleInTree)
            OnKeyBindDown?.Invoke(args);
    }

    public virtual void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        if (VisibleInTree)
            OnKeyBindUp?.Invoke(args);
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

    // TODO: Move into one "Invalidate" or "Validate" method?
    public void ValidateGlobalPosition()
    {
        if (Parent is null)
            GlobalPosition = _position;
        else
            GlobalPosition = Parent.GlobalPosition + _position;

        foreach (var child in Children.ToArray())
        {
            child.ValidateGlobalPosition();
        }
    }

    // TODO: Move into one "Invalidate" or "Validate" method?
    protected virtual void ChildPositionChanged(HUDControl child)
    {
    }

    /// <summary>
    ///     Make this child an orphan. i.e. remove it from its parent if it has one.
    /// </summary>
    public void Orphan()
    {
        Parent?.RemoveChild(this);
    }

    /// <summary>
    ///     This is called before every render frame.
    /// </summary>
    public virtual void FrameUpdate(FrameEventArgs args)
    {
        foreach (var child in Children.ToArray())
        {
            child.FrameUpdate(args);
        }
    }

    /// <summary>
    ///     This is called when we need draw the element.
    /// </summary>
    public virtual void Draw(in ViewportUIDrawArgs args)
    {
        foreach (var child in Children.ToArray())
        {
            if (child.VisibleInTree)
                child.Draw(args);
        }
    }

    public void Dispose()
    {
        if (Disposed)
        {
            return;
        }

        Dispose(true);
        Disposed = true;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        DisposeAllChildren();
        Orphan();

        OnKeyBindDown = null;
    }
}

public enum HUDMouseFilterMode : byte
{
    /// <summary>
    ///     The control will not be considered at all, and will not have any effects.
    /// </summary>
    Ignore = 0,

    /// <summary>
    ///     The control will be able to receive mouse buttons events.
    ///     Furthermore, if a control with this mode does get clicked,
    ///     the event automatically gets marked as handled after every other candidate has been tried,
    ///     so that the rest of the game does not receive it.
    /// </summary>
    Pass = 1,

    /// <summary>
    ///     The control will be able to receive mouse button events like <see cref="Pass" />,
    ///     but the event will be stopped and handled even if the relevant events do not handle it.
    /// </summary>
    Stop = 2,
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
