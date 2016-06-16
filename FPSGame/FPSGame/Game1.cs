using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace FPSGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D pixel;
        Enemy[] enemies = new Enemy[10];

        String[] map ={  
                    "╬═╦═╦════╗",
                    "║1║3║1111║",
                    "║2║4╠════╣",
                    "╠═╩═╣7777║",
                    "║555║7777║",
                    "╠═╦═╬════╣",
                    "║3║9║1111║",
                    "║3╚═╬═╗66║",
                    "║343╠═╝99║",
                    "╚═══╩════╝"          
        };

        String[] eMap ={  
                    "0000001000",
                    "0000000000",
                    "0000010001",
                    "0100000000",
                    "0000000000",
                    "0000100100",
                    "0000000000",
                    "0010000000",
                    "0000001000",
                    "0100000001"          
        };
        //String[] eMap ={  
        //            "0000001000",
        //            "0000000000",
        //            "0000000000",
        //            "0000000000",
        //            "0000000000",
        //            "0000000000",
        //            "0000000000",
        //            "0000000000",
        //            "0000000000",
        //            "0000000000"          
        //};

        List<Street> streets;
        List<Building> buildings;
        Rectangle blockSize;

        Camera camera;

        Player player;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Services.AddService(typeof(ContentManager), Content);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            blockSize = new Rectangle(0, 0, 15, 15);


            streets = new List<Street>();
            buildings = new List<Building>();

            StreetFactory.Init(this, blockSize);
            BuildingFactory.Init(this, blockSize);
            int enemyIndex = 0;
            bool[,] bMap = new bool[map.Length, map[0].Length];
            for (int z = 0; z < map.Length; z++)
            {
                for (int x = 0; x < map[z].Length; x++)
                {
                    char c = map[z][x];
                    if (StreetFactory.streetSymbols.Contains(c))
                    {
                        Street s = StreetFactory.makeStreet(map[z][x], new Vector2(x, z));
                        streets.Add(s);
                    }
                    else if (BuildingFactory.buildingHeights.Contains(c))
                    {
                        Building b = BuildingFactory.makeBuilding(map[z][x], new Vector2(x, z));
                        buildings.Add(b);
                        bMap[x, z] = true;
                    }
                }
            }
            CollisionManager.Initialize(bMap);
            for (int z = 0; z < map.Length; z++)
            {
                for (int x = 0; x < map[z].Length; x++)
                {
                    if (eMap[z][x] == '1')
                    {
                        Enemy enemy = new Enemy(this);
                        enemy.Init(new Vector3(x * 15 + (blockSize.Width - 7) / 2f, 2, z * 15 + (blockSize.Height - 4) / 2f), Vector3.Up);
                        enemies[enemyIndex++] = enemy;
                        CollisionManager.Instance().AddEnemy(enemy);
                    }
                }
            }


            camera = new Camera();
            camera.Init(new Vector3(0, 1, 0), new Vector3(50, 0, 50), Vector3.Up, 0.6f, graphics.GraphicsDevice.Viewport.AspectRatio, 1, 1000, map);
            player = new Player(this);
            player.Init(new Vector3((blockSize.Width - 7) / 2f, 2, (blockSize.Height - 4) / 2f), Vector3.Up, bMap);
            CollisionManager.Instance().AddPlayer(player);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            player.LoadContent(Content);
            foreach (Enemy e in enemies)
                e.LoadContent(Content);
            font = Content.Load<SpriteFont>("font");
            pixel = Content.Load<Texture2D>("Pixel");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            if (this.IsActive)
                camera.Update(gameTime);
            camera.UpdatePos(player.Position);
            if(!camera.Flying)
            player.Update(gameTime, camera.Direction);

            foreach (Enemy e in enemies)
                e.Update(gameTime, player.Position);
            CollisionManager.Instance().CheckCollisions();
            if (!player.Alive)
                this.Exit();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque; 
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp; // need to do this on reach devices to allow non 2^n textures
            RasterizerState rs = RasterizerState.CullNone;

            GraphicsDevice.RasterizerState = rs;
            // TODO: Add your drawing code here
            player.Draw(camera);

            foreach (Street s in streets)
            {
                s.Draw(gameTime, camera);
            }
            foreach (Building b in buildings)
            {
                b.Draw(gameTime, camera);
            }

            foreach (Enemy e in enemies)
                e.Draw(camera);


            spriteBatch.Begin();
            spriteBatch.Draw(pixel, new Rectangle(5, this.graphics.PreferredBackBufferHeight - 35, player.Health * 40, 30), Color.FromNonPremultiplied(255 - (int)((player.Health / 5f) * 255), (int)((player.Health / 5f) * 255), 0, 255));
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
