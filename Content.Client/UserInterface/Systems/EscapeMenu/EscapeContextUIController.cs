using Content.Client.UserInterface.Systems.Info;
using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;

namespace Content.Client.UserInterface.Systems.EscapeMenu;

[UsedImplicitly]
public sealed class EscapeContextUIController : UIController
{
    [Dependency] private readonly IInputManager _inputManager = default!;

    [Dependency] private readonly CloseRecentWindowUIController _closeRecentWindowUIController = default!;

    public override void Initialize()
    {
        _inputManager.SetInputCommand(ContentKeyFunctions.EscapeContext,
            InputCmdHandler.FromDelegate(_ => CloseWindowOrOpenGameMenu()));
    }

    private void CloseWindowOrOpenGameMenu()
    {
        if (_closeRecentWindowUIController.HasClosableWindow())
        {
            _closeRecentWindowUIController.CloseMostRecentWindow();
        }
    }
}
