using Robust.Shared.Analyzers;
using Robust.Shared.GameObjects;

namespace Content.Shared.Follower.Components;

[RegisterComponent]
[Friend(typeof(FollowerSystem))]
public class FollowerComponent : Component
{
    public EntityUid Following;
}
