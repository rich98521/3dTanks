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
    class Enemy : GameObject
    {
        Vector3 position;
        Vector3 up;
        Vector3 playerLastSeenPos;
        bool playerVisible;
        bool moving;
        int shootDelay = 0;
        int shootInterval = 100;
        Rectangle size;
        float gunYaw, tankYaw;
        Model turret;
        Model tank;
        List<Missile> missiles = new List<Missile>();
        Texture2D metal;
        Model missile;
        List<Vector3> path = new List<Vector3>();
        float speed = 0.01f;
        float distanceToNextPoint;
        List<Point> open = new List<Point>();
        List<Point> closed = new List<Point>();
        Point[] steps = new Point[4] { new Point(0, 1), new Point(1, 0), new Point(0, -1), new Point(-1, 0), };
        bool pathFound;
        Point[,] parents;
        Point target;
        int pathSteps;
        Vector3 direction;
        bool alive = true;
        int health = 2;     

        public Enemy(Game game)
            : base(game)
        {


        }

        public Vector2[] Corners()
        {
            Vector2 centre = new Vector2(position.X, position.Z) + new Vector2(size.Width / 2f, size.Height / 2f);
            float angle = (float)Math.Atan(size.Height / (float)size.Width) + tankYaw;
            float angle2 = (float)(Math.Atan(size.Width / (float)size.Height) + tankYaw - Math.PI / 2);
            float length = (float)Math.Sqrt(Math.Pow(size.Height, 2) + Math.Pow(size.Width, 2)) / 2;
            Vector2[] corners = new Vector2[4];
            Vector2 cornerOffset1 = new Vector2((float)Math.Cos(angle) * length, (float)Math.Sin(angle) * length);
            Vector2 cornerOffset2 = new Vector2((float)Math.Cos(angle2) * length, (float)Math.Sin(angle2) * length);
            corners[0] = centre - cornerOffset1;
            corners[1] = centre - cornerOffset2;
            corners[2] = centre + cornerOffset1;
            corners[3] = centre + cornerOffset2;
            return corners;
        }

        public void Collision()
        {
            health--;
            if (health <= 0)
                alive = false;
        }

        public void Init(Vector3 p, Vector3 u)
        {
            position = p;
            up = u;
            size = new Rectangle(0, 0, 7, 4);
            effect = new BasicEffect(graphicsDevice);
            effect.VertexColorEnabled = false;
            effect.TextureEnabled = true;
        }

        public void Update(GameTime gameTime, Vector3 playerPos)
        {
            if (alive)
            {
                if (PathOpen(playerPos, Position))
                {
                    playerVisible = true;
                    playerLastSeenPos = playerPos;
                    moving = false;
                    Shooting(playerPos);
                }
                else
                {
                    shootDelay = 0;
                    if (playerVisible)
                    {
                        playerVisible = false;
                        Reset();
                        target = new Point((int)(playerPos.X / 7.5f), (int)(playerPos.Z / 7.5f));
                        CreatePath();
                        float distanceToNextPoint = Vector3.Distance(Position, path[0]);
                        if (distanceToNextPoint == 0)
                            return;
                        direction = path[0] - Position;
                        direction.Normalize();
                        direction.Y = 0;
                        moving = true;
                    }
                }
                if (moving)
                {
                    Move(gameTime);
                    // moving code here
                }

                for (int i = 0; i < missiles.Count; i++)
                {
                    if (!missiles[i].Alive())
                        missiles.RemoveAt(i);
                    else
                        missiles[i].Update();
                }
            }
        }

        public void LoadContent(ContentManager Content)
        {
            metal = Content.Load<Texture2D>("EnemyMetal");
            turret = Content.Load<Model>("Turret");
            tank = Content.Load<Model>("Tank");
            missile = Content.Load<Model>("Missile");
        }

        private int Direction(float ang1, float ang2)
        {
            float difference = Mod(ang1 - ang2, (float)Math.PI * 2);
            int direction = 0;
            if (difference != 0)
                direction = (int)(difference / Math.Abs(difference));
            if (difference >= Math.PI || difference <= -Math.PI)
                direction *= -1;
            return direction;
        }

        private float Mod(float x, float m)
        {
            float result = (x % m + m) % m;
            return result;
        }

        private void Move(GameTime gameTime)
        {
            float yawToPoint = (float)((Math.Atan2(path[0].Z - Position.Z, path[0].X - Position.X) + Math.PI * 2) % (Math.PI * 2));
            tankYaw = (float)((tankYaw + Math.PI * 2) % (Math.PI * 2));
            if (Math.Abs(yawToPoint - tankYaw) > 0.01)
            {

                int direction = Direction(yawToPoint, tankYaw);
                float difference = Math.Abs(tankYaw - yawToPoint);
                if (difference > Math.PI)
                    difference -= (float)Math.PI * 2;
                float yawSpeed = Math.Min(0.5f, Math.Abs(difference));

                tankYaw += yawSpeed / 20 * direction;
            }
            else
            {
                float distanceTravelled = speed * gameTime.ElapsedGameTime.Milliseconds;
                position += direction * distanceTravelled;
                Vector3 currentDir = path[0] - Position;
                float distance = Vector3.Distance(Position, path[0]);
                if (distance < 0.01 || (currentDir.X < 0 != direction.X < 0) || (currentDir.Z < 0 != direction.Z < 0))
                {
                    //position = path[0] - new Vector3(3.5f, 2, 2);
                    path.RemoveAt(0);
                    if (path.Count > 0)
                    {
                        distanceToNextPoint = Vector3.Distance(Position, path[0]);
                        direction = path[0] - Position;
                        direction.Normalize();
                        direction.Y = 0;
                    }
                    else
                        moving = false;
                }
            }
        }

        public void Shooting(Vector3 playerPos)
        {
            float yawToPlayer = (float)((Math.Atan2(playerPos.Z - Position.Z, playerPos.X - Position.X) + Math.PI * 2) % (Math.PI * 2));
            gunYaw = (float)((gunYaw + Math.PI * 2) % (Math.PI * 2));

            int direction = Direction(yawToPlayer, gunYaw);
            float difference = Math.Abs(gunYaw - yawToPlayer);
            if (difference > Math.PI)
                difference -= (float)Math.PI * 2;
            float yawSpeed = Math.Min(0.5f, Math.Abs(difference));

            gunYaw += yawSpeed / 20 * direction;


            if (Math.Abs(gunYaw - yawToPlayer) < 0.5f)
            {
                shootDelay++;
                if (shootDelay > shootInterval)
                {
                    Shoot();
                }
            }
        }

        public bool Alive
        {
            get { return alive; }
        }

        public void Shoot()
        {
            Missile msl = new Missile(position + new Vector3(size.Width / 2f, 1.2f, size.Height / 2f), gunYaw, missile, metal);
            missiles.Add(msl);
            CollisionManager.Instance().AddMissile(msl);
            shootDelay = 0;
        }

        public void CreatePath()
        {
            Reset();
            while (!pathFound)
                CalcOpen();
            BuildPath();
            //path.Add(new Vector3(0, 0, 0));
            //path.Add(new Vector3(15, 0, 0));
            //path.Add(new Vector3(15, 0, 15));
            //path.Add(new Vector3(0, 0, 15));
            //path.Add(new Vector3(0, 0, 0));
        }

        public void Reset()
        {
            open.Clear();
            closed.Clear();
            path.Clear();
            pathSteps = 0;
            open.Add(new Point((int)(Position.X / 7.5f), (int)(Position.Z / 7.5f)));
            pathFound = false;
            parents = new Point[20, 20];
        }

        public void CalcOpen()
        {
            pathSteps++;
            Point start = new Point((int)(Position.X / 7.5f), (int)(Position.Z / 7.5f));
            int openCount = open.Count;
            CollisionManager cm = CollisionManager.Instance();
            for (int i = 0; i < openCount; i++)
            {
                foreach (Point step in steps)
                {
                    int tempX = open[i].X + step.X;
                    int tempY = open[i].Y + step.Y;
                    if (!closed.Contains(new Point(tempX, tempY)) && tempX >= 0 && tempX < 20 && tempY >= 0 && tempY < 20 && !cm.IsBuilding(new Vector2(tempX / 2, tempY / 2)))
                    {
                        if (!(steps.Length == 8 && ((step.X + step.Y) % 2 == 0) && (!cm.IsBuilding(new Vector2(tempX/2, open[i].Y/2)) || !cm.IsBuilding(new Vector2(open[i].X/2, tempY/2)))))
                        {
                            if (new Point(tempX, tempY) == target)
                            {
                                pathFound = true;
                                parents[tempX, tempY] = open[i];
                                break;
                            }
                            if (!open.Contains(new Point(tempX, tempY)))
                            {
                                parents[tempX, tempY] = open[i];
                                open.Add(new Point(tempX, tempY));
                            }
                        }
                    }
                }
                if (pathFound)
                {
                    open.Clear();
                    openCount = 0;
                    break;
                }
            }
            closed.AddRange(open.GetRange(0, openCount));
            open.RemoveRange(0, openCount);
            //open = ListSort(open);
        }

        public void BuildPath()
        {

            //use line intersection instead of ray acsting to check clear
            Point tempEnd = target;
            Point tempStart = target;
            for (int i = 0; i < pathSteps; i++)
            {
                Point parent = parents[tempEnd.X, tempEnd.Y];
                if (PathOpen(new Vector3(parent.X,0,parent.Y), new Vector3(tempStart.X,0,tempStart.Y)))
                {
                    path.Add(new Vector3(tempEnd.X * 7.5f + 3.75f, 0, tempEnd.Y * 7.5f + 3.75f));
                    tempStart = tempEnd;
                }
                tempEnd = parents[tempEnd.X, tempEnd.Y];
            }
            path.Reverse();
            //path.Add(new Vector3(start.X, 0, start.Z));
        }

        public bool PathOpen(Vector3 start, Vector3 end)
        {
            bool ans = true;
            Vector3 direction = end - start;
            direction.Normalize();
            float dist = Vector3.Distance(end, start);
            int intvervals = (int)(dist / 0.1);
            for (int i = 0; i <= intvervals; i++)
            {
                Vector3 temp =start+ (direction/10) * i;
                if (CollisionManager.Instance().BuildingCollision(temp))
                {
                    ans = false;
                    break;
                }
            }
            return ans;
        }

        public void Draw(Camera camera)
        {
            if (alive)
            {
                Vector3 centreDisplace = new Vector3(size.Width / 2.0f, 0, size.Height / 2.0f);
                Matrix rotation = Matrix.CreateRotationX((float)-Math.PI / 2) * Matrix.CreateRotationY(-gunYaw - (float)Math.PI / 2);
                Matrix pos = Matrix.CreateTranslation(position + new Vector3(size.Width / 2f, 0.2f, size.Height / 2));
                foreach (ModelMesh mesh in turret.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.TextureEnabled = true;
                        effect.Texture = metal;
                        effect.View = camera.View;
                        effect.Projection = camera.Projection;
                        effect.World = rotation * pos;
                    }
                    mesh.Draw();
                }
                rotation = Matrix.CreateRotationX((float)-Math.PI / 2) * Matrix.CreateRotationY(-tankYaw);
                pos = Matrix.CreateTranslation(position + new Vector3(size.Width / 2f, -1.1f, size.Height / 2f));
                foreach (ModelMesh mesh in tank.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.TextureEnabled = true;
                        effect.Texture = metal;
                        effect.View = camera.View;
                        effect.Projection = camera.Projection;
                        effect.World = rotation * pos;
                    }
                    mesh.Draw();
                }
                foreach (Missile msl in missiles)
                    msl.Draw(camera);
            }
        }

        public Vector3 Position
        {
            get { return position + new Vector3(size.Width / 2.0f, -2, size.Height / 2.0f); }
        }
    }
}
