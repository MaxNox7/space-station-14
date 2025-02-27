﻿using Content.Server.Chemistry.Components;
using Content.Server.Chemistry.EntitySystems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Popups;
using Content.Shared.ActionBlocker;
using Content.Shared.Audio;
using Content.Shared.CharacterAppearance.Systems;
using Content.Shared.Extinguisher;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Player;

namespace Content.Server.Extinguisher;

public class FireExtinguisherSystem : EntitySystem
{
    [Dependency] private readonly SolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FireExtinguisherComponent, ComponentInit>(OnFireExtinguisherInit);
        SubscribeLocalEvent<FireExtinguisherComponent, DroppedEvent>(OnDropped);
        SubscribeLocalEvent<FireExtinguisherComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<FireExtinguisherComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<FireExtinguisherComponent, GetVerbsEvent<InteractionVerb>>(OnGetInteractionVerbs);
        SubscribeLocalEvent<FireExtinguisherComponent, SprayAttemptEvent>(OnSprayAttempt);
    }

    private void OnFireExtinguisherInit(EntityUid uid, FireExtinguisherComponent component, ComponentInit args)
    {
        if (component.HasSafety)
        {
            UpdateAppearance(uid, component);
        }
    }

    private void OnDropped(EntityUid uid, FireExtinguisherComponent component, DroppedEvent args)
    {
        // idk why this has to be done??????????
        UpdateAppearance(uid, component);
    }

    private void OnUseInHand(EntityUid uid, FireExtinguisherComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        ToggleSafety(uid, args.User, component);

        args.Handled = true;
    }

    private void OnAfterInteract(EntityUid uid, FireExtinguisherComponent component, AfterInteractEvent args)
    {
        if (args.Target == null || !args.CanReach)
        {
            return;
        }

        if (args.Handled)
            return;

        args.Handled = true;

        if (component.HasSafety && component.Safety)
        {
            _popupSystem.PopupEntity(Loc.GetString("fire-extinguisher-component-safety-on-message"), uid,
                Filter.Entities(args.User));
            return;
        }

        if (args.Target is not {Valid: true} target ||
            !_solutionContainerSystem.TryGetDrainableSolution(target, out var targetSolution) ||
            !_solutionContainerSystem.TryGetRefillableSolution(uid, out var container))
        {
            return;
        }

        var transfer = container.AvailableVolume;
        if (TryComp<SolutionTransferComponent>(uid, out var solTrans))
        {
            transfer = solTrans.TransferAmount;
        }
        transfer = FixedPoint2.Min(transfer, targetSolution.DrainAvailable);

        if (transfer > 0)
        {
            var drained = _solutionContainerSystem.Drain(target, targetSolution, transfer);
            _solutionContainerSystem.TryAddSolution(uid, container, drained);

            SoundSystem.Play(Filter.Pvs(uid), component.RefillSound.GetSound(), uid);
            _popupSystem.PopupEntity(Loc.GetString("fire-extinguisher-component-after-interact-refilled-message", ("owner", uid)),
                uid, Filter.Entities(args.Target.Value));
        }
    }

    private void OnGetInteractionVerbs(EntityUid uid, FireExtinguisherComponent component, GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanInteract)
            return;

        var verb = new InteractionVerb
        {
            Act = () => ToggleSafety(uid, args.User, component),
            Text = Loc.GetString("fire-extinguisher-component-verb-text"),
        };

        args.Verbs.Add(verb);
    }

    private void OnSprayAttempt(EntityUid uid, FireExtinguisherComponent component, SprayAttemptEvent args)
    {
        if (component.HasSafety && component.Safety)
        {
            _popupSystem.PopupEntity(Loc.GetString("fire-extinguisher-component-safety-on-message"), uid,
                Filter.Entities(args.User));
            args.Cancel();
        }
    }

    private void UpdateAppearance(EntityUid uid, FireExtinguisherComponent comp,
        AppearanceComponent? appearance=null)
    {
        if (!Resolve(uid, ref appearance, false))
            return;

        if (comp.HasSafety)
        {
            appearance.SetData(FireExtinguisherVisuals.Safety, comp.Safety);
        }
    }

    public void ToggleSafety(EntityUid uid, EntityUid user,
        FireExtinguisherComponent? extinguisher = null)
    {
        if (!Resolve(uid, ref extinguisher))
            return;

        extinguisher.Safety = !extinguisher.Safety;
        SoundSystem.Play(Filter.Pvs(uid), extinguisher.SafetySound.GetSound(), uid,
            AudioHelpers.WithVariation(0.125f).WithVolume(-4f));
        UpdateAppearance(uid, extinguisher);
    }
}
