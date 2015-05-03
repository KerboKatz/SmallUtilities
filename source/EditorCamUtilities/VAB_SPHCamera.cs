using KerboKatz.Extensions;
using System;
using UnityEngine;

namespace KerboKatz
{
  [KSPAddon(KSPAddon.Startup.EditorAny, false)]
  public partial class VAB_SPHCamera : KerboKatzBase
  {
    private float distance = 0;
    private float heading;
    private float pitch;
    private bool vabControls = true;
    private Vector3 camFocus = new Vector3(0, 0, 0);
    public float MouseSensitivity = 0.002f;
    private VABCamera VABCam;
    private float rotationSpeed = 10;
    private float HeightSpeed = 10;
    private float zoomSpeed = 10;
    private SPHCamera SPHCam;
    private bool isVAB;
    private ScreenMessage VABControlMessage;
    private ScreenMessage SPHControlMessage;

    public VAB_SPHCamera()
    {
      modName = "VAB_SPHCamera";
      displayName = "Editor Camera Extension";
      tooltip = "Use left click to toggle between VAB and SPH Camera.\n Use right click to open the settings menu.";
      requiresUtilities = new Version(1, 2, 2);
    }

    protected override void Started()
    {
      currentSettings.load("SmallUtilities", "VAB_SPHCameraSettings", modName);
      currentSettings.setDefault("rotationSpeed", "10");
      currentSettings.setDefault("HeightSpeed", "10");
      currentSettings.setDefault("zoomSpeed", "10");

      settingsWindowRect.x = currentSettings.getFloat("settingsWindowRectX");
      settingsWindowRect.y = currentSettings.getFloat("settingsWindowRectY");

      rotationSpeed = currentSettings.getFloat("rotationSpeed");
      HeightSpeed = currentSettings.getFloat("HeightSpeed");
      zoomSpeed = currentSettings.getFloat("zoomSpeed");

      setIcon(Utilities.getTexture("EditorCamUtilities", "SmallUtilities/Textures"));
      setAppLauncherScenes(ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB);

      VABControlMessage = new ScreenMessage("Using VAB controls!", 5, ScreenMessageStyle.LOWER_CENTER);
      SPHControlMessage = new ScreenMessage("Using SPH controls!", 5, ScreenMessageStyle.LOWER_CENTER);
      initMod();
      if (isVAB)
      {
        vabControls = true;
      }
      else
      {
        vabControls = false;
      }
    }

    protected override void onToolbar()
    {
      if (Input.GetMouseButtonUp(0))
      {
        if (vabControls)
        {
          vabControls = false;
          ScreenMessages.PostScreenMessage(SPHControlMessage);
        }
        else
        {
          vabControls = true;
          ScreenMessages.PostScreenMessage(VABControlMessage);
        }
      }
      else if (Input.GetMouseButtonUp(1))
      {
        toggleSettingsWindow();
      }
    }

    private void toggleSettingsWindow()
    {
      if (currentSettings.getBool("showSettings"))
      {
        currentSettings.set("showSettings", false);
      }
      else
      {
        currentSettings.set("showSettings", true);
      }
    }

    protected override void beforeSaveOnDestroy()
    {
      if (currentSettings != null)
      {
        currentSettings.set("rotationSpeed", rotationSpeed);
        currentSettings.set("HeightSpeed", HeightSpeed);
        currentSettings.set("zoomSpeed", zoomSpeed);
        currentSettings.set("vabControls", vabControls);
        currentSettings.set("showSettings", false);
        currentSettings.set("settingsWindowRectX", settingsWindowRect.x);
        currentSettings.set("settingsWindowRectY", settingsWindowRect.y);
      }
    }

