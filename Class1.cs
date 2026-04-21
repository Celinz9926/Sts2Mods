using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using System.Reflection;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using System.Reflection.Emit;

namespace StsSelfUseMod
{
    [ModInitializer("Initialize")]
    public static class ModEntry
    {
        public static void Initialize()
        {
            config.ModConfig.Load();
            new Harmony("sts2.StsSelfUseMod.mod").PatchAll();
        }
    }
}

namespace StsSelfUseMod.config
{
    public static class ModConfig
    {
        public static int ExtraEnergy = 0;
        public static int ExtraGold = 0;

        public static bool ZeroRemoveFlag = false;
        public static bool InfiniteRemoveFlag = false;

        public static int ExtraCardNum = 0;
        public static bool BlurUpgradeFlag = false;
        public static bool ProductionUpgradeFlag = false;
        public static bool InfiniteSmithFlag = false;
        public static int SmithNum = 0;
        public static bool BridgeLossHpFlag = false;
        public static bool EnvenomUpgradeFlag = false;
        public static bool BrightestFlameUpgradeFlag = false;
        public static bool DirgeUpgradeFlag = false;
        public static bool AcrobaticsRarityRestoreFlag = false;

        public static void Load()
        {
            try
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
                string path = Path.Combine(dir, "config.ini");

                Console.WriteLine($"[ModConfig] Loading config from: {path}");

                if (!File.Exists(path))
                {
                    File.WriteAllText(path,
@"# StsSelfUseMod 配置文件
# 修改数值后重启游戏生效。
# 功能标志：true为开启，false为关闭

[StartingState]
# 全局额外初始能量（0为不修改）
ExtraEnergy = 0

# 全角色额外起始金币
ExtraGold = 500

[ShopState]
# 零费删卡标志
ZeroRemoveFlag = true

# 无限删卡标志
InfiniteRemoveFlag = true

[OtherState]
# 战斗后奖励额外卡牌数（数值在【-2，2】之间，0为不修改）
ExtraCardNum = 1

# 自定义锻造升级卡牌功能
InfiniteSmithFlag = true
# 锻造卡牌数(InfiniteSmithFlag=false时，该值设定无效)，<=0为全部卡牌，>0的值为必选升级卡牌数（设定后，必须选择设定数值的卡牌数升级，自动限定不超过可升级卡牌数）
SmithNum = 0

[CardState]

# 残影升级格挡保留额外提升1回合
BlurUpgradeFlag = true

# 涂毒升级耗能减1，中毒额外加1
EnvenomUpgradeFlag = true

# 杂技回滚为普通卡
AcrobaticsRarityRestoreFlag = true

# 挽歌升级去除消耗
DirgeUpgradeFlag = true

# 生产制造升级去除消耗
ProductionUpgradeFlag = true

# 至亮之焰升级不再消耗最大生命
BrightestFlameUpgradeFlag = true

[EventsState]
# 湿滑木桥是否掉血
BridgeLossHpFlag = true"
                    );
                }

                string currentSection = "";

                foreach (var line in File.ReadAllLines(path))
                {
                    string l = line.Trim();

                    if (string.IsNullOrEmpty(l) || l.StartsWith("#"))
                        continue;

                    if (l.StartsWith("[") && l.EndsWith("]"))
                    {
                        currentSection = l;
                        continue;
                    }

                    var parts = l.Split('=');
                    if (parts.Length != 2) continue;

                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    switch (currentSection)
                    {
                        case "[StartingState]":
                            if (key == "ExtraEnergy") int.TryParse(value, out ExtraEnergy);
                            if (key == "ExtraGold") int.TryParse(value, out ExtraGold);
                            break;

                        case "[ShopState]":
                            if (key == "ZeroRemoveFlag") bool.TryParse(value, out ZeroRemoveFlag);
                            if (key == "InfiniteRemoveFlag") bool.TryParse(value, out InfiniteRemoveFlag);
                            break;

                        case "[OtherState]":
                            if (key == "ExtraCardNum") int.TryParse(value, out ExtraCardNum);
                            if (key == "InfiniteSmithFlag") bool.TryParse(value, out InfiniteSmithFlag);
                            if (key == "SmithNum") int.TryParse(value, out SmithNum);
                            break;

                        case "[CardState]":
                            if (key == "BlurUpgradeFlag") bool.TryParse(value, out BlurUpgradeFlag);
                            if (key == "ProductionUpgradeFlag") bool.TryParse(value, out ProductionUpgradeFlag);
                            if (key == "EnvenomUpgradeFlag") bool.TryParse(value, out EnvenomUpgradeFlag);
                            if (key == "BrightestFlameUpgradeFlag") bool.TryParse(value, out BrightestFlameUpgradeFlag);
                            if (key == "DirgeUpgradeFlag") bool.TryParse(value, out DirgeUpgradeFlag);
                            if (key == "AcrobaticsRarityRestoreFlag") bool.TryParse(value, out AcrobaticsRarityRestoreFlag);
                            break;

                        case "[EventsState]":
                            if (key == "BridgeLossHpFlag") bool.TryParse(value, out BridgeLossHpFlag);
                            break;
                    }
                }

                Console.WriteLine($"[Config] Energy+{ExtraEnergy}, Gold+{ExtraGold}, Card+{ExtraCardNum}");
            }
            catch (Exception e)
            {
                Console.WriteLine("Config load failed: " + e);
            }
        }
    }
}

