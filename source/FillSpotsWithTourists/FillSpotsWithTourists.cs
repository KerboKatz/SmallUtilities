using Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace KerboKatz
{
  [KSPAddon(KSPAddon.Startup.Flight, false)]
  public partial class FillSpotsWithTourists : KerboKatzBase
  {
    private ModalWindowClass modalWindow;
    private List<ProtoCrewMember> touristList = new List<ProtoCrewMember>();
    public FillSpotsWithTourists()
    {
      modName = "SmallUtilities.FillSpotsWithTourists";
      displayName = "Fill spots with tourists";
      requiresUtilities = new Version(1, 2, 6);
    }

    protected override void Started()
    {
      currentSettings.load("SmallUtilities/FillSpotsWithTourists", "FillSpotsWithTourists", modName);
      GameEvents.onVesselSituationChange.Add(situationChange);
      GameEvents.onFlightReady.Add(checkForContractsAndEmptySpots);
    }

    private bool isVesselPrelaunch()
    {
      if (FlightGlobals.ready)
      {
        if (FlightGlobals.ActiveVessel != null)
        {
          if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH)
          {
            if (Vessel.GetLandedAtString(FlightGlobals.ActiveVessel.landedAt) == "LaunchPad" || Vessel.GetLandedAtString(FlightGlobals.ActiveVessel.landedAt) == "Runway")
            {
              return true;
            }
            else
            {
              Utilities.debug(modName, Vessel.GetLandedAtString(FlightGlobals.ActiveVessel.landedAt));
            }
          }
        }
      }
      return false;
    }

    private void checkForContractsAndEmptySpots()
    {
      GameEvents.onFlightReady.Remove(checkForContractsAndEmptySpots);
      if (!isVesselPrelaunch())
        return;
      var TourismContracts = ContractSystem.Instance.GetCurrentActiveContracts<FinePrint.Contracts.TourismContract>().Length;
      if (TourismContracts > 0)
      {
        int freeSpots = 0;
        int touristCount = 0;
        var currentVessel = FlightGlobals.ActiveVessel;
        foreach (var part in currentVessel.parts)
        {
          if (part.CrewCapacity > 0)
          {
            if (part.protoModuleCrew.Count < part.CrewCapacity)
            {
              freeSpots += part.CrewCapacity - part.protoModuleCrew.Count;
            }
          }
        }
        if (freeSpots == 0)
          return;
        touristList.Clear();
        var nextTourist = HighLogic.CurrentGame.CrewRoster.Kerbals(ProtoCrewMember.KerbalType.Tourist, ProtoCrewMember.RosterStatus.Available);
        foreach (var tourist in nextTourist)
        {
          touristCount++;
          touristList.Add(tourist);
        }
        if (touristCount == 0)
          return;
        var text = new StringBuilder();
        if (TourismContracts == 1)
        {
          text.Append("There is one tourism contract available");
        }
        else
        {
          text.Append("There are " + TourismContracts + " contracts available.");
        }
        if (touristCount == 1)
        {
          text.Append("You have one tourist waiting");
        }
        else
        {
          text.Append("You have " + touristCount + " tourists waiting");
        }
        if (freeSpots == 1)
        {
          text.Append(" and you have one spot free.");
        }
        else
        {
          text.Append(" and you have " + freeSpots + " spots free.");
        }

        text.Append(" Do you want to fill your empty spots with tourists ?");
        modalWindow = new ModalWindowClass(displayName, text.ToString(), "Yes", "No", addTourists, removeModal);
        ModalWindow.instance.add(modalWindow);
      }
    }

    private void situationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> data)
    {
      if (data.host == FlightGlobals.ActiveVessel)
      {
        if (data.from == Vessel.Situations.PRELAUNCH)
        {
          removeModal();
          GameEvents.onVesselSituationChange.Remove(situationChange);
        }
      }
    }

    private void removeModal()
    {
      ModalWindow.instance.remove(modalWindow);
    }

    private void addTourists()
    {
      removeModal();
      bool added = false;

      if (isVesselPrelaunch())
      {
        var currentVessel = FlightGlobals.ActiveVessel;
        foreach (var part in currentVessel.parts)
        {
          if (part.CrewCapacity > 0)
          {
            if (part.protoModuleCrew.Count < part.CrewCapacity)
            {
              var emptySpace = part.CrewCapacity - part.protoModuleCrew.Count;
              foreach (var tourist in touristList)
              {
                if (emptySpace == 0)
                  break;
                if (tourist.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
                  continue;
                tourist.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
                part.AddCrewmember(tourist);
                if (tourist.seat != null)
                {
                  emptySpace--;
                  tourist.seat.SpawnCrew();
                  added = true;
                }
              }
            }
          }
        }

        if (added)
        {
          GameEvents.onVesselChange.Fire(currentVessel);
        }
      }
    }

    protected override void beforeSaveOnDestroy()
    {
      GameEvents.onVesselSituationChange.Remove(situationChange);
      GameEvents.onFlightReady.Remove(checkForContractsAndEmptySpots);
      if (currentSettings != null)
      {
        currentSettings.set("showSettings", false);
      }
    }
  }
}