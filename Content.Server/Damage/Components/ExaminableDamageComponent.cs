﻿using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Damage.Components;

/// <summary>
///     This component shows entity damage severity when it is examined by player.
/// </summary>
[RegisterComponent]
public class ExaminableDamageComponent : Component
{
    [DataField("messages", required: true, customTypeSerializer:typeof(PrototypeIdSerializer<ExaminableDamagePrototype>))]
    public string? MessagesProtoId;

    public ExaminableDamagePrototype? MessagesProto;
}
