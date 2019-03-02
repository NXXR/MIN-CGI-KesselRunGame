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
using cgimin.engine.material;

namespace Examples.Tutorial
{
    public class Level
    {   
        // settings for curve
        // default control points
        private double[][] trackPoints = {
            // fixed entry
            new []{-30.0, 0.0, 0.0},
            new []{-20.0, 0.0, 0.0},
            // variable track
            new []{-15.0, 0.0, -20.0},
            new []{-10.0, 20.0, -20.0},
            new []{-5.0, 20.0, 0.0},
            new []{0.0, 0.0, 0.0},
            new []{5.0, -20.0, 0.0},
            new []{10.0, -20.0, 20.0},
            new []{15.0, 0.0, 20.0},
            //fixed exit
            new []{20.0, 0.0, 0.0},
            new []{30.0, 0.0, 0.0}
        };
        // degree of curve
        private const int trackDeg = 3;
        // knot array of curve
        private readonly double[] trackKnots = new double[15]
        {
            0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 8, 8, 8
        };
        // variable for absolute length
        float trackLength = 0;
        // velocity of camera & game object
        private float velocity = 3.0f; // velocity = units / updateCounter = x / 1sec (60fps/60)
        // distance from start to racing point (for normalization & interpolation of curve)
        private float distFromStart;

        // settings for obstacle (asteroid) generation
        // Threshold, after which distance obstacle generation should be attempted
        private float genThreshold = 10.0f; // threshold for first obstacle (is increased with each attempt)
        private const float minGenDistance = 3.0f; // minimum distance for next obstacle on generation success
        private const float genFailInc = 0.5f; // threshold increment on failed generation attempt
        // starting probability to generate an obstacle (in percent 0..100)
        private const int genProbabilityDefault = 45; // base percentage
        private int genProbability = genProbabilityDefault; // current percentage (increased if generation fails)
        private const int genProbabilityInc = 5; // increase of probability when generation fails
        
        // threshold for drawing the track in debug mode
        private int debugTrackThreshold = 0;
        
        // entity to contain all octree information except translation
        private struct Entity
        {
            public ObjLoaderObject3D obj;
            public BaseMaterial material;
            public MaterialSettings settings;
            public float radius;
        }
        
        private Entity[] obstacles;
        
        private Entity normalAsteroidEntity;
        private float normalAsteroidRadius = 1.4f; // for collision etc.
        
        private Entity smallAsteroidEntity;
        private float smallScaling = 0.8f;
        
        private Entity tinyAsteroidEntity;
        private float tinyScaling = 0.4f;
        
        private Entity largeAsteroidEntity;
        private float largeScaling = 1.5f;
        
        private Entity hugeAsteroidEntity;
        private float hugeScaling = 2.0f;
        
        private Entity debugTrackEntity;
        private Entity debugControlEntity;
        
        // texture IDs
        private int asteroidColorTexture;
        private int asteroidNormalTexture;
        private int asteroidEnvCubeTexture;
        private int debugControlColorTexture;
        private int debugTrackColorTexture;
        
        // Materials
        private NormalMappingCubeSpecularMaterial asteroidMaterial;
        private NormalMappingMaterial debugMaterial;
        
        // updateCounter value when level is started
        private int levelUpdateCounter = 0;

