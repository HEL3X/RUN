using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Used https://www.youtube.com/watch?v=PmIPqGqp8UY&t=3s tutorial by Acacia Developer to get me started with some basic movement

    [SerializeField] Transform playerCam = null;
    [SerializeField] public float sens;
    [SerializeField] public float walkSpeed = 5f;
    [SerializeField] public float jumpHeight = 2f;
    [SerializeField] public float gravity = -13f;
    float camVert = 0f;
    float yVelocity = 0f;
    CharacterController controller = null; 


    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        MouseUpdate();
        MovementKeys();
    }

    void MouseUpdate()
    {
        Vector2 mouseDt = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        
        //pitch
        camVert = camVert - mouseDt.y * sens;

        //clamps the max angle you can look up / down
        camVert = Mathf.Clamp(camVert, -90f, 90f);

        //rotate by right vector 
        playerCam.localEulerAngles = Vector3.right * camVert;

        //transforms
        transform.Rotate(Vector3.up * mouseDt.x * sens);
    }

    private void MovementKeys()
    {
        Vector2 DirInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        //Normalize the vector so you don't go faster walking diagonally 
        DirInput.Normalize();

        //movement
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

        if (Input.GetKey("left shift"))
        {
            walkSpeed = 10f;
        }
        else
        {
            walkSpeed = 5f;
        }

        if (Input.GetKey("space"))
        {
           
        }
    }
}
