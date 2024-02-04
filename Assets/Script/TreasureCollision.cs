using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureCollision : MonoBehaviour
{
    public string playerName = "PlayerArmature";

    public GameObject v_camera;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter");
        if (other.name.Equals(playerName))
        {
            v_camera.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerExit");
        if (other.name.Equals(playerName))
        {
            v_camera.SetActive(false);
        }
    }
}
