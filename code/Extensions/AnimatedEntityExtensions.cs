﻿using Sandbox;

namespace Survivor.Extensions;

public static class AnimatedEntityExtensions
{
	private static readonly EntityLimit Limit = new() { MaxTotal = 20 };

	public static ModelEntity BecomeRagdoll( this AnimatedEntity animatedEntity, Vector3 velocity,
	                                         Vector3 forcePos, Vector3 force, int bone,
	                                         bool bullet, bool physicsImpact, bool blast )
	{
		var ragdollEntity = new ModelEntity
		{
				Position = animatedEntity.Position,
				Rotation = animatedEntity.Rotation,
				Scale = animatedEntity.Scale,
				UsePhysicsCollision = true,
				EnableAllCollisions = true,
				SurroundingBoundsMode = SurroundingBoundsType.Physics,
				RenderColor = animatedEntity.RenderColor
		};

		ragdollEntity.CopyFrom( animatedEntity );
		ragdollEntity.Tags.Add( "ragdoll", "solid", "debris" );

		ragdollEntity.PhysicsGroup.Velocity = velocity;
		ragdollEntity.PhysicsEnabled = true;

		foreach ( var child in animatedEntity.Children )
		{
			if ( !child.Tags.Has( "clothes" ) || child is not ModelEntity e )
				continue;
			var clothModel = new ModelEntity( e.GetModelName(), ragdollEntity ) { RenderColor = e.RenderColor };
			clothModel.CopyFrom( e );
		}

		if ( bullet || physicsImpact )
		{
			var physicsBody = bone > 0 ? animatedEntity.GetBonePhysicsBody( bone ) : null;
			if ( physicsBody != null )
				physicsBody.ApplyImpulseAt( forcePos, force * physicsBody.Mass );
			else
				animatedEntity.PhysicsGroup.ApplyImpulse( force );
		}

		if ( blast )
		{
			if ( animatedEntity.PhysicsGroup != null )
			{
				animatedEntity.PhysicsGroup.AddVelocity(
						(animatedEntity.Position - (forcePos + Vector3.Down * 100.0f)).Normal * (force.Length * 0.2f) );
				var angularDir = (Rotation.FromYaw( 90 ) * force.WithZ( 0 ).Normal).Normal;
				animatedEntity.PhysicsGroup.AddAngularVelocity( angularDir * (force.Length * 0.02f) );
			}
		}

		Limit.Watch( ragdollEntity );

		return ragdollEntity;
	}
}