        public void OnLoad()
        {
            // textures
            asteroidColorTexture = TextureManager.LoadTexture("data/textures/marble_blue.png");
            asteroidNormalTexture = TextureManager.LoadTexture("data/textures/stone_normal.png");
            asteroidEnvCubeTexture = TextureManager.LoadCubemap(new List<string>{ "data/textures/cmap2_left.png", "data/textures/cmap2_right.png",
                                                                                  "data/textures/cmap2_top.png",  "data/textures/cmap2_bottom.png",
                                                                                  "data/textures/cmap2_back.png", "data/textures/cmap2_front.png"});
            
            debugControlColorTexture = TextureManager.LoadTexture("data/textures/single_color.png");
            debugTrackColorTexture = TextureManager.LoadTexture("data/textures/single_color2.png");
            
            // materials
            asteroidMaterial = new NormalMappingCubeSpecularMaterial();
            debugMaterial = new NormalMappingMaterial();
            
            // material settings
            MaterialSettings asteroidMaterialSettings = new MaterialSettings();
            asteroidMaterialSettings.colorTexture = asteroidColorTexture;
            asteroidMaterialSettings.normalTexture = asteroidNormalTexture;
            asteroidMaterialSettings.cubeTexture = asteroidEnvCubeTexture;
            
            MaterialSettings debugMaterialSettings = new MaterialSettings();
            // color texture is set inside debug Entities
            debugMaterialSettings.normalTexture = asteroidNormalTexture;
            
            
            // preload Objects with different predetermined sizes to reduce loading time and pack them into Entities
            normalAsteroidEntity = new Entity(); //normal size
            normalAsteroidEntity.obj = new ObjLoaderObject3D("data/objects/round_stone.obj", 0.3f, true);
            normalAsteroidEntity.radius = normalAsteroidRadius;
            normalAsteroidEntity.material = asteroidMaterial;
            normalAsteroidEntity.settings = asteroidMaterialSettings;
            
            smallAsteroidEntity = new Entity(); // slightly smaller size
            smallAsteroidEntity.obj = new ObjLoaderObject3D("data/objects/round_stone.obj", 0.3f * smallScaling, true);
            smallAsteroidEntity.radius = normalAsteroidRadius * smallScaling;
            smallAsteroidEntity.material = asteroidMaterial;
            smallAsteroidEntity.settings = asteroidMaterialSettings;
            
            tinyAsteroidEntity = new Entity(); // smallest size
            tinyAsteroidEntity.obj = new ObjLoaderObject3D("data/objects/round_stone.obj", 0.3f * tinyScaling, true);
            tinyAsteroidEntity.radius = normalAsteroidRadius * tinyScaling;
            tinyAsteroidEntity.material = asteroidMaterial;
            tinyAsteroidEntity.settings = asteroidMaterialSettings;
            
            largeAsteroidEntity = new Entity(); // slightly larger size
            largeAsteroidEntity.obj = new ObjLoaderObject3D("data/objects/round_stone.obj", 0.3f * largeScaling, true);
            largeAsteroidEntity.radius = normalAsteroidRadius * largeScaling;
            largeAsteroidEntity.material = asteroidMaterial;
            largeAsteroidEntity.settings = asteroidMaterialSettings;
            
            hugeAsteroidEntity = new Entity(); // largest size for track obstacles
            hugeAsteroidEntity.obj = new ObjLoaderObject3D("data/objects/round_stone.obj", 0.3f * hugeScaling, true);
            hugeAsteroidEntity.radius = normalAsteroidRadius * hugeScaling;
            hugeAsteroidEntity.material = asteroidMaterial;
            hugeAsteroidEntity.settings = asteroidMaterialSettings;
            
            debugTrackEntity = new Entity(); // debug entity to visualize track
            debugTrackEntity.obj = new ObjLoaderObject3D("data/objects/sphere.obj", 0.2f, true);
            debugTrackEntity.radius = 0.0f;
            debugTrackEntity.material = debugMaterial;
            debugTrackEntity.settings = debugMaterialSettings;
            debugTrackEntity.settings.colorTexture = debugTrackColorTexture;
            
            debugControlEntity = new Entity(); // debug entity to visualize track control points
            debugControlEntity.obj = new ObjLoaderObject3D("data/objects/sphere.obj", 0.5f, true);
            debugControlEntity.radius = 0.0f;
            debugControlEntity.material = debugMaterial;
            debugControlEntity.settings = debugMaterialSettings;
            debugControlEntity.settings.colorTexture = debugControlColorTexture;

            // generate array containig obstacles for generation
            obstacles = new[]
            {
                tinyAsteroidEntity,
                smallAsteroidEntity,
                normalAsteroidEntity,
                largeAsteroidEntity,
                hugeAsteroidEntity
            };
        }

