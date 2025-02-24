using Content.Shared.Destructible.Thresholds;

namespace Content.Client._ViewportGui.ViewportUserInterface.UI;

public class HUDBoxContainer : HUDControl
{
    /// <summary>
    /// Should control is auto sized by adding childs and another events.
    /// </summary>
    public bool IsAutoSized { get; set; } = true;

    public HUDBoxContainer()
    {
    }

    protected override void ChildAdded(HUDControl newChild)
    {
        base.ChildAdded(newChild);

        UpdateSize();
    }

    protected override void ChildRemoved(HUDControl child)
    {
        base.ChildRemoved(child);

        UpdateSize();
    }

    protected override void ChildPositionChanged(HUDControl child)
    {
        base.ChildPositionChanged(child);

        UpdateSize();
    }

    public void UpdateSize()
    {
        if (!IsAutoSized)
            return;

        var maxSize = new Vector2i(0, 0);

        foreach (var child in Children)
        {
            var currentMaxSize = child.Position + child.Size;

            if (currentMaxSize >= maxSize)
                maxSize = currentMaxSize;
        }

        Size = maxSize;
    }
}
