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
            "1.0.2";

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

        public static int turretCount = 2;

        private void Awake()
        {
            var harmony = new Harmony("com.pudy248.TurretCountChanger");
            harmony.PatchAll();

            Log = new Log(Logger);
            config = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);
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
            return $"Place a turret that inherits all your items. Fires a cannon for 100% damage." +
                   $" Can place up to {turretCount}.";
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