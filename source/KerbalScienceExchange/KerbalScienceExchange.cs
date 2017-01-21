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

namespace KerboKatz.KSX
{
  [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
  public class KerbalScienceExchange : KerboKatzBase<Settings>, IToolbar
  {
    private List<GameScenes> _activeScences = new List<GameScenes>() { GameScenes.SPACECENTER };
    private UIData exchangeWindow;
    private Button buyButton;
    private Button sellButton;
    private Tooltip buyTooltip;
    private Tooltip sellTooltip;
    private float value;
    private InputField scienceField;

    public KerbalScienceExchange()
    {
      modName = "KerbalScienceExchange";
      displayName = "Kerbal Science Exchange";
      requiresUtilities = new Version(1, 4, 6);
      ToolbarBase.instance.Add(this);
      Log("Init done!");
    }

    public override void OnAwake()
    {
      LoadSettings("SmallUtilities/KerbalScienceExchange", "Settings");
      LoadUI("KerbalScienceExchange", "SmallUtilities/KerbalScienceExchange/KerbalScienceExchange");
      //LoadUI("KerbalScienceExchangeSettings", "SmallUtilities/KerbalScienceExchange/KerbalScienceExchange");
    }

    protected override void AfterDestroy()
    {
      ToolbarBase.instance.Remove(this);
      Log("AfterDestroy");
    }

    #region ui

    protected override void OnUIElemntInit(UIData uiWindow)
    {
      switch (uiWindow.name)
      {
        case "KerbalScienceExchange":
          exchangeWindow = uiWindow;
          var content = exchangeWindow.gameObject.transform.FindChild("Content");
          var buttons = content.FindChild("ExchangeButtons");
          //InitInputField(exchangeWindow.)
          scienceField = InitInputField(content, "ScienceField", "0");
          scienceField.onValueChanged.AddListener(OnInputChange);
          InitTextField(content.FindChild("ConversionLabel"), "Label", settings.ratio.ToString());
          InitTextField(content.FindChild("TaxFeeLabel"), "Label", settings.tax.ToString());
          buyButton = InitButton(buttons, "MoneyToScience", OnBuyScience);
          sellButton = InitButton(buttons, "ScienceToMoney", OnSellScience);
          buyTooltip = buyButton.GetComponent<Tooltip>();
          sellTooltip = sellButton.GetComponent<Tooltip>();
          break;
      }
    }

    private void OnSellScience()
    {
      if (value == 0)
        return;
      if (ResearchAndDevelopment.CanAfford(value))
      {
        var funds = GetBuyValue(value);
        Funding.Instance.AddFunds(funds, TransactionReasons.None);
        ResearchAndDevelopment.Instance.AddScience(-value, TransactionReasons.None);
      }
      ScreenMessages.PostScreenMessage("Transaction complete. Pleasure doing business with you!");
      scienceField.text = "0";
    }

    private void OnBuyScience()
    {
      if (value == 0)
        return;
      var funds = GetBuyValue(value);
      if (Funding.CanAfford(funds))
      {
        Funding.Instance.AddFunds(-funds, TransactionReasons.None);
        ResearchAndDevelopment.Instance.AddScience(value, TransactionReasons.None);
      }
      ScreenMessages.PostScreenMessage("Transaction complete. Pleasure doing business with you!");
      scienceField.text = "0";
    }

    private void OnInputChange(string arg0)
    {
      value = Math.Max(arg0.ToFloat(), 0);
      if (value == 0)
      {
        buyTooltip._text = "";
        sellTooltip._text = "";
        return;
      }
      var sb = new StringBuilder();
      sb.Append("Buy ");
      sb.Append(arg0);
      sb.Append(" Science for ");
      sb.Append(GetBuyValue(value));
      buyTooltip._text = sb.ToString();
      sb.Clear();
      sb.Append("Sell ");
      sb.Append(arg0);
      sb.Append(" Science for ");
      sb.Append(GetSellValue(value));
      sellTooltip._text = sb.ToString();
    }

    #endregion ui

    private float GetBuyValue(float scienceValue)
    {
      return scienceValue = Mathf.Ceil(scienceValue * settings.ratio * GetModifier());
    }

    private float GetSellValue(float scienceValue)
    {
      return scienceValue = Mathf.Ceil(scienceValue * settings.ratio / GetModifier());
    }

    private float GetModifier()
    {
      float repScale = ((Mathf.Min(Reputation.CurrentRep, settings.repHigh) - settings.repHigh) / (settings.repLow - settings.repHigh));
      return (1 + (repScale * settings.repDelta + settings.tax) / 100);
    }

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
      settings.showExchange = !settings.showExchange;
      if (settings.showExchange)
      {
        FadeCanvasGroup(exchangeWindow.canvasGroup, 1, settings.uiFadeSpeed);
      }
      else
      {
        FadeCanvasGroup(exchangeWindow.canvasGroup, 0, settings.uiFadeSpeed);
      }
    }

    public Sprite icon
    {
      get
      {
        return AssetLoader.GetAsset<Sprite>("KerbalScienceExchange", "Icons", "SmallUtilities/KerbalScienceExchange/KerbalScienceExchange");
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