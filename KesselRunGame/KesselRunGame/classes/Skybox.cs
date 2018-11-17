using System;
using System.Collections.Generic;
using cgimin.engine.material.simpletexture;
using cgimin.engine.object3d;
using cgimin.engine.texture;
using OpenTK;
using OpenTK.Graphics.ES11;

namespace KesselRunGame.classes
{
    /// <summary>
    /// Skybox Class to generate and texturize a Skybox
    /// </summary>
    public class Skybox
    {
        /// <summary> 3D Object of the Skybox </summary>
        private BaseObject3D skyboxObject;

        /// <summary> Skybox Texture </summary>
        private int texture = 0;
        
        /// <summary> Skybox Material </summary>
        private SimpleTextureMaterial material;

        /// <summary>
        /// Constructor to generate a new Skybox
        /// </summary>
        /// <param name="size">Dimensions of the Skybox</param>
        /// <param name="SkyboxTexturePath">Path to the Skybox Texture</param>
        public Skybox(float size = 100.0f, string SkyboxTexturePath = "data/textures/cubemapTemplate.png")
        {
            // Initialization
            skyboxObject = new BaseObject3D();
            texture = 0;
            material = new SimpleTextureMaterial();
            
            // List of Corner-Points
            //
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
            //
            Vector3[] corners =
            {
                new Vector3(0.0f),
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(0.0f, size, 0.0f),
                new Vector3(size, size, 0.0f),
                new Vector3(size, 0.0f, 0.0f),
                new Vector3(0.0f, 0.0f, size),
                new Vector3(0.0f, size, size),
                new Vector3(size, size, size),
                new Vector3(size, 0.0f, size)
            };

            // setting Skybox-Faces
            //
            //       2----3           - 3/3
            //       | +Y |                    2------3    3
            //  2----6----7----3----2 - 2/3    |    _/   _/|
            //  | -X | +Z | +X | -Z |          |  _/   _/  |
            //  1----5----8----4----1 - 1/3    |_/   _/    |
            //       | -Y |                    1    1------2
            //       1----4           - 0/3
            //  |    |    |    |    |
            // 0/4  1/4  2/4  3/4  4/4
            //
            // Left Face (-X) TODO: System.NullReferenceException: Object reference not set to an instance of an object.
            skyboxObject.addTriangle(corners[1], corners[2], corners[6], Vector3.UnitX, Vector3.UnitX, Vector3.UnitX, new Vector2(0/4, 1/3), new Vector2(0/4, 2/3), new Vector2(1/4, 2/3));  // 1-2-6
            skyboxObject.addTriangle(corners[1], corners[5], corners[6], Vector3.UnitX, Vector3.UnitX, Vector3.UnitX, new Vector2(0/4, 1/3), new Vector2(1/4, 1/3), new Vector2(1/4, 2/3));  // 1-5-6
            // Right Face (+X)
            skyboxObject.addTriangle(corners[8], corners[7], corners[3], -Vector3.UnitX, -Vector3.UnitX, -Vector3.UnitX, new Vector2(2/4, 1/3), new Vector2(2/4, 2/3), new Vector2(3/4, 2/3));  // 8-7-3
            skyboxObject.addTriangle(corners[8], corners[4], corners[3], -Vector3.UnitX, -Vector3.UnitX, -Vector3.UnitX, new Vector2(2/4, 1/3), new Vector2(3/4, 1/3), new Vector2(3/4, 2/3));  // 8-4-3
            // Bottom Face (-Y)
            skyboxObject.addTriangle(corners[1], corners[5], corners[8], Vector3.UnitY, Vector3.UnitY, Vector3.UnitY, new Vector2(1/4, 0/3), new Vector2(1/4, 1/3), new Vector2(2/4, 1/3));  // 1-5-8
            skyboxObject.addTriangle(corners[1], corners[4], corners[8], Vector3.UnitY, Vector3.UnitY, Vector3.UnitY, new Vector2(1/4, 0/3), new Vector2(2/4, 0/3), new Vector2(2/4, 1/3));  // 1-4-8
            // Top Face (+Y)
            skyboxObject.addTriangle(corners[6], corners[2], corners[3], -Vector3.UnitY, -Vector3.UnitY, -Vector3.UnitY, new Vector2(1/4, 2/3), new Vector2(1/4, 3/3), new Vector2(2/4, 3/3));  // 6-2-3
            skyboxObject.addTriangle(corners[6], corners[7], corners[3], -Vector3.UnitY, -Vector3.UnitY, -Vector3.UnitY, new Vector2(1/4, 2/3), new Vector2(2/4, 2/3), new Vector2(2/4, 3/3));  // 6-7-3
            // Back Face (-Z)
            skyboxObject.addTriangle(corners[4], corners[3], corners[2], Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ, new Vector2(3/4, 1/3), new Vector2(3/4, 2/3), new Vector2(4/4, 2/3));  // 4-3-2
            skyboxObject.addTriangle(corners[4], corners[1], corners[2], Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ, new Vector2(3/4, 1/3), new Vector2(4/4, 1/3), new Vector2(4/4, 2/3));  // 4-1-2
            // Front Face (+Z)
            skyboxObject.addTriangle(corners[5], corners[6], corners[7], -Vector3.UnitZ, -Vector3.UnitZ, -Vector3.UnitZ, new Vector2(1/4, 1/3), new Vector2(1/4, 2/3), new Vector2(2/4, 2/3));  // 5-6-7
            skyboxObject.addTriangle(corners[5], corners[8], corners[7], -Vector3.UnitZ, -Vector3.UnitZ, -Vector3.UnitZ, new Vector2(1/4, 1/3), new Vector2(2/4, 1/3), new Vector2(2/4, 2/3));  // 5-8-7
            // Move Origin to Cube Center
            skyboxObject.Transformation *= Matrix4.CreateTranslation(-(size / 2), -(size / 2), -(size / 2));
            
            
            // ----------------------
            // setting Skybox-Texture
            texture = TextureManager.LoadTexture(SkyboxTexturePath);
            
            // -----------------------
            // setting Skybox-Material
            material = new SimpleTextureMaterial();
        }
        
        /// <summary>
        /// Apply Transformation to Skybox
        /// </summary>
        /// <param name="transformationMatrix">Transformation Matrix</param>
        public void transform(Matrix4 transformationMatrix)
        {
            skyboxObject.Transformation *= transformationMatrix;
        }
        
        /// <summary>
        /// Draw the Skybox
        /// </summary>
        public void Draw()
        {
            material.Draw(skyboxObject, texture);
        }
    }
}
