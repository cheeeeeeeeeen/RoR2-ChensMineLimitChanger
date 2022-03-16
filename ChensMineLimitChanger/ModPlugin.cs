#undef DEBUG

using BepInEx;
using BepInEx.Configuration;
using Chen.Helpers.LogHelpers;
using HarmonyLib;
using R2API;
using R2API.Utils;
using RoR2.Skills;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("ChensMineLimitChanger.Tests")]

namespace Chen.MineLimitChanger
{
    /// <summary>
    /// Description of the plugin.
    /// </summary>
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(Helpers.HelperPlugin.ModGuid, Helpers.HelperPlugin.ModVer)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class ModPlugin : BaseUnityPlugin
    {
        /// <summary>
        /// This mod's version.
        /// </summary>
        public const string ModVer =
#if DEBUG
            "0." +
#endif
            "2.0.3";

        /// <summary>
        /// This mod's name.
        /// </summary>
        public const string ModName = "ChensMineLimitChanger";

        /// <summary>
        /// This mod's GUID.
        /// </summary>
        public const string ModGuid = "com.Chen.ChensMineLimitChanger";

        private const string PressureMinesDescToken = "CHENSMINELIMITCHANGER_PRESSURE_MINES_DESC";
        private const string SpiderMinesDescToken = "CHENSMINELIMITCHANGER_SPIDER_MINES_DESC";
        private const string TurretDescToken = "TURRETCOUNTCHANGER_TURRET_COUNT_DESC";

        private static Log Log;
        private static ConfigFile config;
        private static int pressureMinesCount = 10;
        private static int spiderMinesCount = 4;
        private static int turretCount = 2;

        public static int turretFieldCount = turretCount;
        public static int pressureMinesFieldCount = pressureMinesCount;
        public static int spiderMinesFieldCount = spiderMinesCount;
        public static bool lysateCellIncrement = false;

        private void Awake()
        {
            Log = new Log(Logger);
            config = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);
            pressureMinesCount = config.Bind("Main", "Pressure Mines Count", pressureMinesCount, "Changes the storage of the Pressure Mines.").Value;
            spiderMinesCount = config.Bind("Main", "Spider Mines Count", spiderMinesCount, "Changes the storage of the Spider Mines.").Value;
            turretCount = config.Bind("Main", "Turret Count", turretCount, "Changes the storage of the base Turret.").Value;
            pressureMinesFieldCount = config.Bind("Main", "Pressure Mines Field Limit", pressureMinesFieldCount, "Changes the field limit of Pressure Mines.").Value;
            spiderMinesFieldCount = config.Bind("Main", "Spider Mines Field Limit", spiderMinesFieldCount, "Changes the field limit of Spider Mines.").Value;
            turretFieldCount = config.Bind("Main", "Turret Field Limit", turretFieldCount, "Changes the field limit of turrets.").Value;
            lysateCellIncrement = config.Bind("Main", "Lysate Cell Mechanics", lysateCellIncrement,
                                              "Set to true if you want the number of turrets on field to also increase by the number of Lysate Cells held. " +
                                              "Set to false to only increase by turret field count by 1 if any Lysate Cells are held.").Value;

            var harmony = new Harmony("com.pudy248.TurretCountChanger");
            Log.Message("Harmony Patch com.pudy248.TurretCountChanger initialized.");
            harmony.PatchAll();
            Log.Message($"Changed, by logic, Pressure Mines Field Limit to {pressureMinesFieldCount}.");
            Log.Message($"Changed, by logic, Spider Mines Field Limit to {spiderMinesFieldCount}.");
            Log.Message($"Changed, by logic, Turrets Field Limit to {turretFieldCount}.");

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
                   $" Can store up to {pressureMinesCount}." +
                   $" Can place up to {pressureMinesFieldCount}.";
        }

        private string BuildSpiderMinesDescription()
        {
            return $"Place a robot mine that deals <style=cIsDamage>600% damage</style> when an enemy walks nearby." +
                   $" Can store up to {spiderMinesCount}." +
                   $" Can place up to {spiderMinesFieldCount}.";
        }

        private string BuildTurretDescription()
        {
            return $"Place a turret that inherits all your items. Fires a cannon for 100% damage." +
                   $" Can store up to {turretCount}." +
                   $" Can place up to {turretFieldCount}.";
        }

        internal static bool DebugCheck()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}