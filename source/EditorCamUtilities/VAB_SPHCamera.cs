using KerboKatz.Extensions;
using System;
using UnityEngine;

namespace KerboKatz
{
  [KSPAddon(KSPAddon.Startup.EditorAny, false)]
  public partial class EditorCamUtilities : KerboKatzBase
  {
    private float distance = 0;
    private float heading;
    private float pitch;
    private bool vabControls = true;
    private Vector3 camFocus = new Vector3(0, 0, 0);
    public float MouseSensitivity = 0.002f;
    private VABCamera VABCam;
    private float rotationSpeed;
    private float HeightSpeed;
    private float zoomSpeed;
    private SPHCamera SPHCam;
    private bool isVAB;
    private ScreenMessage VABControlMessage;
    private ScreenMessage SPHControlMessage;

    public EditorCamUtilities()
    {
      modName = "EditorCamUtilities";
      displayName = "Editor Camera Extension";
      tooltip = "Use left click to toggle between VAB and SPH Camera.\n Use right click to open the settings menu.";
      requiresUtilities = new Version(1, 2, 2);
    }

    protected override void Started()
    {
      currentSettings.load("SmallUtilities/EditorCamUtilities", "EditorCamUtilitiesSettings", modName);
      currentSettings.setDefault("rotationSpeed", "5");
      currentSettings.setDefault("HeightSpeed", "5");
      currentSettings.setDefault("zoomSpeed", "5");

      settingsWindowRect.x = currentSettings.getFloat("settingsWindowRectX");
      settingsWindowRect.y = currentSettings.getFloat("settingsWindowRectY");

      rotationSpeed = currentSettings.getFloat("rotationSpeed");
      HeightSpeed = currentSettings.getFloat("HeightSpeed");
      zoomSpeed = currentSettings.getFloat("zoomSpeed");

      setIcon(Utilities.getTexture("EditorCamUtilities", "SmallUtilities/EditorCamUtilities/Textures"));
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
          distance -= zoomSpeed / 20;
          isCamUpdateRequired = true;
        }
        if (Input.GetKey(GameSettings.ZOOM_OUT.getDefaultPrimary()))
        {
          distance += zoomSpeed / 20;
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
        #endregion keyboard controls
        if (isCamUpdateRequired)
        {
          updateCam();
        }
      }
    }

    private void updateCam()
    {
      if (isVAB)
      {
        camFocus.y += VABCam.scrollHeight;
        VABCam.PlaceCamera(camFocus, VABCam.Distance+distance);
        VABCam.camHdg += heading;
        VABCam.camPitch += pitch;
      }
      else
      {
        camFocus.x = SPHCam.offset.x;
        camFocus.z = SPHCam.offset.y;
        camFocus.y += SPHCam.scrollHeight;
        SPHCam.PlaceCamera(camFocus, SPHCam.Distance + distance);
        SPHCam.camHdg += heading;
        SPHCam.camPitch += pitch;
        camFocus.x = 0;
        camFocus.z = 0;
      }
      camFocus.y = 0;
      pitch = 0;
      heading = 0;
      distance = 0;
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