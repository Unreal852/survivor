﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sandbox;
using Survivor.Navigation;
using Survivor.Players;
using Survivor.Utils;
using Survivor.Weapons;

// ReSharper disable MemberCanBePrivate.Global

namespace Survivor.Entities.Zombies;

// TODO: Whole movement system isn't that performant at all

public abstract partial class BaseZombie : BaseNpc
{
	[ConVar.Replicated] public static bool      nav_drawpath { get; set; } = false;
	protected readonly                NavSteer  NavSteer = new();
	protected readonly                BBox      BBox     = BBox.FromHeightAndRadius( 64, 4 );
	protected                         Vector3   InputVelocity;
	protected                         Vector3   LookDirection;
	protected                         TimeSince SinceLastAttack;
	protected                         TimeSince SinceLastMoan;

	public BaseZombie()
	{
		// Ignored
	}

	public float  MoveSpeed     { get; set; } = 1f;
	public float  AttackSpeed   { get; set; } = 1f;
	public float  AttackDamages { get; set; } = 1f;
	public float  AttackRange   { get; set; } = 1f;
	public Entity Target        => NavSteer.TargetEntity;

	protected virtual void Prepare()
	{
		SetModel( "models/citizen/citizen.vmdl" );
		SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, Capsule.FromHeightAndRadius( 72, 8 ) );
		EyePosition = Position + Vector3.Up * 64;
		EnableHitboxes = true;
		UsePhysicsCollision = true;
		RenderColor = Color.Green;
		Health = 100;

		Tags.Add( "zombie" );

		FindTarget();
	}

	private void FindTarget()
	{
		if ( IsClient )
			return;
		var clients = Client.All;
		Client client = clients[Rand.Int( 0, clients.Count - 1 )];
		if ( client.Pawn is SurvivorPlayer player )
			NavSteer.TargetEntity = player;
	}

	public void SetTarget( Entity entity )
	{
		Host.AssertServer();
		if ( entity is not { IsValid: true } )
			return;
		NavSteer.TargetEntity = entity;
	}

	public void SetTarget( Vector3 position, bool force = false )
	{
		Host.AssertServer();
		NavSteer.TargetPosition = position;
		if ( force )
			NavSteer.TargetEntity = null;
	}

	public override void Spawn()
	{
		base.Spawn();
		Prepare();
	}

	public override void OnKilled()
	{
		base.OnKilled();
		if ( IsServer && LastAttacker is SurvivorPlayer player )
		{
			SurvivorGame.Current.GameMode.EnemiesRemaining--;
			player.Money += Rand.Int( 5, 10 );
			player.Client.AddInt( "kills" );
		}
	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );
		// TODO: Target change
	}

	public override void OnServerUpdate()
	{
		InputVelocity = 0;
		if ( NavSteer != null )
		{
			NavSteer.Tick( Position );

			if ( !NavSteer.Output.Finished )
			{
				InputVelocity = NavSteer.Output.Direction.Normal;
				Velocity = Velocity.AddClamped( InputVelocity * Time.Delta * 500, MoveSpeed );
			}

			if ( nav_drawpath )
				NavSteer.DebugDrawPath();
		}

		Move( Time.Delta );

		var walkVelocity = Velocity.WithZ( 0 );
		if ( walkVelocity.Length > 0.5f )
		{
			var turnSpeed = walkVelocity.Length.LerpInverse( 0, 100 );
			var targetRotation = Rotation.LookAt( walkVelocity.Normal, Vector3.Up );
			Rotation = Rotation.Lerp( Rotation, targetRotation, turnSpeed * Time.Delta * 20.0f );
		}

		var animHelper = new CitizenAnimationHelper( this );

		LookDirection = Vector3.Lerp( LookDirection, InputVelocity.WithZ( 0 ) * 1000, Time.Delta * 100.0f );
		animHelper.WithLookAt( EyePosition + LookDirection );
		animHelper.WithVelocity( Velocity );
		animHelper.WithWishVelocity( InputVelocity );

		CheckSurroundings();

		if ( CanAttack() )
		{
			Attack( ref animHelper );
		}
	}

	protected virtual bool CanAttack()
	{
		return NavSteer?.TargetEntity != null
		    && NavSteer.TargetEntity.IsValid
		    && NavSteer.TargetEntity.Health                        > 0.0f
		    && NavSteer.TargetEntity.Position.Distance( Position ) < AttackRange
		    && SinceLastAttack                                     > AttackSpeed;
	}

	protected virtual void Attack( ref CitizenAnimationHelper animHelper )
	{
		SinceLastAttack = 0;
	}

	protected virtual void CheckSurroundings()
	{
		foreach ( var entity in FindInSphere( Position, 20.0f ) )
		{
			if ( entity is DoorEntity doorEntity )
				doorEntity.Open( this );
		}
	}

	protected virtual void Move( float timeDelta )
	{
		MoveHelper move = new(Position, Velocity) { MaxStandableAngle = 50 };
		move.Trace = move.Trace.Ignore( this ).Size( BBox );

		if ( !Velocity.IsNearlyZero( 0.001f ) )
		{
			move.TryUnstuck();
			if ( GroundEntity != null )
				move.TryMoveWithStep( timeDelta, 30 );
			else
				move.TryMove( timeDelta );
		}

		var tr = move.TraceDirection( Vector3.Down * 10.0f );
		if ( move.IsFloor( tr ) )
		{
			SetAnimParameter( "b_grounded", true );
			GroundEntity = tr.Entity;
			if ( !tr.StartedSolid )
				move.Position = tr.EndPosition;

			if ( InputVelocity.Length > 0 )
			{
				var movement = move.Velocity.Dot( InputVelocity.Normal );
				move.Velocity -= movement * InputVelocity.Normal;
				move.ApplyFriction( tr.Surface.Friction * 10.0f, timeDelta );
				move.Velocity += movement * InputVelocity.Normal;
			}
			else
				move.ApplyFriction( tr.Surface.Friction * 10.0f, timeDelta );
		}
		else
		{
			GroundEntity = null;
			move.Velocity += Vector3.Down * 900 * timeDelta;
			SetAnimParameter( "b_grounded", false );
		}

		Position = move.Position;
		Velocity = move.Velocity;
	}
}
