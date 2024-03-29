﻿using System.Linq;
using Sandbox;
using Survivor.Players;
using Survivor.UI.World;

namespace Survivor.Entities.Components;

public class NameTagComponent : EntityComponent<SurvivorPlayer>
{
	private PlayerNameTag _nameTag;

	protected override void OnActivate()
	{
		_nameTag = new(Entity.Client?.Name ?? Entity.Name, Entity.Client?.SteamId);
	}

	protected override void OnDeactivate()
	{
		_nameTag?.Delete();
		_nameTag = null;
	}

	[GameEvent.Client.Frame]
	public void FrameUpdate()
	{
		var transform = Entity.GetAttachment( "hat" ) ?? Entity.Transform;
		transform.Position += Vector3.Up * 10.0f;
		transform.Rotation = Rotation.LookAt( -Camera.Rotation.Forward );
		_nameTag.Transform = transform;
	}

	[GameEvent.Client.Frame]
	public static void SystemUpdate()
	{
		// TODO: I don't think doing this every frame is good
		foreach ( var player in Sandbox.Entity.All.OfType<SurvivorPlayer>() )
		{
			if ( player.IsLocalPawn && player.IsFirstPersonMode )
			{
				player.Components.Get<NameTagComponent>()?.Remove();
				continue;
			}

			player.Components.GetOrCreate<NameTagComponent>();
		}
	}
}
