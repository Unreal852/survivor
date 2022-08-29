﻿using Sandbox;

namespace Survivor.Players;

public class SurvivorPlayer : Player
{
	private readonly ClothingContainer _clothing = new();

	public SurvivorPlayer()
	{
		// Ignored	
	}

	public SurvivorPlayer( Client client )
	{
		_clothing.LoadFromClient( client );
	}

	private void Prepare()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		_clothing.DressEntity( this );

		Controller = new WalkController();
		Animator = new StandardPlayerAnimator();
		CameraMode = new FirstPersonCamera();

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableTouch = true;
		Health = 100;
	}

	public override void Respawn()
	{
		Prepare();
		base.Respawn();
	}

	public override void BuildInput( InputBuilder input )
	{
		base.BuildInput( input );
	}

	public override void ClientSpawn()
	{
		// // if ( !IsLocalPawn )
		// 	Components.GetOrCreate<NameTagComponent>();
	}
}
