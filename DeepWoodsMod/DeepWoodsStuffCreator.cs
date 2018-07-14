﻿using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;
using static DeepWoodsMod.DeepWoodsRandom;

namespace DeepWoodsMod
{
    class DeepWoodsStuffCreator
    {
        private const int MIN_LEVEL_FOR_METEORITE = 10;
        private const int MIN_LEVEL_FOR_FLOWERS = 10;
        private const int MIN_LEVEL_FOR_FRUITS = 10;
        private const int MIN_LEVEL_FOR_GINGERBREAD_HOUSE = 20;

        private readonly static Probability CHANCE_FOR_GINGERBREAD_HOUSE = new Probability(1);
        private readonly static Probability CHANCE_FOR_RESOURCECLUMP = new Probability(5);
        private readonly static Probability CHANCE_FOR_LARGE_BUSH = new Probability(10);
        private readonly static Probability CHANCE_FOR_MEDIUM_BUSH = new Probability(5);
        private readonly static Probability CHANCE_FOR_SMALL_BUSH = new Probability(5);
        private readonly static Probability CHANCE_FOR_GROWN_TREE = new Probability(25);
        private readonly static Probability CHANCE_FOR_MEDIUM_TREE = new Probability(10);
        private readonly static Probability CHANCE_FOR_SMALL_TREE = new Probability(10);
        private readonly static Probability CHANCE_FOR_GROWN_FRUIT_TREE = new Probability(1);
        private readonly static Probability CHANCE_FOR_SMALL_FRUIT_TREE = new Probability(5);
        private readonly static Probability CHANCE_FOR_WEED = new Probability(20);
        private readonly static Probability CHANCE_FOR_TWIG = new Probability(10);
        private readonly static Probability CHANCE_FOR_STONE = new Probability(10);
        private readonly static Probability CHANCE_FOR_FLOWER = new Probability(10);
        private readonly static Probability CHANCE_FOR_FLOWER_IN_WINTER = new Probability(5);

        private readonly static Probability CHANCE_FOR_METEORITE = new Probability(1);
        private readonly static Probability CHANCE_FOR_BOULDER = new Probability(10);
        private readonly static Probability CHANCE_FOR_HOLLOWLOG = new Probability(30);

        private readonly static Probability CHANCE_FOR_FRUIT = new Probability(50);

        private DeepWoods deepWoods;
        private DeepWoodsRandom random;
        private DeepWoodsSpaceManager spaceManager;

        private DeepWoodsStuffCreator(DeepWoods deepWoods, DeepWoodsRandom random, DeepWoodsSpaceManager spaceManager)
        {
            this.deepWoods = deepWoods;
            this.random = random;
            this.spaceManager = spaceManager;
        }

        public static void AddStuff(DeepWoods deepWoods, DeepWoodsRandom random, DeepWoodsSpaceManager spaceManager)
        {
            new DeepWoodsStuffCreator(deepWoods, random, spaceManager).ClearAndAddStuff();
        }

        private void ClearAndAddStuff()
        {
            if (!Game1.IsMasterGame)
                return;

            this.random.EnterMasterMode();

            ClearStuff();
            AddStuff();

            this.random.LeaveMasterMode();
        }

        private void ClearStuff()
        {
            deepWoods.resourceClumps.Clear();
            deepWoods.largeTerrainFeatures.Clear();
            deepWoods.terrainFeatures.Clear();
            deepWoods.objects.Clear();
            deepWoods.debris.Clear();
        }

