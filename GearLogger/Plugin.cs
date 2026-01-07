using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.IoC;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace GearLogger;

public sealed class GearLogger : IDalamudPlugin
{
    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    public GearLogger()
    {
        PluginInterface.Create<Services>();
        Services.Plugin = this;
        Services.PluginInterface = PluginInterface;

        Services.AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, "CharacterInspect", OnInspect);
    }

    public void Dispose()
    {
        Services.AddonLifecycle.UnregisterListener(AddonEvent.PostRefresh, "CharacterInspect", OnInspect);
    }

    public void OnInspect(AddonEvent e, AddonArgs args)
    {
        var eventVal = args.Addon.AtkValues.FirstOrDefault(0);
        if (eventVal.GetValue() is not uint state || state != 2) return;

        var logsOutput = ProcessExamineInventory();

        GearLog.AppendToCsv(logsOutput);
    }

    private List<GearLog> ProcessExamineInventory()
    {
        List<GearLog> gearLogs = new List<GearLog>();

        unsafe
        {
            var container = InventoryManager.Instance()->GetInventoryContainer(InventoryType.Examine);

            for (var i = 0; i < container->Size; i++)
            {
                if (i == 5) continue; // skip ArmoryWaist

                var slot = container->GetInventorySlot(i);
                var itemId = slot->ItemId;
                var itemRow = Services.DataManager.GetExcelSheet<Item>()?.GetRow(itemId);
                if (itemRow != null && itemId != 0)
                {
                    var item = itemRow.Value;

                    GearLog log = new GearLog
                    {
                        PieceName = item.Name.ExtractText(),
                        PieceId = (int)itemId,
                        ItemLevel = (int)item.LevelItem.RowId
                    };

                    ProcessParamRow(item, ref log, false);
                    ProcessParamRow(item, ref log, true);

                    // log.LogInfoFormatted();

                    gearLogs.Add(log);
                }
            }
        }

        return gearLogs;
    }

    private void ProcessParamRow(Item item, ref GearLog log, bool special)
    {
        var usedParam = special ? item.BaseParamValueSpecial : item.BaseParamValue;

        for (var j = 0; j < usedParam.Count; j++)
        {
            var paramValue = usedParam[j];
            var paramName = item.BaseParam[j].Value.Name.ExtractText();
            paramName = GetEngParamName(paramName);
            switch (paramName)
            {
                case "Strength":
                    log.Strength += paramValue;
                    break;
                case "Dexterity":
                    log.Dexterity += paramValue;
                    break;
                case "Intelligence":
                    log.Intelligence += paramValue;
                    break;
                case "Mind":
                    log.Mind += paramValue;
                    break;
                case "Critical Hit":
                    log.CriticalHit += paramValue;
                    break;
                case "Determination":
                    log.Determination += paramValue;
                    break;
                case "Direct Hit Rate":
                    log.DirectHit += paramValue;
                    break;
                case "Skill Speed":
                    log.SkillSpeed += paramValue;
                    break;
                case "Spell Speed":
                    log.SpellSpeed += paramValue;
                    break;
                case "Piety":
                    log.Piety += paramValue;
                    break;
                case "Vitality":
                    log.Vitality += paramValue;
                    break;
                case "Tenacity":
                    log.Tenacity += paramValue;
                    break;
            }
        }
    }

    private string GetEngParamName(string s)
    {
        string[] strength = ["Strength", "STR", "Stärke", "Force"];
        string[] dexterity = ["Dexterity", "DEX", "Geschick", "Dextérité"];
        string[] intelligence = ["Intelligence", "INT", "Intelligenz", "Intelligence"];
        string[] mind = ["Mind", "MND", "Willenskraft", "Esprit"];
        string[] criticalHit = ["Critical Hit", "クリティカル", "Kritische Treffer", "Critique"];
        string[] determination = ["Determination", "意思力", "Entschlossenheit", "Détermination"];
        string[] directHit = ["Direct Hit Rate", "ダイレクトヒット", "Direkter Treffer", "Coups nets"];
        string[] skillSpeed = ["Skill Speed", "スキルスピード", "Schnelligkeit", "Vivacité"];
        string[] spellSpeed = ["Spell Speed", "スペルスピード", "Zaubertempo", "Célérité"];
        string[] piety = ["Piety", "信仰", "Frömmigkeit", "Piété"];
        string[] vitality = ["Vitality", "VIT", "Konstitution", "Vitalité"];
        string[] tenacity = ["Tenacity", "不屈", "Unbeugsamkeit", "Ténacité"];
        
        if (strength.Contains(s)) return "Strength";
        if (dexterity.Contains(s)) return "Dexterity";
        if (intelligence.Contains(s)) return "Intelligence";
        if (mind.Contains(s)) return "Mind";
        if (criticalHit.Contains(s)) return "Critical Hit";
        if (determination.Contains(s)) return "Determination";
        if (directHit.Contains(s)) return "Direct Hit Rate";
        if (skillSpeed.Contains(s)) return "Skill Speed";
        if (spellSpeed.Contains(s)) return "Spell Speed";
        if (piety.Contains(s)) return "Piety";
        if (vitality.Contains(s)) return "Vitality";
        if (tenacity.Contains(s)) return "Tenacity";
        return s;
    }
}
