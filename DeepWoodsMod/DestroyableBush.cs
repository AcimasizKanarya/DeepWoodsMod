﻿using DeepWoodsMod;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace DeepWoodsMod
{
    // Hacky class that overrides StardewValley's bush class to allow destroying bushes with an axe anywhere.
    class DestroyableBush : Bush
    {
        private Multiplayer GetMultiplayer()
        {
            return Game1MultiplayerAccessProvider.GetMultiplayer();
        }

        public DestroyableBush()
            : base()
        {
        }

        public DestroyableBush(Vector2 tileLocation, int size, GameLocation location)
            : base(tileLocation, size, location)
        {
        }

        public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation, GameLocation location)
        {
            if (location == null)
                location = Game1.currentLocation;
            if (explosion > 0)
            {
                ModEntry.GetReflection().GetMethod(this, "shake").Invoke(tileLocation, true);
                // this.shake(tileLocation, true);
                return false;
            }
            if (t != null && t is Axe)
            {
                location.playSound("leafrustle");
                ModEntry.GetReflection().GetMethod(this, "shake").Invoke(tileLocation, true);
                //this.shake(tileLocation, true);
                if ((t as Axe).upgradeLevel >= 1)
                {
                    this.health -= (t as Axe).upgradeLevel / 5f;
                    if ((double)this.health <= -1.0)
                    {
                        location.playSound("treethud");
                        DelayedAction.playSoundAfterDelay("leafrustle", 100, (GameLocation)null);
                        Color color = Color.Green;
                        string currentSeason = Game1.currentSeason;
                        if (!(currentSeason == "spring"))
                        {
                            if (!(currentSeason == "summer"))
                            {
                                if (!(currentSeason == "fall"))
                                {
                                    if (currentSeason == "winter")
                                        color = Color.Cyan;
                                }
                                else
                                    color = Color.IndianRed;
                            }
                            else
                                color = Color.ForestGreen;
                        }
                        else
                            color = Color.Green;
                        for (int index1 = 0; index1 <= this.size; ++index1)
                        {
                            for (int index2 = 0; index2 < 12; ++index2)
                            {
                                // Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[1]
                                GetMultiplayer().broadcastSprites(location, new TemporaryAnimatedSprite[1]
                                {
                                      new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(355, 1200 + (Game1.IsFall ? 16 : (Game1.IsWinter ? -16 : 0)), 16, 16), Utility.getRandomPositionInThisRectangle(this.getBoundingBox(), Game1.random) - new Vector2(0.0f, (float) Game1.random.Next(64)), false, 0.01f, Game1.IsWinter ? Color.Cyan : Color.White)
                                      {
                                        motion = new Vector2((float) Game1.random.Next(-10, 11) / 10f, (float) -Game1.random.Next(5, 7)),
                                        acceleration = new Vector2(0.0f, (float) Game1.random.Next(13, 17) / 100f),
                                        accelerationChange = new Vector2(0.0f, -1f / 1000f),
                                        scale = 4f,
                                        layerDepth = (float) ((double) tileLocation.Y * 64.0 / 10000.0),
                                        animationLength = 11,
                                        totalNumberOfLoops = 99,
                                        interval = (float) Game1.random.Next(20, 90),
                                        delayBeforeAnimationStart = (index1 + 1) * index2 * 20
                                      }
                                });
                                if (index2 % 6 == 0)
                                {
                                    // Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[1]
                                    GetMultiplayer().broadcastSprites(location, new TemporaryAnimatedSprite[1]
                                    {
                                        new TemporaryAnimatedSprite(50, Utility.getRandomPositionInThisRectangle(this.getBoundingBox(), Game1.random) - new Vector2(32f, (float) Game1.random.Next(32, 64)), color, 8, false, 100f, 0, -1, -1f, -1, 0)
                                    });
                                    // Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[1]
                                    GetMultiplayer().broadcastSprites(location, new TemporaryAnimatedSprite[1]
                                    {
                                        new TemporaryAnimatedSprite(12, Utility.getRandomPositionInThisRectangle(this.getBoundingBox(), Game1.random) - new Vector2(32f, (float) Game1.random.Next(32, 64)), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
                                    });
                                }
                            }
                        }
                        return true;
                    }
                    location.playSound("axchop");
                }
            }
            return false;
        }
    }
}
