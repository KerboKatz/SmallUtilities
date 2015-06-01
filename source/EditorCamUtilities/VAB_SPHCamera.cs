using KerboKatz.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
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
    private float rotationSpeed;
    private float HeightSpeed;
    private float zoomSpeed;
    private SPHCamera EditorCamera;
    private ScreenMessage VABControlMessage;
    private ScreenMessage SPHControlMessage;
    private Vector3 OriginalSize;
    private Vector2 heightLimits;

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
        ThreadPool.QueueUserWorkItem(new WaitCallback(updateBounds));
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
        if (Input.GetMouseButton(2) && vabControls)
        {
          //zoom in/out
          distance -= Input.GetAxis("Mouse Y") * zoomSpeed * (MouseSensitivity*10);
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
      if (EditorDriver.fetch.vabCamera.enabled)
      {
        if (EditorDriver.fetch.vabCamera.camHdg != EditorCamera.camHdg ||
            EditorDriver.fetch.vabCamera.camPitch != EditorCamera.camPitch ||
            EditorDriver.fetch.vabCamera.Distance != EditorCamera.Distance ||
            EditorDriver.fetch.vabCamera.scrollHeight != EditorCamera.scrollHeight)
        {
          updateCam();
        }

      }
    }

    private void updateCam()
    {
      if (vabControls)
      {
        camFocus.x = 0;
        camFocus.z = 0;
      }
      else
      {
        camFocus.x = EditorCamera.pivotPosition.x;
        camFocus.z = EditorCamera.pivotPosition.z;
      }
      camFocus.y += EditorCamera.scrollHeight;
      camFocus.y = Mathf.Clamp(camFocus.y, heightLimits.x, heightLimits.y);
      EditorCamera.PlaceCamera(camFocus, EditorCamera.Distance + distance);
      EditorCamera.camHdg += heading;
      EditorCamera.camPitch += pitch;
      if (EditorDriver.fetch.vabCamera.enabled)
      {
        EditorDriver.fetch.vabCamera.PlaceCamera(camFocus, EditorCamera.Distance);
        EditorDriver.fetch.vabCamera.camHdg = EditorCamera.camHdg;
        EditorDriver.fetch.vabCamera.camPitch = EditorCamera.camPitch;
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
      EditorDriver.fetch.sphCamera.enabled = true;
      EditorCamera = EditorDriver.fetch.sphCamera;      

      if (Utilities.getEditorScene() == "VAB")
      {
        EditorDriver.fetch.vabCamera.enabled = true;
        vabControls = true;
        heightLimits = new Vector2(EditorDriver.fetch.vabCamera.minHeight, EditorDriver.fetch.vabCamera.maxHeight);
      }
      else
      {
        vabControls = false;
        heightLimits = new Vector2(EditorDriver.fetch.sphCamera.minHeight, EditorDriver.fetch.sphCamera.maxHeight);
      }
      ThreadPool.QueueUserWorkItem(new WaitCallback(updateBounds));

      //save and overwrite all the camera controls so they dont interfere
      GameSettings.AXIS_MOUSEWHEEL.saveDefault("AXIS_MOUSEWHEEL");
      GameSettings.AXIS_MOUSEWHEEL.setZero();

      GameSettings.SCROLL_VIEW_UP.saveDefault("SCROLL_VIEW_UP");
      GameSettings.SCROLL_VIEW_UP.setNone();

      GameSettings.SCROLL_VIEW_DOWN.saveDefault("SCROLL_VIEW_DOWN");
      GameSettings.SCROLL_VIEW_DOWN.setNone();

      GameSettings.ZOOM_IN.saveDefault("ZOOM_IN");
      GameSettings.ZOOM_IN.setNone();

      GameSettings.ZOOM_OUT.saveDefault("ZOOM_OUT");
      GameSettings.ZOOM_OUT.setNone();

      GameSettings.CAMERA_ORBIT_UP.saveDefault("CAMERA_ORBIT_UP");
      GameSettings.CAMERA_ORBIT_UP.setNone();

      GameSettings.CAMERA_ORBIT_DOWN.saveDefault("CAMERA_ORBIT_DOWN");
      GameSettings.CAMERA_ORBIT_DOWN.setNone();

      GameSettings.CAMERA_ORBIT_LEFT.saveDefault("CAMERA_ORBIT_LEFT");
      GameSettings.CAMERA_ORBIT_LEFT.setNone();

      GameSettings.CAMERA_ORBIT_RIGHT.saveDefault("CAMERA_ORBIT_RIGHT");
      GameSettings.CAMERA_ORBIT_RIGHT.setNone();
      EditorCamUtilitiesExtensions.setSaveFileStatusToStarted();
    }

    private void updateBounds(object state)
    {
      while (EditorBounds.Instance == null && EditorBounds.Instance.cameraOffsetBounds == null)
      {
        Thread.Sleep(50);
      }
      if (OriginalSize==null)
      {
        OriginalSize=EditorBounds.Instance.cameraOffsetBounds.size;
      }
      var size = EditorBounds.Instance.cameraOffsetBounds.size;
      
      if (vabControls)
      {
        size.x = 0;
        size.z = 0;
      }
      else
      {
        if (OriginalSize.x == 0 && OriginalSize.z == 0)
        {
          size.x = size.y;
          size.z = size.y;
        }
        else
        {
          size.x = OriginalSize.x;
          size.z = OriginalSize.z;
        }
      }
      EditorBounds.Instance.cameraOffsetBounds = new Bounds(EditorBounds.Instance.cameraOffsetBounds.center, size);
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
      EditorCamUtilitiesExtensions.setSaveFileStatusToEnded();
    }
  }
}