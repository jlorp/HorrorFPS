using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayerMotor : MonoBehaviour
{
    //Controls 
    [HideInInspector] public PlayerInput input;
    [HideInInspector] public Vector2 move;
    [HideInInspector] public Vector3 moveRelative;
    [HideInInspector] public bool jump;
    [HideInInspector] public bool sprint;
    [HideInInspector] public bool crouch;
    public Transform playerInputSpace = default;
    public CameraControl cameraScript;
    [HideInInspector] public InteractionManager interactionBrain;

    [Header("Jump")]
    [Range(0, 15)] public int coyoteTimeSteps = 2;
    [Range(0f, 100f)] public float maxAirAcceleration = 1f;
    [Range(0f, 10f)] public float jumpHeight = 2f;
    [Range(0f, .5f)] public float jumpBufferTime = .2f;
    private float _currentJumpBufferTime = 0f;

    [Header("Grounded")]
    [SerializeField] public AnimationCurve GroundAccellerationCurve;
    public LayerMask probeMask = -1;
    [Min(0f)] public float probeDistance = 1f;
    [Range(0f, 10f)] public float walkSpeed = 4f;
    [Range(0f, 20f)] public float runSpeed = 7f;
    [HideInInspector] public float walkHeight = .3f;
    [HideInInspector] public float crouchHeight= -0.25f;
    [Range(0f, 100f)] public float maxAcceleration = 10f;
    [Range(0f, 90f)] public float maxGroundAngle = 25f;
    [Range(0f, 100f)] public float maxSnapSpeed = 100f;
    
    [HideInInspector] public bool currentlySprinting=false;
    [HideInInspector] public bool currentlyCrouching=false;

    [Header("Mantling")]
    [HideInInspector] public Vector3 mantleGoal;
    [HideInInspector] public Vector3 mantleUpDirection;
    [HideInInspector] public Vector3 mantleNormalDirection;
    [Range(0f, 2f)] public float maxMantleDistance = 1f;

    // Axises
    [HideInInspector] public Vector3 forwardAxis;
    [HideInInspector] public Vector3 rightAxis;
    [HideInInspector] public Vector3 upAxis;
     // Tracking
    [HideInInspector] public int jumped = 0;

    //PlayerStates
    private PlayerBaseState _currentState;
    public PlayerGroundState _groundState;
    public PlayerMantleState _mantleState;
    public PlayerAirState _airState;

    // Velocities
    [HideInInspector] public Vector3 lastVelocity;
    [HideInInspector] public Vector3 velocity;
    [HideInInspector] public Vector3 connectionVelocity;
    [HideInInspector] public Vector3 relativeVelocity;
    public Vector3 gravity = new Vector3(0,1,0);

    // Rigidbodies
    [HideInInspector] public Rigidbody body;
    [HideInInspector] public Rigidbody connectedBody;
    [HideInInspector] public Rigidbody previousConnectedBody;
    [HideInInspector] public Vector3 connectionWorldPosition;
    [HideInInspector] public Vector3 connectionLocalPosition;
    [HideInInspector] public CapsuleCollider playerCollider;
    [HideInInspector] public CapsuleCollider playerColliderStanding;

    // Functions 
    public bool OnGround => groundContactCount > 0;
    public bool OnSteep => steepContactCount > 0;

    // Step counts
    [HideInInspector] public int stepsSinceLastGrounded;
    [HideInInspector] public int stepsSinceLastJump;

    // Min dots
    [HideInInspector] public float minGroundDotProduct;
 

    // Normals
    [HideInInspector] public Vector3 contactNormal;
    [HideInInspector] public Vector3 steepNormal;

    // Contact counts
    [HideInInspector] public int groundContactCount;
    [HideInInspector] public int steepContactCount;

    private void InitializeStates()
    {
        _groundState = new PlayerGroundState(this);
        _airState = new PlayerAirState(this);
        _mantleState = new PlayerMantleState(this);
    }

    private void Start()
    {
        input = GetComponent<PlayerInput>();
        body = GetComponent<Rigidbody>();
        interactionBrain= GetComponent<InteractionManager>();
        body.useGravity = false;
        playerCollider = GetComponent<CapsuleCollider>();
        sprint = false;
        upAxis=Vector3.up;

        cameraScript.cameraPositionGoal= new Vector3(0,walkHeight,0);

        InitializeStates();
        _currentState = _groundState;
        _currentState.EnterState();
    }

    public void AdjustVelocity(Vector2 move, float speed, float acceleration, bool useCurve)
    {
        Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
        Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);
        relativeVelocity = velocity - connectionVelocity;

        float currentX = Vector3.Dot(relativeVelocity, xAxis);
        float currentZ = Vector3.Dot(relativeVelocity, zAxis);
        float maxSpeedChange = acceleration * Time.deltaTime;
        
        Vector3 m_unitgoal = (xAxis*move.x)+ (zAxis*move.y);

        float accel=1f;
        if (useCurve)
        {
            accel= GroundAccellerationCurve.Evaluate(Vector3.Dot(m_unitgoal,relativeVelocity));  
        }
        
        float newX = Mathf.MoveTowards(currentX, move.x * speed, maxSpeedChange * accel);
        float newZ = Mathf.MoveTowards(currentZ, move.y * speed, maxSpeedChange * accel);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    private void GetControls()
    {
        move = new Vector2(input.horizontal, input.vertical);
        move = Vector2.ClampMagnitude(move, 1f);
        
        jump |= (input.jumpDown || _currentJumpBufferTime > 0);
        sprint = input.sprint;
        crouch = input.crouchDown;

        if (input.jumpDown)
        {
            _currentJumpBufferTime = jumpBufferTime;
        }
        else if (_currentJumpBufferTime < 0)
        {
            jump = false;
        }
        _currentJumpBufferTime -= Time.deltaTime;
    }

    private void Update()
    {
        GetControls();
        UpdateAxises();
        _currentState.UpdateState();
    }
     private void FixedUpdate()
    {
        _currentState.FixedUpdateState();
        ClearState();
    } 

    private void OnCollisionEnter(Collision col)
    {
        EvaluateCollision(col);
    }

    private void OnCollisionStay(Collision col)
    {
        EvaluateCollision(col);
    }

    private void EvaluateCollision(Collision collision)
    {
        int layer = collision.gameObject.layer;
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(upAxis, normal);
            float impactDot = Vector3.Dot(normal, lastVelocity.normalized);

            if (upDot >= minGroundDotProduct)
            {
                groundContactCount += 1;
                contactNormal += normal;
                connectedBody = collision.rigidbody;
            }
            else
            {
                if (upDot > 0.1f)
                {
                    steepContactCount += 1;
                    steepNormal += normal;

                    if (groundContactCount == 0)
                    {
                        connectedBody = collision.rigidbody;
                    }
                }
            }
        }
    }

    public void SwitchState(PlayerBaseState state)
    {
        _currentState.ExitState();
        _currentState = state;
        _currentState.EnterState();
    }
    private void UpdateAxises()
    {
        rightAxis = playerInputSpace ?
            ProjectDirectionOnPlane(playerInputSpace.right, upAxis) :
            ProjectDirectionOnPlane(Vector3.right, upAxis);
        forwardAxis = playerInputSpace ?
            ProjectDirectionOnPlane(playerInputSpace.forward, upAxis) :
            ProjectDirectionOnPlane(Vector3.forward, upAxis);
    }

    public Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }
    private void ClearState()
    {
        // Clear contact counts
        groundContactCount = 0;
        steepContactCount = 0;

        // Clear normals
        contactNormal = Vector3.zero;
        steepNormal = Vector3.zero;
        connectionVelocity = Vector3.zero;

        // Clear connected body data
        previousConnectedBody = connectedBody;
        connectedBody = null;

        // Update last velocity
        lastVelocity = velocity;
    }

    public bool SnapToGround()
    {
        if (stepsSinceLastGrounded > 1 && stepsSinceLastJump <= 3)
        {
            return false;
        }

        float speed = velocity.magnitude;
    
        if (speed > maxSnapSpeed)
        {
            return false;
        }

        if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit, probeDistance, probeMask))
        {   
            return false;
        }

        float upDot = Vector3.Dot(upAxis, hit.normal);
        if (upDot < minGroundDotProduct)
        {
            return false;
        }

        groundContactCount = 1;
        contactNormal = hit.normal;

        float dot = Vector3.Dot(velocity, hit.normal);
        if (dot > 0f)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }
        connectedBody = hit.rigidbody;

        return true;
    }

    public bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();
            float upDot = Vector3.Dot(upAxis, steepNormal);
            if (upDot >= minGroundDotProduct)
            {
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }

    public void ResetJump()
    {
        jump = false;
        _currentJumpBufferTime = -1;
    }

    public void Jump(Vector3 gravity)
    {
        jumped = 0;
        stepsSinceLastJump = 0;
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);

       
        Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
        Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);
        relativeVelocity = velocity - connectionVelocity;

        jumped += 1;
        
        float jumptweak=0f;
        if(velocity.y>0)
        {
            jumptweak=velocity.y*.6f;
        }
        else
        {
            jumptweak=velocity.y;
        }
    
        velocity += Vector3.up* (jumpSpeed-jumptweak);
        SwitchState(_airState);
    }
}