        private void AddStuff()
        {
            int mapWidth = this.spaceManager.GetMapWidth();
            int mapHeight = this.spaceManager.GetMapHeight();

            // Add a gingerbread house
            if (deepWoods.GetLevel() >= MIN_LEVEL_FOR_GINGERBREAD_HOUSE && this.random.GetChance(CHANCE_FOR_GINGERBREAD_HOUSE))
            {
                deepWoods.resourceClumps.Add(new GingerBreadHouse(new Vector2(mapWidth / 2, mapHeight / 2)));
            }

            // Calculate maximum theoretical amount of terrain features for the current map.
            int maxTerrainFeatures = (mapWidth * mapHeight) / DeepWoodsSpaceManager.MINIMUM_TILES_FOR_TERRAIN_FEATURE;

            for (int i = 0; i < maxTerrainFeatures; i++)
            {
                Vector2 location = new Vector2(this.random.GetRandomValue(1, mapWidth - 1), this.random.GetRandomValue(1, mapHeight - 1));
                if (deepWoods.terrainFeatures.ContainsKey(location) || !deepWoods.isTileLocationTotallyClearAndPlaceable(location))
                    continue;

                if (this.random.GetChance(CHANCE_FOR_RESOURCECLUMP) && IsSpaceFree(location, new Size(2, 2)))
                {
                    ResourceClump resourceClump = new ResourceClump(GetRandomResourceClumpType(), 2, 2, location);
                    deepWoods.resourceClumps.Add(resourceClump);
                }
                else if (this.random.GetChance(CHANCE_FOR_LARGE_BUSH) && IsSpaceFree(location, new Size(3, 1)))
                {
                    deepWoods.largeTerrainFeatures.Add(new DestroyableBush(location, Bush.largeBush, deepWoods));
                }
                else if (this.random.GetChance(CHANCE_FOR_MEDIUM_BUSH) && IsSpaceFree(location, new Size(2, 1)))
                {
                    deepWoods.largeTerrainFeatures.Add(new DestroyableBush(location, Bush.mediumBush, deepWoods));
                }
                else if (this.random.GetChance(CHANCE_FOR_SMALL_BUSH))
                {
                    deepWoods.largeTerrainFeatures.Add(new DestroyableBush(location, Bush.smallBush, deepWoods));
                }
                else if (this.random.GetChance(CHANCE_FOR_GROWN_TREE) && IsRegionTreeFree(location, 1))
                {
                    deepWoods.terrainFeatures.Add(location, new Tree(GetRandomTreeType(), Tree.treeStage));
                }
                else if (this.random.GetChance(CHANCE_FOR_MEDIUM_TREE))
                {
                    deepWoods.terrainFeatures.Add(location, new Tree(GetRandomTreeType(), Tree.bushStage));
                }
                else if (this.random.GetChance(CHANCE_FOR_SMALL_TREE))
                {
                    deepWoods.terrainFeatures.Add(location, new Tree(GetRandomTreeType(), this.random.GetRandomValue(Tree.sproutStage, Tree.saplingStage)));
                }
                else if (this.random.GetChance(CHANCE_FOR_GROWN_FRUIT_TREE) && IsRegionTreeFree(location, 2))
                {
                    FruitTree fruitTree = new FruitTree(GetRandomFruitTreeType(), FruitTree.treeStage);
                    deepWoods.terrainFeatures.Add(location, fruitTree);
                    if (deepWoods.GetLevel() >= MIN_LEVEL_FOR_FRUITS && this.random.GetChance(CHANCE_FOR_FRUIT))
                    {
                        int numFruits = this.random.GetRandomValue(new int[] { 1, 2, 3 }, Probability.FIFTY_FIFTY);
                        for (int j = 0; j < numFruits; j++)
                        {
                            fruitTree.dayUpdate(deepWoods, location);
                        }
                    }
                }
                else if (this.random.GetChance(CHANCE_FOR_SMALL_FRUIT_TREE))
                {
                    deepWoods.terrainFeatures.Add(location, new FruitTree(GetRandomFruitTreeType(), FruitTree.bushStage));
                }
                else if (this.random.GetChance(CHANCE_FOR_WEED))
                {
                    deepWoods.objects.Add(location, new StardewValley.Object(location, GetRandomWeedType(), 1));
                }
                else if (this.random.GetChance(CHANCE_FOR_TWIG))
                {
                    deepWoods.objects.Add(location, new StardewValley.Object(location, GetRandomTwigType(), 1));
                }
                else if (this.random.GetChance(CHANCE_FOR_STONE))
                {
                    deepWoods.objects.Add(location, new StardewValley.Object(location, GetRandomStoneType(), 1));
                }
                else if (deepWoods.GetLevel() >= MIN_LEVEL_FOR_FLOWERS && this.random.GetChance(Game1.currentSeason == "winter" ? CHANCE_FOR_FLOWER_IN_WINTER : CHANCE_FOR_FLOWER))
                {
                    deepWoods.terrainFeatures.Add(location, new Flower(GetRandomFlowerType(), location));
                }
                else
                {
                    deepWoods.terrainFeatures.Add(location, new LootFreeGrass(GetSeasonGrassType(), this.random.GetRandomValue(1, 3)));
                }
            }

            // Fill up with grass
            int maxGrass = maxTerrainFeatures * 2;
            if (Game1.currentSeason == "winter")
            {
                // Leaveless trees and snow ground make winter forest look super empty and open,
                // so we fill it with plenty of icy grass to give it a better atmosphere.
                maxGrass *= 2;
            }
            for (int i = 0; i < maxGrass; i++)
            {
                Vector2 location = new Vector2(this.random.GetRandomValue(1, mapWidth - 1), this.random.GetRandomValue(1, mapHeight - 1));
                if (deepWoods.terrainFeatures.ContainsKey(location) || !deepWoods.isTileLocationTotallyClearAndPlaceable(location))
                    continue;

                deepWoods.terrainFeatures.Add(location, new LootFreeGrass(GetSeasonGrassType(), this.random.GetRandomValue(1, 3)));
            }
        }

