using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public float horizontal { get; private set; }
    public float vertical { get; private set; }
    public float aimHorizontal { get; private set; }
    public float aimVertical { get; private set; }
    
    public bool jump { get; private set; }
    public bool jumpDown { get; private set; }
    public bool jumpUp{ get; private set; }

    public bool sprint  { get; private set; }
    public bool sprintUp    { get; private set; }
    public bool sprintDown  { get; private set; }

    public bool crouch { get; private set; }
    public bool crouchUp { get; private set; }
    public bool crouchDown { get; private set; }

    public bool interact  { get; private set; }
    public bool interactUp { get; private set; }
    public bool interactDown { get; private set; }

    public bool toss { get; private set; }
    public bool tossDown { get; private set; }


    private void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        aimHorizontal = Input.GetAxisRaw("HorizontalAim");
        aimVertical = Input.GetAxisRaw("VerticalAim");

        jump = Input.GetButton("Jump");
        jumpDown = Input.GetButtonDown("Jump");
        jumpUp = Input.GetButtonUp("Jump");

        sprint = Input.GetButton("Sprint");
        sprintDown = Input.GetButtonDown("Sprint");
        sprintUp = Input.GetButtonUp("Sprint");

        crouch = Input.GetButton("Crouch");
        crouchDown = Input.GetButtonDown("Crouch");
        crouchUp = Input.GetButtonUp("Crouch");

        interact = Input.GetButton("Interact");
        interactUp = Input.GetButtonUp("Interact");
        interactDown = Input.GetButtonDown("Interact");
        
        toss = Input.GetButton("Toss");
        tossDown = Input.GetButtonDown("Toss");
    }
}
