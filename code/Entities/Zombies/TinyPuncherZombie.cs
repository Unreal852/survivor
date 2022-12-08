﻿// ReSharper disable PartialTypeWithSinglePart

namespace Survivor.Entities.Zombies;

public sealed partial class TinyPuncherZombie : PuncherZombie
{
	public TinyPuncherZombie()
	{
		// Ignored
	}

	public override ZombieType ZombieType => ZombieType.TinyPuncher;

	protected override void Prepare()
	{
		base.Prepare();
		//EyePosition = Position + Vector3.Up * 32;
	}
}
