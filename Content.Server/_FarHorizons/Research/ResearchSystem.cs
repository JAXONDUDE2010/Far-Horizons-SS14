using Content.Server.Chat.Systems;
using Content.Server.Research.Systems;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._FarHorizons.Research;

public sealed partial class FHResearchSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly ResearchSystem _research = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeConsole();
    }
}