namespace Content.Shared._Shitmed.Targeting;


/// <summary>
/// Represents and enum of possible target parts.
/// </summary>
/// <remarks>
/// To get all body parts as an Array, use static
/// method in SharedTargetingSystem GetValidParts.
/// </remarks>
[Flags]
public enum TargetBodyPart : ushort
{
    Head = 1,
    Neck = 1 << 1,
    Torso = 1 << 2,
    Groin = 1 << 3,
    LeftArm = 1 << 4,
    LeftHand = 1 << 5,
    RightArm = 1 << 6,
    RightHand = 1 << 7,
    LeftLeg = 1 << 8,
    LeftFoot = 1 << 9,
    RightLeg = 1 << 10,
    RightFoot = 1 << 11,

    Hands = LeftHand | RightHand,
    Arms = LeftArm | RightArm,
    Legs = LeftLeg | RightLeg,
    Feet = LeftFoot | RightFoot,
    All = Head | Neck | Torso | Groin | LeftArm | LeftHand | RightArm | RightHand | LeftLeg | LeftFoot | RightLeg | RightFoot,
}
