﻿using Sandbox;
using SWB_Player;

namespace Survivor.Players.Controllers;

public partial class SurvivorPlayerWalkController : PlayerWalkController
{
	[Net]
	public bool IsSprinting { get; set; }

	public override float GetWishSpeed()
	{
		var ws = Duck.GetWishSpeed();
		if ( ws >= 0 )
			return ws;

		if ( GroundEntity != null && Input.Down( "run" ) && Input.Down( "forward" ) )
		{
			IsSprinting = true;
			return SprintSpeed;
		}

		IsSprinting = false;

		if ( Input.Down( "walk" ) )
			return WalkSpeed;

		return DefaultSpeed;
	}
}
