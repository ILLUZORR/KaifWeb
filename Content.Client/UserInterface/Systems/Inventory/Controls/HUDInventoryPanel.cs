using System.Linq;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Client.Inventory;
using Content.Shared.Destructible.Thresholds;
using Content.Shared.Hands.Components;
using Robust.Client.UserInterface;
using Robust.Shared.Timing;
using static Content.Client.Inventory.ClientInventorySystem;

namespace Content.Client.UserInterface.Systems.Inventory.Controls;

/// <summary>
/// Left panel of the HUD.
/// </summary>
public class HUDInventoryPanel : HUDTextureRect
{
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    private HUDInventoryUIController _controller;
    private HUDHandButton? _activeHand;

    /// <summary>
    /// Used by inventoryTemplate. We should separate some buttons into groups.
    /// </summary>
    public const string MiscGroup = "Misc";
    /// <summary>
    /// DON'T USE IT WITH inventoryTemplate! It is only for hands definition on UIControler's layer.
    /// </summary>
    public const string HandsGroup = "Hands";

    public static int DefaultButtonSize => HUDSlotControl.DefaultButtonSize;

    /// <summary>
    /// Contain all slots from <seealso cref="HUDBoxContainer"'s in single list./>
    /// </summary>
    public List<HUDSlotControl> Slots { get; private set; } = new();

    /// <summary>
    /// Slots, what should be visible everytime.
    /// </summary>
    public HUDBoxContainer SlotsContainer;

    /// <summary>
    /// Unnecessary slots, what can be toggled by player.
    /// </summary>
    public HUDBoxContainer MiscSlotsContainer;

    /// <summary>
    /// For hands!
    /// </summary>
    public HUDBoxContainer HandsContainer;

    /// <summary>
    /// Enable/Disable visible for unnecessary (MiscSlots)
    /// </summary>
    public HUDTextureButton ToggleMiscSlotsButton;

    public HUDInventoryPanel(HUDInventoryUIController controller)
    {
        IoCManager.InjectDependencies(this);

        _controller = controller;

        SlotsContainer = new();
        SlotsContainer.OnChildAdded += AddChildToSlots;
        SlotsContainer.OnChildRemoved += RemoveChildFromSlots;
        AddChild(SlotsContainer);

        MiscSlotsContainer = new();
        MiscSlotsContainer.OnChildAdded += AddChildToSlots;
        MiscSlotsContainer.OnChildRemoved += RemoveChildFromSlots;
        AddChild(MiscSlotsContainer);

        HandsContainer = new();
        HandsContainer.OnChildAdded += AddChildToSlots;
        HandsContainer.OnChildRemoved += RemoveChildFromSlots;
        AddChild(HandsContainer);

        ToggleMiscSlotsButton = new();
        ToggleMiscSlotsButton.Texture = _uiManager.CurrentTheme.ResolveTexture("slots_toggle");
        if (ToggleMiscSlotsButton.Texture != null)
            ToggleMiscSlotsButton.Size = ToggleMiscSlotsButton.Texture.Size;
        ToggleMiscSlotsButton.OnPressed += (_) =>
        {
            ToggleMiscSlots();
        };
        AddChild(ToggleMiscSlotsButton);
    }

    private void AddChildToSlots(HUDControl child)
    {
        var slot = child as HUDSlotControl;
        if (slot != null)
            Slots.Add(slot);
    }

    private void RemoveChildFromSlots(HUDControl child)
    {
        HUDSlotControl? childToRemove = null;
        foreach (var slot in Slots)
        {
            if (slot.Name == child.Name)
            {
                childToRemove = slot;
                break;
            }
        }
        if (childToRemove != null)
            Slots.Remove(childToRemove);
    }

    private void ToggleMiscSlots()
    {
        MiscSlotsContainer.Visible = !MiscSlotsContainer.Visible;
    }

    public void UpdateSlots(InventorySlotsComponent? invComp)
    {
        ClearSlots();

        // Inventory slots
        if (invComp is not null)
        {
            foreach (var (_, data) in invComp.SlotData)
            {
                CreateSlotButton(data);
            }

            // TODO: Fuck! Need to think how i can set position by better way
            /*
            SlotsContainer.Position = (0, Math.Clamp(0 - (SlotsContainer.Size.Y - DefaultButtonSize), -480, 0));
            if (HandsContainer.Position.Y == 0 && HandsContainer.Position.X == 0)
                MiscSlotsContainer.Position = (0, 0);
            else
                MiscSlotsContainer.Position = (HandsContainer.Size.X, 0);
            */
            SlotsContainer.Position = (0, -64);
            MiscSlotsContainer.Position = (HandsContainer.Size.X, -64);
        }

        UpdateToggleMiscButton();
    }

    public void ClearSlots()
    {
        SlotsContainer.RemoveAllChildren();
        SlotsContainer.Size = (0, 0);
        SlotsContainer.Position = (0, 0);

        MiscSlotsContainer.RemoveAllChildren();
        MiscSlotsContainer.Size = (0, 0);
        MiscSlotsContainer.Position = (0, 0);
    }