        public void createLevel(Octree octree, Random lvlSeed = null, bool debugMode = false)
        {
            DateTime timeStartGeneration = DateTime.Now;
            // create default level seed if none is provided
            if(lvlSeed is null) lvlSeed = new Random(1234678900);
            
            // randomize track control points
            trackPoints = randomizeTrack(trackPoints, lvlSeed);

            if (debugMode) debugDrawControlPoints(trackPoints, octree, debugControlEntity);
            
            // generate track
            // initialize trackLength and lastPoint
            trackLength = 0;
            float[] lastPoint = Array.ConvertAll( // convert all elements of the resulting double[] to float
                BSpline.Interpolate(0, trackDeg, trackPoints, trackKnots, null, null),
                input => (float) input);

            for (double t = 0; t <= 1; t += 0.001) // run along curve in 0.001 steps (accuracy)
            {
                float[] point = Array.ConvertAll(
                    BSpline.Interpolate(t, trackDeg, trackPoints, trackKnots, null, null),
                    input => (float) input);
                
                // increment trackLength with length of vector from lastPoint to Point
                trackLength += new Vector3(point[0] - lastPoint[0], point[1] - lastPoint[1], point[2] - lastPoint[2]).Length;
                
                if (debugMode) debugDrawTrack((int)trackLength, point, octree, debugTrackEntity);
                
                //generate asteroids along the track
                if (trackLength > genThreshold) // checks if threshold to attempt new generation is passed
                {
                    // attempt asteroid/obstacle generation
                    if (genAsteroid(lastPoint, point, genProbability, obstacles[lvlSeed.Next(obstacles.Length - 1)], octree, lvlSeed))
                    {
                        // if successful, threshold for next object is increased by minGenDistance
                        genThreshold += minGenDistance;
                        // and the probability is reset to default value
                        genProbability = genProbabilityDefault;
                    }
                    else
                    {
                        // if unsuccessful, threshold is slightly increased
                        genThreshold += genFailInc;
                        // and the probability is increased as well
                        genProbability += genProbabilityInc;
                    }
                }
                
                // save point for next iteration
                lastPoint = point;
            }

            double generationTime = timeStartGeneration.Subtract(DateTime.Now).Duration().TotalSeconds;
            Console.WriteLine("[INFO]    Generation finished! (took " + generationTime + "sec., Track length: " + trackLength + ")");
        }

        public void OnUpdateframe()
        {
            // calculate distance based on velocity and time
            distFromStart = velocity * ((float) levelUpdateCounter / 60);
            
            // normalize distance
            double distNormalized = distFromStart / trackLength;
            // cutoff for end of track
            if (distNormalized > 1) distNormalized = 1.0f;
            
            // find racing point for LookAt position
            float[] lookAtPos = Array.ConvertAll(
                BSpline.Interpolate(distNormalized, trackDeg, trackPoints, trackKnots, null, null),
                input => (float)input);
            // find trailing point for Camera position
            float[] cameraPos = Array.ConvertAll(
                BSpline.Interpolate(distNormalized <= 0.001? 0.0f : distNormalized-0.001, trackDeg, trackPoints, trackKnots, null, null),
                input => (float) input);
            
            // set camera along track
            Camera.SetLookAt(new Vector3(cameraPos[0], cameraPos[1], cameraPos[2]), new Vector3(lookAtPos[0], lookAtPos[1], lookAtPos[2]), Vector3.UnitY);
            
            // increase level Updatecounter
            levelUpdateCounter++;
        }

        #region --- sub-functions ---
        
        // function to randomize track based on seed
        private double[][] randomizeTrack(double[][] track, Random seed = null)
        {
            // create random seed if not supplied
            if (seed is null) seed = new Random(Guid.NewGuid().GetHashCode());
            
            for (int i = 0; i < track.Length; i++)
            {
                // keep fist and last 2 control points unchanged for straight & constant ending (appendable)
                if (i > 1 && i < track.Length-2)
                {
                    // keep random points within -20, 20 Cube (entrance & exit at +/- 30)
                    track[i][0] = seed.Next(-20, 20);
                    track[i][1] = seed.Next(-20, 20);
                    track[i][2] = seed.Next(-20, 20);
                }
            }
            
            Console.WriteLine("[Info]    Track control points randomized");
            return track;
        }
        
