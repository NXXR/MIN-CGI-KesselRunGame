using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using cgimin.engine.object3d;

namespace KesselRunGame
{
    /// <summary>
    /// Skybox Class to generate and texturize a Skybox
    /// </summary>
    public class Skybox
    {
        /// <summary> 3D Object of the Skybox </summary>
        private BaseObject3D skyboxObject;
        /// <summary>  </summary>
        private int texture;
        private int material;

        public Skybox(int size)
        {
            // initialize & create Skybox 3D Object
            skyboxObject.Positions = new List<Vector3>();
            skyboxObject.UVs = new List<Vector2>();
            skyboxObject.Normals = new List<Vector3>();
            skyboxObject.Indices = new List<int>();

            /*
            private List<Vector3> corners = new List<Vector3>
            {
                new Vector3(0, 0, 0),  //      6--------7
                new Vector3(0, 1, 0),  //     /|       /|
                new Vector3(1, 1, 0),  //    / |      / |
                new Vector3(1, 0, 0),  //   2--|-----3  |
                new Vector3(0, 0, 1),  //   |  5-----|--8
                new Vector3(0, 1, 1),  //   | /      | /
                new Vector3(1, 1, 1),  //   |/       |/
                new Vector3(1, 0, 1)   //   1--------4
            };
            */

    }
    }
}
