namespace Content.Server._Finster.Surrend;

[RegisterComponent]
public sealed partial class SurrenderComponent : Component
{
    /// <summary>
    /// Knockdown duration in seconds.
    /// </summary>
    [DataField]
    public float StunDuration { get; set; } = 10f;

    /// <summary>
    /// Should entity do stuned by action.
    /// </summary>
    [DataField]
    public bool IsStunable { get; set; } = true;
}
