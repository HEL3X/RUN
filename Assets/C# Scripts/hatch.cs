
// This script represents a hatch in a Unity game.
// It checks for collisions with the player and determines if the player has the key to win.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hatch : MonoBehaviour
{
    public bool hasKey = false; 
    public GameObject onWin; 
    public AudioSource winSong; 
    public bool hasWon; 

    public void Start()
    {
        onWin.SetActive(false);
    }

    // This function is called when the player collides with the hatch trigger
    void OnTriggerEnter(Collider trigger)
    {
        Debug.Log("" + trigger.gameObject.tag);

        // Check if the collided object is the player and if the player has the key
        if (trigger.gameObject.tag == "Player" && hasKey)
        {
            Debug.Log("You Won!");

            // Activate the win object, freeze the game, and show the cursor
            onWin.SetActive(true);
            Time.timeScale = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            winSong.Play();
            hasWon = true;
        }
        // If the collided object is the player but the player doesn't have the key
        else if (trigger.gameObject.tag == "Player")
        {
            Debug.Log("Find the key!");
        }
    }
}
