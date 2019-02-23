// git submodule update --remote --force --recursive // to update engine repository

#region --- Using Directives ---

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.CompilerServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using cgimin.engine.object3d;
using cgimin.engine.texture;
using cgimin.engine.camera;
using cgimin.engine.light;
using cgimin.engine.material.normalmapping;
using cgimin.engine.material.cubereflectionnormal;
using cgimin.engine.material.normalmappingcubespecular;
using cgimin.engine.material.ambientdiffuse;
using static cgimin.engine.material.BaseMaterial;
using cgimin.engine.skybox;
using cgimin.engine.gui;

using Engine.cgimin.engine.octree;
using Engine.cgimin.engine.material.simpleblend;
using Engine.cgimin.engine.terrain;

using Blinkenlights.Splines;

#endregion --- Using Directives ---

namespace Examples.Tutorial
{
    public class CubeExample : GameWindow
    {
        private const int NUMBER_OF_OBJECTS = 500;

        // the objects we load
        private ObjLoaderObject3D cubeObject;
        private ObjLoaderObject3D smoothObject;
        private ObjLoaderObject3D torusObject;
        private ObjLoaderObject3D cornerObject;

        // our textur-IDs
        private int checkerColorTexture;
        private int blueMarbleColorTexture;
        private int primitiveColorTexture;

        // normal map textures
        private int brickNormalTexture;
        private int stoneNormalTexture;
        private int primitiveNormalTexture;

        // cubical environment reflection texture
        private int environmentCubeTexture;
        private int darkerEnvCubeTexture;

        // Materials
        private NormalMappingMaterial normalMappingMaterial;
        private CubeReflectionNormalMaterial cubeReflectionNormalMaterial;
        private NormalMappingCubeSpecularMaterial normalMappingCubeSpecularMaterial;
        private AmbientDiffuseSpecularMaterial ambientDiffuseSpecularMaterial;
        private SimpleBlendMaterial simpleBlendMaterial;

        // Octree
        private Octree octree;

        // Terrain
        //private Terrain terrain;

        // Skybox
        private SkyBox skyBox;

        // Font
        //private BitmapFont abelFont;

        // Bitmap Graphics
        //private List<BitmapGraphic> bitmapGraphics;

        // global update counter for animations etc.
        private int updateCounter = 0;