namespace StsSelfUseMod.Patches
{
    // 初始能量
    [HarmonyPatch(typeof(CharacterModel), "get_MaxEnergy")]
    public static class Patch_MaxEnergy
    {
        static void Postfix(ref int __result)
        {
            if (config.ModConfig.ExtraEnergy != 0)
                __result += config.ModConfig.ExtraEnergy;
        }
    }

    //角色初始金币
    [HarmonyPatch]
    public static class Patch_AllCharactersStartingGold
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            return new[]
            {
                AccessTools.Method(typeof(Ironclad), "get_StartingGold"),
                AccessTools.Method(typeof(Silent), "get_StartingGold"),
                AccessTools.Method(typeof(Regent), "get_StartingGold"),
                AccessTools.Method(typeof(Necrobinder), "get_StartingGold"),
                AccessTools.Method(typeof(Defect), "get_StartingGold"),
                AccessTools.Method(typeof(Deprived), "get_StartingGold")
            };
        }

        static void Postfix(ref int __result)
        {
            if (config.ModConfig.ExtraGold != 0)
                __result += config.ModConfig.ExtraGold;
        }
    }

    // 商店移除卡牌费用
    [HarmonyPatch(typeof(MerchantCardRemovalEntry), "CalcCost")]
    public static class Patch_RemoveCardZeroCost
    {
        static readonly AccessTools.FieldRef<MerchantCardRemovalEntry, int> costField =
            AccessTools.FieldRefAccess<MerchantCardRemovalEntry, int>("_cost");

        static void Postfix(MerchantCardRemovalEntry __instance)
        {
            if (!config.ModConfig.ZeroRemoveFlag) return;

            costField(__instance) = 0;
        }
    }

    //商店移除无限
    [HarmonyPatch(typeof(MerchantCardRemovalEntry), "get_Used")]
    public static class Patch_InfiniteRemoveCard
    {
        static void Postfix(ref bool __result)
        {
            if (config.ModConfig.InfiniteRemoveFlag)
                __result = false;
        }
    }

    //战斗奖励卡牌变更为4张
    [HarmonyPatch]
    class Patch_MoreCardReward
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(CardReward).GetConstructors(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        static readonly AccessTools.FieldRef<CardReward, int> optionCountRef =
            AccessTools.FieldRefAccess<CardReward, int>("<OptionCount>k__BackingField");

        static void Postfix(CardReward __instance)
        {
            if (config.ModConfig.ExtraCardNum == 0) return;

            ref int val = ref optionCountRef(__instance);

            val += config.ModConfig.ExtraCardNum;

            // 防止极端值
            if (val < 1) val = 1;
            if (val > 5) val = 5;
        }
    }

    //自定义锻造升级卡牌功能
    [HarmonyPatch(typeof(SmithRestSiteOption), "OnSelect")]
    public static class Patch_SmithUpgradesAll
    {
        static bool Prefix(SmithRestSiteOption __instance, ref Task<bool> __result)
        {
            if (!config.ModConfig.InfiniteSmithFlag)
                return true;

            __result = Run(__instance);
            return false;
        }

        static async Task<bool> Run(SmithRestSiteOption instance)
        {
            var owner = Traverse.Create(instance)
                .Property("Owner")
                .GetValue<Player>();

            var list = PileType.Deck.GetPile(owner).Cards
                .Where(c => c.IsUpgradable)
                .ToList();

            if (list.Count == 0)
                return false;

            int maxSelect = list.Count;

            if (config.ModConfig.SmithNum > 0)
                maxSelect = Math.Min(config.ModConfig.SmithNum, list.Count);

            var prefs = new CardSelectorPrefs(
                CardSelectorPrefs.UpgradeSelectionPrompt,
                0,
                maxSelect
            )
            {
                Cancelable = true,
                RequireManualConfirmation = false
            };

            var cards = await CardSelectCmd.FromDeckForUpgrade(owner, prefs);

            var selected = cards?.ToList();

            if (selected == null || selected.Count == 0)
                return false;

            Traverse.Create(instance)
                .Field("_selection")
                .SetValue(selected);

            foreach (var c in selected)
            {
                CardCmd.Upgrade(c, CardPreviewStyle.None);
            }

            await Hook.AfterRestSiteSmith(owner.RunState, owner);

            return true;
        }
    }

    //湿滑木桥不掉血
    [HarmonyPatch(typeof(SlipperyBridge), "get_CurrentHpLoss")]
    public static class Patch_BridgeLossHp
    {
        static void Postfix(ref int __result)
        {
            if (!config.ModConfig.BridgeLossHpFlag) return;

            __result = 0;
        }
    }

    //残影升级格挡保留追加一回合
    [HarmonyPatch(typeof(Blur), "OnUpgrade")]
    public static class Patch_BlurUpgrade
    {
        static void Postfix(Blur __instance)
        {
            if (!config.ModConfig.BlurUpgradeFlag) return;

            __instance.DynamicVars["Blur"].UpgradeValueBy(1m);
        }
    }

    //涂毒升级减少1点能量，增加1点中毒伤害
    [HarmonyPatch(typeof(Envenom), "OnUpgrade")]
    public static class Patch_EnvenomUpgrade
    {
        static void Postfix(Envenom __instance)
        {
            if (!config.ModConfig.EnvenomUpgradeFlag) return;

            __instance.EnergyCost.UpgradeBy(-1);
            __instance.DynamicVars["EnvenomPower"].UpgradeValueBy(1m);
        }
    }

    //杂技回滚至普通卡
    [HarmonyPatch(typeof(Acrobatics))]
        [HarmonyPatch(MethodType.Constructor)]
        public static class Patch_AcrobaticsRarity
        {
            static readonly AccessTools.FieldRef<CardModel, CardRarity> rarityRef =
                AccessTools.FieldRefAccess<CardModel, CardRarity>("<Rarity>k__BackingField");

            static void Postfix(Acrobatics __instance)
            {
                if (!config.ModConfig.AcrobaticsRarityRestoreFlag) return;

                rarityRef(__instance) = CardRarity.Common;
            }
        }

    //挽歌升级去除消耗
    [HarmonyPatch(typeof(Dirge), "OnUpgrade")]
    public static class Patch_DirgeUpgrade
    {
        static void Postfix(Dirge __instance)
        {
            if (!config.ModConfig.DirgeUpgradeFlag) return;

            __instance.RemoveKeyword(CardKeyword.Exhaust);
        }
    }

    //生产制造升级去除消耗
    [HarmonyPatch(typeof(Production), "OnUpgrade")]
    public static class Patch_ProductionUpgrade
    {
        static void Postfix(Production __instance)
        {
            if (!config.ModConfig.ProductionUpgradeFlag) return;

            __instance.RemoveKeyword(CardKeyword.Exhaust);
        }
    }

    //至亮之焰升级不再消耗最大生命
    [HarmonyPatch(typeof(BrightestFlame), "OnUpgrade")]
    public static class Patch_BrightestFlameUpgrade
    {
        static void Postfix(BrightestFlame __instance)
        {
            if (!config.ModConfig.BrightestFlameUpgradeFlag) return;

            __instance.DynamicVars.MaxHp.UpgradeValueBy(-1m);
        }
    }

}