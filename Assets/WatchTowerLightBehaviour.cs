using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchTowerLightBehaviour : MonoBehaviour
{
    public GameObject playerHead;

    Light spotlight;

    RaycastHit hitPoint;

    private void Start() {
        //player = GameObject.FindWithTag("Player");
        spotlight = GetComponent<Light>();
    }

    private void Update() {
        transform.LookAt(playerHead.transform);

        if(Physics.Raycast(transform.position, transform.forward, out hitPoint, Mathf.Infinity)){
            if(hitPoint.collider.tag == "cover"){
                print("Hit cover");
                spotlight.color = Color.Lerp(spotlight.color, Color.white, 0.1f);
            }
            else if(hitPoint.collider.tag == "Player")
            {
                else if(hitPoint.collider.gameObject.GetComponent<CharacterMovement>().isLeaning)
                    spotlight.color = Color.Lerp(spotlight.color, Color.blue, 0.1f);
                else
                    spotlight.color = Color.Lerp(spotlight.color, Color.red, 0.1f);
            }
        }
    }
}
