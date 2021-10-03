using HarmonyLib;
using RoR2;

namespace Chen.MineLimitChanger.Patches
{
    [HarmonyPatch(typeof(CharacterMaster), "GetDeployableSameSlotLimit")]
    internal class TurretCountChanger
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Harmony.")]
        private static void Postfix(CharacterMaster __instance, ref int __result, DeployableSlot slot)
        {
            if (!__instance) return;
            if (slot == DeployableSlot.EngiTurret) __result = ModPlugin.turretCount;
        }
    }
}