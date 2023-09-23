using UnityEngine;
using UnityEngine.EventSystems;
using Gamelogic.Extensions;

public class OrbitalCamera : MonoBehaviour
{
    [System.Serializable]
    public enum CameraMode
    {
        Normal, FirstPerson, TopDownView
    }

    [Header("Basic Parameters")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private RectTransform clickArea;
    private Vector3 defaultPosition; //will be camera position in editor
    [SerializeField] private Vector3 defaultTarget;
    public bool enableTransition = true;
    public bool focusByPanningOnly = true;
    public CameraMode cameraMode = CameraMode.Normal;

    [Header("Limits")]
    public float defaultMinAngle = 10.0f, defaultMaxAngle = 85.0f;
    public float minAngleFPMode = -45.0f, maxAngleFPMode = 45.0f;

    [Header("Rotate")]
    public bool enableRotate = true;
    public bool enableElevation = true;
    public float rotateSensitivity = 1.0f;
    public float rotateTransitionSpeed = 3.0f;

    public bool IsRotating { get; private set; }

    public Vector3 Target { get; set; }
    public float Azimuth { get; set; }
    public float Elevation { get; set; }
    public float MinAngle { get; set; }
    public float MaxAngle { get; set; }

    //real states
    private Vector3 c_target;
    private float c_azimuth, c_elevation;

    //recorded states
    private Vector3 r_target;
    private float r_azimuth, r_elevation;

    //pointer movement
    private Vector2 vec0, vec1;
    private Vector3 currPointerPosition, lastPointerDownPosition;

    // is pointer down
    public bool Pointer { get; private set; }

    // pointer up in the same position
    public bool PointerUp { get; private set; }

    // pointer down in scene
    public bool PointerDown { get; private set; }

    // has pointer moved since pointer down
    public bool PointerMoved => (currPointerPosition - lastPointerDownPosition).sqrMagnitude > Screen.dpi;

    public Transform TrackedTarget { get; private set; } = null;
    public bool Orthographic => targetCamera.orthographic;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
    }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        defaultPosition = targetCamera.transform.position;
        SetCameraMode(cameraMode, true);
        SkipTransitionNow();
    }

    public void RecordStates()
    {
        r_target = Target;
        r_azimuth = Azimuth;
        r_elevation = Elevation;
    }

    public void LoadRecordedStates()
    {
        Target = r_target;
        Azimuth = r_azimuth;
        Elevation = r_elevation;
    }

    public void ResetCamera()
    {
        ResetCamera(defaultPosition, defaultTarget);
    }

    public void ResetCamera(Vector3 position, Vector3 target)
    {
        Target = target;
        Vector3 tp = target - position;
        Quaternion q = Quaternion.LookRotation(tp);
        Elevation = Mathf.Clamp(q.eulerAngles.x, defaultMinAngle, defaultMaxAngle);
        Azimuth = Mathf.Repeat(q.eulerAngles.y, 360.0f);
        if (c_azimuth - Azimuth > 180.0f) Azimuth += 360.0f;
        if (c_azimuth - Azimuth < -180.0f) Azimuth -= 360.0f;
    }

    public void SetCameraMode(CameraMode mode, bool reset, bool skipTransition = true)
    {
        cameraMode = mode;
        switch (cameraMode)
        {
            case CameraMode.Normal:
                MinAngle = defaultMinAngle;
                MaxAngle = defaultMaxAngle;
                break;

            case CameraMode.FirstPerson:
                MinAngle = minAngleFPMode;
                MaxAngle = maxAngleFPMode;
                break;

            case CameraMode.TopDownView:
                MinAngle = MaxAngle = 90.0f;
                break;

            default:
                break;
        }
        if (reset) ResetCamera();
        if (mode == CameraMode.FirstPerson)
            Elevation = (minAngleFPMode + maxAngleFPMode) * 0.5f;
        if (skipTransition) SkipTransitionNow();
    }

    public void SetTarget(Vector3 target)
    {
        if (focusByPanningOnly) Target = target;
        else ResetCamera(targetCamera.transform.position, target);
    }

    public void SetTarget(Vector3 target, float percentage)
    {
        if (focusByPanningOnly) Target = target;
        else ResetCamera(targetCamera.transform.position, target);
    }

    public void SetPosition(Vector3 position)
    {
        ResetCamera(position, Target);
    }

    public void SetOrthographic(bool og)
    {
        targetCamera.orthographic = og;
    }

    public void ToggleOrthographic()
    {
        SetOrthographic(!targetCamera.orthographic);
    }

    public void EnableMovement(bool ebl)
    {
        enableRotate = ebl;
    }

    //pass in null to stop following
    public void FollowTarget(Transform target)
    {
        TrackedTarget = target;
    }

    public void UnclickCamera()
    {
        TrackedTarget = null;
    }

    public void SkipTransitionNow()
    {
        c_target = Target;
        c_azimuth = Azimuth;
        c_elevation = Elevation;
    }

    private bool IsPointerOverUIObject()
    {
        bool flag = false;
        if (Input.touchCount > 0)
            flag = EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId);
        else
            flag = EventSystem.current.IsPointerOverGameObject();
        return flag;
    }

    private bool PointInClickArea()
    {
        if (clickArea == null) return true;
        Vector3[] wc = new Vector3[4];
        clickArea.GetWorldCorners(wc);
        Rect touchRegion = new Rect(wc[0].x, wc[0].y, wc[2].x - wc[0].x, wc[2].y - wc[0].y);
        return touchRegion.Contains(Input.mousePosition);
    }

    private bool PointInWindow()
    {
        return Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width &&
               Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height;
    }

    private float GetElevationValue()
    {
        float delta = (Input.touches[0].deltaPosition.y + Input.touches[1].deltaPosition.y) * 0.5f;
        return delta / Screen.dpi;
    }

    private float GetAzimuthValue()
    {
        float delta = Vector2.SignedAngle(vec0, vec1) / 180.0f * 3.14159f;
        return delta;
    }

    private float GetZoomValue()
    {
        float l1 = vec1.magnitude, l0 = vec0.magnitude;
        float delta = (l1 - l0) / l1;
        return delta;
    }

    private void CaculateTouchVector()
    {
        if (Input.touchCount < 2) return;

        Vector2 p0 = Input.touches[0].position;
        Vector2 p0_p = Input.touches[0].position - Input.touches[0].deltaPosition;
        Vector2 p1 = Input.touches[1].position;
        Vector2 p1_p = Input.touches[1].position - Input.touches[1].deltaPosition;

        vec0 = p1 - p0;
        vec1 = p1_p - p0_p;
    }

    private void CaculatePointerState()
    {
        PointerUp = PointerDown = false;
        if (Input.touchCount > 0)
        {
            currPointerPosition = Input.touches[0].position;
            if (Input.touches[0].phase == TouchPhase.Began)
                PointerDown = !EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId);
        }
        else
        {
            currPointerPosition = Input.mousePosition;
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                PointerDown = !EventSystem.current.IsPointerOverGameObject();
        }

        if (PointerDown) { Pointer = true; lastPointerDownPosition = currPointerPosition; }

        if (Pointer)
        {
            if (Input.touchCount == 0 && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
            {
                Pointer = false;
                if (!PointerMoved) PointerUp = true;
            }
        }
        else
        {
            lastPointerDownPosition = currPointerPosition;
        }
    }

    private void Update()
    {
        Vector2 mouseDelta = (Input.mousePosition - currPointerPosition) / 10f;

        CaculateTouchVector();
        CaculatePointerState();
        //bool inClickArea = PointInWindow();
        bool inClickArea = PointInClickArea();
        bool overUI = IsPointerOverUIObject();

        //update goal rotation here
        if (enableRotate && Pointer)
        {
            float azimuthValue = 0.0f;
            float elevationValue = 0.0f;

            //get touch rotation value
            if (Input.touchCount == 2)
            {
                azimuthValue = -GetAzimuthValue();
                elevationValue = -GetElevationValue();
            }
            //get mouse rotation value
            else if (Input.touchCount == 0 && inClickArea && Input.GetMouseButton(1))
            {
                if (cameraMode == CameraMode.TopDownView)
                {
                    Vector2 screenCenter = new Vector2(Screen.width, Screen.height) * 0.5f;
                    Vector2 mousePos = Input.mousePosition.To2DXY();
                    Vector2 temp = (screenCenter - mousePos).normalized;
                    azimuthValue = (mouseDelta.x * temp.y - mouseDelta.y * temp.x) * rotateSensitivity;
                    elevationValue = 0.0f;
                }
                else
                {
                    azimuthValue = mouseDelta.x * rotateSensitivity;
                    elevationValue = -mouseDelta.y * rotateSensitivity;
                }
            }

            //update goal rotation
            if (azimuthValue != 0.0f)
            {
                Azimuth += azimuthValue;
                c_azimuth = Azimuth;
            }
            if (elevationValue != 0.0f && enableElevation)
            {
                Elevation += elevationValue;
                c_elevation = Elevation;
            }

            IsRotating = azimuthValue != 0.0f && elevationValue != 0.0f;
        }
        else IsRotating = false;

        //follow target
        if (TrackedTarget != null)
        {
            Target = TrackedTarget.position;
        }

        //clamp here
        Elevation = Mathf.Clamp(Elevation, MinAngle, MaxAngle);
        float temp_azimuth = Azimuth;
        Azimuth = Mathf.Repeat(Azimuth, 360.0f);
        c_azimuth += Azimuth - temp_azimuth;

        //update real states here
        if (enableTransition)
        {
            //update real rotation
            c_azimuth = Mathf.Lerp(c_azimuth, Azimuth, rotateTransitionSpeed * Time.deltaTime);
            c_elevation = Mathf.Lerp(c_elevation, Elevation, rotateTransitionSpeed * Time.deltaTime);
            c_elevation = Mathf.Clamp(c_elevation, MinAngle, MaxAngle);

            //update moving states
            IsRotating = IsRotating || Mathf.Abs(Azimuth - c_azimuth) > 0.01f || Mathf.Abs(Elevation - c_elevation) > 0.01f;
        }
        else { SkipTransitionNow(); }

        //update final result
        targetCamera.transform.rotation = Quaternion.Euler(c_elevation, c_azimuth, 0);
    }

    //-------------------------------------------------------------------
    //support for old orbital camera
    //-------------------------------------------------------------------
    //public Vector3 TargetPosition { get { return m_target; } set { m_target = value; } }
    //public Vector3 targetPosition { get { return m_target; } set { m_target = value; } }
    //public bool HasMoved { get { return GetPointerMoved; } }
    //public bool canPan { get { return enablePan; } set { enablePan = value; } }
    //public bool canZoom { get { return enableZoom; } set { enableZoom = value; } }
    //public bool canRotate { get { return enableRotate; } set { enableRotate = value; } }
    //public float minDistance { get { return m_minDistance; } set { m_minDistance = value; } }
    //public float maxDistance { get { return m_maxDistance; } set { m_maxDistance = value; } }
    //public float minAngle { get { return m_minAngle; } set { m_minAngle = value; } }
    //public float maxAngle { get { return m_maxAngle; } set { m_maxAngle = value; } }

    //public void Reset()
    //{
    //    ResetCamera();
    //}

    //public void Reset(Transform trans, Vector3 target)
    //{
    //    ResetCamera(trans.position, target);
    //}

    //public void SetFocusTarget(Vector3 target)
    //{
    //    SetTarget(target);
    //}

    //public void SetZoom(float percentage)
    //{
    //    Zoom = percentage;
    //}

    //public void SetViewTarget(Transform trans, Vector3 target)
    //{
    //    ResetCamera(trans.position, target);
    //}
}