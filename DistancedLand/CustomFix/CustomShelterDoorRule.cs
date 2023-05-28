using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CustomShelterDoorRule
{
    public static void HookOn()
    {
        On.ShelterDoor.ctor += ShelterDoor_ctor;
    }

    private static void ShelterDoor_ctor(On.ShelterDoor.orig_ctor orig, ShelterDoor self, Room room)
    {
        orig.Invoke(self, room);
        if (room.abstractRoom.name.Contains("SF"))
        {
            if (!self.isAncient)
            {
                self.closeTiles = new IntVector2[4];
                for (int y = 0; y <= room.TileHeight; y++)
                {
                    for (int x = 0; x < room.TileWidth; x++)
                    {
                        if (room.GetTile(x, y).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
                        {
                            self.pZero = room.MiddleOfTile(new IntVector2(x, y));
                            self.dir = new Vector2(1f, 0f);
                            for (int n = 0; n < 4; n++)
                            {
                                if (room.GetTile(x + Custom.fourDirections[n].x, y + Custom.fourDirections[n].y).Terrain != Room.Tile.TerrainType.Solid)
                                {
                                    self.dir = Custom.fourDirections[n].ToVector2();
                                    for (int num = 0; num < 4; num++)
                                    {
                                        self.closeTiles[num] = new IntVector2(x, y) + Custom.fourDirections[n] * (num + 2);
                                    }
                                    self.playerSpawnPos = new IntVector2(x, y);
                                    while (room.GetTile(self.playerSpawnPos + Custom.fourDirections[n]).Terrain != Room.Tile.TerrainType.Solid)
                                    {
                                        self.playerSpawnPos += Custom.fourDirections[n];
                                    }
                                    if (self.dir.y == 1f)
                                    {
                                        while (room.GetTile(self.playerSpawnPos + new IntVector2(-1, 0)).Terrain != Room.Tile.TerrainType.Solid)
                                        {
                                            self.playerSpawnPos.x = self.playerSpawnPos.x - 1;
                                        }
                                    }
                                    while (room.GetTile(self.playerSpawnPos + new IntVector2(0, -1)).Terrain != Room.Tile.TerrainType.Solid)
                                    {
                                        self.playerSpawnPos.y = self.playerSpawnPos.y - 1;
                                    }
                                    break;
                                }
                            }
                            self.pZero += self.dir * 60f;
                            foreach(var tile in self.closeTiles)
                            {
                                Debug.Log($"Room:{self.room.abstractRoom.name},CloseTile : {tile}");
                            }
                            Debug.Log($"{self.dir}");


                            self.perp = Custom.PerpendicularVector(self.dir);
                            self.segmentPairs = new float[5, 3];
                            for (int num2 = 0; num2 < 5; num2++)
                            {
                                self.segmentPairs[num2, 0] = self.closedFac;
                                self.segmentPairs[num2, 1] = self.closedFac;
                            }
                            self.pistons = new float[2, 3];
                            for (int num3 = 0; num3 < 2; num3++)
                            {
                                self.pistons[num3, 0] = self.closedFac;
                                self.pistons[num3, 1] = self.closedFac;
                            }
                            self.covers = new float[4, 3];
                            for (int num4 = 0; num4 < 4; num4++)
                            {
                                self.covers[num4, 0] = self.closedFac;
                                self.covers[num4, 1] = self.closedFac;
                            }
                            self.pumps = new float[8, 3];
                            for (int num5 = 0; num5 < 8; num5++)
                            {
                                self.pumps[num5, 0] = self.closedFac;
                                self.pumps[num5, 1] = self.closedFac;
                            }

                            self.Reset();

                            if (self.isAncient)
                            {
                                self.ancientDoors = new ShelterDoor.Door[2];
                                self.ancientDoors[0] = new ShelterDoor.Door(self, 0);
                                self.ancientDoors[0].closedFac = self.closedFac;
                                self.ChangeAncientDoorsStatus(0, self.closedFac < 1f);
                                self.ancientDoors[1] = new ShelterDoor.Door(self, 1);
                                self.ancientDoors[1].closedFac = self.closedFac;
                                self.ChangeAncientDoorsStatus(1, self.closedFac < 1f);
                                self.doorGraphs = new ShelterDoor.DoorGraphic[2];
                                for (int num8 = 0; num8 < 2; num8++)
                                {
                                    self.doorGraphs[num8] = new ShelterDoor.DoorGraphic(self, self.ancientDoors[num8]);
                                }
                            }

                            return;
                        }
                    }
                }
            }
        }
    }
}