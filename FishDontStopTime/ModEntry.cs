using GenericModConfigMenu;
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
        private ModConfig config = null;
        public static ModConfig StaticConfig { get; private set; }

        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<ModConfig>();
            StaticConfig = config;

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Display.RenderedActiveMenu += this.OnActiveMenu;

            Harmony harmony = new(ModManifest.UniqueID);
            harmony.Patch(
               original: AccessTools.Method(typeof(PathFindController), nameof(PathFindController.update)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(Pathfind_Transpiler))
            );

        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Get Generic Mod Config Menu's API
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // Register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.config)
            );

            // Add config options
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Enabled",
                tooltip: () => "Enable the mod",
                getValue: () => config.Enabled,
                setValue: value => config.Enabled = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Freeze Villagers",
                tooltip: () => "Freeze villagers while fishing",
                getValue: () => config.VillagerFreeze,
                setValue: value => config.VillagerFreeze = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Freeze Enemies",
                tooltip: () => "Freeze enemies while fishing",
                getValue: () => config.EnemiesFreeze,
                setValue: value => config.EnemiesFreeze = value
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
            return !(StaticConfig.Enabled && !StaticConfig.VillagerFreeze && menu is BobberBar || menu == null);
        }


        private void OnActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (Game1.activeClickableMenu is BobberBar && config.Enabled)
            {
                Game1.UpdateGameClock(Game1.currentGameTime);
                if (config.EnemiesFreeze)
                {
                    foreach (NPC character in Game1.currentLocation.characters)
                    {

                        if (character.IsMonster)
                        {
                            Game1.player.temporarilyInvincible = true;
                            Game1.player.temporaryInvincibilityTimer = 1000;
                            character.movementPause = 1000;
                        }

                    }
                }
            }
        }
    }

    public class ModConfig
    {
        public bool Enabled { get; set; } = true;
        public bool VillagerFreeze { get; set; } = false;
        public bool EnemiesFreeze { get; set; } = true;
    }
}