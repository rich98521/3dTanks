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
    class Player : GameObject
    {
        Vector3 position;
        Vector3 up;
        float tankYaw;
        float gunYaw;
        float tankYawSpeed = 0.1f;
        float speed = 0.01f;
        Texture2D metal;
        Rectangle size;
        float camYaw;
        bool[,] map;
        float newYaw;
        Vector3 newDir;
        Model turret, tank;
        bool mDown;
        List<Missile> missiles = new List<Missile>();
        int shootDelay;
        Model missile;
        int health = 5;
        bool alive = true;

        public Player(Game game)
            : base(game)
        {


        }

        public void Collision()
        {
            health--;
            if (health <= 0)
            {
                alive = false;
            }
        }

        public bool Alive
        {
            get { return alive; }
        }

        public int Health
        {
            get { return health; }
        }

        public void Init(Vector3 p, Vector3 u, bool[,] m)
        {
            up = u;
            position = p;
            map = m;
            size = new Rectangle(0, 0, 7, 4);
            
            effect = new BasicEffect(graphicsDevice);
            effect.VertexColorEnabled = false;
            effect.TextureEnabled = true;
        }
        public void LoadContent(ContentManager Content)
        {
            metal = Content.Load<Texture2D>("Gun");
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

        public void Update(GameTime gameTime, Vector3 camDir)
        {
            MouseState mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed && !mDown && shootDelay >= 50)
            {
                Shoot();
                mDown = true;
            }
            if (mouse.LeftButton == ButtonState.Released && mDown)
                mDown = false;
            if (shootDelay < 50)
                shootDelay++;

            for (int i = 0; i < missiles.Count; i++)
            {
                if (!missiles[i].Alive())
                    missiles.RemoveAt(i);
                else
                    missiles[i].Update();
            }

            KeyboardState kb = Keyboard.GetState();
            float distanceTravelled = speed * gameTime.ElapsedGameTime.Milliseconds;
            if (!(Mouse.GetState().MiddleButton == ButtonState.Pressed))
            camYaw = (float)((Math.Atan2(camDir.Z, camDir.X) + Math.PI * 2) % (Math.PI * 2));
            gunYaw = (float)((gunYaw + Math.PI * 2) % (Math.PI * 2));

            int direction = Direction(camYaw, gunYaw);
            float difference = Math.Abs(gunYaw - camYaw);
            if (difference > Math.PI)
                difference -= (float)Math.PI * 2;
            float yawSpeed = Math.Min(0.5f, Math.Abs(difference));

            gunYaw += yawSpeed / 20 * direction;
            int rotate = 0;
            int move = 0;
            Vector3 dir = new Vector3((float)(Math.Cos(tankYaw)), 0, (float)(Math.Sin(tankYaw)));
            if (kb.IsKeyDown(Keys.A))
            {
                rotate = -1;
            }
            else if (kb.IsKeyDown(Keys.D))
            {
                rotate = 1;
            }
            if (kb.IsKeyDown(Keys.W))
            {
                move = 1;
            }
            else if (kb.IsKeyDown(Keys.S))
            {
                move = -1;
            }
            newDir = dir * distanceTravelled * move;
            newYaw = (tankYawSpeed * distanceTravelled * rotate);
            CheckCollision(tankCorners(newDir, newYaw));
            tankYaw += newYaw;
            position += newDir;
        }

        private Vector2[] tankCorners(Vector3 dir, float yaw)
        {
            Vector2 centre = new Vector2(position.X, position.Z) + new Vector2(dir.X, dir.Z) + new Vector2(size.Width / 2f, size.Height / 2f);
            float angle = (float)Math.Atan(size.Height / (float)size.Width) + tankYaw + yaw;
            float angle2 = (float)(Math.Atan(size.Width / (float)size.Height) + tankYaw + yaw - Math.PI / 2);
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

        public Vector2[] Corners()
        {
            Vector2 centre = new Vector2(position.X, position.Z) + new Vector2(size.Width / 2f, size.Height / 2f);
            float angle = (float)Math.Atan(size.Height / (float)size.Width) + tankYaw ;
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

        public float GunYaw
        {
            get { return gunYaw; }
        }
        public float CamYaw
        {
            get { return camYaw; }
        }

        public void Shoot()
        {
            Missile msl = new Missile(position + new Vector3(size.Width / 2f, 1.2f, size.Height / 2f), gunYaw, missile, metal);
            missiles.Add(msl);
            CollisionManager.Instance().AddMissile(msl);
            shootDelay = 0;
        }

        public void Draw(Camera camera)
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

        private void CheckCollision(Vector2[] tankCornersNew)
        {
            Vector2 mapIndex = new Vector2((int)((position.X + size.Width / 2f) / 15), (int)((position.Z + size.Height / 2f) / 15));
            int xMin = Math.Max((int)mapIndex.X - 1, 0), xMax = Math.Min((int)mapIndex.X + 1, 9), yMin = Math.Max((int)mapIndex.Y - 1, 0), yMax = Math.Min((int)mapIndex.Y + 1, 9);
            for (int i = 0; i < 4; i++)
            {
                if (!(tankCornersNew[i].X > 0 && tankCornersNew[i].Y > 0 && tankCornersNew[i].X < 150 && tankCornersNew[i].Y < 150))
                {
                    newDir = new Vector3();
                    newYaw = 0;
                    return;
                }
            }
            for (int x = xMin; x < xMax+1; x++)
            {
                for (int y = yMin; y < yMax+1; y++)
                {
                    if (map[x, y])
                    {
                        Vector3[] RotBuildingCorners = new Vector3[]
                        { 
                            RotatedVec(new Vector3(x * 15, y * 15, 0), -tankYaw),
                            RotatedVec(new Vector3(x * 15 + 15, y * 15 + 15, 0), -tankYaw),
                            RotatedVec(new Vector3(x * 15, y * 15+15, 0), -tankYaw),
                            RotatedVec(new Vector3(x * 15 + 15, y * 15, 0), -tankYaw)};
                        for (int i = 0; i < 4; i++)
                        {
                            if (tankCornersNew[i].X >= x * 15 
                                && tankCornersNew[i].X < x * 15 + 15
                                && tankCornersNew[i].Y >= y * 15
                                && tankCornersNew[i].Y < y * 15 + 15)
                            {
                                newDir = new Vector3();
                                newYaw = 0;
                                break;
                            }
                            Vector3 rotatedTankCorner1 = RotatedVec(new Vector3(tankCornersNew[0], 0), -tankYaw);
                            Vector3 rotatedTankCorner2 = RotatedVec(new Vector3(tankCornersNew[2], 0), -tankYaw);
                            if (RotBuildingCorners[i].X >= rotatedTankCorner1.X
                                && RotBuildingCorners[i].X < rotatedTankCorner2.X
                                && RotBuildingCorners[i].Y >= rotatedTankCorner1.Y
                                && RotBuildingCorners[i].Y < rotatedTankCorner2.Y)
                            {
                                newDir = new Vector3();
                                newYaw = 0;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private Vector3 RotatedVec(Vector3 vec, float yaw)
        {
            return (Matrix.CreateTranslation(vec) * Matrix.CreateRotationZ(yaw)).Translation;
        }

        public Vector3 Position
        {
            get { return position + new Vector3(size.Width / 2.0f, 0, size.Height / 2.0f); }
        }
    }
}
