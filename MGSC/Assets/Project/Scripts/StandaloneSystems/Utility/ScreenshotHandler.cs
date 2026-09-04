
using UnityEngine;
using System.IO;

#if ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.PlayerLoop;
#endif

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Will bind a screenshot action to the "Screenshot" action of the default action map, or to F9 if the old input system is used and the "Screenshot" action couldn't be found.
/// </summary>
public class ScreenshotHandler
{
    public static string ScreenshotPath = Path.Combine(Application.persistentDataPath, "Screenshots");
    public static bool AllowDebugLogs = true;

    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        BindInputs();
    }

    private static void BindInputs()
    {
        bool wasBound = false;
#if ENABLE_INPUT_SYSTEM
        InputAction action = InputSystem.actions.FindAction("Screenshot");
        if (action != null)
        {
            action.performed += _ => TakeScreenshot(2);
            wasBound = true;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (!wasBound)
        {
            PlayerLoopUtility.InsertMethodCallInPlayerLoopAfter<PostLateUpdate, ScreenshotHandler>(CheckForOldInputManagerInput);
            wasBound = true;
        }
#endif
        if (AllowDebugLogs && !wasBound) Debug.LogWarning("Couldn't bind the screenshot input. " +
            "Please make sure a 'Screenshot' action is available in the input action map, or that the old input system is enabled");
    }

    public static void TakeScreenshot(int sizeMultiplier = 1)
    {
        if (!Directory.Exists(ScreenshotPath))
            Directory.CreateDirectory(ScreenshotPath);
        string path = Path.Combine(ScreenshotPath, "Screenshot " + System.DateTime.Now.ToString().Replace("/", "-").Replace(":","-") + ".png");
        ScreenCapture.CaptureScreenshot(path, sizeMultiplier);
        if (AllowDebugLogs) Debug.Log($"Screenshot taken, saved at path '{path}', size multiplier is {sizeMultiplier}, " +
            $"resolution is {Screen.width * sizeMultiplier}x{Screen.height * sizeMultiplier}");
    }

    static void CheckForOldInputManagerInput()
    {
        if (Input.GetKey(KeyCode.F9))
            TakeScreenshot(2);
    }
}
