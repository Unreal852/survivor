﻿using Sandbox;
using Survivor.Entities;

namespace Survivor.Navigation;

public class NavSteer
{
	public NavSteerOutput Output;

	public NavSteer()
	{
		Path = new NavPath();
	}

	public    Vector3 TargetPosition { get; set; }
	public    Entity  TargetEntity   { get; set; }
	public    Vector3 Target         => TargetEntity?.Position ?? TargetPosition;
	protected NavPath Path           { get; private set; }

	public virtual void Tick( Vector3 currentPosition )
	{
		Path.Update( ref currentPosition, Target );
		Output.Finished = Path.IsEmpty;
		if ( Output.Finished )
		{
			Output.Direction = Vector3.Zero;
			return;
		}

		Output.Direction = Path.GetDirection( currentPosition );

		var avoid = GetAvoidance( currentPosition, 500 );
		if ( !avoid.IsNearlyZero() )
			Output.Direction = (Output.Direction + avoid).Normal;
	}

	private Vector3 GetAvoidance( Vector3 position, float radius )
	{
		var center = position + Output.Direction * radius * 0.5f;

		var objectRadius = 200.0f;
		Vector3 avoidance = default;

		foreach ( var ent in Entity.FindInSphere( center, radius ) )
		{
			if ( ent.IsWorld || ent is not BaseNpc )
				continue;

			var delta = (position - ent.Position).WithZ( 0 );
			var closeness = delta.Length;
			if ( closeness < 0.001f ) continue;
			var thrust = ((objectRadius - closeness) / objectRadius).Clamp( 0, 1 );
			if ( thrust <= 0 ) continue;

			//avoidance += delta.Cross( Output.Direction ).Normal * thrust * 2.5f;
			avoidance += delta.Normal * thrust * thrust;
		}

		return avoidance;
	}

	public virtual void DebugDrawPath()
	{
		Path.DebugDraw( 0.1f, 0.1f );
	}
}
