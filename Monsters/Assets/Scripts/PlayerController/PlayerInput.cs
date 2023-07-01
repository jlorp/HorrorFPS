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
   

    private void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        aimHorizontal = Input.GetAxisRaw("HorizontalAim");
        aimVertical = Input.GetAxisRaw("VerticalAim");

        jump = Input.GetButton("Jump");
        jumpDown = Input.GetButtonDown("Jump");
        jumpUp = Input.GetButtonUp("Jump");
    }
}
