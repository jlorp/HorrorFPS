using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirState : PlayerBaseState
{
    private PlayerMotor _player;
    private int _stepsSinceStartedFalling;
    bool releasedJump;
    float speedMax;
    public override void EnterState()
    {   
        releasedJump=false;
        speedMax = _player.currentlySprinting == false ? _player.walkSpeed: _player.runSpeed;
    }
    public override void ExitState()
    {
    }
    public override void UpdateState()
    {
        if (releasedJump==false && _player.input.jumpUp && _player.velocity.y>0f)
        {
            _player.velocity += -Vector3.up*_player.velocity.y *.4f;
            _player.body.velocity = _player.velocity;
            releasedJump=true;
        }
    }
     public PlayerAirState(PlayerMotor player)
    {
        this._player = player;
        _stepsSinceStartedFalling = 0;
    }

    public override void FixedUpdateState()
    {
        UpdateInternalState();
        LedgeDetection(_player.forwardAxis);
        
        AdjustVelocityAir(_player.move, speedMax, _player.maxAirAcceleration);

        if (_player.SnapToGround())
        {
            _player.SwitchState(_player._groundState);
        }


        float gravityMultiplier = _player.velocity.y < .5 ? 1.5f : 1;

        _player.velocity += (_player.gravity) * gravityMultiplier * Time.deltaTime;

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

     void AdjustVelocityAir(Vector2 move, float speed, float acceleration)
    {
        float currentX = Vector3.Dot(_player.velocity, _player.rightAxis);
        float currentZ = Vector3.Dot(_player.velocity, _player.forwardAxis);
        float maxSpeedChange = acceleration * Time.deltaTime;
        _player.relativeVelocity = _player.velocity - _player.connectionVelocity;
        Vector3 m_unitgoal = (_player.rightAxis*move.x)+ (_player.forwardAxis*move.y);
        
        
        float newX = Mathf.MoveTowards(currentX, move.x * speed, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, move.y * speed, maxSpeedChange);
        _player.velocity += _player.rightAxis* (newX - currentX) + _player.forwardAxis * (newZ - currentZ);
    }

    void LedgeDetection(Vector3 aimDirection)
    {
        if (Physics.Raycast(_player.body.position+(Vector3.up*-.25f), aimDirection, out RaycastHit hit, 4) && Mathf.Abs(hit.normal.y)<.25f)
        {
            Vector3 wallUp= _player.ProjectDirectionOnPlane(Vector3.up, hit.normal);

             if (Physics.Raycast(hit.point+(wallUp*2)+(hit.normal*-.05f), -wallUp, out RaycastHit hit2, 1.95f))
             {
                if (Vector3.Distance(_player.body.position,hit2.point)< _player.maxMantleDistance)
                {
                    if (_player.body.position.y-.5f - hit2.point.y<-.5f && _player.body.velocity.y<1)
                    {
                        _player.mantleGoal=hit2.point;
                        _player.mantleUpDirection =wallUp;
                        _player.mantleNormalDirection= hit.normal;
                        _player.SwitchState(_player._mantleState);
                    }
                }
             }
        }
    }
}
