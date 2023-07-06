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
        //crouching 
        if(_player.crouch)
        {
           if(_player.currentlyCrouching == false)
            {
                _player.currentlyCrouching=true;
            }
            else if (_player.currentlyCrouching==true)
            {
                checkUncrouch();
            }
        }

        if (_player.currentlyCrouching)
        {
            _player.cameraScript.cameraPositionGoal= new Vector3(0,_player.crouchHeight,0);
            _player.currentlySprinting=false;
            _player.playerColliderStanding.isTrigger=true;
        }

    }
    bool checkUncrouch()
    {
      CollisionSensor cs=  _player.playerColliderStanding.GetComponent<CollisionSensor>();
      if (cs._overlaps>0)
      {
            return false;
      }
      else
      {
            _player.playerColliderStanding.isTrigger=false;
            _player.currentlyCrouching = false;
            _player.cameraScript.cameraPositionGoal= new Vector3(0,_player.walkHeight,0);
            return true;
      }
    }
    public override void FixedUpdateState()
    {
        UpdateInternalState();

        float speedgoal = _player.currentlySprinting == false ? _player.walkSpeed: _player.runSpeed;
        if (_player.currentlyCrouching)
        {
            speedgoal=_player.walkSpeed*.5f;
        }
        _player.AdjustVelocity(_player.move, speedgoal, _player.maxAcceleration,true);

        //sprinting
        if (_player.velocity.magnitude>.1 && _player.sprint)
        {
            _player.currentlySprinting=true;
        }
        if(_player.move.magnitude<.6f && !_player.sprint)
        {
            _player.currentlySprinting=false;
        }

       if (_player.jump)
        {   
            if(_player.currentlyCrouching)
            {
                _player.ResetJump();
                checkUncrouch();
            }
            else
            {
                _player.ResetJump();
                _player.Jump(_player.gravity);
            }
    
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
