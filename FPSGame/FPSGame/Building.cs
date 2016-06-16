using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace FPSGame
{
    class BuildingFactory
    {
        public static String buildingHeights = "123456789";
        static Game theGame;
        static Rectangle blockSize;
        static Texture2D[] textures = new Texture2D[9];
        static Texture2D[] roofTextures = new Texture2D[4];
        public static Building makeBuilding(char c, Vector2 position)
        {
            Building b = new Building(theGame);
            int height = Convert.ToInt32("" + c);

            b.Texture = textures[height - 1];
            b.RoofTexture = roofTextures[3 - (height + 1) / 3];

            b.Position = new Vector3(position.X * blockSize.Width, 0, position.Y * blockSize.Height);
            b.Size = blockSize;
            b.Height = height * 5;
            b.Rotation = (float)Math.PI;
            b.Init();


            return b;
        }

        public static void Init(Game game, Rectangle bS)
        {
            theGame = game;
            LoadTextures();
            blockSize = bS;

        }

        static void LoadTextures()
        {

            ContentManager contentManger = (ContentManager)theGame.Services.GetService(typeof(ContentManager));
            for (int i = 0; i < 9; i++)
            {
                textures[i] = contentManger.Load<Texture2D>("building" + (i + 1));
            }
            for (int i = 0; i < 4; i++)
            {
                roofTextures[i] = contentManger.Load<Texture2D>("roof" + (i + 1));
            }


        }

    }

    class Building : GameObject
    {
        Texture2D texture;
        Texture2D roofTexture;

        float rotation;
        Matrix rotationMatrix;

        VertexPositionNormalTexture[] wallVertices;
        VertexPositionNormalTexture[] roofVertices;
        short[] indices;
        int numTriangles;
        int numVertices;

        public override void Init()
        {
            effect = new BasicEffect(graphicsDevice);
            effect.VertexColorEnabled = false;
            effect.TextureEnabled = true;


            numVertices = 4;
            wallVertices = new VertexPositionNormalTexture[numVertices];

            wallVertices[0].Position = new Vector3(Position.X, 0, Position.Z);
            wallVertices[0].TextureCoordinate = new Vector2(1, 1);

            wallVertices[1].Position = new Vector3(Position.X + size.Width, 0, Position.Z);
            wallVertices[1].TextureCoordinate = new Vector2(0, 1);

            wallVertices[2].Position = new Vector3(Position.X + size.Width, height, Position.Z);
            wallVertices[2].TextureCoordinate = new Vector2(0, 0);

            wallVertices[3].Position = new Vector3(Position.X, height, Position.Z);
            wallVertices[3].TextureCoordinate = new Vector2(1, 0);


            roofVertices = new VertexPositionNormalTexture[numVertices];

            roofVertices[0].Position = new Vector3(Position.X, height, Position.Z);
            roofVertices[0].TextureCoordinate = new Vector2(1, 1);

            roofVertices[1].Position = new Vector3(Position.X + size.Width, height, Position.Z);
            roofVertices[1].TextureCoordinate = new Vector2(0, 1);

            roofVertices[2].Position = new Vector3(Position.X + size.Width, height, Position.Z + size.Height);
            roofVertices[2].TextureCoordinate = new Vector2(0, 0);

            roofVertices[3].Position = new Vector3(Position.X, height, Position.Z + size.Height);
            roofVertices[3].TextureCoordinate = new Vector2(1, 0);

            numTriangles = 2;
            indices = new short[numTriangles + 2];

            int i = 0;
            indices[i++] = 0;
            indices[i++] = 1;
            indices[i++] = 3;
            indices[i++] = 2;


        }
        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        public Texture2D RoofTexture
        {
            get { return roofTexture; }
            set { roofTexture = value; }
        }

        Rectangle size;

        public Rectangle Size
        {
            get { return size; }
            set { size = value; }
        }
        int height;

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        public Building(Game game)
            : base(game)
        {


        }
        public override void Draw(GameTime gametime, Camera camera)
        {
            Vector3 centreDisplace = position + new Vector3(size.Width / 2.0f, 0, size.Height / 2.0f);
            effect.View = camera.View;
            effect.Projection = camera.Projection;
            effect.Texture = texture;

            for (int i = 0; i < 4; i++)
            {
                rotationMatrix = Matrix.CreateTranslation(-centreDisplace) * Matrix.CreateRotationY((float)(Math.PI / 2 * i)) * Matrix.CreateTranslation(centreDisplace);

                effect.World = rotationMatrix;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, wallVertices, 0, numVertices, indices, 0, numTriangles);

                }
            }

            effect.Texture = roofTexture;
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, roofVertices, 0, numVertices, indices, 0, numTriangles);

            }
        }
    }
}