        // function to attempt to generate an obstacle (asteroid) along the track
        private bool genAsteroid(float[] lastPoint, float[] point, int probability, Entity obstacle, Octree octree, Random seed = null)
        {
            // create random seed if not supplied
            if (seed is null) seed = new Random(Guid.NewGuid().GetHashCode());
            
            // check probability to generate obstacle
            if (seed.Next(0, 100) > probability) return false;
            
            // create Vector v along spline (lastPoint -> point)
            Vector3 v = new Vector3(point[0] - lastPoint[0], point[1] - lastPoint[1], point[2] - lastPoint[2]);
            // get perpendicular Vector x for distance from track
            Vector3 x = getPerpendicular(v, seed);
            
            // add slight deviation to distance to track
            float distObstacle = obstacle.radius * ((float) seed.Next(8, 12) / 10);
            
            // create position for obstacle (point + perpendicular vector x * dist to track
            Vector3 genPos = Vector3.Add(new Vector3(point[0], point[1], point[2]), x * distObstacle);
            
            // check if location is valid (return false if invalid)
            if (!genIsValid(octree, genPos, obstacle.radius)) return false;
            
            // generate transformation with random rotation (rotation * translation)
            Matrix4 transformation = Matrix4.Identity;
            transformation *= Matrix4.CreateRotationX(seed.Next(0, 360)); // rotate randomly around x-axis
            transformation *= Matrix4.CreateRotationY(seed.Next(0, 360)); // rotate randomly around y-axis
            transformation *= Matrix4.CreateRotationZ(seed.Next(0, 360)); // rotate randomly around z-axis
            transformation *= Matrix4.CreateTranslation(genPos); // move to genPos
            
            // generate obstacle at position
            octree.AddEntity(new OctreeEntity(obstacle.obj, obstacle.material, obstacle.settings, transformation));
            
            Console.WriteLine("[INFO]    Obstacle generated at " + genPos);
            return true;
        }
        
        // function that returns a vector perpendicular to the input vector with random direction
        private Vector3 getPerpendicular(Vector3 x, Random seed = null)
        {
            // create random seed if not supplied
            if (seed is null) seed = new Random(Guid.NewGuid().GetHashCode());
            
            // create random vector e
            Vector3 e = new Vector3((float)seed.Next(-9999,9999)/100, (float)seed.Next(-9999,9999)/100, (float)seed.Next(-9999,9999)/100);
            // recreate vector e if it is parallel to input vector x
            while (e.Normalized().Equals(x.Normalized()))
            {
                e = new Vector3((float)seed.Next(-9999,9999)/100, (float)seed.Next(-9999,9999)/100, (float)seed.Next(-9999,9999)/100);
            }
            
            // return the normalized cross product of input and random vector to get a unit vector perpendicular to x
            return Vector3.Cross(x, e).Normalized();
        }
        
        // function to check if obstacle position is valid
        private bool genIsValid(Octree octree, Vector3 position, float radius)
        {
            // TODO: generation validation
            
            
            return true;
        }
        
        // debug function to draw track control points
        private void debugDrawControlPoints(double[][] points, Octree octree, Entity entity)
        {
            foreach (double[] point in points)
            {
                octree.AddEntity(new OctreeEntity(entity.obj, entity.material, entity.settings, Matrix4.CreateTranslation((float)point[0], (float)point[1], (float)point[2])));
            }
        }
        
        // debug function to draw track points
        private void debugDrawTrack(int trackPosition, float[] point, Octree octree, Entity entity)
        {
            if (trackPosition > debugTrackThreshold && trackPosition % 2 == 0)
            {
                octree.AddEntity(new OctreeEntity(entity.obj, entity.material, entity.settings, Matrix4.CreateTranslation(point[0], point[1], point[2])));
                debugTrackThreshold = trackPosition;
            }
        }
        
        #endregion --- sub-functions ---
    }
}