    public void UpdateHands(HandsComponent? handsComp)
    {
        ClearHands();

        // Hand slots
        if (handsComp is not null)
        {
            foreach (var (name, data) in handsComp.Hands)
            {
                CreateHandButton(name, data, handsComp);
            }

            // TODO: Fuck! Need to think, how i can move my hands in some slot by better way
            /*
            HandsContainer.Position = (0, SlotsContainer.Position.Y + DefaultButtonSize);
            MiscSlotsContainer.Position = (HandsContainer.Size.X, 0);
            */
            HandsContainer.Position = (0, 0 - DefaultButtonSize);
            MiscSlotsContainer.Position = (HandsContainer.Size.X, -64);

            if (handsComp.ActiveHand != null)
                SetActiveHand(handsComp.ActiveHand.Name);
        }

        UpdateToggleMiscButton();
    }

    public void ClearHands()
    {
        HandsContainer.RemoveAllChildren();
        HandsContainer.Size = (0, 0);
        HandsContainer.Position = (0, 0);
    }

    // FIXME: Make it looks better?
    private void UpdateToggleMiscButton()
    {
        if (MiscSlotsContainer.ChildCount >= 1)
        {
            ToggleMiscSlotsButton.Visible = true;
        }
        else
        {
            ToggleMiscSlotsButton.Visible = false;
            return;
        }

        // Calculate position for the button.
        ToggleMiscSlotsButton.Position = (HandsContainer.Size.X, HandsContainer.Position.Y);
    }

    private HUDSlotButton CreateSlotButton(SlotData data)
    {
        var slotDef = data.SlotDef;

        var slotButton = new HUDSlotButton(data);
        slotButton.Position = slotDef.HUDWindowPosition * DefaultButtonSize;
        slotButton.Pressed += _controller.ItemPressed;

        if (slotDef.HUDSlotGroup == MiscGroup)
            MiscSlotsContainer.AddChild(slotButton);
        else
            SlotsContainer.AddChild(slotButton);

        return slotButton;
    }

    private HUDHandButton CreateHandButton(string name, Hand data, HandsComponent? handsComp = null)
    {
        var parentChildCount = HandsContainer.ChildCount;
        var handButton = new HUDHandButton(name, data.Location);
        handButton.Position = (parentChildCount * DefaultButtonSize, 0);
        handButton.Pressed += _controller.HandPressed;

        HandsContainer.AddChild(handButton);

        // TODO: Move to another UI's button. Because it works sucks
        // Add switch hands button. It only shows when entity has 2 hands (left and right)
        if (handsComp != null &&
            parentChildCount <= 0 &&
            handsComp.Hands.Count == 2) // because two hands
        {
            var switchHandsButton = new HUDHandButton("switchbutton", data.Location);
            switchHandsButton.Position = ((parentChildCount + 1) * DefaultButtonSize, 0);
            switchHandsButton.OnPressed += (_) =>
            {
                SwitchHands(switchHandsButton);
            };

            // Force texture update
            if (handsComp.ActiveHand != null)
            {
                if (handsComp.ActiveHand.Location == HandLocation.Right)
                    switchHandsButton.ButtonTexturePath = "Buttons/hands_right";
                else
                    switchHandsButton.ButtonTexturePath = "Buttons/hands_left";
            }

            HandsContainer.AddChild(switchHandsButton);
        }

        return handButton;
    }

    // Use only with switch hand button
    private void SwitchHands(HUDHandButton switchButton)
    {
        if (_activeHand is null)
            return;

        var curIdx = _activeHand.GetPositionInParent();

        if (curIdx + 2 >= HandsContainer.ChildCount)
            curIdx = 0;
        else
            curIdx += 2;

        var childsList = HandsContainer.Children.ToArray();
        var hand = childsList[curIdx] as HUDHandButton;
        if (hand is null || hand.Name == switchButton.Name)
            return;

        _controller.SetHand(hand.HandName);
    }

    public void SetActiveHand(string? handName)
    {
        if (_activeHand is not null)
            _activeHand.Highlight = false;

        HUDHandButton? switchHandsButton = null;

        foreach (var child in HandsContainer.Children)
        {
            var hand = child as HUDHandButton;

            if (hand is null)
                continue;

            // For updating switch button
            if (hand.HandName == "switchbutton")
                switchHandsButton = hand;

            if (hand.HandName == handName)
            {
                _activeHand = hand;
                _activeHand.Highlight = true;
            }
        }

        if (switchHandsButton is null || _activeHand is null)
            return;

        if (_activeHand.HandLocation == HandLocation.Right)
            switchHandsButton.ButtonTexturePath = "Buttons/hands_right";
        else
            switchHandsButton.ButtonTexturePath = "Buttons/hands_left";
    }

    public void SetHandEntity(string? handName, EntityUid? ent, bool blocked = false, bool doNotSetEntity = false)
    {
        if (handName == null)
            return;

        HUDHandButton? handButton = null;
        foreach (var child in HandsContainer.Children)
        {
            var childButton = child as HUDHandButton;
            if (childButton is null || childButton.HandName != handName)
                continue;
            else
                handButton = childButton;
        }
        if (handButton == null)
            return;

        if (!doNotSetEntity)
            handButton.SetEntity(ent);
        handButton.Blocked = blocked;
    }

    public void SetSlotEntity(string? slotName, EntityUid? ent, bool blocked = false)
    {
        if (slotName == null)
            return;

        var slotButton = Slots.FirstOrDefault(control => control.SlotName == slotName);
        if (slotButton == null)
            return;

        slotButton.SetEntity(ent);
        slotButton.Blocked = blocked;
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
    }
}
