using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public GameObject reticleObject;
    public GameObject heldPosition;
    
    [HideInInspector] public PlayerInput input;
    [HideInInspector] public bool interact;
    [HideInInspector] public bool toss;
    bool hitPickup;
    bool hitInteract;

    public GameObject heldObject;

    RaycastHit hit;

    void Start()
    {
        input = GetComponent<PlayerInput>();
    }

    void Update()
    {
        GetControls();

        if (heldObject!=null)
        {
            if (interact)
            {
                Drop();
            }
            else if (toss)
            {
                Throw();
            }
        }
        else if (interact && hitPickup && hit.distance < 2f)
        {
            PickUp(hit.collider.gameObject);
        }

    }
    void FixedUpdate()
    {
        Ray rayReticle = Camera.main.ScreenPointToRay(reticleObject.transform.position);

        if (Physics.Raycast(rayReticle,out hit))
        {
            if (hit.collider != null)
            {
                Pickupable pickupScript = hit.collider.gameObject.GetComponent<Pickupable>();
                Interactable interactScript = hit.collider.gameObject.GetComponent<Interactable>();

                if (pickupScript!=null)
                {
                    hitPickup = true;
                }
                else
                {
                    hitPickup=false;
                }

                if (interactScript!=null)
                {
                    hitInteract=true;
                }
                else
                {
                    hitInteract=false;
                }
            }
        }
    }

    private void GetControls()
    {
        interact= input.interactDown;
        toss = input.tossDown;
    }

    void PickUp(GameObject target)
    {
        heldObject= target;

        heldObject.transform.SetParent(heldPosition.transform);
        heldObject.transform.localPosition= Vector3.zero;
        heldObject.transform.localRotation= Quaternion.identity;

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if(rb!=null)
        {
            rb.isKinematic=true;
            rb.detectCollisions=false;
        }
    }

    void Drop()
    {
        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if(rb!=null)
        {
            rb.isKinematic=false;
            rb.detectCollisions=true;
        }
        heldObject.transform.SetParent(null);
        heldObject=null;
    }

    void Throw()
    {
        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if(rb!=null)
        {
            rb.isKinematic=false;
            rb.detectCollisions=true;
        }
        heldObject.transform.SetParent(null);

        rb.AddForce(heldPosition.transform.forward * 150f);
        heldObject=null;
    }
}
