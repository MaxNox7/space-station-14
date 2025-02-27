﻿using Content.Shared.Storage.EntitySystems;
using Content.Shared.Whitelist;
using Robust.Shared.Analyzers;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.Storage.Components
{

    /// <summary>
    /// Storage that spawns and counts a single item.
    /// Usually used for things like matchboxes, cigarette packs,
    /// cigar cases etc.
    /// </summary>
    /// <code>
    ///  - type: ItemCounter
    ///    amount: 6 # Note: this field can be omitted.
    ///    count:
    ///      tags: [Cigarette]
    /// </code>
    [RegisterComponent]
    [Friend(typeof(SharedItemCounterSystem))]
    public class ItemCounterComponent : Component
    {
        [DataField("count", required: true)]
        public EntityWhitelist Count { get; set; } = default!;

        [DataField("amount")]
        public int? MaxAmount { get; set; }
    }
}
