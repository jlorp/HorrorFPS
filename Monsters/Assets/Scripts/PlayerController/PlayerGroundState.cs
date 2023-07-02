using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundState : PlayerBaseState
{
    private PlayerMotor _player;

    public PlayerGroundState(PlayerMotor player)
    {
        this._player = player;
    }

    public override void EnterState()
    {   
       
    }
    public override void ExitState()
    {
    }
    public override void UpdateState()
    {
    }
    public override void FixedUpdateState()
    {
        UpdateInternalState();
       
        _player.AdjustVelocity(_player.move, _player.walkSpeed, _player.maxAcceleration,true);

       if (_player.jump)
        {
                _player.ResetJump();
                _player.Jump(_player.gravity);
        }

        _player.velocity += _player.contactNormal * (Vector3.Dot(_player.gravity, _player.contactNormal) * Time.deltaTime);
        _player.body.velocity = _player.velocity;
    }

    //probaby want this in motor?

    private void UpdateInternalState()
    {
        _player.stepsSinceLastGrounded += 1;
        _player.stepsSinceLastJump += 1;
        _player.velocity = _player.body.velocity;

        if (_player.CheckSteepContacts() || _player.OnGround || _player.SnapToGround())
        {
            _player.stepsSinceLastGrounded = 0;
            if (_player.groundContactCount > 1)
            {
                _player.contactNormal.Normalize();
            }
            if (_player.stepsSinceLastJump > 1)
            {
                _player.jumped = 0;
            }
        }
        else
        {
            _player.contactNormal = _player.upAxis;
            _player.SwitchState(_player._airState);
        }

        if (_player.connectedBody)
        {
            if (_player.connectedBody.isKinematic || _player.connectedBody.mass >= _player.body.mass)
            {
                UpdateConnectionState();
            }
        }
    }

    

    void UpdateConnectionState()
    {
        if (_player.connectedBody == _player.previousConnectedBody)
        {
            Vector3 connectionMovement = _player.connectedBody.transform.TransformPoint(_player.connectionLocalPosition) - _player.connectionWorldPosition;
            _player.connectionVelocity = connectionMovement / Time.deltaTime;
        }
        _player.connectionWorldPosition = _player.body.position;
        _player.connectionLocalPosition = _player.connectedBody.transform.InverseTransformPoint(_player.connectionWorldPosition);
    }
}
