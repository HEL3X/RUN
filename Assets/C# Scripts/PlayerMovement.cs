using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    //Used https://www.youtube.com/watch?v=PmIPqGqp8UY&t=3s tutorial by Acacia Developer to get me started with some basic movement

    [SerializeField] Transform playerCam = null;
    [SerializeField] public float sens;
    [SerializeField] public float walkSpeed = 5f;
    [SerializeField] public float runSpeed = 10f;
    [SerializeField] public float gravity = -13f;
    float camVert = 0f;
    float yVelocity = 0f;
    CharacterController controller = null;
    public bool isMoving;
    public bool isRunning;
    Vector3 oldPos;
    Vector3 newPos;
    public AudioSource audioSource;
    public AudioClip run;
    public AudioClip walk;

    public float stam;
    public float maxStam;
    public Slider stamBar;
    public float dVal;
    public float runDelay;
    public AudioSource outOfBreath;

    // Start is called before the first frame update
    void Start()
    {
        // Initialization
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isMoving = false;
        isRunning = false;
        audioSource = gameObject.GetComponent<AudioSource>();

        maxStam = stam;
        stamBar.maxValue = maxStam;
    }

    void Update()
    {
        MouseUpdate();
        MovementKeys();
        checkStanding();

        if (stam < 0.1f && isRunning)
        {
            walkSpeed = 5f;
            isRunning = false;
            stam = runDelay;
            outOfBreath.Play();
        }
    }

    private void DecreaseStam()
    {
        if (stam != 0)
        {
            stam -= dVal * Time.deltaTime;
        }
        stamBar.value = stam;
    }

    private void IncreaseStam()
    {
        if (stam != maxStam)
        {
            stam += dVal * Time.deltaTime;
        }
        stamBar.value = stam;
    }

    void MouseUpdate()
    {
        Vector2 mouseDt = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        // Adjust the camera's vertical rotation (pitch)
        camVert = camVert - mouseDt.y * sens;

        // Clamp the maximum angle for looking up/down
        camVert = Mathf.Clamp(camVert, -90f, 90f);

        // Rotate the camera using the right vector
        playerCam.localEulerAngles = Vector3.right * camVert;

        // Rotate the player using the mouse's horizontal movement
        transform.Rotate(Vector3.up * mouseDt.x * sens);
    }

    //Check if Player is Moving or Not
    void checkStanding()
    {
        newPos = transform.position;
        if (newPos != oldPos)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
        oldPos = newPos;
    }

    private void MovementKeys()
    {
        Vector2 DirInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Normalize the input vector to avoid faster diagonal movement
        DirInput.Normalize();

        // Calculate the player's velocity based on the input and current speed
        Vector3 velocity = (transform.forward * DirInput.y + transform.right * DirInput.x) * walkSpeed + Vector3.down * yVelocity;
        controller.Move(velocity * Time.deltaTime);

        if (controller.isGrounded)
        {
            yVelocity = 0f;
        }
        else
        {
            yVelocity += gravity * Time.deltaTime;
        }

        if (stam > 0 && Input.GetKey("left shift"))
        {
            walkSpeed = runSpeed;
            isRunning = true;
            DecreaseStam();
        }
        else
        {
            walkSpeed = 5f;
            isRunning = false;
            if (stam < 50)
            {
                IncreaseStam();
            }
        }

        // Switch audio clip based on movement state
        if (isMoving && isRunning)
        {
            audioSource.clip = run;
        }
        else if (isMoving)
        {
            audioSource.clip = walk;
        }
        else
        {
            audioSource.clip = null;
        }

        // Play audio clip if not already playing
        if (audioSource.clip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
