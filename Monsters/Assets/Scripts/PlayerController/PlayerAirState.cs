using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirState : PlayerBaseState
{
    private PlayerMotor _player;
    private int _stepsSinceStartedFalling;

    public override void EnterState()
    {   
    }
    public override void ExitState()
    {
    }
    public override void UpdateState()
    {
    }
     public PlayerAirState(PlayerMotor player)
    {
        this._player = player;
        _stepsSinceStartedFalling = 0;
    }

    public override void FixedUpdateState()
    {
        UpdateInternalState();

        _player.AdjustVelocity(_player.move.normalized, _player.walkSpeed, _player.maxAirAcceleration,true);

        if (_player.jump)
        {
            //Jump(_player.gravity);
        }

        if (_player.SnapToGround())
        {
            _player.SwitchState(_player._groundState);
        }


        
        _player.velocity += (_player.gravity) * Time.deltaTime;

        _player.body.velocity = _player.velocity;
    }

     private void UpdateInternalState()
    {
        _player.stepsSinceLastGrounded += 1;
        _player.stepsSinceLastJump += 1;
        _player.velocity = _player.body.velocity;
        _stepsSinceStartedFalling += 1;

        if (_player.stepsSinceLastJump >= 4 && (_player.OnGround || _player.OnSteep))
        {
            _player.stepsSinceLastGrounded = 0;
            _player.SwitchState(_player._groundState);
        }
        else
        {
            _player.contactNormal = _player.upAxis;
        }
    }
}