        private int GetRandomFlowerType()
        {
            // 453, // 376, // Poppy, summer
            // 427, // 591, // Tulip, spring
            // 455, // 593, // SummerSpangle, summer
            // 425, // 595, // FairyRose, fall
            // 429, // 597, // BlueJazz, spring

            if (Game1.currentSeason == "winter")
            {
                return this.random.GetRandomValue(new int[] { 425, 429 });
            }
            else
            {
                // TODO: Use season, sell price and player luck for probability
                return this.random.GetRandomValue(new int[] {
                    453, // 376, // Poppy, summer
                    427, // 591, // Tulip, spring
                    455, // 593, // SummerSpangle, summer
                    425, // 595, // FairyRose, fall
                    429, // 597, // BlueJazz, spring
                });
            }
        }

        private bool IsSpaceFree(Vector2 location, Size resourceClumpSize)
        {
            for (int x = 0; x < resourceClumpSize.Width; x++)
            {
                for (int y = 0; y < resourceClumpSize.Height; y++)
                {
                    Vector2 check = new Vector2(location.X + x, location.Y + y);
                    if (deepWoods.terrainFeatures.ContainsKey(check)
                        || !deepWoods.isTileLocationTotallyClearAndPlaceable(check))
                        return false;
                }
            }
            return true;
        }

        private bool TileHasTree(Vector2 location)
        {
            return deepWoods.terrainFeatures.ContainsKey(location)
                && (deepWoods.terrainFeatures[location] is FruitTree
                    || ((deepWoods.terrainFeatures[location] as Tree)?.growthStage ?? 0) >= Tree.treeStage
                );
        }

        private bool IsRegionTreeFree(Vector2 location, int radius)
        {
            for (int x = -radius; x < radius*2; x++)
            {
                for (int y = -radius; y < radius * 2; y++)
                {
                    if (TileHasTree(new Vector2(location.X + x, location.Y + y)))
                        return false;
                }
            }
            return true;
        }

        private int GetSeasonGrassType()
        {
            return Game1.currentSeason == "winter" ? Grass.frostGrass : Grass.springGrass;
        }

        private int GetRandomWeedType()
        {
            return GameLocation.getWeedForSeason(this.random.GetRandom(), Game1.currentSeason);
        }

        private int GetRandomStoneType()
        {
            return this.random.GetRandomValue(new int[] { 343, 450 });
        }

        private int GetRandomTwigType()
        {
            return this.random.GetRandomValue(new int[] { 294, 295 });
        }

        private int GetRandomTreeType()
        {
            return this.random.GetRandomValue(new int[] { Tree.bushyTree, Tree.leafyTree, Tree.pineTree });
        }

        private int GetRandomFruitTreeType()
        {
            Dictionary<int, string> fruitTrees = Game1.content.Load<Dictionary<int, string>>("Data\\fruitTrees");
            int[] fruitTreeTypes = fruitTrees.Keys.ToArray();
            Array.Sort(fruitTreeTypes);
            return this.random.GetRandomValue(fruitTreeTypes);
        }

        private int GetRandomResourceClumpType()
        {
            if (deepWoods.GetLevel() >= MIN_LEVEL_FOR_METEORITE && this.random.GetChance(CHANCE_FOR_METEORITE))
            {
                return ResourceClump.meteoriteIndex;
            }
            else if (this.random.GetChance(CHANCE_FOR_BOULDER))
            {
                return ResourceClump.boulderIndex;
            }
            else if (this.random.GetChance(CHANCE_FOR_HOLLOWLOG))
            {
                return ResourceClump.hollowLogIndex;
            }
            else
            {
                return ResourceClump.stumpIndex;
            }
            /*
            return ResourceClump.mineRock1Index;
            return ResourceClump.mineRock2Index;
            return ResourceClump.mineRock3Index;
            return ResourceClump.mineRock4Index;
            */
        }
    }
}
