using System.Collections.Generic;
using cgimin.engine.material.simpletexture;
using cgimin.engine.object3d;
using cgimin.engine.texture;
using OpenTK;

namespace KesselRunGame.classes
{
    /// <summary>
    /// Skybox Class to generate and texturize a Skybox
    /// </summary>
    public class Skybox
    {
        /// <summary> 3D Object of the Skybox </summary>
        private BaseObject3D skyboxObject = new BaseObject3D();

        /// <summary> enum for the Skybox-Textures </summary>
        private enum Side:int
        {
            Left    = 0,
            Right   = 1,
            Bottom  = 2,
            Top     = 3,
            Back    = 4,
            Front   = 5
        }
        /// <summary> List of Skybox Textures </summary>
        private List<int> textures = new List<int>();
        
        /// <summary> Skybox Material </summary>
        private SimpleTextureMaterial material;

        public Skybox(float size, string SkyboxTextureFolder)
        {
            //      Y
            //      |
            //      2-------3
            //     /|      /|
            //    6-------7 |
            //    | 1-----|-4---X
            //    |/      |/
            //    5-------8
            //   /
            //  Z
            // List of Corner-Points
            List<Vector3> corners = new List<Vector3>
            {
                new Vector3(0.0f),  // 0
                new Vector3(0.0f, 0.0f, 0.0f),  // 1
                new Vector3(0.0f, size, 0.0f),  // 2
                new Vector3(size, size, 0.0f),  // 3
                new Vector3(size, 0.0f, 0.0f),  // 4
                new Vector3(0.0f, 0.0f, size),  // 5
                new Vector3(0.0f, size, size),  // 6
                new Vector3(size, size, size),  // 7
                new Vector3(size, 0.0f, size)   // 8
            };
            // Inverted Unit-Vectors
            Vector3 invX = new Vector3(-1.0f, 0.0f, 0.0f);
            Vector3 invY = new Vector3(0.0f, -1.0f, 0.0f);
            Vector3 invZ = new Vector3(0.0f, 0.0f, -1.0f);

            // setting Skybox-Faces  TODO: UV vectors
                // Left Face (-X)
            skyboxObject.addTriangle(corners[1], corners[2], corners[6], Vector3.UnitX, Vector3.UnitX, Vector3.UnitX, new Vector2(0.0f, 0.0f), Vector2.Zero, Vector2.Zero);  // 1-2-6
            skyboxObject.addTriangle(corners[1], corners[5], corners[6], Vector3.UnitX, Vector3.UnitX, Vector3.UnitX, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 1-5-6
                // Right Face (+X)
            skyboxObject.addTriangle(corners[8], corners[7], corners[3], invX, invX, invX, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 8-7-3
            skyboxObject.addTriangle(corners[3], corners[4], corners[8], invX, invX, invX, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 3-4-8
                // Bottom Face (-Y)
            skyboxObject.addTriangle(corners[1], corners[5], corners[8], Vector3.UnitY, Vector3.UnitY, Vector3.UnitY, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 1-5-8
            skyboxObject.addTriangle(corners[1], corners[4], corners[8], Vector3.UnitY, Vector3.UnitY, Vector3.UnitY, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 1-4-8
                // Top Face (+Y)
            skyboxObject.addTriangle(corners[2], corners[3], corners[7], invY, invY, invY, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 2-3-7
            skyboxObject.addTriangle(corners[2], corners[6], corners[7], invY, invY, invY, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 2-6-7
                // Back Face (-Z)
            skyboxObject.addTriangle(corners[1], corners[2], corners[3], Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 1-2-3
            skyboxObject.addTriangle(corners[3], corners[4], corners[1], Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 3-4-1
                // Front Face (+Z)
            skyboxObject.addTriangle(corners[5], corners[6], corners[7], invZ, invZ, invZ, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 5-6-7
            skyboxObject.addTriangle(corners[5], corners[8], corners[7], invZ, invZ, invZ, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 5-8-7

            skyboxObject.Transformation *= Matrix4.CreateTranslation(-(size / 2), -(size / 2), -(size / 2));
            

            // setting Skybox-Texture
            textures[(int)Side.Left]   = TextureManager.LoadTexture("data/textures/env_reflect_left.png");
            textures[(int)Side.Right]  = TextureManager.LoadTexture("data/textures/env_reflect_right.png");
            textures[(int)Side.Bottom] = TextureManager.LoadTexture("data/textures/env_reflect_bottom.png");
            textures[(int)Side.Top]    = TextureManager.LoadTexture("data/textures/env_reflect_top.png");
            textures[(int)Side.Back]   = TextureManager.LoadTexture("data/textures/env_reflect_back.png");
            textures[(int)Side.Front]  = TextureManager.LoadTexture("data/textures/env_reflect_front.png");
            
            // setting Skybox-Material
            material = new SimpleTextureMaterial();
        }
    }
}
