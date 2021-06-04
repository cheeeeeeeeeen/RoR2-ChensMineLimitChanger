#undef DEBUG

using BepInEx;
using Chen.Helpers.LogHelpers;
using R2API.Utils;
using System.Runtime.CompilerServices;

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
            "1.0.0";

        /// <summary>
        /// This mod's name.
        /// </summary>
        public const string ModName = "ChensMineLimitChanger";

        /// <summary>
        /// This mod's GUID.
        /// </summary>
        public const string ModGuid = "com.Chen.ChensMineLimitChanger";

        internal static Log Log;

        private void Awake()
        {
            Log = new Log(Logger);

#if DEBUG
            Chen.Helpers.GeneralHelpers.MultiplayerTest.Enable(Log);
#endif
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