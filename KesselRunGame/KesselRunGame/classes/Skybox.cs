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
    class Skybox : BaseObject3D
    {

        public Skybox()
        {
            Positions = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(0, 110, 0),
                new Vector3(110, 110, 0),
                new Vector3(110, 0, 0),
                new Vector3(0, 0, 110),
                new Vector3(0, 110, 110),
                new Vector3(110, 110, 110),
                new Vector3(110, 0, 110)
            };
            UVs = new List<Vector2>();
            Normals = new List<Vector3>();
            Indices = new List<int>();


            addTriangle(new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 2, 0), new Vector3(1, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 1),
                        new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1));


            CreateVAO();
        }
    }
}
