using System.Linq;
using Content.Shared._FarHorizons.Research;
using Content.Shared._FarHorizons.Research.Components;
using Content.Shared.Research.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._FarHorizons.Research;

public sealed partial class FHResearchSystem
{
    public bool HandleResearch(Entity<FHResearchTreeComponent?> ent, int points)
    {
        if (!Resolve(ent, ref ent.Comp) || 
            ent.Comp.Queue.Count == 0)
            return false;
        
        var totalPoints = points;
        while (totalPoints > 0)
            Research ((ent, ent.Comp), ref totalPoints);

        RefreshUIOnClients(ent);
        
        return true;
    }

    public void Research(Entity<FHResearchTreeComponent> ent, ref int points)
    {
        if (points <= 0)
            return;

        if (ent.Comp.Queue.Count == 0)
        {
            ent.Comp.BankedPoints += points;
            points = 0;
            return;
        }

        var nextNode = ent.Comp.Queue.First();
        var nextNodeProto = _protoMan.Index(nextNode);

        if (!ent.Comp.Progress.ContainsKey(nextNode))
            ent.Comp.Progress[nextNode] = 0;
        
        var pointsRemaining = nextNodeProto.Cost - ent.Comp.Progress[nextNode];
        
        if (points >= pointsRemaining)
        {
            UnlockNode(ent, nextNode);
            points -= pointsRemaining;
        } else {
            ent.Comp.Progress[nextNode] += points;
            points = 0;
        }
    }

    public void UnlockNode(Entity<FHResearchTreeComponent> ent, ProtoId<ResearchTreeNodePrototype> node)
    {
        ent.Comp.Queue.Remove(node);
        ent.Comp.Progress.Remove(node);
        ent.Comp.Researched.Add(node);

        if (!TryComp(ent, out TechnologyDatabaseComponent? techDb))
            return;
        
        var nodeProto = _protoMan.Index(node);

        foreach(var recipe in nodeProto.Unlocks)
            _research.AddLatheRecipe(ent, recipe, techDb);
    }

    public void AddResearchToQueue(Entity<FHResearchTreeComponent?> ent, ProtoId<ResearchTreeNodePrototype> node)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (ent.Comp.Queue.Count < ent.Comp.MaxQueueSize)
        {
            var nodeProto = _protoMan.Index(node);
            var nodes = GetTreeNodes((ent, ent.Comp));
            if (nodes.Contains(nodeProto) && IsNodeUnlocked((ent, ent.Comp), nodeProto))
                ent.Comp.Queue.Add(node);
            while (ent.Comp.BankedPoints > 0)
                Research ((ent, ent.Comp), ref ent.Comp.BankedPoints);
        } else
            SendErrorToClients(ent, Loc.GetString("research-tree-console-error-queue-full"));

        RefreshUIOnClients(ent);
    }

    public void RemoveResearchFromQueue(Entity<FHResearchTreeComponent?> ent, ProtoId<ResearchTreeNodePrototype> node)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        ent.Comp.Queue.Remove(node);
        ent.Comp.Queue = [.. ent.Comp.Queue.Intersect([.. GetUnlockedNodes((ent, ent.Comp))])];

        RefreshUIOnClients(ent);
    }

    public void SendErrorToClients(Entity<FHResearchTreeComponent?> ent, string message = "")
    {
        if (!Resolve(ent, ref ent.Comp) || !TryComp(ent, out ResearchServerComponent? serverComp))
            return;
        
        foreach (var client in serverComp.Clients)
            if(TryComp(client, out FHResearchConsoleComponent? console))
                ShowError((client, console), message);
    }

    public void RefreshUIOnClients(Entity<FHResearchTreeComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp) || !TryComp(ent, out ResearchServerComponent? serverComp))
            return;
        
        foreach (var client in serverComp.Clients)
            if(TryComp(client, out FHResearchConsoleComponent? console))
                UpdateUI((client, console));

    }

    public bool IsNodeUnlocked(Entity<FHResearchTreeComponent> ent, ResearchTreeNodePrototype node) =>
        GetUnlockedNodes(ent).Contains(node.ID);
    public HashSet<ProtoId<ResearchTreeNodePrototype>> GetUnlockedNodes(Entity<FHResearchTreeComponent> ent, bool withQueue = true)
    {
        HashSet<ProtoId<ResearchTreeNodePrototype>> result = [];
        
        var unlockedTiers = GetUnlockedTiers(ent);

        foreach (var node in GetTreeNodes(ent))
        {
            var nodeTierUnlocked = unlockedTiers.Contains(node.Tier);
            var nodeRequirementsUnlocked = node.Requires.All(ent.Comp.Researched.Contains);
            if (nodeTierUnlocked && nodeRequirementsUnlocked)
            {
                result.Add(node.ID);
                continue;
            }

            if (withQueue)
            {
                var pointsIncludingQueue = GetTotalPointsSpent(ent) + ent.Comp.Queue.Sum(p => _protoMan.Index(p).Cost);
                var nodeTierUnlockedWithQueue = GetTreeTiers(ent).Where(p => _protoMan.Index(p).UnlocksAt <= pointsIncludingQueue).Contains(node.Tier);
                var nodeRequirementsWithQueue = node.Requires.All(p => ent.Comp.Researched.Contains(p) || ent.Comp.Queue.Contains(p));
                if (nodeTierUnlockedWithQueue && nodeRequirementsWithQueue)
                    result.Add(node.ID);
            }
        }

        return result;
    }
    public HashSet<ProtoId<ResearchTreeTierPrototype>> GetUnlockedTiers(Entity<FHResearchTreeComponent> ent)
    {
        var spent = GetTotalPointsSpent(ent);
        return GetTreeTiers(ent).Where(p => _protoMan.Index(p).UnlocksAt <= spent).ToHashSet();
    }
    public int GetTotalPointsSpent(Entity<FHResearchTreeComponent> ent) =>
        ent.Comp.Researched.Sum(p => _protoMan.Index(p).Cost) + ent.Comp.Progress.Sum(p => p.Value);

    public HashSet<ProtoId<ResearchTreeTierPrototype>> GetTreeTiers(Entity<FHResearchTreeComponent> ent) =>
        GetTreeNodes(ent).Select(p => p.Tier).Distinct().ToHashSet();

    public HashSet<ResearchTreeNodePrototype> GetTreeNodes(Entity<FHResearchTreeComponent> ent) =>
        _protoMan.Index(ent.Comp.Tree).Nodes.Select(p => _protoMan.Index(p)).ToHashSet();
}