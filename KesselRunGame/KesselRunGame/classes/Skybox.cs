using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

using cgimin.engine.object3d;
using cgimin.engine.texture;
using cgimin.engine.material;
using cgimin.engine.material.simpletexture;

namespace KesselRunGame
{
    /// <summary>
    /// Skybox Class to generate and texturize a Skybox
    /// </summary>
    public class Skybox
    {
        /// <summary> 3D Object of the Skybox </summary>
        private BaseObject3D skyboxObject = new BaseObject3D();
        /// <summary> ID of the Texture (returned by TextureManager </summary>
        private int texture = 0;
        /// <summary> Skybox Material </summary>
        private BaseMaterial material;

        public Skybox(int size, string texturePath)
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
                new Vector3(0.0f, 1.0f, 0.0f),  // 2
                new Vector3(1.0f, 1.0f, 0.0f),  // 3
                new Vector3(1.0f, 0.0f, 0.0f),  // 4
                new Vector3(0.0f, 0.0f, 1.0f),  // 5
                new Vector3(0.0f, 1.0f, 1.0f),  // 6
                new Vector3(1.0f, 1.0f, 1.0f),  // 7
                new Vector3(1.0f, 0.0f, 1.0f)   // 8
            };
            // Inverted Unit-Vectors
            Vector3 invX = new Vector3(-1.0f, 0.0f, 0.0f);
            Vector3 invY = new Vector3(0.0f, -1.0f, 0.0f);
            Vector3 invZ = new Vector3(0.0f, 0.0f, -1.0f);
            
            // setting Skybox-Faces
            skyboxObject.addTriangle(corners[1], corners[2], corners[3], Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 1-2-3
            skyboxObject.addTriangle(corners[3], corners[4], corners[1], Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 3-4-1
            skyboxObject.addTriangle(corners[1], corners[2], corners[6], Vector3.UnitX, Vector3.UnitX, Vector3.UnitX, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 1-2-6
            skyboxObject.addTriangle(corners[1], corners[5], corners[6], Vector3.UnitX, Vector3.UnitX, Vector3.UnitX, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 1-5-6
            skyboxObject.addTriangle(corners[5], corners[6], corners[7], invZ, invZ, invZ, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 5-6-7
            skyboxObject.addTriangle(corners[5], corners[8], corners[7], invZ, invZ, invZ, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 5-8-7            
            skyboxObject.addTriangle(corners[8], corners[7], corners[3], invX, invX, invX, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 8-7-3
            skyboxObject.addTriangle(corners[3], corners[4], corners[8], invX, invX, invX, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 3-4-8
            skyboxObject.addTriangle(corners[1], corners[5], corners[8], Vector3.UnitY, Vector3.UnitY, Vector3.UnitY, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 1-5-8
            skyboxObject.addTriangle(corners[1], corners[4], corners[8], Vector3.UnitY, Vector3.UnitY, Vector3.UnitY, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 1-4-8
            skyboxObject.addTriangle(corners[2], corners[3], corners[7], invY, invY, invY, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 2-3-7
            skyboxObject.addTriangle(corners[2], corners[6], corners[7], invY, invY, invY, Vector2.Zero, Vector2.Zero, Vector2.Zero);  // 2-6-7
            
            // setting Skybox-Texture
            texture = TextureManager.LoadTexture(texturePath);
            
            // setting Skybox-Material
            material = new SimpleTextureMaterial();
        }
    }
}
