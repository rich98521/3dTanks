using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FPSGame
{
    public class Camera
    {
        Vector3 pos;
        Vector3 dir;
        Vector3 up;
        Vector3 tankPos;
        Vector3 flyPos;

        float fieldOfView;
        float aspectRatio;
        float near;
        float far;
        float yaw;
        float pitch;
        float speed = 0.01f;
        float angVel = 0.0025f; // set max angular velocity for rotating camera
        int zoomStep = 0;
        bool zooming;
        float zoomPitchStep;
        float zoomStepMax = 32;
        float pitchStart;
        int[,] map;
        bool xDown;
        bool topView;

        Matrix view;
        Matrix projection;

        bool flying;

        public Matrix View
        {
            get { return view; }
            set { view = value; }
        }

        public bool Flying
        {
            get { return flying; }
            set { flying = value; }
        }

        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; }
        }

        public Vector3 Direction
        {
            get { return dir; }
            set { dir = value; }
        }

        private float ThirdPersonDistance()
        {
            float distance = 25;
            bool hit = false;
            Vector3 start = tankPos + pos;
            Vector3 direction = -dir;
            Vector2 mapIndex = new Vector2((int)start.X / 15, (int)start.Z / 15);
            direction.Normalize();
            Vector3 testPos;
            int xMin = Math.Max((int)mapIndex.X - 2, 0), xMax = Math.Min((int)mapIndex.X + 2, 9), yMin = Math.Max((int)mapIndex.Y - 2, 0), yMax = Math.Min((int)mapIndex.Y + 2, 9);
            for (int i = 0; i < distance+3; i++)
            {
                testPos = start + direction * i;
                for (int x = xMin; x < xMax + 1; x++)
                {
                    for (int y = yMin; y < yMax + 1; y++)
                    {   
                        int height = map[x, y] * 5;
                        if (testPos.Y < height)
                        {
                            if (testPos.X > x * 15 && testPos.X < x * 15 + 15 && testPos.Z > y * 15 && testPos.Z < y * 15 + 15)
                            {
                                hit = true;
                                start = start + direction * (i - 1);
                                distance = (direction * (i - 1)).Length();
                                for (int i2 = 0; i2 < 100; i2++)
                                {
                                    testPos = start + (direction / 100f) * i2;
                                    if (testPos.Y < height)
                                    {
                                        if (testPos.X > x * 15 && testPos.X < x * 15 + 15 && testPos.Z > y * 15 && testPos.Z < y * 15 + 15)
                                        {
                                            distance += ((direction / 100f) * (i2 - 1)).Length() - 3;
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    if (hit)
                        break;
                }
                if (hit)
                    break;
            }
            return distance;
        }

        public void Init(Vector3 p, Vector3 lookat, Vector3 u, float FOV, float ar, float n, float f, string[] m)
        {
            pos = p;
            dir = (lookat - p);
            dir.Normalize();
            up = u;
            map = new int[10, 10];

            fieldOfView = FOV;
            aspectRatio = ar;
            near = n;
            far = f;
            UpdateView();
            UpdateProj();
            for (int z = 0; z < m.Length; z++)
            {
                for (int x = 0; x < m[z].Length; x++)
                {
                    try
                    {
                        map[z, x] = Convert.ToInt32("" + m[x][z]);
                    }
                    catch { }
                }
            }
        }



        void UpdateView()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                view = Matrix.CreateLookAt(tankPos + new Vector3(0, 75, 0), tankPos+dir, up);
            else
            {
                if(!flying)
                dir *= ThirdPersonDistance();
                view = Matrix.CreateLookAt(pos + tankPos - dir * ((zoomStepMax - zoomStep) / zoomStepMax) + flyPos, pos + tankPos + ((dir / 2f) * (zoomStep / zoomStepMax)) + flyPos, up);
            }
        }
        void UpdateProj()
        {
            projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, near, far);
        }
        public void UpdatePos(Vector3 Tankposition)
        {
            tankPos = Tankposition;
        }

        public void Update(GameTime gameTime)
        {
            KeyboardState kb = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            if (kb.IsKeyDown(Keys.E))
            {
                speed = 0.02f;
            }
            else
            {
                speed = 0.05f;
            }

            yaw += (mouse.X - 400) / 200.0f;
            if (mouse.RightButton == ButtonState.Pressed)
            {
                if (!zooming)
                {
                    zoomPitchStep = (float)(Math.PI / 2 - pitch) / zoomStepMax;
                    pitchStart = pitch;
                    zooming = true;
                }
                if (zoomStep < zoomStepMax)
                {
                    zoomStep++;
                    pitch = pitchStart + zoomPitchStep * zoomStep;
                }
                else
                    pitch = (float)Math.PI / 2;
            }
            else
            {
                pitch += (mouse.Y - 240) / 200.0f;
                pitch = (float)Math.Max(Math.Min(pitch, Math.PI - 0.01), 0.01);
                zooming = false;
                if (zoomStep > 0)
                    zoomStep--;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.X))
            {
                if (!xDown)
                {
                    xDown = true;
                    flying = !flying;
                }
                else
                {
                    flyPos = new Vector3();
                }
            }
            else if (xDown && Keyboard.GetState().IsKeyUp(Keys.X))
                xDown = false;

            dir = new Vector3((float)(Math.Cos(yaw) * Math.Sin(pitch)), (float)Math.Cos(pitch), (float)(Math.Sin(yaw) * Math.Sin(pitch)));
            if (flying)
            {
                Vector3 right = Vector3.Cross(dir, up);
                right.Normalize();
                float distanceTravelled = speed * gameTime.ElapsedGameTime.Milliseconds;
                if (kb.IsKeyDown(Keys.A))
                {
                    flyPos -= right * distanceTravelled;
                }
                if (kb.IsKeyDown(Keys.D))
                {
                    flyPos += right * distanceTravelled;
                }
                if (kb.IsKeyDown(Keys.W))
                {
                    flyPos += dir * distanceTravelled;
                }
                if (kb.IsKeyDown(Keys.S))
                {
                    flyPos -= dir * distanceTravelled;
                }
            }
            Mouse.SetPosition(400, 240);
            UpdateView();
            UpdateProj();
        }
    }
}
