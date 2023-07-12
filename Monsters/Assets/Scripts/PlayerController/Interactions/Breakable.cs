using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    [SerializeField] private float minHitForce = 10f;
    private bool isBroken = false;
    [SerializeField] private List<Collider> ignoreColliders = new List<Collider>();
    [SerializeField] private GameObject[] spawnOnBreak = new GameObject[0];

    public bool CanBreak()
	{
		if(!enabled)
			return false;

		return true;
	}

    public void OnCollisionEnter(Collision collision)
    {
		if (!CanBreak())
			return;

		if (collision.relativeVelocity.magnitude < minHitForce)
			return;

        Break(collision);
    }

    public void Break()
	{
		Break(null);
	}
    public void Break(Collision collision = null)
	{
		if (!CanBreak())
			return;

        isBroken = true;

        if (isBroken)
		{
            SpawnOnBreak();
            Destroy(this.gameObject);
		}
	}

    public void SpawnOnBreak()
	{
		for(int i= 0; i < spawnOnBreak.Length; i++)
		{
			Instantiate(spawnOnBreak[i], transform.position, transform.rotation);
		}
	}
}
