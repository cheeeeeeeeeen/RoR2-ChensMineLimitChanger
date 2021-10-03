#undef DEBUG

using BepInEx;
using BepInEx.Configuration;
using Chen.Helpers.LogHelpers;
using R2API;
using R2API.Utils;
using RoR2.Skills;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using HarmonyLib;
using RoR2;

namespace Chen.MineLimitChanger
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(Helpers.HelperPlugin.ModGuid, Helpers.HelperPlugin.ModVer)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class ModPlugin : BaseUnityPlugin
    {
        public const string ModVer = "1.1.0";

        public const string ModName = "ChensMineLimitChanger";

        public const string ModGuid = "com.Chen.ChensMineLimitChanger";

        private const string PressureMinesDescToken = "CHENSMINELIMITCHANGER_PRESSURE_MINES_DESC";
        private const string SpiderMinesDescToken = "CHENSMINELIMITCHANGER_SPIDER_MINES_DESC";
        private const string TurretDescToken = "TURRETCOUNTCHANGER_TURRET_COUNT_DESC";

        private static Log Log;
        private static ConfigFile config;
        private static int pressureMinesCount = 10;
        private static int spiderMinesCount = 4;
        private static int turretCount = 4;

        public static int GetConfigTurretLimit()
        {
            ConfigFile config = new ConfigFile(System.IO.Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);
            int tc = 0;
            tc = config.Bind("Main", "Turret Count", tc, "Changes the limit and storage of the base Turret.").Value;
            return tc;
        }

        private void Awake()
        {
            var harmony = new Harmony("com.pudy248.TurretCountChanger");
            harmony.PatchAll();

            Log = new Log(Logger);
            config = new ConfigFile(System.IO.Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);
            pressureMinesCount = config.Bind("Main", "Pressure Mines Count", pressureMinesCount, "Changes the limit and storage of the Pressure Mines.").Value;
            spiderMinesCount = config.Bind("Main", "Spider Mines Count", spiderMinesCount, "Changes the limit and storage of the Spider Mines.").Value;
            turretCount = config.Bind("Main", "Turret Count", turretCount, "Changes the limit and storage of the base Turret.").Value;

            SkillDef pressureMineSkillDef = Resources.Load<SkillDef>("skilldefs/engibody/EngiBodyPlaceMine");
            if (pressureMineSkillDef.baseMaxStock == pressureMinesCount)
            {
                Log.Message($"Vanilla has the same Pressure Mines Count as the one in config ({pressureMinesCount}). Skipping.");
            }
            else
            {
                pressureMineSkillDef.skillDescriptionToken = PressureMinesDescToken;
                pressureMineSkillDef.baseMaxStock = pressureMinesCount;
                LanguageAPI.Add(PressureMinesDescToken, BuildPressureMinesDescription());
                Log.Message($"Changed Pressure Mines Count to {pressureMinesCount}.");
            }

            SkillDef spiderMineSkillDef = Resources.Load<SkillDef>("skilldefs/engibody/EngiBodyPlaceSpiderMine");
            if (spiderMineSkillDef.baseMaxStock == spiderMinesCount)
            {
                Log.Message($"Vanilla has the same Spider Mines Count as the one in config ({spiderMinesCount}). Skipping.");
            }
            else
            {
                spiderMineSkillDef.skillDescriptionToken = SpiderMinesDescToken;
                spiderMineSkillDef.baseMaxStock = spiderMinesCount;
                LanguageAPI.Add(SpiderMinesDescToken, BuildSpiderMinesDescription());
                Log.Message($"Changed Spider Mines Count to {spiderMinesCount}.");
            }

            SkillDef placeTurretDef = Resources.Load<SkillDef>("skilldefs/engibody/EngiBodyPlaceTurret");
            if (placeTurretDef.baseMaxStock == turretCount)
            {
                Log.Message($"Vanilla has the same Turret Count as the one in config ({turretCount}). Skipping.");
            }
            else
            {
                placeTurretDef.skillDescriptionToken = TurretDescToken;
                placeTurretDef.baseMaxStock = turretCount;
                LanguageAPI.Add(TurretDescToken, BuildTurretDescription());
                Log.Message($"Changed Turret Count to {turretCount}.");
            }
        }

        private string BuildPressureMinesDescription()
        {
            return $"Place a two-stage mine that deals <style=cIsDamage>300% damage</style>, or <style=cIsDamage>900% damage</style> if fully armed." +
                   $" Can place up to {pressureMinesCount}.";
        }

        private string BuildSpiderMinesDescription()
        {
            return $"Place a robot mine that deals <style=cIsDamage>600% damage</style> when an enemy walks nearby." +
                   $" Can place up to {spiderMinesCount}.";
        }

        private string BuildTurretDescription()
        {
            return $"blah blah place a turret it shoots enemies. I don't know what the original desc said." +
                   $" Can place up to {turretCount}.";
        }
    }

    [HarmonyPatch(typeof(CharacterMaster), "GetDeployableSameSlotLimit")]
    public class LimitPatcher
    {
        static void Postfix(CharacterMaster __instance, ref int __result, DeployableSlot slot)
        {
            int result = 0;
            int num = 1;
            if (RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.swarmsArtifactDef))
            {
                num = 2;
            }
            switch (slot)
            {
                case DeployableSlot.EngiMine:
                    result = 4;
                    if (__instance.bodyInstanceObject)
                    {
                        result = __instance.bodyInstanceObject.GetComponent<SkillLocator>().secondary.maxStock;
                    }
                    break;
                case DeployableSlot.EngiTurret:
                    result = ModPlugin.GetConfigTurretLimit(); //there's a way to do this with the transpiler but I don't know how
                    break;
                case DeployableSlot.BeetleGuardAlly:
                    result = __instance.inventory.GetItemCount(RoR2Content.Items.BeetleGland) * num;
                    break;
                case DeployableSlot.EngiBubbleShield:
                    result = 1;
                    break;
                case DeployableSlot.LoaderPylon:
                    result = 3;
                    break;
                case DeployableSlot.EngiSpiderMine:
                    result = 4;
                    if (__instance.bodyInstanceObject)
                    {
                        result = __instance.bodyInstanceObject.GetComponent<SkillLocator>().secondary.maxStock;
                    }
                    break;
                case DeployableSlot.RoboBallMini:
                    result = 3;
                    break;
                case DeployableSlot.ParentPodAlly:
                    result = __instance.inventory.GetItemCount(RoR2Content.Items.Incubator) * num;
                    break;
                case DeployableSlot.ParentAlly:
                    result = __instance.inventory.GetItemCount(RoR2Content.Items.Incubator) * num;
                    break;
                case DeployableSlot.PowerWard:
                    result = 1;
                    break;
                case DeployableSlot.CrippleWard:
                    result = 5;
                    break;
                case DeployableSlot.DeathProjectile:
                    result = 3;
                    break;
                case DeployableSlot.RoboBallRedBuddy:
                case DeployableSlot.RoboBallGreenBuddy:
                    result = num;
                    break;
            }
            __result = result;
        }
    }
}
