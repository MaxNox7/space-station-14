using System.Collections.Generic;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Shared.Chemistry.Reaction;

[RegisterComponent]
public class ReactiveComponent : Component
{
    /// <summary>
    ///     A dictionary of reactive groups -> methods that work on them.
    /// </summary>
    [DataField("groups", readOnly: true, serverOnly: true,
        customTypeSerializer:
        typeof(PrototypeIdDictionarySerializer<HashSet<ReactionMethod>, ReactiveGroupPrototype>))]
    public Dictionary<string, HashSet<ReactionMethod>>? ReactiveGroups;

    /// <summary>
    ///     Special reactions that this prototype can specify, outside of any that reagents already apply.
    ///     Useful for things like monkey cubes, which have a really prototype-specific effect.
    /// </summary>
    [DataField("reactions", true, serverOnly: true)]
    public List<ReactiveReagentEffectEntry>? Reactions;
}

[DataDefinition]
public class ReactiveReagentEffectEntry
{
    [DataField("methods")]
    public HashSet<ReactionMethod> Methods = default!;

    [DataField("reagents", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<ReagentPrototype>))]
    public HashSet<string>? Reagents = null;

    [DataField("effects", required: true)]
    public List<ReagentEffect> Effects = default!;
    [DataField("groups", required: true, readOnly: true, serverOnly: true,
        customTypeSerializer:typeof(PrototypeIdDictionarySerializer<HashSet<ReactionMethod>, ReactiveGroupPrototype>))]
    public Dictionary<string, HashSet<ReactionMethod>> ReactiveGroups { get; } = default!;
}
