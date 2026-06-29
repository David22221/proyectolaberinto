using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class ActivateTeleportationRay : MonoBehaviour
{
    public GameObject leftTeleportation;
    public GameObject rightTeleportation;

    public InputActionProperty leftActive;
    public InputActionProperty rigthActivate;

    
    // Update is called once per frame
    void Update()
    {
        leftTeleportation.SetActive(leftActive.action.ReadValue<float>() > 0.1f);
        rightTeleportation.SetActive(rigthActivate.action.ReadValue<float>() > 0.1f);
    }
}
