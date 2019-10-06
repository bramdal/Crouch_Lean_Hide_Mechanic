using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    CharacterController characterController;
    float inputX;
    float inputZ;
    float inputY;

    Vector3 movementVector = Vector3.zero;
    //movement variables
    [Header("Movement Variables")]
    public float movementSpeed;

    public float gravity;
    //uncomment if jump allowed
    //public float jumpSpeed;

    public float rotationSlerpTime;

    //public references
    [Space]
    [Header("Lean Shoulders")]
    public Transform leftShoulder;
    public Transform rightShoulder;

    //state bools
    bool walking;
    bool canTakeCover = false;
    [HideInInspector]public bool isLeaning = false;
    [HideInInspector]public bool crouched = false;

    Animator anim;

    //camera controls while leaning
    [Space]
    [Header("Camera variables for leaning")]
    public CinemachineFreeLook cam1;
    //public CinemachineFreeLook cam2;   //  use cam2 to change view while crouch
    public CinemachineCameraOffset cameraOffset;
    public Vector3 crouchedCameraOffset;
    public float leanXOffset = 1f;

    public float rayCastDistance = 0.5f;

    private void Start() {
        characterController = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        //cam2.enabled = false;
    }
    private void Update() {
        if(!crouched)
            GetInputs();
        else
            GetCrouchedInputs();    
    }
    private void LateUpdate() {
        if(!crouched)
            SetAnimations();
        else
            SetCrouchAnimations();
    }
    
    
    void GetInputs(){
        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");

        Vector3 forwardDirection = Camera.main.transform.forward;
        Vector3 rightDirection = Camera.main.transform.right;

        forwardDirection.y = rightDirection.y = 0f;

        forwardDirection.Normalize();
        rightDirection.Normalize();

        movementVector = forwardDirection * movementSpeed * inputZ + rightDirection * movementSpeed * inputX;
        if(movementVector.magnitude == 0f)
            walking = false;
        else 
            walking = true;   

        if(movementVector != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movementVector), rotationSlerpTime);

        //vertical
        //uncomment if jump allowed
        // if(characterController.isGrounded){
        //     if(Input.GetButtonDown("Jump"))
        //         movementVector.y = jumpSpeed;
        // }    
        movementVector.y -= gravity * Time.deltaTime;

        characterController.Move(movementVector * Time.deltaTime);

    }

    void GetCrouchedInputs(){
        //Debug.DrawRay(leftShoulder.position, -leftShoulder.forward * rayCastDistance, Color.blue);  //Debug the raycasts
        bool leftEdgeInfo = Physics.Raycast(leftShoulder.position, -leftShoulder.forward, rayCastDistance);       
        bool rightEdgeInfo = Physics.Raycast(rightShoulder.position, -rightShoulder.forward, rayCastDistance);
        
        inputX = -Input.GetAxis("Horizontal");

        if((inputX > 0f && rightEdgeInfo) || (inputX < 0f && leftEdgeInfo)){
            movementVector = new Vector3(inputX, 0f, 0f);
            movementVector = transform.TransformDirection(movementVector);
            if(movementVector.magnitude == 0f)
                walking = false;
            else
                walking = true;
            
            //movementVector.y -= gravity * Time.deltaTime;

            characterController.Move(movementVector * Time.deltaTime);
        }
        else if(inputX >0f && !rightEdgeInfo)
        {
            walking = false;
            isLeaning = true;
            anim.SetBool("CrouchLean", true);
            Vector3 tempVector = cameraOffset.m_Offset;
            tempVector.x = Mathf.Clamp(tempVector.x, 0f, leanXOffset);
            tempVector.z = Mathf.Clamp(tempVector.z, 0f, 4.5f);
            cameraOffset.m_Offset = Vector3.Lerp(cameraOffset.m_Offset, new Vector3(tempVector.x - leanXOffset, tempVector.y, tempVector.z + 0.5f), Time.deltaTime);
        }
        else if(inputX <0f && !leftEdgeInfo)
        {
            walking = false;
            isLeaning = true;
            anim.SetBool("CrouchLean", true);
            Vector3 tempVector = cameraOffset.m_Offset;
            tempVector.x = Mathf.Clamp(tempVector.x, leanXOffset, 0f);
            tempVector.z = Mathf.Clamp(tempVector.z, 0f, 4.5f);
            cameraOffset.m_Offset = Vector3.Lerp(cameraOffset.m_Offset, new Vector3(tempVector.x + leanXOffset, tempVector.y, tempVector.z + 0.5f), Time.deltaTime);
        }
        else
        {
            walking = false;
            isLeaning = false;
            anim.SetBool("CrouchLean", false);
            //cameraOffset.m_Offset.x = 0f;
            Vector3 tempVector = cameraOffset.m_Offset;
            cameraOffset.m_Offset = Vector3.Lerp(cameraOffset.m_Offset, new Vector3(0f, tempVector.y, 4f), 3 * Time.deltaTime);
        }
        
        //left and right lean on edges
        //add camera of gameplay functionality here
        // if(!leftEdgeInfo){
        //     print("left lean");
        // }

        // if(!rightEdgeInfo){
        //     print("right lean");
        // }
    }

    void SetAnimations(){
        if(walking){
            anim.SetBool("Moving", true);
            anim.SetFloat("Velocity Z", movementVector.magnitude);
        }
        else
        {
            anim.SetBool("Moving", false);
            anim.SetFloat("Velocity Z", 0f);
        }
    }

    void SetCrouchAnimations(){
        if (walking)
        {
            anim.SetBool("Moving", true);
            anim.SetFloat("Velocity X", inputX);
            if(inputX>0)
                anim.SetBool("CrouchDirection", true);
            else
                anim.SetBool("CrouchDirection", false);    
        }
        else
        {
            anim.SetBool("Moving", false);
            anim.SetFloat("Velocity X", 0f);
        }
    }
    
    private void OnTriggerEnter(Collider other) {
        if(other.tag == "cover")
            canTakeCover = true;

    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "cover")
            canTakeCover = false;
    }

    private void OnTriggerStay(Collider other) {
        if(other.tag == "cover"){
            if(canTakeCover){
                if(Input.GetButtonDown("Fire2")){
                    if(!crouched){
                        transform.parent = other.gameObject.transform.parent;
                        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-other.transform.forward), 1f);
                        anim.SetBool("Moving", false);
                        anim.SetFloat("Velocity Z", 0f);
                        crouched = true;
                        movementSpeed -= 2f;

                        anim.SetBool("Crouch", true);

                        //rotate player appropriately
                        if(transform.localPosition.z > 0f){
                            print("behind the wall");
                            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(other.transform.forward), 1f);      
                        }    
                        else{
                            print("ahead of the wall");
                            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-other.transform.forward), 1f);
                        }

                        cameraOffset.m_Offset = new Vector3(0f, -1f, 4f);

                        // cam2.gameObject.SetActive(true);         //switch between cameras if 2nd camera added
                        // cam1.gameObject.SetActive(false);
                        
                        
                    } 
                    else if(crouched){
                        transform.parent = null;
                        crouched = false;
                        isLeaning = false;
                        movementSpeed += 2f;
                        anim.SetBool("Crouch", false);

                        cameraOffset.m_Offset = Vector3.zero;

                        // cam1.gameObject.SetActive(true);
                        // cam2.gameObject.SetActive(false);

                        print("new parent cover released");
                    }   
                }
            }
        }    
    }


   
    
    
}
