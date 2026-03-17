using Content.Shared.Cloning; // FarHorizons
using Robust.Shared.Prototypes; // FarHorizons

namespace Content.Server.Humanoid.Components;

[RegisterComponent]
public sealed partial class RandomHumanoidAppearanceComponent : Component
{
    [DataField]
    public bool RandomizeName = true;
}

// FarHorizons Start
[RegisterComponent]
public sealed partial class RandomSpeciesAppearanceComponent : Component
{
    [DataField]
    public bool RandomizeName = true;

    [DataField]
    public string namePrefix = "";

    [DataField]
    public bool lastNameOnly = false;
    public ProtoId<CloningSettingsPrototype> TransformCloningSettings = "ChangelingCloningSettings";
}
// FarHorizons End