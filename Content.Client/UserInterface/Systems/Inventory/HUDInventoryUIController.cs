using System.Linq;
using System.Numerics;
using Content.Client._ViewportGui.ViewportUserInterface;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Client.Gameplay;
using Content.Client.Hands.Systems;
using Content.Client.Inventory;
using Content.Client.Storage.Systems;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Client.UserInterface.Systems.Hands;
using Content.Client.UserInterface.Systems.Inventory.Controls;
using Content.Client.UserInterface.Systems.Inventory.Widgets;
using Content.Client.UserInterface.Systems.Inventory.Windows;
using Content.Client.UserInterface.Systems.Storage;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Input;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Storage;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Utility;
using static Content.Client.Inventory.ClientInventorySystem;

namespace Content.Client.UserInterface.Systems.Inventory;

/// <summary>
/// TODO: Need fix virtual entity rendering;
/// TODO 2: Need add hover items rendering in slots, when we held entity in hands
/// </summary>
public sealed class HUDInventoryUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>,
    IOnSystemChanged<ClientInventorySystem>, IOnSystemChanged<HandsSystem>
{
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly IViewportUserInterfaceManager _vpUIManager = default!; // VPGui edit

    [UISystemDependency] private readonly ClientInventorySystem _inventorySystem = default!;
    [UISystemDependency] private readonly HandsSystem _handsSystem = default!;
    [UISystemDependency] private readonly ContainerSystem _container = default!;

    private InventoryUIController? _inventory;
    private HandsUIController? _hands;
    private StorageUIController? _storage;

    // VPGui edit
    /// <summary>
    /// Should be used to attach all left content to the... Left.
    /// </summary>
    public HUDInventoryPanel? InventoryPanel;
    // VPGui edit end

    private EntityUid? _playerUid;

    // Information about player's inventory
    private InventorySlotsComponent? _playerInventory;
    // Information about player's hands
    private HandsComponent? _playerHandsComponent;

    // We only have two item status controls (left and right hand),
    // but we may have more than two hands.
    // We handle this by having the item status be the *last active* hand of that side.
    // These variables store which that is.
    // ("middle" hands are hardcoded as right, whatever)
    private HUDHandButton? _statusHandLeft;
    private HUDHandButton? _statusHandRight;

    // Current selected hands
    private HUDHandButton? _activeHand = null;

    // Last hovered slot
    private HUDSlotControl? _lastHovered;

    public override void Initialize()
    {
        base.Initialize();

        // VPGui edit
        InventoryPanel = new HUDInventoryPanel(this);
        InventoryPanel.Name = "InventoryPanel";
        InventoryPanel.Texture = _vpUIManager.GetTexturePath("/Textures/Interface/Default/down_panel_background_full.png");
        if (InventoryPanel.Texture is not null)
            InventoryPanel.Size = (InventoryPanel.Texture.Size.X, InventoryPanel.Texture.Size.Y);
        InventoryPanel.Position = (0, 32 * (15 - 1)); // fucking calculus

        _vpUIManager.Root.AddChild(InventoryPanel);
        // VPGui edit end

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreenLoad;

        _inventory = UIManager.GetUIController<InventoryUIController>();
        _hands = UIManager.GetUIController<HandsUIController>();
        _storage = UIManager.GetUIController<StorageUIController>();
    }

    private void OnScreenLoad()
    {
        UpdateInventoryHotbar(_playerInventory);
    }

    public void OnStateEntered(GameplayState state)
    {
        UpdateInventoryHotbar(_playerInventory);
    }

    public void OnStateExited(GameplayState state)
    {
    }

    /*
    public HUDSlotButton CreateSlotButton(SlotData data)
    {
        var button = new HUDSlotButton(data);
        button.HUDSlotGroup = data.SlotDef.HUDSlotGroup;
        button.Pressed += ItemPressed;
        // TODO: Add StoragePressed for container items
        //button.Hover += SlotButtonHovered;

        return button;
    }

    public HUDSlotButton CreateHandButton(SlotData data)
    {
        var button = new HUDSlotButton(data);
        button.HUDSlotGroup = HUDInventoryPanel.HandsGroup;
        button.Pressed += HandPressed;
        // TODO: Add StoragePressed for container items
        //button.Hover += SlotButtonHovered;

        return button;
    }
    */

    // Neuron Activation
    public void OnSystemLoaded(ClientInventorySystem system)
    {
        //if (_inventory is null)
        //    return;

        _inventorySystem.OnSlotAdded += AddSlot;
        _inventorySystem.OnSlotRemoved += RemoveSlot;
        _inventorySystem.OnLinkInventorySlots += LoadSlots;
        _inventorySystem.OnUnlinkInventory += UnloadSlots;
        _inventorySystem.OnSpriteUpdate += SpriteUpdated;
    }

    // Neuron Deactivation
    public void OnSystemUnloaded(ClientInventorySystem system)
    {
        //if (_inventory is null)
        //    return;

        _inventorySystem.OnSlotAdded -= AddSlot;
        _inventorySystem.OnSlotRemoved -= RemoveSlot;
        _inventorySystem.OnLinkInventorySlots -= LoadSlots;
        _inventorySystem.OnUnlinkInventory -= UnloadSlots;
        _inventorySystem.OnSpriteUpdate -= SpriteUpdated;
    }

    public void SetHand(string handName)
    {
        EntityManager.RaisePredictiveEvent(new RequestSetHandEvent(handName));
    }

    public void ItemPressed(GUIBoundKeyEventArgs args, HUDSlotControl control)
    {
        var slot = control.SlotName;

        if (args.Function == ContentKeyFunctions.MoveStoredItem) // TODO: Becacuse UIClick doesn't work
        {
            _inventorySystem.UIInventoryActivate(control.SlotName);
            args.Handle();
            return;
        }

        if (_playerInventory == null || _playerUid == null)
        {
            return;
        }

        if (args.Function == EngineKeyFunctions.TextCursorSelect) // (args.Function == ContentKeyFunctions.ExamineEntity)
        {
            _inventorySystem.UIInventoryExamine(slot, _playerUid.Value);
        }
        else if (args.Function == EngineKeyFunctions.UIRightClick) // (args.Function == EngineKeyFunctions.UseSecondary)
        {
            _inventorySystem.UIInventoryOpenContextMenu(slot, _playerUid.Value);
        }
        else if (args.Function == ContentKeyFunctions.ActivateItemInWorld)
        {
            _inventorySystem.UIInventoryActivateItem(slot, _playerUid.Value);
        }
        else if (args.Function == ContentKeyFunctions.AltActivateItemInWorld)
        {
            _inventorySystem.UIInventoryAltActivateItem(slot, _playerUid.Value);
        }
        else
        {
            return;
        }

        args.Handle();
    }

    public void HandPressed(GUIBoundKeyEventArgs args, HUDSlotControl hand)
    {
        if (_playerHandsComponent == null)
        {
            return;
        }

        if (args.Function == ContentKeyFunctions.MoveStoredItem)
        {
            _handsSystem.UIHandClick(_playerHandsComponent, hand.SlotName);
        }
        else if (args.Function == EngineKeyFunctions.UIRightClick) // (args.Function == EngineKeyFunctions.UseSecondary)
        {
            _handsSystem.UIHandOpenContextMenu(hand.SlotName);
        }
        else if (args.Function == ContentKeyFunctions.ActivateItemInWorld)
        {
            _handsSystem.UIHandActivate(hand.SlotName);
        }
        else if (args.Function == ContentKeyFunctions.AltActivateItemInWorld)
        {
            _handsSystem.UIHandAltActivateItem(hand.SlotName);
        }
        else if (args.Function == EngineKeyFunctions.TextCursorSelect) // (args.Function == ContentKeyFunctions.ExamineEntity)
        {
            _handsSystem.UIInventoryExamine(hand.SlotName);
        }
    }

    private void StoragePressed(GUIBoundKeyEventArgs args, SlotControl control)
    {
        _inventorySystem.UIInventoryStorageActivate(control.SlotName);
    }

    private void SlotButtonHovered(GUIMouseHoverEventArgs args, HUDSlotControl control)
    {
        _lastHovered = control;
    }

    private void AddSlot(SlotData data)
    {
        InventoryPanel?.UpdateSlots(_playerInventory);
    }

    private void RemoveSlot(SlotData data)
    {
        InventoryPanel?.UpdateSlots(_playerInventory);
    }

    private void AddHand(string handName, HandLocation location)
    {
        InventoryPanel?.UpdateHands(_playerHandsComponent);
    }

    private void RemoveHand(string handName)
    {
        InventoryPanel?.UpdateHands(_playerHandsComponent);
        //RemoveHand(handName, out var _);
    }

    private bool RemoveHand(string handName, out HUDHandButton? handButton)
    {
        handButton = null;
        return false;
    }

    private void LoadSlots(EntityUid clientUid, InventorySlotsComponent clientInv)
    {
        UnloadSlots();
        _playerUid = clientUid;
        _playerInventory = clientInv;

        UpdateInventoryHotbar(_playerInventory);
    }

    private void UnloadSlots()
    {
        _playerUid = null;
        _playerInventory = null;

        UpdateInventoryHotbar(_playerInventory);
    }

    private void UpdateInventoryHotbar(InventorySlotsComponent? clientInv)
    {
        InventoryPanel?.UpdateSlots(clientInv);

        if (clientInv is null)
            return;

        foreach (var (_, data) in clientInv.SlotData)
        {
            var showStorage = _entities.HasComponent<StorageComponent>(data.HeldEntity);
            var update = new SlotSpriteUpdate(data.HeldEntity, data.SlotGroup, data.SlotName, showStorage);
            SpriteUpdated(update);
        }
    }

    private void UpdateHandsHotbar(HandsComponent? handsComp)
    {
        InventoryPanel?.UpdateHands(handsComp);

        if (handsComp is null)
            return;

        foreach (var (_, data) in handsComp.Hands)
        {
            var handName = data.Name;
            var entity = data.HeldEntity;

            // TODO: Move into single method?
            if (_entities.TryGetComponent(entity, out VirtualItemComponent? virt))
            {
                // TODO: By some reasons we can'not render virtual items.
                // And Action events works shity. So - just block hands, instead of show pulling or vitual items.
                //virt.BlockingEntity
                InventoryPanel?.SetHandEntity(handName, null, true);
            }
            else
            {
                InventoryPanel?.SetHandEntity(handName, entity);
            }
        }
    }

    private void SpriteUpdated(SlotSpriteUpdate update)
    {
        var (entity, group, name, showStorage) = update;

        if (_entities.TryGetComponent(entity, out VirtualItemComponent? virtb))
            InventoryPanel?.SetSlotEntity(name, virtb.BlockingEntity, true);
        else
            InventoryPanel?.SetSlotEntity(name, entity);
    }

    // Monkey Sees Action
    // Neuron Activation
    // Monkey copies code
    public void OnSystemLoaded(HandsSystem system)
    {
        if (_hands is null)
            return;

        _hands.OnPlayerAddHand += OnAddHand;
        _hands.OnPlayerItemAdded += OnHandItemAdded;
        _hands.OnPlayerItemRemoved += OnHandItemRemoved;
        _hands.OnPlayerSetActiveHand += SetActiveHand;
        _hands.OnPlayerRemoveHand += RemoveHand;
        _hands.OnPlayerHandsAdded += LoadPlayerHands;
        _hands.OnPlayerHandsRemoved += UnloadPlayerHands;
        _hands.OnPlayerHandBlocked += HandBlocked;
        _hands.OnPlayerHandUnblocked += HandUnblocked;
    }

    public void OnSystemUnloaded(HandsSystem system)
    {
        if (_hands is null)
            return;

        _hands.OnPlayerAddHand -= OnAddHand;
        _hands.OnPlayerItemAdded -= OnHandItemAdded;
        _hands.OnPlayerItemRemoved -= OnHandItemRemoved;
        _hands.OnPlayerSetActiveHand -= SetActiveHand;
        _hands.OnPlayerRemoveHand -= RemoveHand;
        _hands.OnPlayerHandsAdded -= LoadPlayerHands;
        _hands.OnPlayerHandsRemoved -= UnloadPlayerHands;
        _hands.OnPlayerHandBlocked -= HandBlocked;
        _hands.OnPlayerHandUnblocked -= HandUnblocked;
    }

    private void OnAddHand(string name, HandLocation location)
    {
        InventoryPanel?.UpdateHands(_playerHandsComponent);
    }

    private void OnHandItemAdded(string name, EntityUid entity)
    {
        if (_entities.TryGetComponent(entity, out VirtualItemComponent? virt))
        {
            // TODO: By some reasons we can'not render virtual items.
            // And Action events works shity. So - just block hands, instead of show pulling or vitual items.
            //virt.BlockingEntity
            InventoryPanel?.SetHandEntity(name, null, true);
        }
        else
        {
            InventoryPanel?.SetHandEntity(name, entity);
        }
    }

    private void OnHandItemRemoved(string name, EntityUid entity)
    {
        InventoryPanel?.SetHandEntity(name, null);
    }

    private void SetActiveHand(string? handName)
    {
        InventoryPanel?.SetActiveHand(handName);
    }

    private void LoadPlayerHands(HandsComponent handsComp)
    {
        _playerHandsComponent = handsComp;

        UpdateHandsHotbar(_playerHandsComponent);
    }

    private void UnloadPlayerHands()
    {
        _playerHandsComponent = null;

        UpdateHandsHotbar(_playerHandsComponent);
    }

    private void HandBlocked(string handName)
    {
        InventoryPanel?.SetHandEntity(handName, null, true, doNotSetEntity: true);
    }

    private void HandUnblocked(string handName)
    {
        InventoryPanel?.SetHandEntity(handName, null, false, doNotSetEntity: true);
    }
}