    private void Update()
    {
      if (EditorLogic.fetch.editorCamera == null)
        return;
      if (!UIManager.instance.DidAnyPointerHitUI() && !InputLockManager.IsLocked(ControlTypes.CAMERACONTROLS))
      {
        var isCamUpdateRequired = false;
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
          var shift = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
          if (shift && vabControls || !vabControls && !shift)
          {
            //zoom in/out
            distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
          }
          else if (shift && !vabControls || vabControls && !shift)
          {
            //move cam up/down
            camFocus.y += Input.GetAxis("Mouse ScrollWheel") * HeightSpeed;
          }
          isCamUpdateRequired = true;
        }
        if (Input.GetMouseButton(1))
        {
          //rotate
          heading += Input.GetAxis("Mouse X") * rotationSpeed * MouseSensitivity;
          pitch -= Input.GetAxis("Mouse Y") * rotationSpeed * MouseSensitivity;
          isCamUpdateRequired = true;
        }
        #region keyboard controls
        if (Input.GetKey(GameSettings.ZOOM_IN.getDefaultPrimary()))
        {
          distance -= zoomSpeed/20;
          isCamUpdateRequired = true;
        }
        if (Input.GetKey(GameSettings.ZOOM_OUT.getDefaultPrimary()))
        {
          distance += zoomSpeed/20;
          isCamUpdateRequired = true;
        }
        if (Input.GetKey(GameSettings.SCROLL_VIEW_UP.getDefaultPrimary()))
        {
          camFocus.y += HeightSpeed / 20;
          isCamUpdateRequired = true;
        }
        if (Input.GetKey(GameSettings.SCROLL_VIEW_DOWN.getDefaultPrimary()))
        {
          camFocus.y -= HeightSpeed / 20;
          isCamUpdateRequired = true;
        }
        if (Input.GetKey(GameSettings.CAMERA_ORBIT_UP.getDefaultPrimary()))
        {
          pitch += rotationSpeed * MouseSensitivity;
          isCamUpdateRequired = true;
        }
        if (Input.GetKey(GameSettings.CAMERA_ORBIT_DOWN.getDefaultPrimary()))
        {
          pitch -= rotationSpeed * MouseSensitivity;
          isCamUpdateRequired = true;
        }
        if (Input.GetKey(GameSettings.CAMERA_ORBIT_LEFT.getDefaultPrimary()))
        {
          heading += rotationSpeed * MouseSensitivity;
          isCamUpdateRequired = true;
        }
        if (Input.GetKey(GameSettings.CAMERA_ORBIT_RIGHT.getDefaultPrimary()))
        {
          heading -= rotationSpeed * MouseSensitivity;
          isCamUpdateRequired = true;
        }
        #endregion
        if (isCamUpdateRequired)
        {
          updateCam();
        }
      }
      if (isVAB)
      {
        if (VABCam.camHdg == 0 && VABCam.camPitch == 0)
        {
          heading = 0;
          pitch = 0;
        }else if (VABCam.camHdg != heading || VABCam.camPitch != pitch || VABCam.scrollHeight != camFocus.y || VABCam.Distance != distance)
        {
          ResetCam();
        }
      }
      else
      {
        if (SPHCam.camHdg == 0 && SPHCam.camPitch == 0)
        {
          heading = 0;
          pitch = 0;
        }
        else if (SPHCam.camHdg != heading || SPHCam.camPitch != pitch || SPHCam.scrollHeight != camFocus.y || SPHCam.Distance != distance)
        {
          ResetCam();
        }
      }
    }

    private void clampPitch()
    {
      if (isVAB)
      {
        pitch = Mathf.Clamp(pitch, VABCam.minPitch, VABCam.maxPitch);
      }
      else
      {
        pitch = Mathf.Clamp(pitch, SPHCam.minPitch, SPHCam.maxPitch);
      }
    }

    private void clampFocus()
    {
      if (isVAB)
      {
        camFocus.y = Mathf.Clamp(camFocus.y, VABCam.minHeight, VABCam.maxHeight);
      }
      else
      {
        camFocus.y = Mathf.Clamp(camFocus.y, SPHCam.minHeight, SPHCam.maxHeight);
      }
    }

