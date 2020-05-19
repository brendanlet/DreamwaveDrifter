using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour, IPowerup
{
    private GameObject owner;
    private string name = "Health";
    public bool isActive;

    public void Activate()
    {
        bikeBrain bb = owner.GetComponent<bikeBrain>();
        bb.health = bb.totalHealth;
        bb.UpdateHealth();
    }

    public void Destroy()
    {
        Destroy(this);
    }

    public string GetName()
    {
        return name;
    }

    public void OnDrop()
    {
        throw new System.NotImplementedException();
    }

    public void OnPickup()
    {
        throw new System.NotImplementedException();
    }

    public void OnPowerUpEnd()
    {
        throw new System.NotImplementedException();
    }

    public void SetOwner(GameObject owner)
    {
        this.owner = owner;
    }

    public void SetIsActive(bool active)
    {
        isActive = active;
    }

    public bool GetIsActive()
    {
        return isActive;
    }

    public void Update()
    {
        if (isActive)
        {
            //Do stuff while active
        }
    }
}
