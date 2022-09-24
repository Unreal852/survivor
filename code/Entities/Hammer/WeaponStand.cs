﻿using Sandbox;
using SandboxEditor;
using Survivor.Assets;
using Survivor.Interaction;
using Survivor.Players;
using Survivor.Players.Inventory;
using Survivor.Weapons;

// resharper disable all

namespace Survivor.Entities.Hammer;

[Library( "survivor_weapon_stand" )]
[Title( "Weapon Stand" ), Category( "Map" ), Icon( "place" ), Description( "This entity defines weapon stand" )]
[HammerEntity, SupportsSolid, Model( Model = "models/objects/tall_plate.vmdl", Archetypes = ModelArchetype.generic_actor_model )]
[RenderFields, VisGroup( VisGroup.Dynamic )]
public partial class WeaponStand : ModelEntity, IUsable
{
	[Property]
	[Category( "Weapon Stand" ), Title( "Enabled" ), Description( "Unchecking this will prevent this weapon from being bought" )]
	public bool IsEnabled { get; set; } = true;

	[Property]
	[Category( "Weapon Stand" ), Title( "Weapon" ), Description( "The weapon sold by this weapon stand" )]
	public WeaponType Weapon { get; set; }

	[Property, Net]
	[Category( "Weapon Stand" ), Title( "Cost" ), Description( "The cost to buy this weapon" )]
	public int Cost { get; set; } = 0;

	[Property, Net]
	[Category( "Weapon Stand" ), Title( "Ammo Cost" ), Description( "The cost to buy ammo for this weapon" )]
	public int AmmoCost { get; set; } = 0;

	[Net]
	private WeaponAsset WeaponAsset { get; set; }

	private WeaponWorldModel WorldModel { get; set; }

	public int UseCost
	{
		get
		{
			if ( Local.Pawn is not SurvivorPlayer player )
				return 0;
			if ( player.Inventory is SurvivorPlayerInventory inventory && inventory.IsCarryingType( WeaponAsset.GetWeaponClassType() ) )
				return AmmoCost;
			return Cost;
		}
	}

	public string UseMessage
	{
		get
		{
			if ( Local.Pawn is not SurvivorPlayer player )
				return string.Empty;
			if ( player.Inventory is SurvivorPlayerInventory inventory && inventory.IsCarryingType( WeaponAsset.GetWeaponClassType() ) )
				return "Buy Ammo";
			return $"Buy {WeaponAsset.Name}";
		}
	}

	public override void Spawn()
	{
		base.Spawn();
		SetupPhysicsFromModel( PhysicsMotionType.Static );
		var weapSpawnPos = GetAttachment( "weapon" );
		if ( !weapSpawnPos.HasValue )
		{
			Log.Warning( "Missing weapon attachement" );
			return;
		}

		WeaponAsset = WeaponAsset.GetWeaponAsset( Weapon );
		if ( WeaponAsset == null )
		{
			Log.Error( $"No weapon asset found for '{Weapon}'" );
			Delete();
			return;
		}

		WorldModel = new WeaponWorldModel( WeaponAsset ) { Transform = weapSpawnPos.Value, Parent = this };
	}

	public bool OnUse( Entity user )
	{
		if ( !IsEnabled )
			return false;
		if ( user is SurvivorPlayer player && player.TryUse() )
		{
			var weaponType = WeaponAsset.GetWeaponClassType();
			if ( player.Inventory.IsCarryingType( weaponType ) && player.Money >= AmmoCost )
				player.Money -= AmmoCost;
			else if ( player.Money >= Cost )
				player.Money -= Cost;
			player.Inventory.Add( WeaponAsset.CreateWeaponInstance(), true );
		}

		return false;
	}

	public bool IsUsable( Entity user )
	{
		return true;
	}
}
