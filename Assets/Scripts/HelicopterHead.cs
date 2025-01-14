﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopterHead : MonoBehaviour
{
    public PlayerInput playerInput;
    private PlayerPhysics playerPhysics;
    public Animator playerAnim;
    public GameObject heliHead;
    public float x, y, z, w;
    public GameObject pointer;
    public float xzBase;
    public float yBase;
    public float xzRate;
    public float yRate;
    public float xzGoal;
    public float yGoal;

    // Start is called before the first frame update
    void Start()
    {
        // Asks which device is being used (maybe this shouldn't be handled in the Helicopter Head class)
        // if android, playerInput = new AndroidPlayerInput();
        // maybe in the awake method, we define an enum which can decide which device you're on
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            playerInput = new WindowsPlayerInput();
            playerInput.InitializeInput(gameObject);
        } else if ( Application.platform == RuntimePlatform.Android)
        {
            playerInput = new OculusPlayerInput();
            playerInput.InitializeInput(gameObject);
        } else
        {
            playerInput = new WindowsPlayerInput();
        }
        playerPhysics = new PlayerPhysics();

    }

    // Update is called once per frame
    void Update()
    {
        playerPhysics.HeadHandling(heliHead, playerInput);
    }

    private void FixedUpdate()
    {
        Vector3 tryHard = Vector3.Normalize(new Vector3(pointer.transform.position.x - transform.position.x, pointer.transform.position.y - transform.position.y, pointer.transform.position.z - transform.position.z));
        float chargeTime = playerPhysics.FlightHandling(heliHead.transform.localRotation.eulerAngles, GetComponent<Rigidbody>(), playerInput);
        if (chargeTime > 0f)
        {
            HelicopterEvents.instance.BoostTriggered();
            playerAnim.SetTrigger("Boost");
            StartCoroutine(superDash(tryHard, chargeTime));
        }
    }


    // Look at how much this deals directly with wind. This would be hard to trace and edit. Consider decoupling
    public void OnTriggerEnter(Collider other)
    {
        var windCheck = other.GetComponent<WindBox>();
        if (windCheck != null)
        {
            if (windCheck.FillWind())
            {
                playerPhysics.wind = 1.5f;
            }
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 9)
        {
            playerPhysics.wind += 1.5f * Time.deltaTime;
            if (playerPhysics.wind >= 1.5f)
            {
                playerPhysics.wind = 1.5f;
            }
        }
    }

    //This feels like a physics thing but whatever
    IEnumerator superDash(Vector3 tums, float chargeTime)
    {
        // suspend execution for 5 seconds
        float startTime = 0;
        Rigidbody rigid = GetComponent<Rigidbody>();
        Debug.Log(tums);
        Vector3 dashVelocity = new Vector3(Vector3.Normalize(tums).x * xzBase, Vector3.Normalize(tums).y * yBase, Vector3.Normalize(tums).z * xzBase);
        Debug.Log(dashVelocity);
        float yRateRate = 0f;
        while (startTime <= chargeTime)
        {
            Debug.Log(rigid.velocity);

            dashVelocity = new Vector3(
                Vector3.MoveTowards(dashVelocity, new Vector3(xzGoal * tums.x, 0f, 0f), xzRate * Time.deltaTime).x,
                Vector3.MoveTowards(dashVelocity, new Vector3(0f, yGoal, 0f), yRateRate  * Time.deltaTime).y, // used to be 12
                Vector3.MoveTowards(dashVelocity, new Vector3(0f, 0f, xzGoal * tums.z), xzRate * Time.deltaTime).z
                );
            rigid.velocity = dashVelocity;
            startTime += Time.deltaTime;
            yRateRate = yRate * startTime;
            yield return new WaitForFixedUpdate();
        }
        print("Honestly Tried " + Time.time);
        
    }

    public float GetCharge()
    {
        return playerPhysics.charge;
    }
    public float GetWind()
    {
        return playerPhysics.wind;
    }
    public float GetWindForUI()
    {
        var physo = playerPhysics.wind/1.5f;
        if (physo >= 1f)
        {
            physo = 0.99f;
        } else if (physo <= 0f)
        {
            physo = 0f;
        }
        return physo;
    }
}
