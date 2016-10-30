using KerboKatz.Assets;
using KerboKatz.Extensions;
using KerboKatz.Toolbar;
using KerboKatz.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KerboKatz.RA
{
  [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
  public partial class RecoverAll : KerboKatzBase<Settings>, IToolbar
  {
    public class VesselData
    {
      public Vessel vessel;
      public bool recover;
      public Toggle toggle;

      public VesselData(Vessel vessel, bool recover = true)
      {
        this.vessel = vessel;
        this.recover = recover;
      }
    }

    private List<GameScenes> _activeScences = new List<GameScenes>() { GameScenes.TRACKSTATION };
    private Transform vesselContainer;
    private UIData settingsWindow;
    private UIData vesselWindow;
    private Transform activeVesselsTemplate;
    private Dictionary<Vessel, VesselData> vessels = new Dictionary<Vessel, VesselData>();

    public RecoverAll()
    {
      modName = "RecoverAll";
      displayName = "Recover All";
      requiresUtilities = new Version(1, 4, 6);
      Log("Init done!");
    }

    public override void OnAwake()
    {
      ToolbarBase.instance.Add(this);
      LoadSettings("SmallUtilities/RecoverAll", "Settings");
      LoadUI("RecoverAllSettings", "SmallUtilities/RecoverAll/RecoverAll");
      LoadUI("RecoverAll", "SmallUtilities/RecoverAll/RecoverAll");
    }

    protected override void AfterDestroy()
    {
      ToolbarBase.instance.Remove(this);
    }

    #region ui

    protected override void OnUIElemntInit(UIData uiWindow)
    {
      var prefabWindow = uiWindow.gameObject.transform as RectTransform;
      var content = prefabWindow.FindChild("Content");
      switch (uiWindow.name)
      {
        case "RecoverAllSettings":
          settingsWindow = uiWindow;
          var typeTemplate = content.FindChild("Template");
          typeTemplate.SetParent(prefabWindow);
          typeTemplate.gameObject.SetActive(false);
          InitToggle(content, "IncludePrelaunch", settings.includePrelaunch, (arg0) =>
          {
            settings.includePrelaunch = arg0;
            if (vesselWindow != null)
              UpdateActiveVesselsWindow();
          });
          InitToggle(content, "Debug", settings.debug, (arg0) =>
          {
            settings.debug = arg0;
          });
          foreach (var type in Utilities.GetValues<VesselType>())
          {
            var newToolbarOption = Instantiate(typeTemplate.gameObject);
            newToolbarOption.name = "VesselType";
            newToolbarOption.SetActive(true);
            newToolbarOption.transform.SetParent(content, false);

            InitTextField(newToolbarOption.transform, "Text", type.ToString());
            var vesselToggle = settings.GetType(type);
            InitToggle(newToolbarOption.transform, "Toggle", vesselToggle.toggle, (arg0) =>
             {
               vesselToggle.toggle = arg0;
               if (vesselWindow != null)
                 UpdateActiveVesselsWindow();
             });
          }
          break;

        case "RecoverAll":
          vesselWindow = uiWindow;
          var scrollView = content.FindChild("Scroll View");
          var viewport = scrollView.FindChild("Viewport");
          vesselContainer = viewport.FindChild("VesselContainer");
          activeVesselsTemplate = vesselContainer.FindChild("Template");
          activeVesselsTemplate.SetParent(prefabWindow);
          activeVesselsTemplate.gameObject.SetActive(false);
          var buttons = content.FindChild("Buttons");
          InitButton(buttons, "Recover", OnRecoverAll);
          InitButton(buttons, "Select", OnSelectAll);
          InitButton(buttons, "Deselect", OnDeselectAll);
          UpdateActiveVesselsWindow();
          break;
      }
    }

    private void OnDeselectAll()
    {
      Log("OnDeselectAll");
      foreach (var data in vessels)
      {
        data.Value.toggle.isOn =
        data.Value.recover = false;
      }
    }

    private void OnSelectAll()
    {
      Log("OnSelectAll");
      foreach (var data in vessels)
      {
        data.Value.toggle.isOn =
        data.Value.recover = true;
      }
    }

    private void OnRecoverAll()
    {
      Log("OnRecoverAll");
      var recoveredSomething = false;
      var removed = new List<Vessel>();
      foreach (var vesselData in vessels)
      {
        if (!vesselData.Value.recover)
          continue;
        var vessel = vesselData.Key;
        if (vessel == null)
          continue;
        //use kerbals own event to recover the vessel, second parameter is set to true. this skips the recovery dialog
        GameEvents.onVesselRecovered.Fire(vessel.protoVessel, settings.hideRecoveryDialog);
        //now we need to detroy it
        Destroy(vessel);
        vessel.OnDestroy();//this will remove the vessel from the tracking station
        Log(vessel.name, " IsRecoverable");
        recoveredSomething = true;
        removed.Add(vessel);
      }

      if (recoveredSomething)
      {
        foreach (var vessel in removed)
        {
          vessels.Remove(vessel);
        }
        UpdateActiveVesselsWindow();
      }
    }

    private void UpdateActiveVesselsWindow()
    {
      DeleteChildren(vesselContainer);
      foreach (var vessel in FlightGlobals.Vessels)
      {
        if (!CanVesselBeRecovered(vessel))
        {
          vessels.Remove(vessel);
          continue;
        }

        var newToolbarOption = Instantiate(activeVesselsTemplate.gameObject);
        newToolbarOption.name = vessel.vesselName;
        newToolbarOption.SetActive(true);
        newToolbarOption.transform.SetParent(vesselContainer, false);

        var tooltip = GetComponentInChild<Tooltip>(newToolbarOption.transform, "CrewMembers");
        var crew = vessel.protoVessel.GetVesselCrew();
        if (crew.Count > 0)
        {
          var tooltipString = new StringBuilder();
          foreach (var crewMember in crew)
          {
            if (tooltipString.Length > 0)
              tooltipString.AppendLine();
            tooltipString.Append(crewMember.name);
          }
          tooltip._text = tooltipString.ToString();
        }
        VesselData data;
        if (!vessels.TryGetValue(vessel, out data))
        {
          data = new VesselData(vessel);
          vessels.Add(vessel, data);
        }

        InitTextField(newToolbarOption.transform, "Name", vessel.vesselName);
        InitTextField(newToolbarOption.transform, "CrewMembers", crew.Count.ToString());
        data.toggle = InitToggle(newToolbarOption.transform, "Toggle", data.recover, (status) =>
        {
          data.recover = status;
        });
      }
      vesselContainer.SortChildrenByName();
    }

    private bool CanVesselBeRecovered(Vessel vessel)
    {
      if (!vessel.IsRecoverable)
        return false;
      if (!settings.includePrelaunch && vessel.situation == Vessel.Situations.PRELAUNCH)
        return false;
      if (!settings.GetType(vessel.vesselType).toggle)
        return false;
      return true;
    }

    #endregion ui

    #region toolbar

    public List<GameScenes> activeScences
    {
      get
      {
        return _activeScences;
      }
    }

    public UnityAction onClick
    {
      get
      {
        return OnToolbar;
      }
    }

    private void OnToolbar()
    {
      if (Input.GetMouseButtonUp(1))
      {
        settings.showSettings = !settings.showSettings;
        if (settings.showSettings)
        {
          FadeCanvasGroup(settingsWindow.canvasGroup, 1, settings.uiFadeSpeed);
        }
        else
        {
          FadeCanvasGroup(settingsWindow.canvasGroup, 0, settings.uiFadeSpeed);
        }
      }
      else
      {
        settings.showActiveVessels = !settings.showActiveVessels;
        if (settings.showActiveVessels)
        {
          FadeCanvasGroup(vesselWindow.canvasGroup, 1, settings.uiFadeSpeed);
        }
        else
        {
          FadeCanvasGroup(vesselWindow.canvasGroup, 0, settings.uiFadeSpeed);
        }
      }
    }

    public Sprite icon
    {
      get
      {
        return AssetLoader.GetAsset<Sprite>("RecoverAll", "Icons", "SmallUtilities/RecoverAll/RecoverAll");
      }
    }

    public bool useKKToolbar
    {
      get
      {
        return true;
      }
    }

    #endregion toolbar
  }
}