        public CubeExample()
            : base(1280, 720, new GraphicsMode(32, 24, 8, 2), "CGI-MIN Example", GameWindowFlags.Default, DisplayDevice.Default, 3, 0, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        { }

        private readonly double[][] trackPoints = {
            new []{-20.0, 0.0, 0.0},
            new []{-15.0, 0.0, -20.0},
            new []{-10.0, 20.0, -20.0},
            new []{-5.0, 20.0, 0.0},
            new []{0.0, 0.0, 0.0},
            new []{5.0, -20.0, 0.0},
            new []{10.0, -20.0, 20.0},
            new []{15.0, 0.0, 20.0},
            new []{20.0, 0.0, 0.0}
        };

        private const int trackDeg = 3;

        private readonly double[] trackKnots = new double[13]
        {
            0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 6, 6, 6
        };
        

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            // Initialize Camera
            Camera.Init();
            Camera.SetWidthHeightFov(1280, 720, 60);
            Camera.SetLookAt(new Vector3(0, 0, 3), new Vector3(0, 0, 0), Vector3.UnitY);

            // Initialize Light
            cgimin.engine.light.Light.SetDirectionalLight(new Vector3(1, -1, 2), new Vector4(0.3f, 0.3f, 0.3f, 0), new Vector4(0.8f, 0.8f, 0.8f, 0), new Vector4(1, 1, 1, 0));

            // Loading the object
            cubeObject = new ObjLoaderObject3D("data/objects/cube.obj", 0.8f, true);
            smoothObject = new ObjLoaderObject3D("data/objects/round_stone.obj", 0.3f, true);
            torusObject = new ObjLoaderObject3D("data/objects/torus_smooth.obj", 0.8f, true);
            cornerObject = new ObjLoaderObject3D("data/objects/cube.obj", 0.3f, true);
            

            // Loading color textures
            checkerColorTexture = TextureManager.LoadTexture("data/textures/b_checker.png");
            blueMarbleColorTexture = TextureManager.LoadTexture("data/textures/marble_blue.png");
            primitiveColorTexture = TextureManager.LoadTexture("data/textures/single_color.png");

            // Loading normal textures
            brickNormalTexture = TextureManager.LoadTexture("data/textures/brick_normal.png");
            stoneNormalTexture = TextureManager.LoadTexture("data/textures/stone_normal.png");
            primitiveNormalTexture = TextureManager.LoadTexture("data/textures/primitives_normal.png");
    
            // Load cube textures
            environmentCubeTexture = TextureManager.LoadCubemap(new List<string>{ "data/textures/env_reflect_left.png", "data/textures/env_reflect_right.png",
                                                                                  "data/textures/env_reflect_top.png",  "data/textures/env_reflect_bottom.png",
                                                                                  "data/textures/env_reflect_back.png", "data/textures/env_reflect_front.png"});

            darkerEnvCubeTexture = TextureManager.LoadCubemap(new List<string>{ "data/textures/cmap2_left.png", "data/textures/cmap2_right.png",
                                                                                "data/textures/cmap2_top.png",  "data/textures/cmap2_bottom.png",
                                                                                "data/textures/cmap2_back.png", "data/textures/cmap2_front.png"});

            // initialize material
            normalMappingMaterial = new NormalMappingMaterial();
            cubeReflectionNormalMaterial = new CubeReflectionNormalMaterial();
            normalMappingCubeSpecularMaterial = new NormalMappingCubeSpecularMaterial();
            ambientDiffuseSpecularMaterial = new AmbientDiffuseSpecularMaterial();
            simpleBlendMaterial = new SimpleBlendMaterial();

            // enebale z-buffer
            GL.Enable(EnableCap.DepthTest);

            // backface culling enabled
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            // set keyboard event
            this.KeyDown += new EventHandler<KeyboardKeyEventArgs>(KeyDownEvent);


            // init matrial settings

            // 'golden brick'
            MaterialSettings brickGoldSettings = new MaterialSettings();
            brickGoldSettings.colorTexture = checkerColorTexture;
            brickGoldSettings.normalTexture = brickNormalTexture;
            brickGoldSettings.shininess = 10.0f;

            // 'completely mirrored cube'
            MaterialSettings cubeReflectSettings = new MaterialSettings();
            cubeReflectSettings.cubeTexture = environmentCubeTexture;
            cubeReflectSettings.normalTexture = stoneNormalTexture;

            // 'blue shiny stone"
            MaterialSettings blueShinyStoneSettings = new MaterialSettings();
            blueShinyStoneSettings.colorTexture = blueMarbleColorTexture;
            blueShinyStoneSettings.normalTexture = stoneNormalTexture;
            blueShinyStoneSettings.cubeTexture = darkerEnvCubeTexture;

            // transparent blended material
            MaterialSettings blendMaterialSettings = new MaterialSettings();
            blendMaterialSettings.colorTexture = checkerColorTexture;
            blendMaterialSettings.SrcBlendFactor = BlendingFactor.SrcColor;
            blendMaterialSettings.DestBlendFactor = BlendingFactor.DstColor;
            
            // "primitive corner stone"
            MaterialSettings primitiveCornerSettings = new MaterialSettings();
            primitiveCornerSettings.colorTexture = primitiveColorTexture;
            primitiveCornerSettings.normalTexture = primitiveNormalTexture;
            primitiveCornerSettings.shininess = 10.0f;

            // Init Skybox
            skyBox = new SkyBox("data/skybox/neon_front.png", "data/skybox/neon_back.png", "data/skybox/neon_left.png", "data/skybox/neon_right.png", "data/skybox/neon_up.png", "data/skybox/neon_down.png");

            // Load Font
            //abelFont = new BitmapFont("data/fonts/abel_normal.fnt", "data/fonts/abel_normal.png");

            // Load Sprites
            /*
            bitmapGraphics = new List<BitmapGraphic>();
            int marioTexture = TextureManager.LoadTexture("data/textures/mario_sprite.png");
            for (int i = 0; i < 8; i++)
            {
                bitmapGraphics.Add(new BitmapGraphic(marioTexture, 512, 128, i * 64, 0, 64, 128));
            }
            */

            // Init Octree
            octree = new Octree(new Vector3(-30, -30, -30), new Vector3(30, 30, 30));

            // add cornerstones & center
            octree.AddEntity(new OctreeEntity(cornerObject, normalMappingMaterial, primitiveCornerSettings, Matrix4.CreateTranslation(   0.0f,   0.0f,   0.0f)));
            octree.AddEntity(new OctreeEntity(cornerObject, normalMappingMaterial, primitiveCornerSettings, Matrix4.CreateTranslation(  20.0f,  20.0f,  20.0f)));
            octree.AddEntity(new OctreeEntity(cornerObject, normalMappingMaterial, primitiveCornerSettings, Matrix4.CreateTranslation( -20.0f,  20.0f,  20.0f)));
            octree.AddEntity(new OctreeEntity(cornerObject, normalMappingMaterial, primitiveCornerSettings, Matrix4.CreateTranslation(  20.0f,  20.0f, -20.0f)));
            octree.AddEntity(new OctreeEntity(cornerObject, normalMappingMaterial, primitiveCornerSettings, Matrix4.CreateTranslation( -20.0f,  20.0f, -20.0f)));
            octree.AddEntity(new OctreeEntity(cornerObject, normalMappingMaterial, primitiveCornerSettings, Matrix4.CreateTranslation(  20.0f, -20.0f,  20.0f)));
            octree.AddEntity(new OctreeEntity(cornerObject, normalMappingMaterial, primitiveCornerSettings, Matrix4.CreateTranslation( -20.0f, -20.0f,  20.0f)));
            octree.AddEntity(new OctreeEntity(cornerObject, normalMappingMaterial, primitiveCornerSettings, Matrix4.CreateTranslation(  20.0f, -20.0f, -20.0f)));
            octree.AddEntity(new OctreeEntity(cornerObject, normalMappingMaterial, primitiveCornerSettings, Matrix4.CreateTranslation( -20.0f, -20.0f, -20.0f)));

            /*
            // generate random positions
            Random random = new Random();

            for (int i = 0; i < NUMBER_OF_OBJECTS; i++)
            {
                Matrix4 tranlatePos = Matrix4.CreateTranslation(random.Next(-200, 200) / 10.0f, random.Next(-200, 200) / 10.0f, random.Next(-200, 200) / 10.0f);

                int whichObject = random.Next(4);

                switch (whichObject)
                {
                    case 0:
                        octree.AddEntity(new OctreeEntity(smoothObject, normalMappingCubeSpecularMaterial, blueShinyStoneSettings, tranlatePos));
                        break;
                    case 1:
                        octree.AddEntity(new OctreeEntity(cubeObject, cubeReflectionNormalMaterial, cubeReflectSettings, tranlatePos));
                        break;
                    case 2:
                        octree.AddEntity(new OctreeEntity(torusObject, normalMappingMaterial, brickGoldSettings, tranlatePos));
                        break;
                    case 3:
                        octree.AddEntity(new OctreeEntity(cubeObject, simpleBlendMaterial, blendMaterialSettings, tranlatePos));
                        break;
                }
            }
            */

            // Init terrain
            //terrain = new Terrain();
            
            float trackLength = 0;
            float[] oldPoint = Array.ConvertAll(BSpline.Interpolate(0.0, trackDeg, trackPoints, trackKnots, null, null), input => (float) input);
            for (double t = 0; t <= 1; t += 0.001)
            {
                float[] point = Array.ConvertAll(BSpline.Interpolate(t, trackDeg, trackPoints, trackKnots, null, null), input => (float)input);
                octree.AddEntity(new OctreeEntity(smoothObject, normalMappingCubeSpecularMaterial, blueShinyStoneSettings, Matrix4.CreateTranslation(point[0], point[1], point[2])));
                // calculate length of vector form old to new point and add to trackLength
                trackLength += new Vector3(point[0] - oldPoint[0], point[1] - oldPoint[1], point[2] - oldPoint[2]).Length;
            }
            Console.WriteLine("[INFO]    trackLength: " + trackLength);

        }

