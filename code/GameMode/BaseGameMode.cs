﻿using System;
using Sandbox;
using Sandbox.Internal;
using Survivor.Extensions;
using Survivor.GameMode;
using Survivor.Players;
using Survivor.Players.Controllers;
using Survivor.UI.Hud;

// ReSharper disable All

namespace Survivor.Gamemodes;

public abstract partial class BaseGameMode : BaseNetworkable
{
	private TimeSince _sinceCounterUpdate;

	[Net]
	public int EnemiesRemaining { get; set; } = 0;

	[Net]
	public int CurrentWave { get; set; } = 0;

	[Net]
	public Difficulty Difficulty { get; set; } = Difficulty.Normal;

	[Net]
	public GameState State { get; set; } = GameState.Lobby;

	[Net]
	public int Counter { get; set; }

	public abstract string GameModeName { get; }

	protected BaseGameMode()
	{
		if ( Host.IsServer )
		{
			Event.Register( this );
			OnStartServer();
		}
	}

	public void SetCounter( int value )
	{
		if ( Counter == value )
			return;
		Counter = value;
		_sinceCounterUpdate = 0;
	}

	public void SetGameState( GameState state )
	{
		if ( State == state )
		{
			Log.Warning( $"The state is already set to {state}" );
			return;
		}

		State = state;
		switch ( State )
		{
			case GameState.Starting:
				SetCounter( 20 );
				break;
			case GameState.Playing:
				StartGame();
				break;
			default:
				break;
		}
	}

	public virtual void StartGame()
	{
		foreach ( var client in Client.All )
		{
			if ( client.Pawn is SurvivorPlayer player )
				player.Respawn();
		}

		PlayerHudEntity.ShowGameHud( To.Everyone );
	}

	public virtual bool CanRespawn( SurvivorPlayer player )
	{
		return true;
	}

	public virtual void OnClientJoin( Client client )
	{
		var player = new SurvivorPlayer( client );
		player.Respawn();
		client.Pawn = player;
		if ( State == GameState.Playing )
			PlayerHudEntity.ShowGameHud( To.Single( client ) );
		else if ( State == GameState.Lobby && Client.All.Count >= SurvivorGame.VarMinimumPlayers )
			SetGameState( GameState.Starting );
	}

	public virtual void OnClientDisconnected( Client client, NetworkDisconnectionReason reason )
	{
	}

	public virtual void MovePlayerToSpawnPoint( Entity player )
	{
		Entity entity = State switch
		{
				GameState.Lobby    => SurvivorGame.Current.PlayerLobbySpawnsPoints.RandomElement(),
				GameState.Starting => SurvivorGame.Current.PlayerLobbySpawnsPoints.RandomElement(),
				GameState.Playing  => SurvivorGame.Current.PlayerSpawnPoints.RandomElement(),
				_                  => null
		};

		if ( entity == null )
			return;

		player.Transform = entity.Transform;
	}

	public virtual void OnDoPlayerDevCam( Client client )
	{
		if ( !client.IsListenServerHost )
			return;
		EntityComponentAccessor components = client.Components;
		var devCamera = components.Get<DevCamera>( true );
		if ( devCamera == null )
		{
			var component = new DevCamera();
			components = client.Components;
			components.Add( component );
		}
		else
			devCamera.Enabled = !devCamera.Enabled;
	}

	public virtual void OnDoPlayerNoclip( Client client )
	{
		if ( !client.IsListenServerHost )
			return;


		if ( client.Pawn is not SurvivorPlayer pawn )
			return;
		if ( pawn.DevController is PlayerNoclipController )
			pawn.DevController = null;
		else
			pawn.DevController = new PlayerNoclipController();
	}

	protected virtual void OnStartServer()
	{
	}

	[Event.Tick.Server]
	protected virtual void OnServerTick()
	{
		if ( State == GameState.Starting )
		{
			if ( _sinceCounterUpdate >= 1 )
			{
				if ( Counter <= 0 )
				{
					SetGameState( GameState.Playing );
					return;
				}

				SetCounter( Counter - 1 );
			}
		}
	}
}
