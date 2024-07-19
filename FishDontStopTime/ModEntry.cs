using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace FishDontStopTime
{
    internal sealed class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.Display.RenderedActiveMenu += this.OnActiveMenu;
        }

        private void OnActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (Game1.activeClickableMenu is BobberBar)
            {

                // Maybe some of these might not be needed but still forcing all of them just to make sure time don't stop
                Game1.paused = false;
                Game1.player.forceTimePass = true;
                Game1.isTimePaused = false;

                // This was an attempt to make NPCs move while reeling, it doesn't work, characters keep moving straight
                // until fishing animation is done so they don't collide with anything, walk on water like jesus, etc.

                /*
                foreach (Character character in Utility.getAllCharacters())
                {
                    if (character.currentLocation == Game1.currentLocation)
                    {
                        character.MovePosition(Game1.currentGameTime, Game1.viewport, Game1.currentLocation);
                    }
                }
                */

                Game1.UpdateGameClock(Game1.currentGameTime);
            }
        }
    }
}