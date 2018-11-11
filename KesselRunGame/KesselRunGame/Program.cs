using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using cgimin.engine.object3d;
using cgimin.engine.texture;
using cgimin.engine.material.simpletexture;
using cgimin.engine.material.wobble1;
using cgimin.engine.material.simplereflection;
using cgimin.engine.camera;
using cgimin.engine.material.ambientdiffuse;
using cgimin.engine.light;
using cgimin.engine.material.zbuffershader;
// ReSharper disable All

namespace KesselRunGame
{
    /*
    class Program
    {
        static void Main(string[] args)
        {

        }
    }
    */
    public class CubeExample : GameWindow
    {
        // enum for camera switch
        private enum CameraMode : int
        {
            Corner,
            Net,
            TopView,
            AroundBall
        }

        // camera mode
        private CameraMode cameraMode;

        // Constants
        private const float BALL_RADIUS = 0.01f;
        private const float FIELD_X_BORDER = 2.65835f;
        private const float FIELD_Z_BORDER = 1.39379f;
        private const float GRAVITY = 0.0004f;
        private const float ENERGY_LOSS_ON_BOTTOM = .99f;

        // the objects we load
        private ObjLoaderObject3D tennisBallObject;
        private ObjLoaderObject3D tennisArenaObject;

        // our textur-IDs
        private int tennisBallTexture;
        private int tennisArenaTexture;
        private int shadowTexture;

        // Materials
        private AmbientDiffuseSpecularMaterial ambientDiffuseSpecularMaterial;
        private AmbientDiffuseMaterial ambientDiffuseMaterial;
        private SimpleTextureMaterial simpleTextureMaterial;
        private ZBufferMaterial zBufferMaterial;

        // the ball coordinates
        private float ballPositionX;
        private float ballPositionY;
        private float ballPositionZ;

        private float ballDirectionX;
        private float ballDirectionZ;
        private float ballYVelocity;

        private int updateCounter = 0;

