using Content.Client._ViewportGui.ViewportUserInterface.UI;

namespace Content.Client.UserInterface.Systems.Inventory.Controls;

/// <summary>
/// Left panel of the HUD.
/// </summary>
public class HUDInventoryPanel : HUDTextureRect
{
    /// <summary>
    /// Slots, what should be visible everytime.
    /// </summary>
    private HUDBoxContainer SlotsContainer;

    /// <summary>
    /// Unnecessary slots, what can be toggled by player.
    /// </summary>
    private HUDBoxContainer MiscSlotsContainer;

    /// <summary>
    /// For hands!
    /// </summary>
    private HUDBoxContainer HandsContainer;

    /// <summary>
    /// Enable/Disable visible for unnecessary (MiscSlots)
    /// </summary>
    private HUDToggleSlotsButton ToggleMiscSlotsButton;

    public HUDInventoryPanel()
    {
        SlotsContainer = new();
        AddChild(SlotsContainer);

        MiscSlotsContainer = new();
        AddChild(MiscSlotsContainer);

        HandsContainer = new();
        AddChild(HandsContainer);

        ToggleMiscSlotsButton = new();
        ToggleMiscSlotsButton.OnPressed += (_) =>
        {
            ToggleMiscSlots();
        };
        AddChild(ToggleMiscSlotsButton);
    }

    public void ToggleMiscSlots()
    {
        MiscSlotsContainer.Visible = !MiscSlotsContainer.Visible;
    }

    /// <summary>
    /// Update slots: position, size, toggle button position and etc.
    /// </summary>
    public void UpdateSlots()
    {
    }
}
