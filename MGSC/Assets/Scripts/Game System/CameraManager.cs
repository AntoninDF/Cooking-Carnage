using Controller;
using GameSystem;
using UI;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : IManager
{
    #region Properties

    #endregion

    #region Fields

    private UIManager _uiManager;
    private TimerManager _timerManager;


    private CinemachineVirtualCameraBase camFPV;
    private CinemachineVirtualCameraBase camFocus;

    private GameObject focusPivot;
        
    private CameraState currentState = CameraState.FirstPerson;
    
    #endregion

    #region Methods
   
    public CameraManager()
    {
        Register();
    }
        
    public void Register()
    {
        Reference.SetManager(this);
    }
    public void Unregister()
    {
        Reference.RemoveManager(this);
    }

    public void Init()
    {
        Reference.GetManager(out _uiManager);
        Reference.GetManager(out _timerManager);

        if (Reference.TryGet(out Player player))
        {
            foreach (var cam in player.GetComponentsInChildren<CinemachineVirtualCameraBase>())
            {
                if (cam.Name == "FP Camera")
                {
                    camFPV = cam;
                }
                else if (cam.Name == "Focus Camera")
                {
                    camFocus = cam;
                }
            }

            if (camFocus == null || camFPV == null)
                this.Log(LogType.Error, "No Camera found, please fix this");
        }

        
            
            
        focusPivot = new GameObject("Focus Pivot");
    }

    public void LookAt(Transform target, float duration = 0, bool showMouse = false, float distance = 0.5f)
    {
        Vector3 targetPos = target.transform.position + target.transform.forward * -distance;
        focusPivot.transform.position = targetPos;
            
        camFocus.Follow = focusPivot.transform;
        camFocus.LookAt = target;
            
        ChangeState(CameraState.Focus);

        if (showMouse)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (duration > 0)
        {
            _timerManager.StartTimer("camFocus", duration, () => EnableFirstPersonView(), false);
            _uiManager.SwitchPlayerInputs(false);
            this.Log(LogType.Log, $"Camera will focus on {target.name} for {duration}s");
        }
        else
        {
            _uiManager.PauseGame();
            this.Log(LogType.Log, $"Camera will focus on {target.name}");
        }
            
    }

    public void EnableFirstPersonView()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
            
        ChangeState(CameraState.FirstPerson);
        _uiManager.SwitchPlayerInputs(true);
    }

    public void Animate(Transform start, Transform end, float duration, AnimationCurve motion=null, Transform lookAtTarget = null)
    {
        if (motion == null)
            motion = AnimationCurve.EaseInOut(0, 0, 1, 1);
        _timerManager.StartTimer("Animate camera", duration, () => { }, onUpdate:(elapsed) => Set(LerpedTransform(focusPivot.transform, lookAtTarget, elapsed/duration), lookAtTarget));
    }

    public Transform LerpedTransform(Transform start, Transform end, float t)
    {
        focusPivot.transform.position = Vector3.Lerp(start.position, end.position, t);
        focusPivot.transform.rotation = Quaternion.Lerp(start.rotation, end.rotation, t);
        focusPivot.transform.localScale = Vector3.Lerp(start.localScale, end.localScale, t);
        return focusPivot.transform;
    }

    public void Set(Transform position, Transform lookAt=null)
    {
        camFocus.Follow = position;
        if (lookAt != null) camFocus.LookAt = lookAt;
    }

    void ChangeState(CameraState newState)
    {
        if (currentState != newState)
        {
            switch (newState)
            {
                case CameraState.FirstPerson :
                    (camFocus.Priority,camFPV.Priority) = (10,20);
                        
                    break;
                case CameraState.Focus :
                    (camFocus.Priority,camFPV.Priority) = (20,10);
                    break;
            }
                
            currentState = newState;
            this.Log(LogType.Log, $"Camera mode updated to {newState}");
        }
        else
        {
            this.Log(LogType.Warning, $"Camera was already in {currentState} mode");
        }
    }

        
    
    #endregion

    enum CameraState
    {
        FirstPerson,
        Focus,
    }
}
