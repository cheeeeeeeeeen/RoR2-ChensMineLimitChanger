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
            switch (slot)
            {
                case DeployableSlot.EngiTurret:
                    __result = ModPlugin.turretFieldCount;
                    break;

                case DeployableSlot.EngiMine:
                    __result = ModPlugin.pressureMinesFieldCount + __instance.inventory.GetItemCount(RoR2Content.Items.SecondarySkillMagazine);
                    break;

                case DeployableSlot.EngiSpiderMine:
                    __result = ModPlugin.spiderMinesFieldCount + __instance.inventory.GetItemCount(RoR2Content.Items.SecondarySkillMagazine);
                    break;
            }
        }
    }
}