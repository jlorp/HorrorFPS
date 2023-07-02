using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMantleState : PlayerBaseState
{
    private PlayerMotor _player;
    public Vector3 goalPoint;

    public override void EnterState()
    {  
        goalPoint= _player.mantleGoal;
        float yTweak=.5f;

        if(_player.velocity.y<0f)
        {
            yTweak=0f;
        }
        _player.velocity = new Vector3(0,_player.velocity.y* yTweak, 0);

        _player.body.velocity = _player.velocity;
    }
    public override void ExitState()
    {
        _player.mantleGoal= Vector3.zero;
        Debug.Log("We done mantlin.");
    }
    public override void UpdateState()
    {
    }
     public PlayerMantleState(PlayerMotor player)
    {
        this._player = player;
     
    }

    public override void FixedUpdateState()
    {
        AdjustVelocityMantle(goalPoint, 4f , 35);
        float gravityMultiplier = _player.velocity.y < .5 ? 1.5f : 1;
        _player.velocity += (_player.gravity) * gravityMultiplier * Time.deltaTime;
        _player.body.velocity = _player.velocity;

        if (goalPoint.y+1f<=_player.body.position.y && Vector3.Distance(new Vector3(goalPoint.x,0,goalPoint.z), new Vector3(_player.body.position.x, 0, _player.body.position.z))<.1f)
        {
            _player.SwitchState(_player._groundState);
        }
    }

    void AdjustVelocityMantle(Vector3 goal, float speed, float acceleration)
    {
        float yGoal=0;
        
        Vector3 flatgoal= goal- _player.body.position;
        flatgoal.y=0;
        flatgoal= flatgoal.normalized;

        if(goal.y+1f>_player.body.position.y)
        {
            yGoal=1;
        }
        float maxSpeedChange = acceleration * Time.deltaTime;
        _player.relativeVelocity = _player.velocity - _player.connectionVelocity;

        float currentY= _player.relativeVelocity.y;
  
        float newY= Mathf.MoveTowards(currentY, speed, maxSpeedChange);

       
        _player.velocity+= -_player.mantleUpDirection* yGoal * (currentY-newY) + (flatgoal*maxSpeedChange*.1f);
    }

}
