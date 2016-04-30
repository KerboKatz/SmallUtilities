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

namespace KerboKatz.DA
{
  [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
  public partial class DestroyAll : KerboKatzBase<Settings>, IToolbar
  {
    public class VesselData
    {
      public Vessel vessel;
      public bool destroy;
      public Toggle toggle;

      public VesselData(Vessel vessel, bool destroy = true)
      {
        this.vessel = vessel;
        this.destroy = destroy;
      }
    }
    private List<GameScenes> _activeScences = new List<GameScenes>() { GameScenes.TRACKSTATION };
    private Transform vesselContainer;
    private UIData settingsWindow;
    private UIData vesselWindow;
    private Transform activeVesselsTemplate;
    private Dictionary<Vessel, VesselData> vessels = new Dictionary<Vessel, VesselData>();

    #region init
    public DestroyAll()
    {
      modName = "DestroyAll";
      displayName = "Destroy All";
      requiresUtilities = new Version(1, 3, 3);
      Log("Init done!");
    }
    public override void OnAwake()
    {
      ToolbarBase.instance.Add(this);
      LoadSettings("SmallUtilities/DestroyAll", "Settings");
      LoadUI("DestroyAllSettings", "SmallUtilities/DestroyAll/DestroyAll");
      LoadUI("DestroyAll", "SmallUtilities/DestroyAll/DestroyAll");
    }
    protected override void AfterDestroy()
    {
      ToolbarBase.instance.Remove(this);
    }
    #endregion


    #region ui
    protected override void OnUIElemntInit(UIData uiWindow)
    {
      var prefabWindow = uiWindow.gameObject.transform as RectTransform;
      var content = prefabWindow.FindChild("Content");
      switch (uiWindow.name)
      {
        case "DestroyAllSettings":
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
        case "DestroyAll":
          vesselWindow = uiWindow;
          var scrollView = content.FindChild("Scroll View");
          var viewport = scrollView.FindChild("Viewport");
          vesselContainer = viewport.FindChild("VesselContainer");
          activeVesselsTemplate = vesselContainer.FindChild("Template");
          activeVesselsTemplate.SetParent(prefabWindow);
          activeVesselsTemplate.gameObject.SetActive(false);
          var buttons = content.FindChild("Buttons");
          InitButton(buttons, "Destroy", OnDestroyAll);
          InitButton(buttons, "Select", OnSelectAll);
          InitButton(buttons, "Deselect", OnDeselectAll);
          UpdateActiveVesselsWindow();
          break;
      }
    }

    private void OnDeselectAll()
    {
      foreach (var data in vessels)
      {
        data.Value.toggle.isOn =
        data.Value.destroy = false;
      }
    }

    private void OnSelectAll()
    {
      foreach (var data in vessels)
      {
        data.Value.toggle.isOn =
        data.Value.destroy = true;
      }
    }

    private void OnDestroyAll()
    {
      var removed = new List<Vessel>();
      foreach (var vesselData in vessels)
      {
        if (!vesselData.Value.destroy)
          continue;
        var vessel = vesselData.Key;
        if (vessel == null)
          continue;
        //use kerbals own event to destroy the vessel
        vessel.Die();
        removed.Add(vessel);
      }

      if (removed.Count>0)
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
        if (!CanVesselBeDestroyed(vessel))
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
          tooltip.text = tooltipString.ToString();
        }
        VesselData data;
        if (!vessels.TryGetValue(vessel, out data))
        {
          data = new VesselData(vessel);
          vessels.Add(vessel, data);
        }

        InitTextField(newToolbarOption.transform, "Name", vessel.vesselName);
        InitTextField(newToolbarOption.transform, "CrewMembers", crew.Count.ToString());
        data.toggle = InitToggle(newToolbarOption.transform, "Toggle", data.destroy, (status) =>
        {
          data.destroy = status;
        });
      }
      vesselContainer.SortChildrenByName();
    }
    private bool CanVesselBeDestroyed(Vessel vessel)
    {
      //to do for the future:
      //need to figure out if vessel is owned by the player
      if (!settings.includePrelaunch && vessel.situation == Vessel.Situations.PRELAUNCH)
        return false;
      if (!settings.GetType(vessel.vesselType).toggle)
        return false;
      return true;
    }
    #endregion
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
        return AssetLoader.GetAsset<Sprite>("DestroyAll", "Icons", "SmallUtilities/DestroyAll/DestroyAll");
      }
    }
    #endregion
  }
}