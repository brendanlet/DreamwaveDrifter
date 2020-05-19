using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPowerup
{
    void Activate();

    void OnDrop();

    void OnPickup();

    void OnPowerUpEnd();

    void SetOwner(GameObject owner);

    void Destroy();

    string GetName();

    bool GetIsActive();

    void SetIsActive(bool active);

    void Update();
}