        public CubeExample()
            : base(1280, 720, new GraphicsMode(32, 24, 8, 2), "CGI-MIN Example", GameWindowFlags.Default, DisplayDevice.Default, 3, 0, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        {
            this.KeyDown += KeyboardKeyDown;
        }

        void KeyboardKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.Exit();

            if (e.Key == Key.F11)
                if (WindowState != WindowState.Fullscreen)
                    WindowState = WindowState.Fullscreen;
                else
                    WindowState = WindowState.Normal;

            if (e.Key == Key.Number1) cameraMode = CameraMode.Corner;
            if (e.Key == Key.Number2) cameraMode = CameraMode.TopView;
            if (e.Key == Key.Number3) cameraMode = CameraMode.Net;
            if (e.Key == Key.Number4) cameraMode = CameraMode.AroundBall;

        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Initialize Camera
            Camera.Init();
            Camera.SetWidthHeightFov(800, 600, 60);

            // Initialize Light
            Light.SetDirectionalLight(new Vector3(1.0f, 1.0f, 0), new Vector4(0.3f, 0.3f, 0.3f, 0), new Vector4(1, 1, 1, 0), new Vector4(1, 1, 1, 0));

            // Loading the object
            tennisBallObject = new ObjLoaderObject3D("data/objects/tennis_ball.obj");
            tennisArenaObject = new ObjLoaderObject3D("data/objects/tennis_arena.obj");

            // Loading the textures
            tennisBallTexture = TextureManager.LoadTexture("data/textures/tennis_ball.png");
            tennisArenaTexture = TextureManager.LoadTexture("data/textures/tennis_field.png");
            shadowTexture = TextureManager.LoadTexture("data/textures/shadow_color.png");

            // initialize material
            ambientDiffuseSpecularMaterial = new AmbientDiffuseSpecularMaterial();
            ambientDiffuseMaterial = new AmbientDiffuseMaterial();
            simpleTextureMaterial = new SimpleTextureMaterial();
            zBufferMaterial = new ZBufferMaterial();

            // enebale z-buffer
            GL.Enable(EnableCap.DepthTest);

            // backface culling enabled
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            // initial ball values
            ballPositionX = 0.0f;
            ballPositionY = 0.5f;
            ballPositionZ = 0.0f;

            // the initial direction
            ballDirectionX = 0.02f;
            ballDirectionZ = 0.01f;

            // initial camera
            cameraMode = CameraMode.AroundBall;
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // updateCounter simply increaes
            updateCounter++;


            // ---------------------------------------------
            // update the ball (fake and simplified physics)
            // ---------------------------------------------

            // first the x and z position
            ballPositionX += ballDirectionX;
            ballPositionZ += ballDirectionZ;

            if (ballPositionX > FIELD_X_BORDER - BALL_RADIUS) ballDirectionX = -Math.Abs(ballDirectionX);
            if (ballPositionX < -FIELD_X_BORDER + BALL_RADIUS) ballDirectionX = Math.Abs(ballDirectionX);

            if (ballPositionZ > FIELD_Z_BORDER - BALL_RADIUS) ballDirectionZ = -Math.Abs(ballDirectionZ);
            if (ballPositionZ < -FIELD_Z_BORDER + BALL_RADIUS) ballDirectionZ = Math.Abs(ballDirectionZ);

            // y-position affected by gravity
            ballPositionY -= ballYVelocity;
            ballYVelocity += GRAVITY;

            if (ballPositionY < BALL_RADIUS)
            {
                ballYVelocity = -Math.Abs(ballYVelocity) * ENERGY_LOSS_ON_BOTTOM; // velocity always moving ball up, some kinetic energy lost so multiplied by ENERGY_LOSS_ON_BOTTOM 
                ballPositionY = BALL_RADIUS;
            }


            // ---------------------------------------------
            // set the camera, depending on state
            // ---------------------------------------------
            switch (cameraMode)
            {
                case CameraMode.Corner:
                    Camera.SetLookAt(new Vector3(1, 1, 1), new Vector3(ballPositionX, ballPositionY, ballPositionZ), Vector3.UnitY);
                    break;

                case CameraMode.TopView:
                    Camera.SetLookAt(new Vector3(0, 3, 0), new Vector3(0, 0, 0), Vector3.UnitZ);
                    break;

                case CameraMode.Net:
                    Camera.SetLookAt(new Vector3(0, 1, 2), new Vector3(ballPositionX, ballPositionY, ballPositionZ), Vector3.UnitY);
                    break;

                case CameraMode.AroundBall:
                    Camera.SetLookAt(new Vector3(ballPositionX + (float)Math.Sin(updateCounter * 0.01f) * 0.2f, ballPositionY, ballPositionZ + (float)Math.Cos(updateCounter * 0.01f) * 0.2f),
                                     new Vector3(ballPositionX, ballPositionY, ballPositionZ), Vector3.UnitY);
                    break;
            }





        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // the screen and the depth-buffer are cleared
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // ----------------------------------------------------------------------
            // draw the arena
            // ----------------------------------------------------------------------
            ambientDiffuseSpecularMaterial.Draw(tennisArenaObject, tennisArenaTexture, 30.0f);


            // ----------------------------------------------------------------------
            // calculate ball's transformation matrix and draw the ball
            // ----------------------------------------------------------------------

            // reset the ball's transformation matrix
            tennisBallObject.Transformation = Matrix4.Identity;

            // first scale the ball's matrix
            tennisBallObject.Transformation *= Matrix4.CreateScale(BALL_RADIUS, BALL_RADIUS, BALL_RADIUS);

            // rotation of object, around x-axis
            tennisBallObject.Transformation *= Matrix4.CreateRotationX(updateCounter / 20.0f);

            // around y-axis
            tennisBallObject.Transformation *= Matrix4.CreateRotationY(updateCounter / 10.0f);

            // set the balls translation
            tennisBallObject.Transformation *= Matrix4.CreateTranslation(ballPositionX, ballPositionY, ballPositionZ);

            // draw the ball
            ambientDiffuseMaterial.Draw(tennisBallObject, tennisBallTexture);


            // ----------------------------------------------------------------------
            // calculate shadow matrix unsing the ball object to draw a fake shadow
            // ----------------------------------------------------------------------

            // reset the ball shadows transformation matrix
            tennisBallObject.Transformation = Matrix4.Identity;

            // first scale the ball shadow matrix, y is 0 so the ball is flat
            tennisBallObject.Transformation *= Matrix4.CreateScale(BALL_RADIUS, 0, BALL_RADIUS);

            // set the ball shadows translation, y is constantly on the bottom (0.001f).
            tennisBallObject.Transformation *= Matrix4.CreateTranslation(ballPositionX, 0.001f, ballPositionZ);

            //tennisBallObject.Transformation *= Matrix4.CreateTranslation(ballPositionX - Light.lightDirection.X * ballPositionY, 0.001f, ballPositionZ - Light.lightDirection.Z * ballPositionY);

            // draw the ball shadow
            simpleTextureMaterial.Draw(tennisBallObject, shadowTexture);


            SwapBuffers();
        }



        protected override void OnUnload(EventArgs e)
        {
            tennisBallObject.UnLoad();
        }


        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            Camera.SetWidthHeightFov(Width, Height, 60);
        }


        [STAThread]
        public static void Main()
        {
            using (CubeExample example = new CubeExample())
            {
                example.Run(60.0, 60.0);
            }
        }


    }
}

/***************************************************
/ git submodule update --recursive --remote --force
/ to update engine
****************************************************/
