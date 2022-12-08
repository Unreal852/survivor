﻿using Sandbox;

namespace SWB_Base;

public class FirstPersonCamera : CameraMode
{
    public FirstPersonCamera()
    {
    }

    public FirstPersonCamera(PlayerBase player) : base(player)
    {
    }

    public override void UpdateCamera()
    {
        base.UpdateCamera();
        Camera.ZNear = 1f;
        Camera.ZFar = 25000.0f;
        Camera.Rotation = player.ViewAngles.ToRotation();

        // From PlayerPawnController -> EyeLocalPosition
        Camera.Position = player.EyePosition;
        Camera.FieldOfView = Local.UserPreference.FieldOfView;
        Camera.FirstPersonViewer = player;
        Camera.Main.SetViewModelCamera(Camera.FieldOfView, 0.01f, 100.0f);

        if (player.ActiveChild is WeaponBase weapon)
        {
            weapon.UpdateViewmodelCamera();
            weapon.UpdateCamera();
        }
    }
}