        private void KeyDownEvent(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.F11)
            {
                if (WindowState != WindowState.Fullscreen)
                {
                    WindowState = WindowState.Fullscreen;
                }
                else
                {
                    WindowState = WindowState.Normal;
                }
            }

            if (e.Key == OpenTK.Input.Key.Escape) this.Exit();

        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            
            // TODO: Rhino.Geometry to add splines for camera movement

            // update the fly-cam with keyboard input
            Camera.UpdateFlyCamera(keyboardState[Key.Left], keyboardState[Key.Right], keyboardState[Key.Up], keyboardState[Key.Down],
                                   keyboardState[Key.W], keyboardState[Key.S]);

            // updateCounter simply increaes
            updateCounter++;
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // the screen and the depth-buffer are cleared
            GL.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            skyBox.Draw();
            
            octree.Draw();
            //terrain.Draw(blueMarbleColorTexture, 1014, blueMarbleColorTexture, stoneNormalTexture, 0.2f, 60);

            //bitmapGraphics[(updateCounter / 10) % 8].Draw((updateCounter * 2 % 1920) - 1920 * 0.5f, 100, 1);

            //abelFont.DrawString("Hallo, dies ist ein Text! Dargestellt mit der BitmapFont Klasse...", -700, -200,   255, 255, 255, 255);

            SwapBuffers();
        }


        protected override void OnUnload(EventArgs e)
        {
            cubeObject.UnLoad();
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
