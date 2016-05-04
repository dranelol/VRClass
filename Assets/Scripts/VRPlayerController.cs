using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityStandardAssets.ImageEffects;

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

public class VRPlayerController : MonoBehaviour 
{
    public Image Curtain;

    public NavigationMethod NavMethod;

    public CurtainState CurState;

    public bool DimScreen;

    public bool TunnelVision;

    private Rigidbody rigidbody;

    public float MovementSpeed;

    public float ForwardSpeed;

    public float StrafeSpeed;

    public float SegmentSpeed;

    public float LerpDistance;

    

    public float DimTime;

    public float DimDelay;
    public float LerpTime;
    public int LerpSegments;

    private bool lerpMoving;

    private bool moving;

    public Animator Anim;

    public VignetteAndChromaticAberration Vignette;

    public Camera Cam;

	void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        CurState = CurtainState.off;

        if(TunnelVision == true)
        {
            Vignette.enabled = true;
            Cam.fieldOfView = 90;
        }

        else
        {
            Vignette.enabled = false;
            Cam.fieldOfView = 60;
        }
    }
	
	// Update is called once per frame
	void Update () 
    {
        switch(NavMethod)
        {
            case NavigationMethod.normalLinear:
            {
                float horizontal = Input.GetAxisRaw("Horizontal");
                float vertical = Input.GetAxisRaw("Vertical");
                Vector3 movement = MovementSpeed * (transform.forward * vertical * ForwardSpeed + transform.right * horizontal * StrafeSpeed) * Time.deltaTime;

                if (DimScreen == true)
                {
                    if (movement != Vector3.zero)
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

                    if (movement == Vector3.zero)
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

                movePosition(rigidbody.position + movement);

                break;
            }

            case NavigationMethod.normalAccel:
            {
                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");
                Vector3 movement = MovementSpeed * (transform.forward * vertical * ForwardSpeed + transform.right * horizontal * StrafeSpeed) * Time.deltaTime;

                if (DimScreen == true)
                {
                    if (movement != Vector3.zero)
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

                    if (movement == Vector3.zero)
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

                movePosition(rigidbody.position + movement);

                

                break;
            }

            case NavigationMethod.segment:
            {
                if(Input.GetKeyDown(KeyCode.W)
                || Input.GetKeyDown(KeyCode.S)
                || Input.GetKeyDown(KeyCode.A)
                || Input.GetKeyDown(KeyCode.D))
                {
                    float horizontal = Input.GetAxisRaw("Horizontal");
                    float vertical = Input.GetAxisRaw("Vertical");
                    Vector3 movement = SegmentSpeed * MovementSpeed * (transform.forward * vertical * ForwardSpeed + transform.right * horizontal * StrafeSpeed) * Time.deltaTime;
                    /*
                    if (DimScreen == true)
                    {
                        if (movement != Vector3.zero)
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

                        if (movement == Vector3.zero)
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
                    */
                    movePosition(rigidbody.position + movement);
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
                        if (lerpMoving == false)
                        {
                            lerpMoving = true;

                            float horizontal = Input.GetAxisRaw("Horizontal");
                            float vertical = Input.GetAxisRaw("Vertical");

                            Vector3 movement = LerpDistance * MovementSpeed * (transform.forward * vertical * ForwardSpeed + transform.right * horizontal * StrafeSpeed) * Time.deltaTime;

                            if (DimScreen == true)
                            {
                                if (movement != Vector3.zero)
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

                            StartCoroutine(lerpMovement(rigidbody.position + movement, LerpTime));
                        }
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
                        if (lerpMoving == false)
                        {
                            lerpMoving = true;

                            float horizontal = Input.GetAxisRaw("Horizontal");
                            float vertical = Input.GetAxisRaw("Vertical");

                            Vector3 movement = LerpDistance * MovementSpeed * (transform.forward * vertical * ForwardSpeed + transform.right * horizontal * StrafeSpeed) * Time.deltaTime;

                            if (DimScreen == true)
                            {
                                if (movement != Vector3.zero)
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

                            StartCoroutine(lerpMovement(rigidbody.position + movement, LerpTime));
                        }
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
                if (Input.GetKey(KeyCode.W)
                    || Input.GetKey(KeyCode.S)
                    || Input.GetKey(KeyCode.A)
                    || Input.GetKey(KeyCode.D))
                {
                    if(lerpMoving == false)
                    {
                        lerpMoving = true;

                        float horizontal = Input.GetAxisRaw("Horizontal");
                        float vertical = Input.GetAxisRaw("Vertical");

                        Vector3 movement = LerpDistance * MovementSpeed * (transform.forward * vertical * ForwardSpeed + transform.right * horizontal * StrafeSpeed) * Time.deltaTime;


                        StartCoroutine(lerpMovement(rigidbody.position + movement, LerpTime));
                    }
                    
                }

                break;
            }
        }

        
	}

    public void SetVignette(bool vignette)
    {
        if (vignette == true)
        {
            Vignette.enabled = true;
            Cam.fieldOfView = 90;
        }

        else
        {
            Vignette.enabled = false;
            Cam.fieldOfView = 60;
        }
    }

    public void SetDimScreen(bool dim)
    {
        DimScreen = dim;
    }

    public void ChangeNavMethod(int method)
    {
        switch(method)
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
    IEnumerator curtainOn()
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

    IEnumerator curtainOff()
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

    IEnumerator lerpMovement(Vector3 destination, float lerpTime)
    {
        
        float t = 0.0f;

        float seconds = lerpTime;

        Vector3 start = rigidbody.position;

        switch(NavMethod)
        {
            case NavigationMethod.normalLinearLerp:
            {
                while (t <= 1.0f)
                {
                    t += Time.deltaTime / seconds;

                    Vector3 newPos = Vector3.Lerp(start, destination, t);

                    movePosition(newPos);

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

                    movePosition(newPos);

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
                float segmentPercent = 1.0f / LerpSegments;

                while (t <= 1.0f)
                {
                    t += LerpTime * segmentPercent;

                    Vector3 newPos = Vector3.Lerp(start, destination, t);

                    movePosition(newPos);

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

    private void movePosition(Vector3 newPosition)
    {
        if(Vector3.Distance(rigidbody.position, newPosition) > 0.14f)
        {
            Anim.SetBool("Moving", true);
        }

        else
        {
            Anim.SetBool("Moving", false);
        }

        rigidbody.MovePosition(newPosition);

    }
}
