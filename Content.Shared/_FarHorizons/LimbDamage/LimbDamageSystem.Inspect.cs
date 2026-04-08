using System.Linq;
using Content.Shared._FarHorizons.Body;
using Content.Shared._FarHorizons.LimbDamage.Components;
using Content.Shared.Body;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._FarHorizons.LimbDamage;

public partial class LimbDamageSystem
{
    private static List<ProtoId<OrganCategoryPrototype>> _inspectOrder = new()
    {
        "Head",
        "Torso",
        "ArmRight",
        "HandRight",
        "ArmLeft",
        "HandLeft",
        "LegRight",
        "FootRight",
        "LegLeft",
        "FootLeft"
    };
    private static List<FixedPoint2> _inspectThresholds = new() { 0, 10, 30, 50 };
    
    private void InitInspect()
    {
        SubscribeLocalEvent<LimbDamageableComponent, GetVerbsEvent<ExamineVerb>>(OnBodyExamine);
        SubscribeLocalEvent<DamageableLimbComponent, GetVerbsEvent<ExamineVerb>>(OnDetachedExamine);
    }

    private void OnDetachedExamine(Entity<DamageableLimbComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
    {
        var source = args.User;
        var detailsRange = _examine.IsInDetailsRange(source, ent);

        var verb = new ExamineVerb()
        {
            Act = () =>
            {
                var markup = CreateDetachedMarkup(ent);
                _examine.SendExamineTooltip(source, ent, markup, false, false);
            },
            Text = Loc.GetString("health-examinable-verb-text"),
            Category = VerbCategory.Examine,
            Disabled = !detailsRange,
            Message = detailsRange ? null : Loc.GetString("health-examinable-verb-disabled"),
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/rejuvenate.svg.192dpi.png"))
        };

        args.Verbs.Add(verb);
    }

    private void OnBodyExamine(Entity<LimbDamageableComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
    {
        var source = args.User;
        var detailsRange = _examine.IsInDetailsRange(source, ent);

        var verb = new ExamineVerb()
        {
            Act = () =>
            {
                var markup = CreateMarkup(ent);
                _examine.SendExamineTooltip(source, ent, markup, false, false);
            },
            Text = Loc.GetString("health-examinable-verb-text"),
            Category = VerbCategory.Examine,
            Disabled = !detailsRange,
            Message = detailsRange ? null : Loc.GetString("health-examinable-verb-disabled"),
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/rejuvenate.svg.192dpi.png"))
        };

        args.Verbs.Add(verb);
    }

    public FormattedMessage CreateMarkup(Entity<LimbDamageableComponent> ent)
    {
        var msg = new FormattedMessage();

        var fullDamage = TryGetFullBodyDamage((ent, null, ent.Comp));

        if (fullDamage == null)
            return msg;

        if (fullDamage.All(p => p.Value.All(e => e.Value == 0)))
        {
            msg.AddMarkupOrThrow(Loc.GetString("limb-health-no-damage", ("target", Identity.Entity(ent, EntityManager))));
            return msg;
        }

        var first = true;
        foreach (var limb in _inspectOrder)
        {
            var limbName = Loc.GetString($"limb-category-limb-name-{limb.Id.ToLower()}");
            string? finalMessage = null;

            if (!fullDamage.TryGetValue(limb, out var limbDamage))
                finalMessage = Loc.GetString("limb-health-missing", ("target", Identity.Entity(ent, EntityManager)), ("limb", limbName));
            else
            {
                var thresholds = ProcessThresholds(limbDamage, _inspectThresholds);

                if (thresholds.Count != 0)
                    finalMessage = Loc.GetString("limb-health-damge-combined",
                        ("target", Identity.Entity(ent, EntityManager)), ("limb", limbName),
                        ("damage", ProcessDamageText(thresholds)));
            }

            if (finalMessage == null) continue;
            
            if (!first)
                msg.PushNewline();
            else
                first = false;
                
            msg.AddMarkupOrThrow(finalMessage);
        }

        return msg;
    }

    public FormattedMessage CreateDetachedMarkup(Entity<DamageableLimbComponent> ent)
    {
        var msg = new FormattedMessage();

        if (ent.Comp.Organ?.Category == null ||
            ent.Comp.Damageable == null)
            return msg;

        var limbName = Loc.GetString($"limb-category-limb-name-{ent.Comp.Organ.Category.Value.Id.ToLower()}");
        var damage = _damageable.GetPositiveDamage((ent.Owner, ent.Comp.Damageable)).DamageDict;
        var thresholds = ProcessThresholds(damage, _inspectThresholds);

        string baseMessage;

        if (thresholds.Count == 0)
            baseMessage = Loc.GetString("limb-health-examinable-detached-ok", ("limb", limbName));
        else
            baseMessage = Loc.GetString("limb-health-examinable-detached-damge-combined", ("limb", limbName),
                ("damage", ProcessDamageText(thresholds)));
        
        msg.AddMarkupOrThrow(baseMessage);

        if (!TryComp<ConnectedOrganComponent>(ent, out var connectedOrgan) || connectedOrgan.Organs == null) return msg;

        foreach (var organ in connectedOrgan.Organs.ContainedEntities)
            if (TryComp<DamageableLimbComponent>(organ, out var damageableOrgan) &&
                damageableOrgan is { Damageable: not null, Organ: not null } &&
                damageableOrgan.Organ.Category != null)
            {
                var organName = Loc.GetString($"limb-category-limb-name-{damageableOrgan.Organ.Category.Value.Id.ToLower()}");
                var organDamage = _damageable.GetPositiveDamage((organ, damageableOrgan.Damageable)).DamageDict;
                var organThresholds = ProcessThresholds(organDamage, _inspectThresholds);

                msg.PushNewline();

                if (organThresholds.Count == 0)
                    msg.AddMarkupOrThrow(Loc.GetString("limb-health-examinable-detached-attachment-ok",
                        ("limb", organName)));
                else
                    msg.AddMarkupOrThrow(Loc.GetString("limb-health-examinable-detached-attachment-damge-combined",
                        ("limb", organName), ("damage", ProcessDamageText(organThresholds))));
            }

        return msg;
    }

    private Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2> ProcessThresholds(Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2> limbDamage, List<FixedPoint2> thresholds)
    {
        var result = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>();
        foreach (var (damage, value) in limbDamage)
        {
            var matchingThresholds = thresholds.Where(p => p > 0 && p <= value).ToList();
            if (matchingThresholds.Count > 0)
                result[damage] = matchingThresholds.Last();
        }

        return result;
    }

    private string ProcessDamageText(Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2> limbThresholds)
    {
        List<string> damageStrings = new();
        foreach (var (damage, threshold) in limbThresholds)
            if (Loc.TryGetString($"limb-health-{damage.Id.ToLower()}-{threshold}", out var damageString))
                damageStrings.Add(damageString);

        if (damageStrings.Count == 0)
            return "";

        if (damageStrings.Count == 1)
            return damageStrings.First();

        var combinedString = string.Join(", ", damageStrings.SkipLast(1));
        return Loc.GetString("limb-health-damage-combine", ("first", combinedString), ("last", damageStrings.Last()));
    }
}