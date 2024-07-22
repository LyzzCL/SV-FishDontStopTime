using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Pathfinding;
using System.Reflection;
using System.Reflection.Emit;

namespace FishDontStopTime
{
    internal sealed class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.Display.RenderedActiveMenu += this.OnActiveMenu;

            Harmony harmony = new(ModManifest.UniqueID);
            harmony.Patch(
               original: AccessTools.Method(typeof(PathFindController), nameof(PathFindController.update)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(Pathfind_Transpiler))
            );

        }

        public static IEnumerable<CodeInstruction> Pathfind_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new(instructions);
            MethodInfo specialCheck = AccessTools.Method(typeof(ModEntry), nameof(FishingCheck));

            matcher.MatchStartForward(
                new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.activeClickableMenu))),
                new CodeMatch(OpCodes.Brfalse_S)
                )
                .Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Call, specialCheck)
                );

            return matcher.InstructionEnumeration();
        }

        public static bool FishingCheck(IClickableMenu menu)
        {
            return !(menu is BobberBar || menu == null);
        }


        private void OnActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (Game1.activeClickableMenu is BobberBar)
            {
                // Some of these might not be needed but still forcing all of them just to make sure time don't stop
                Game1.paused = false;
                Game1.player.forceTimePass = true;
                Game1.isTimePaused = false;

                Game1.dialogueUp = false;
                Game1.eventUp = false;

                Game1.UpdateGameClock(Game1.currentGameTime);
            }
        }
    }
}