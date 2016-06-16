using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FPSGame
{
    class CollisionManager
    {
        static bool[,] map;
        static CollisionManager cm;
        static List<Missile> missiles = new List<Missile>();
        static List<Enemy> enemies = new List<Enemy>();
        static Player player;

        private CollisionManager()
        {
        }

        public void AddMissile(Missile msl)
        {
            missiles.Add(msl);
        }

        public void AddPlayer(Player plyr)
        {
            player = plyr;
        }

        public void AddEnemy(Enemy nmy)
        {
            enemies.Add(nmy);
        }

        public void CheckCollisions()
        {
            for (int i = 0; i < missiles.Count; i++)
            {
                if (BuildingCollision(missiles[i].Position))
                {
                    missiles[i].Collision();
                    missiles.RemoveAt(i);
                    i--;
                }
                else
                {
                    bool collided = false;
                    for (int i2 = 0; i2 < enemies.Count; i2++)
                    {
                        if (PointWithinRectangle(new Vector2(missiles[i].Position.X, missiles[i].Position.Z), enemies[i2].Corners()))
                        {
                            enemies[i2].Collision();
                            if (!enemies[i2].Alive)
                            {
                                enemies.RemoveAt(i2);
                                i2--;
                            }
                            missiles[i].Collision();
                            missiles.RemoveAt(i);
                            i--;
                            collided = true;
                            break;
                        }

                    }
                    if (!collided)
                    {
                        if (PointWithinRectangle(new Vector2(missiles[i].Position.X, missiles[i].Position.Z), player.Corners()))
                        {
                            player.Collision();
                            missiles[i].Collision();
                            missiles.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
        }

        public static void Initialize(bool[,] mp)
        {
            cm = new CollisionManager();
            map = mp;
        }

        public static CollisionManager Instance()
        {
            return cm;
        }

        public bool PointWithinRectangle(Vector2 point, Vector2[] corners)
        {
            Vector2[] rotatedCorners = new Vector2[4];
            float angle = (float)Math.Atan2(corners[0].Y - corners[1].Y, corners[0].X - corners[1].X) +(float)(Math.PI / 2);
            for (int i = 0; i < 4; i++)
            {
                rotatedCorners[i] = RotatedVec(corners[i], angle);
            }
            Vector2 rotatedPoint = RotatedVec(point, angle);
            return (rotatedPoint.X > rotatedCorners[0].X && rotatedPoint.X < rotatedCorners[2].X && rotatedPoint.Y > rotatedCorners[0].Y && rotatedPoint.Y < rotatedCorners[2].Y);
        }

        private Vector2 RotatedVec(Vector2 vec, float yaw)
        {
            Vector3 ansVec3 = (Matrix.CreateTranslation(new Vector3(vec.X, 0, vec.Y)) * Matrix.CreateRotationY(yaw)).Translation;
            return new Vector2(ansVec3.X,ansVec3.Z);
        }

        public bool BuildingCollision(Vector3 point)
        {
            if (point.X < 0 || point.Z < 0 || point.X > 150 || point.Z > 150)
                return true;
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    if (map[x, y])
                    {
                        if (point.X > x * 15 && point.Z > y * 15 && point.X < x * 15 + 15 && point.Z < y * 15 + 15)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool IsBuilding(Vector2 index)
        {
            return map[(int)index.X, (int)index.Y];
        }
    }
}
