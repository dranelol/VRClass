using System;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.UI;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using UnityStandardAssets.ImageEffects;
using Random = UnityEngine.Random;



namespace UnityStandardAssets.Characters.FirstPerson
{
    public enum NavigationMethod
    {
        normalLinear,
        normalAccel,
        segment,
        normalLinearLerp,
        normalAccelLerp,
        segmentLerp
    }

    public enum CurtainState
    {
        turningOn,
        on,
        turnOnDelay,
        turningOff,
        off,
        turnOffDelay
    }

    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private bool m_IsWalking;
        [SerializeField] private float m_WalkSpeed;
        [SerializeField]
        private float m_RunSpeed;
        [SerializeField]
        private float segmentSpeed;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] private MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

        public bool UIActive = true;
        private Rigidbody rigidbody;



        public float NormalFOV;
        public float TunnelVisionFOV;

        [SerializeField] private VignetteAndChromaticAberration Vignette;
        [SerializeField] public Camera Cam;
        [SerializeField] private bool DimScreen;
        [SerializeField] private Image Curtain;
        private bool lerpMoving;

        public float DimTime;

        public float DimDelay;

        public NavigationMethod NavMethod;

        public CurtainState CurState;
        public float LerpTime;
        public int LerpSegments;

        private Camera m_Camera;
        private bool m_Jump;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
        private AudioSource m_AudioSource;

        public void SetVignette(bool vignette)
        {
            if (vignette == true)
            {
                Vignette.enabled = true;
                Cam.fieldOfView = TunnelVisionFOV;
            }

            else
            {
                Vignette.enabled = false;
                Cam.fieldOfView = NormalFOV;
            }
        }

        public void SetDimScreen(bool dim)
        {
            DimScreen = dim;
        }

        public void SetHeadBob(bool bob)
        {
            m_UseHeadBob = bob;
        }

        public void SetFOVKick(bool kick)
        {
            m_UseFovKick = kick;
        }
        public void ChangeNavMethod(int method)
        {
            switch (method)
            {
                case 0:
                    NavMethod = NavigationMethod.normalLinear;

                    break;

                case 1:
                    NavMethod = NavigationMethod.normalAccel;

                    break;

                case 2:
                    NavMethod = NavigationMethod.segment;

                    break;

                case 3:
                    NavMethod = NavigationMethod.segmentLerp;

                    break;

                case 4:
                    NavMethod = NavigationMethod.normalLinearLerp;

                    break;

                case 5:
                    NavMethod = NavigationMethod.normalAccelLerp;

                    break;
            }
        }

        private IEnumerator curtainOn()
        {
            float t = 0.0f;

            float seconds = DimTime;

            float start = Curtain.color.a;

            float end = 1.0f;

            CurState = CurtainState.turningOn;

            while (t <= 1.0f)
            {
                t += Time.deltaTime / seconds;

                float alpha = Mathf.Lerp(start, end, t);

                Color newColor = Curtain.color;

                newColor.a = alpha;

                Curtain.color = newColor;

                yield return new WaitForFixedUpdate();
            }

            CurState = CurtainState.turnOnDelay;

            yield return new WaitForSeconds(DimDelay);

            CurState = CurtainState.on;

            yield return null;
        }

        private IEnumerator curtainOff()
        {
            float t = 0.0f;

            float seconds = DimTime;

            float start = Curtain.color.a;

            float end = 0.0f;

            CurState = CurtainState.turningOff;

            while (t <= 1.0f)
            {
                t += Time.deltaTime / seconds;

                float alpha = Mathf.Lerp(start, end, t);

                Color newColor = Curtain.color;

                newColor.a = alpha;

                Curtain.color = newColor;

                yield return new WaitForFixedUpdate();
            }

            CurState = CurtainState.turnOffDelay;

            yield return new WaitForSeconds(DimDelay);

            CurState = CurtainState.off;

            yield return null;
        }

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }
        // Use this for initialization
        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Cam.GetComponent<Camera>();
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle/2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();
			m_MouseLook.Init(transform , m_Camera.transform);
        }


        // Update is called once per frame
        private void Update()
        {
            RotateView();
            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }


        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
            if(UIActive == true)
            {
                //rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                //Cam.transform.rotation = Quaternion.identity;
                //Cam.transform.localRotation = Quaternion.identity;
                //UnityEngine.VR.VRSettings.enabled = false;
            }

            if (UIActive == false)
            {
                //rigidbody.constraints = RigidbodyConstraints.None;

                //UnityEngine.VR.VRSettings.enabled = true;
            }

            float speed;
            GetInput(out speed);

            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = Cam.transform.forward*m_Input.y + Cam.transform.right*m_Input.x;
            Debug.Log(transform.forward);
            if(NavMethod == NavigationMethod.segmentLerp)
            {
                
            }


            else
            {
                MovePosition(desiredMove, speed);
            }
            
        }

        private void MovePosition(Vector3 destination, float speed)
        {
            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height / 2f);

            destination = Vector3.ProjectOnPlane(destination, hitInfo.normal);//.normalized;

            m_MoveDir.x = destination.x * speed;
            m_MoveDir.z = destination.z * speed;

            Debug.Log("speed: " + speed);

            switch (NavMethod)
            {
                case NavigationMethod.normalLinear:
                    {
                        if (DimScreen == true)
                        {
                            if (destination != Vector3.zero)
                            {
                                switch (CurState)
                                {
                                    case CurtainState.off:
                                        {
                                            StartCoroutine(curtainOn());

                                            break;
                                        }
                                }
                            }

                            if (destination == Vector3.zero)
                            {
                                switch (CurState)
                                {
                                    case CurtainState.on:
                                        {
                                            StartCoroutine(curtainOff());

                                            break;
                                        }
                                }
                            }
                        }

                        break;
                    }

                case NavigationMethod.normalAccel:
                    {
                        if (DimScreen == true)
                        {
                            if (destination != Vector3.zero)
                            {
                                switch (CurState)
                                {
                                    case CurtainState.off:
                                        {
                                            StartCoroutine(curtainOn());

                                            break;
                                        }
                                }
                            }

                            if (destination == Vector3.zero)
                            {
                                switch (CurState)
                                {
                                    case CurtainState.on:
                                        {
                                            StartCoroutine(curtainOff());

                                            break;
                                        }
                                }
                            }
                        }

                        break;
                    }

                case NavigationMethod.segment:
                    {
                        if (Input.GetKeyDown(KeyCode.W)
                        || Input.GetKeyDown(KeyCode.S)
                        || Input.GetKeyDown(KeyCode.A)
                        || Input.GetKeyDown(KeyCode.D))
                        {

                            PlayFootStepAudio();
                        }

                        break;
                    }

                case NavigationMethod.normalLinearLerp:
                    {
                        if (lerpMoving == false)
                        {
                            if (Input.GetKey(KeyCode.W)
                            || Input.GetKey(KeyCode.S)
                            || Input.GetKey(KeyCode.A)
                            || Input.GetKey(KeyCode.D))
                            {
                                lerpMoving = true;

                                if (DimScreen == true)
                                {
                                    if (destination != Vector3.zero)
                                    {
                                        switch (CurState)
                                        {
                                            case CurtainState.off:
                                                {
                                                    StartCoroutine(curtainOn());

                                                    break;
                                                }
                                        }
                                    }
                                }

                                //StartCoroutine(lerpMovement(rigidbody.position + movement, LerpTime));
                                
                            }

                            else
                            {
                                if (DimScreen == true)
                                {
                                    switch (CurState)
                                    {
                                        case CurtainState.on:
                                            {
                                                StartCoroutine(curtainOff());

                                                break;
                                            }
                                    }
                                }
                            }
                        }
                        break;
                    }

                case NavigationMethod.normalAccelLerp:
                    {
                        if (lerpMoving == false)
                        {
                            if (Input.GetKey(KeyCode.W)
                            || Input.GetKey(KeyCode.S)
                            || Input.GetKey(KeyCode.A)
                            || Input.GetKey(KeyCode.D))
                            {
                                lerpMoving = true;

                                if (DimScreen == true)
                                {
                                    if (destination != Vector3.zero)
                                    {
                                        switch (CurState)
                                        {
                                            case CurtainState.off:
                                                {
                                                    StartCoroutine(curtainOn());

                                                    break;
                                                }
                                        }
                                    }
                                }

                                //StartCoroutine(lerpMovement(rigidbody.position + movement, LerpTime));
                                
                            }
                            else
                            {
                                if (DimScreen == true)
                                {
                                    switch (CurState)
                                    {
                                        case CurtainState.on:
                                            {
                                                StartCoroutine(curtainOff());

                                                break;
                                            }
                                    }
                                }
                            }
                        }

                        break;
                    }

                case NavigationMethod.segmentLerp:
                    {
                        PlayFootStepAudio();

                        break;
                    }
            }

            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            }

            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

            if(    NavMethod != NavigationMethod.segment
                && NavMethod != NavigationMethod.segmentLerp)
            {
                ProgressStepCycle(speed);
            }

            else
            {
                
            }
            

            UpdateCameraPosition(speed);
        }

        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }


        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput(out float speed)
        {
            // Read input

            float horizontal = 0.0f;
            float vertical = 0.0f;

            switch(NavMethod)
            {
                case NavigationMethod.normalLinear:
                {
                    horizontal = CrossPlatformInputManager.GetAxisRaw("Horizontal");
                    vertical = CrossPlatformInputManager.GetAxisRaw("Vertical");



                    break;
                }

                case NavigationMethod.normalAccel:
                {
                    horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
                    vertical = CrossPlatformInputManager.GetAxis("Vertical");

                    break;
                }

                case NavigationMethod.segment:
                {
                    if (Input.GetKeyDown(KeyCode.W)
                    || Input.GetKeyDown(KeyCode.S)
                    || Input.GetKeyDown(KeyCode.A)
                    || Input.GetKeyDown(KeyCode.D))
                    {
                        horizontal = CrossPlatformInputManager.GetAxisRaw("Horizontal");
                        vertical = CrossPlatformInputManager.GetAxisRaw("Vertical");

                        
                    }

                    break;
                }

                case NavigationMethod.normalLinearLerp:
                {
                    if(lerpMoving == false)
                    {
                        if(Input.GetKey(KeyCode.W)
                        || Input.GetKey(KeyCode.S)
                        || Input.GetKey(KeyCode.A)
                        || Input.GetKey(KeyCode.D))
                        {
                            lerpMoving = true;

                            horizontal = CrossPlatformInputManager.GetAxisRaw("Horizontal");
                            vertical = CrossPlatformInputManager.GetAxisRaw("Vertical");
                            
                        }
                    }

                    break;
                }

                case NavigationMethod.normalAccelLerp:
                {
                    if (lerpMoving == false)
                    {
                        if (Input.GetKey(KeyCode.W)
                        || Input.GetKey(KeyCode.S)
                        || Input.GetKey(KeyCode.A)
                        || Input.GetKey(KeyCode.D))
                        {
                            lerpMoving = true;

                            horizontal = CrossPlatformInputManager.GetAxisRaw("Horizontal");
                            vertical = CrossPlatformInputManager.GetAxisRaw("Vertical");

                        }
                    }

                    break;
                }

                case NavigationMethod.segmentLerp:
                {
                    if (lerpMoving == false)
                    {
                        if (Input.GetKey(KeyCode.W)
                        || Input.GetKey(KeyCode.S)
                        || Input.GetKey(KeyCode.A)
                        || Input.GetKey(KeyCode.D))
                        {
                            //lerpMoving = true;
                            horizontal = CrossPlatformInputManager.GetAxisRaw("Horizontal");
                            vertical = CrossPlatformInputManager.GetAxisRaw("Vertical");
                            
                        }
                    }

                    break;
                }
            }


            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;

            if(NavMethod == NavigationMethod.segment ||
                NavMethod == NavigationMethod.segmentLerp)
            {
                speed = segmentSpeed;
            }

            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                //m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }

            if(NavMethod == NavigationMethod.segmentLerp && lerpMoving == false)
            {
                Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;
                if(desiredMove != Vector3.zero)
                {
                    lerpMoving = true;

                    StartCoroutine(lerpMovement(desiredMove, LerpTime, speed));
                }
                
            }
            
        }


        private void RotateView()
        {
            //m_MouseLook.LookRotation (transform, m_Camera.transform);
            //Quaternion quat = InputTracking.GetLocalRotation(VRNode.LeftEye);

            //transform.rotation = quat;

            
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }

       
        IEnumerator lerpMovement(Vector3 destination, float lerpTime, float speed)
        {

            float t = 0.0f;

            float seconds = lerpTime;

            Vector3 start = transform.position;

            switch (NavMethod)
            {
                case NavigationMethod.normalLinearLerp:
                    {
                        while (t <= 1.0f)
                        {
                            t += Time.deltaTime / seconds;

                            Vector3 newPos = Vector3.Lerp(start, destination, t);

                            MovePosition(newPos, speed);

                            yield return new WaitForFixedUpdate();
                        }

                        break;
                    }


                case NavigationMethod.normalAccelLerp:
                    {
                        while (t <= 1.0f)
                        {
                            t += Time.deltaTime / seconds;

                            Vector3 newPos = Vector3.Lerp(start, destination, Mathf.SmoothStep(0.0f, 1.0f, t));

                            MovePosition(newPos, speed);

                            yield return new WaitForFixedUpdate();
                        }

                        break;
                    }

                case NavigationMethod.segmentLerp:
                    {

                        if (DimScreen == true)
                        {
                            StartCoroutine(curtainOn());
                        }


                        // break lerp up into segments
                        float segmentPercent = 1.0f / (float)LerpSegments;

                        while (t <= 1.0f)
                        {
                            t += LerpTime * segmentPercent;

                            MovePosition(destination, speed);

                            yield return new WaitForSeconds(LerpTime * segmentPercent);
                        }

                        if (DimScreen == true)
                        {
                            StartCoroutine(curtainOff());
                        }

                        break;
                    }
            }



            lerpMoving = false;

            yield return null;
        }

    }
}
