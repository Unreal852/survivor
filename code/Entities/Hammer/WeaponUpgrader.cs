﻿using Editor;
using Sandbox;


// resharper disable all

namespace Survivor.Entities.Hammer;

[Library("survivor_weapons_upgrader")]
[Category( "Map" ), Icon( "place" )]
[Title( "Weapon Upgrader" ), Description( "This entity defines a weapon upgrade" )]
[HammerEntity, SupportsSolid, Model( Model = "models/objects/weapons_upgrader.vmdl", Archetypes = ModelArchetype.animated_model )]
[RenderFields, VisGroup( VisGroup.Dynamic )]
public partial class WeaponUpgrader : AnimatedEntity, IUse
{
	// TODO: This class has been written just to test things, this should be rewrite
	// TODO: Use anim tags

	[Property]
	[Title( "Enabled" ), Description( "Unchecking this will prevent this door from being bought" )]
	public bool IsEnabled { get; set; } = true;

	[Property]
	[Title( "Cost" ), Description( "The cost to unlock this door" )]
	public int Cost { get; set; } = 0;

	[Net]
	public bool IsOpened { get; set; } = false;

	private float     FramesBetweenModelColorChanges { get; set; } = 10;
	private float     CurrentFrame                   { get; set; } = 0;
	private float     StayOpenedDuration             { get; set; } = 5;
	private float     DelayBetweenUses               { get; set; } = 3;
	private TimeSince TimeSinceOpened                { get; set; } = 0;
	private TimeSince TimeSinceClosed                { get; set; } = 0;

	public override void Spawn()
	{
		base.Spawn();
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		SetAnimParameter( "close", true );
	}

	public void Open()
	{
		if ( IsOpened || TimeSinceClosed < DelayBetweenUses )
			return;
		IsOpened = true;
		SetAnimParameter( "open", IsOpened );
		TimeSinceOpened = 0;
	}

	public void Close()
	{
		if ( !IsOpened || TimeSinceOpened < StayOpenedDuration )
			return;
		IsOpened = false;
		SetAnimParameter( "close", true );
		TimeSinceClosed = 0;
	}

	public bool OnUse( Entity user )
	{
		Open();
		return false;
	}

	public bool IsUsable( Entity user )
	{
		return !IsOpened && TimeSinceOpened >= StayOpenedDuration;
	}

	[GameEvent.Tick.Server]
	public void OnTick()
	{
		if ( !IsOpened || TimeSinceOpened <= 1 || ++CurrentFrame < FramesBetweenModelColorChanges )
			return;
		CurrentFrame = 0;
		Close();
	}
}