    private void clampDistance()
    {
      if (isVAB)
      {
        distance = Mathf.Clamp(distance, VABCam.minDistance, VABCam.maxDistance);
      }
      else
      {
        distance = Mathf.Clamp(distance, SPHCam.minDistance, SPHCam.maxDistance);
      }
      distance = EditorBounds.ClampCameraDistance(distance);
    }

    private void updateCam()
    {
      clampDistance();
      clampFocus();
      clampPitch();
      if (isVAB)
      {
        VABCam.PlaceCamera(camFocus, distance);
        VABCam.camHdg = heading;
        VABCam.camPitch = pitch;
      }
      else
      {
        SPHCam.PlaceCamera(camFocus, distance);
        SPHCam.camHdg = heading;
        SPHCam.camPitch = pitch;
      }
    }

    private void uninitMod()
    {
      afterDestroy();
    }

    private void initMod()
    {
      if (VABCam == null)
        VABCam = Camera.main.GetComponent<VABCamera>();

      if (SPHCam == null)
        SPHCam = Camera.main.GetComponent<SPHCamera>();
      if (Utilities.getEditorScene() == "VAB")
      {
        isVAB = true;
      }
      else
      {
        isVAB = false;
      }

      ResetCam(true);
      updateCam();
      //save and overwrite all the camera controls so they dont interfere
      GameSettings.AXIS_MOUSEWHEEL.saveDefault();
      GameSettings.AXIS_MOUSEWHEEL.setZero();

      GameSettings.SCROLL_VIEW_UP.saveDefault();
      GameSettings.SCROLL_VIEW_UP.setNone();

      GameSettings.SCROLL_VIEW_DOWN.saveDefault();
      GameSettings.SCROLL_VIEW_DOWN.setNone();

      GameSettings.ZOOM_IN.saveDefault();
      GameSettings.ZOOM_IN.setNone();

      GameSettings.ZOOM_OUT.saveDefault();
      GameSettings.ZOOM_OUT.setNone();

      GameSettings.CAMERA_ORBIT_UP.saveDefault();
      GameSettings.CAMERA_ORBIT_UP.setNone();

      GameSettings.CAMERA_ORBIT_DOWN.saveDefault();
      GameSettings.CAMERA_ORBIT_DOWN.setNone();

      GameSettings.CAMERA_ORBIT_LEFT.saveDefault();
      GameSettings.CAMERA_ORBIT_LEFT.setNone();

      GameSettings.CAMERA_ORBIT_RIGHT.saveDefault();
      GameSettings.CAMERA_ORBIT_RIGHT.setNone();
    }

    private void ResetCam(bool resetDistance = false)
    {
      if (isVAB)
      {
        heading = VABCam.initialHeading;
        pitch = VABCam.initialPitch;
        camFocus.y = 15;//VABCam.initialHeight;
        if (resetDistance)
          distance = VABCam.startDistance;
      }
      else
      {
        heading = 1.266f;
        pitch = 0.286f;
        camFocus.y = 5;//VABCam.initialHeight;
        if (resetDistance)
          distance = 15;
      }
    }

    protected override void afterDestroy()
    {
      //sadly i have to be destroyed so i reset the controlls
      GameSettings.AXIS_MOUSEWHEEL.reset();

      GameSettings.SCROLL_VIEW_UP.reset();

      GameSettings.SCROLL_VIEW_DOWN.reset();

      GameSettings.ZOOM_IN.reset();

      GameSettings.ZOOM_OUT.reset();

      GameSettings.CAMERA_ORBIT_UP.reset();

      GameSettings.CAMERA_ORBIT_DOWN.reset();

      GameSettings.CAMERA_ORBIT_LEFT.reset();

      GameSettings.CAMERA_ORBIT_RIGHT.reset();
    }
  }
}