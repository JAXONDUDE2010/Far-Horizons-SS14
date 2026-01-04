using System.Diagnostics.CodeAnalysis;
using Content.Shared._NullLink;
using Content.Shared.Localizations;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using JetBrains.Annotations;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;


namespace Content.Shared._FarHorizons.Roles.JobRequirement;


[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CompositeRequirement : Shared.Roles.JobRequirement
{
    /// <summary>
    /// What particular role they need the time requirement with.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<PlayTimeTrackerPrototype> Role;

    [DataField(required: true)]
    public List<Shared.Roles.JobRequirement> Requirements;

    // It is ((ALL Requirements) OR (ALL AlternativeRequirements))
    [DataField(required: true)]
    public List<Shared.Roles.JobRequirement> AlternativeRequirements;

    public override bool Check(IEntityManager entManager,
        ICommonSession? player,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan>? playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        bool requirementsOutput = true;
        FormattedMessage? reasonOutput = null;
        foreach (var requirement in Requirements)
        {
            if (!requirement.Check(entManager, player, protoManager, profile, playTimes, out reasonOutput))
            {
                requirementsOutput = false;
                break;
            }
        }
        bool altRequirementsOutput = true;
        foreach (var alternativeRequirement in AlternativeRequirements)
        {
            
            if (!alternativeRequirement.Check(entManager, player, protoManager, profile, playTimes, out reasonOutput))
            {
                altRequirementsOutput = false;
                break;
            }
        }

        if (!requirementsOutput || !altRequirementsOutput)
        {
            reason = reasonOutput!;
            return false;
        }

        reason = null;
        return true;
    }
}
