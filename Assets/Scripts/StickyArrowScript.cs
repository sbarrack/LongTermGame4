﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyArrowScript : MonoBehaviour
{
    Rigidbody arrowRB;
    GameObject arrowGO;

    private void Start()
    {
        arrowRB = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        /*
         * Not sure what would be the most efficient could be done in one of multible ways
         * arrowRB.sleep(); but if arrow is hit by another arrow they both fall
         *
         */
        if (!collision.gameObject.CompareTag("arrow"))//To keep players from stacking arrows oddly
        {
            arrowRB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            arrowRB.isKinematic = true;
            if(collision.rigidbody != null)
                gameObject.transform.parent = collision.gameObject.transform;
            gameObject.transform.position = collision.GetContact(0).point - transform.forward * .4f;

        }
    }
}
