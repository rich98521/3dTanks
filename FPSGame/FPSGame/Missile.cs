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
    class Missile
    {
        Vector3 position;
        float yaw;
        Vector3 dir;
        float speed = 0.5f;
        Model model;
        bool alive = true;
        Texture2D metal;

        public Missile(Vector3 startPos, float angle, Model mdl, Texture2D tex)
        {
            dir = new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle));
            position = startPos + dir * 5;
            yaw = angle;
            model = mdl;
            metal = tex;
        }

        public bool Alive()
        {
            return alive;
        }

        public void Collision()
        {
            alive = false;
        }

        public void Update()
        {
            if (alive)
            {
                alive = !CollisionManager.Instance().BuildingCollision(position);
                position += dir * speed;
            }
        }

        public void Draw(Camera camera)
        {
            Matrix rotation = Matrix.CreateRotationX((float)-Math.PI / 2) * Matrix.CreateRotationY(-yaw - (float)Math.PI / 2);
            Matrix pos = Matrix.CreateTranslation(position - new Vector3(0, 1.1f, 0));
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.TextureEnabled = true;
                    effect.Texture = metal;
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                    effect.World = Matrix.CreateScale(0.5f) * rotation * pos;

                }
                mesh.Draw();
            }
        }

        public Vector3 Position
        {
            get { return position; }
        }
    }
}
