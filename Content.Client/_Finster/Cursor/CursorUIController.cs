using Content.Client.CombatMode;
using Content.Client.Hands.Systems;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.Timing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Content.Client._Finster.Cursor;

public sealed class CursorUIController : UIController
{
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IResourceManager _resManager = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [UISystemDependency] private readonly CombatModeSystem _combatMode = default!;
    [UISystemDependency] private readonly HandsSystem _hands = default!;

    private ICursor? _cursorPressed;
    private ICursor? _cursor;
    private Image<Rgba32>? _cursorPressedImage;
    private Image<Rgba32>? _cursorImage;

    public override void Initialize()
    {
        base.Initialize();

        var stream = _resManager.ContentFileRead($"/Textures/Interface/cursor.png");
        _cursorImage = Image.Load<Rgba32>(stream);

        stream = _resManager.ContentFileRead($"/Textures/Interface/cursor_pressed.png");
        _cursorPressedImage = Image.Load<Rgba32>(stream);
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        // Update cursor
        if (_cursorImage is not null && _cursorPressedImage is not null)
        {
            if (_inputManager.IsKeyDown(Keyboard.Key.MouseLeft) ||
                _inputManager.IsKeyDown(Keyboard.Key.MouseRight))
            {
                _cursorPressed ??= _clyde.CreateCursor(_cursorPressedImage, Vector2i.One);
                if (UIManager.WorldCursor != _cursorPressed)
                    UIManager.WorldCursor = _cursorPressed;
                if (UIManager.ControlFocused is not null)
                    UIManager.ControlFocused.CustomCursorShape = _cursorPressed;
                _clyde.SetCursor(_cursorPressed);
            }
            else
            {
                _cursor ??= _clyde.CreateCursor(_cursorImage, Vector2i.One);
                if (UIManager.WorldCursor != _cursor)
                    UIManager.WorldCursor = _cursor;
                if (UIManager.ControlFocused is not null)
                    UIManager.ControlFocused.CustomCursorShape = _cursor;
                _clyde.SetCursor(_cursor);
            }
        }
    }
}
