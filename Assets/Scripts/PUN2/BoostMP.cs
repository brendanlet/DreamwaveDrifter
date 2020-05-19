using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostMP : MonoBehaviour, IPowerup
{
    [SerializeField]
    private float boostTimer = 10;
    private GameObject owner;
    private string name = "OverClock";
    public bool isActive;

    public void Activate()
    {
        bikeBrainMP bb = owner.GetComponent<bikeBrainMP>();
        bb.SetBoost(true, 10);
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
