﻿using System;
using Sandbox;

namespace Zyglorb;

public partial class Pawn
{
	/// <summary> Whether or not the pawn is touching the ground </summary>
	public bool IsGrounded => GroundEntity != null;

	/// <summary>
	/// Set pawn velocity based on player inputs
	/// </summary>
	private void CalculateVelocity()
	{
		// Build our direction vector
		var direction = MoveInput.Normal;
		direction *= Input.Down( InputButton.Run ) ? 235 : 190;

		// Start updating our pawn's velocity vector
		Velocity = direction.WithZ( Velocity.z );

		// Apply gravity if we aren't grounded
		if ( !IsGrounded )
			Velocity -= new Vector3( 0, 0, Gravity * 0.6f ) * Time.Delta;
		else if ( Input.Pressed( InputButton.Jump ) )
		{
			// If we're grounded and the player pressed jump, then jump!
			var z = Velocity.z + MathF.Sqrt( 2f * Gravity * 26 );
			Velocity = Velocity.WithZ( z );
			Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
		}
	}

	/// <summary>
	/// Simulate is called every tick, both serverside and clientside - we want to use it for movement (and animations)
	/// </summary>
	protected virtual void MovementSimulate( IClient cl )
	{
		CalculateVelocity();

		// Build our MoveHelper
		var helper = new MoveHelper( Position, Velocity ) { MaxStandableAngle = 46.0f };

		// Set our MoveHelper trace size to our bounding box
		helper.Trace = helper.Trace.Size( Mins, Maxs )
			.Ignore( this )
			.IncludeClientside();

		// Attempt movement!
		if ( IsGrounded )
			helper.TryMoveWithStep( Time.Delta, 18.0f ); // Walk up steps if we're grounded
		else
			helper.TryMove( Time.Delta );

		// Move to new pos / vel!
		Position = helper.Position;
		Velocity = helper.Velocity;

		if ( Velocity.z <= 2f )
		{
			// Trace downwards to get GroundEntity
			var trace = helper.TraceDirection( Vector3.Down * 2.0f );
			GroundEntity = trace.Entity;

			if ( GroundEntity == null )
			{
				return;
			}

			if ( Velocity.z < 0.0f ) Velocity = Velocity.WithZ( 0 );
		}
		else
		{
			GroundEntity = null;
		}
	}
}
