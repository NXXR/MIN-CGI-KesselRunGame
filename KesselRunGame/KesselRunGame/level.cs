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
using cgimin.engine.skybox;
using cgimin.engine.gui;

using static cgimin.engine.material.BaseMaterial;
using Engine.cgimin.engine.material.simpleblend;
using cgimin.engine.material.ambientdiffuse;
using cgimin.engine.material.normalmappingcubespecular;
using cgimin.engine.material.cubereflectionnormal;
using cgimin.engine.material.normalmapping;
using cgimin.engine.material.simplereflection;
using cgimin.engine.material.simpletexture;

using Engine.cgimin.engine.octree;

using Engine.cgimin.engine.terrain;

using Blinkenlights.Splines;


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
        private float velocity = 2.0f; // velocity = units / updateCounter = x / 1sec (60fps/60)
        // distance from start to racing point (for normalization & interpolation of curve)
        private float distFromStart;

        // settings for obstacle (asteroid) generation
        // Threshold, after which distance obstacle generation should be attempted
        private float genThreshold = 10.0f; // threshold until next obstacle generation is attempted
        
        private const float minGenDistance = 3.0f; // minimum distance for next obstacle on generation success
        private const float genFailInc = 0.5f; // threshold increment on failed generation attempt
        
        // starting probability to generate an obstacle (in percent 0..100)
        private const int genProbabilityDefault = 45; // base percentage
        private int genProbability = genProbabilityDefault; // current percentage (increased if generation fails)
        private const int genProbabilityInc = 5; // increase of probability when generation fails
        
        // threshold for drawing the track in debug mode
        private int debugTrackThreshold = 0;
        
        
        private List<OctreeEntity> entityList;
        
        private OctreeEntity[] obstacles;
        
        private OctreeEntity normalAsteroidEntity;
        
        private OctreeEntity smallAsteroidEntity;
        private float smallScaling = 0.8f;
        
        private OctreeEntity tinyAsteroidEntity;
        private float tinyScaling = 0.4f;
        
        private OctreeEntity largeAsteroidEntity;
        private float largeScaling = 1.5f;
        
        private OctreeEntity hugeAsteroidEntity;
        private float hugeScaling = 2.0f;
        
        private OctreeEntity debugTrackEntity;
        private OctreeEntity debugControlEntity;

        private OctreeEntity racePoint;

        private OctreeEntity playerEntity;
        private Matrix4 playerBaseTransformation;
        private Vector3 playerControlPosition;
        
        // texture IDs
        private int asteroidColorTexture;
        private int asteroidNormalTexture;
        private int asteroidEnvCubeTexture;
        private int debugControlColorTexture;
        private int debugTrackColorTexture;
        
        private int playerColorTexture;
        
        // Materials
        private NormalMappingCubeSpecularMaterial normalMappingCubeSpecularMaterial;
        private SimpleTextureMaterial simpleTextureMaterial;
        
        // updateCounter value when level is started
        private int levelUpdateCounter = 0;

        // collion true if player collieded with obstacle
        private bool collision;

        public void OnLoad()
        {
            // textures
            asteroidColorTexture = TextureManager.LoadTexture("data/textures/marble_gray.png");
            asteroidNormalTexture = TextureManager.LoadTexture("data/textures/stone_normal.png");
            asteroidEnvCubeTexture = TextureManager.LoadCubemap(new List<string>{ "data/textures/cmap2_left.png", "data/textures/cmap2_right.png",
                                                                                  "data/textures/cmap2_top.png",  "data/textures/cmap2_bottom.png",
                                                                                  "data/textures/cmap2_back.png", "data/textures/cmap2_front.png"});
            
            debugControlColorTexture = TextureManager.LoadTexture("data/textures/single_color.png");
            debugTrackColorTexture = TextureManager.LoadTexture("data/textures/single_color2.png");

            playerColorTexture = TextureManager.LoadTexture("data/textures/space_ship_test_color.png");
            
            // materials
            normalMappingCubeSpecularMaterial = new NormalMappingCubeSpecularMaterial();
            simpleTextureMaterial = new SimpleTextureMaterial();
            
            // material settings
            MaterialSettings asteroidMaterialSettings = new MaterialSettings();
            asteroidMaterialSettings.colorTexture = asteroidColorTexture;
            asteroidMaterialSettings.normalTexture = asteroidNormalTexture;
            asteroidMaterialSettings.cubeTexture = asteroidEnvCubeTexture;
            asteroidMaterialSettings.shininess = 1.0f;
            
            MaterialSettings debugMaterialSettings = new MaterialSettings();
            // color texture is set inside debug entities
            
            MaterialSettings playerMaterialSettings = new MaterialSettings();
            playerMaterialSettings.colorTexture = playerColorTexture;
            
            // preload Objects with different predetermined sizes to reduce loading time and pack them into Entities
            // normal asteroid
            normalAsteroidEntity = new OctreeEntity(
                new ObjLoaderObject3D("data/objects/round_stone.obj", 0.3f, true),
                normalMappingCubeSpecularMaterial,
                asteroidMaterialSettings,
                Matrix4.Identity);
            
            // slightly smaller asteroid
            smallAsteroidEntity = new OctreeEntity(
                new ObjLoaderObject3D("data/objects/round_stone.obj", 0.3f * smallScaling, true),
                normalMappingCubeSpecularMaterial,
                asteroidMaterialSettings,
                Matrix4.Identity);
            
            // smallest asteroid
            tinyAsteroidEntity = new OctreeEntity(
                new ObjLoaderObject3D("data/objects/round_stone.obj", 0.3f * tinyScaling, true),
                normalMappingCubeSpecularMaterial,
                asteroidMaterialSettings,
                Matrix4.Identity);
            
            // slightly larger asteroid
            largeAsteroidEntity = new OctreeEntity(
                new ObjLoaderObject3D("data/objects/round_stone.obj", 0.3f * largeScaling, true),
                normalMappingCubeSpecularMaterial,
                asteroidMaterialSettings,
                Matrix4.Identity);
            
            // largest size for track obstacles
            hugeAsteroidEntity = new OctreeEntity(
                new ObjLoaderObject3D("data/objects/round_stone.obj", 0.3f * hugeScaling, true),
                normalMappingCubeSpecularMaterial,
                asteroidMaterialSettings,
                Matrix4.Identity);
            
            // debug entity to visualize track
            debugTrackEntity = new OctreeEntity(
                new ObjLoaderObject3D("data/objects/sphere.obj", 0.1f, true),
                simpleTextureMaterial,
                debugMaterialSettings,
                Matrix4.Identity);
            debugTrackEntity.MaterialSetting.colorTexture = debugTrackColorTexture;
            
            // debug entity to visualize track control points
            debugControlEntity = new OctreeEntity(
                new ObjLoaderObject3D("data/objects/sphere.obj", 0.5f, true),
                simpleTextureMaterial,
                debugMaterialSettings,
                Matrix4.Identity);
            debugControlEntity.MaterialSetting.colorTexture = debugControlColorTexture;
            
            // Race Point entity to visualize leading track position
            racePoint = new OctreeEntity(
                new ObjLoaderObject3D("data/objects/sphere.obj", 0.05f, true),
                simpleTextureMaterial,
                debugMaterialSettings,
                Matrix4.Identity);
            racePoint.MaterialSetting.colorTexture = debugTrackColorTexture;
            
            // player entity
            playerEntity = new OctreeEntity(
                new ObjLoaderObject3D("data/objects/spaceship.obj", 0.05f, true),
                simpleTextureMaterial,
                playerMaterialSettings,
                Matrix4.Identity);
            playerBaseTransformation = playerEntity.Object3d.Transformation;
            
            
            // generate array containig obstacles for generation
            obstacles = new[]
            {
                tinyAsteroidEntity,
                smallAsteroidEntity,
                normalAsteroidEntity,
                largeAsteroidEntity,
                hugeAsteroidEntity
            };
            
            entityList = new List<OctreeEntity>();
        }

        public void createLevel(Octree octree, Random lvlSeed = null, bool debugMode = false)
        {
            DateTime timeStartGeneration = DateTime.Now;
            // create default level seed if none is provided
            if(lvlSeed is null) lvlSeed = new Random(1234678900);
            
            // initiate player control vector
            playerControlPosition = new Vector3(0.0f);
            
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
                if (trackLength > genThreshold && t < 0.95) // checks threshold to attempt new generation is passed stops generation on last 5% of track (cutoff)
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

            collision = false;
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
            float[] racep = Array.ConvertAll(
                BSpline.Interpolate(distNormalized, trackDeg, trackPoints, trackKnots, null, null),
                input => (float)input);
            Vector3 rp = new Vector3(racep[0], racep[1], racep[2]);
            
            // find trailing point for Camera position
            float[] trailp = Array.ConvertAll(
                BSpline.Interpolate(distNormalized <= 0.01? 0.0f : distNormalized-0.01, trackDeg, trackPoints, trackKnots, null, null),
                input => (float) input);
            Vector3 tp = new Vector3(trailp[0], trailp[1], trailp[2]);
            
            // set trailing point transformation
            Matrix4 tpTransform = playerBaseTransformation *
                                  getRotation(Vector3.Subtract(rp, tp), Vector3.UnitY) *
                                  Matrix4.CreateTranslation(tp);
            
            playerEntity.Transform = Matrix4.CreateTranslation(playerControlPosition);
            playerEntity.Transform *= tpTransform;
            
            // calc pp
            Vector3 pp = playerEntity.Transform.ExtractTranslation();
            
            
            // set camera along track
            Vector3 rp_pp = Vector3.Subtract(pp, rp);
            Vector3 cameye = rp + rp_pp.Normalized() * (rp_pp.Length + 0.5f);
            Camera.SetLookAt(cameye, rp, Vector3.TransformVector(Vector3.UnitZ, tpTransform));
            
            // set race entity
            racePoint.Transform = Matrix4.CreateTranslation(rp);
            
            // test collision between player and obstacle
            collision = collision || testCollision(playerEntity, entityList);
            
            // increase level UpdateCounter
            levelUpdateCounter++;
        }

        public OctreeEntity getRacePoint()
        {
            return racePoint;
        }

        public OctreeEntity getplayerPoint()
        {
            return playerEntity;
        }

        public bool getCollision()
        {
            return collision;
        }

        public void modifyPlayerPos(float x, float y)
        {
            if (playerControlPosition.X <= 1.5f && playerControlPosition.X >= -1.5f) playerControlPosition.X += x;
            if (playerControlPosition.Z <= 1.5f && playerControlPosition.Z >= -1.5f) playerControlPosition.Z += y;
            if (x == 0.0f && playerControlPosition.X > 0) playerControlPosition.X -= 0.01f;
            if (x == 0.0f && playerControlPosition.X < 0) playerControlPosition.X += 0.01f;
            if (y == 0.0f && playerControlPosition.Z > 0) playerControlPosition.Z -= 0.01f;
            if (y == 0.0f && playerControlPosition.Z < 0) playerControlPosition.Z += 0.01f;
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
        private bool genAsteroid(float[] lastPoint, float[] point, int probability, OctreeEntity obstacle, Octree octree, Random seed = null)
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
            float distObstacle = obstacle.Object3d.radius * ((float) seed.Next(8, 12) / 10);
            
            // create position for obstacle (point + perpendicular vector x * dist to track
            Vector3 genPos = Vector3.Add(new Vector3(point[0], point[1], point[2]), x * distObstacle);
            
            // check if location is valid (return false if invalid)
            if (!genIsValid(entityList, genPos, obstacle.Object3d.radius)) return false;
            
            // generate transformation with random rotation (rotation * translation)
            Matrix4 transformation = Matrix4.Identity;
            transformation *= Matrix4.CreateRotationX(seed.Next(0, 360)); // rotate randomly around x-axis
            transformation *= Matrix4.CreateRotationY(seed.Next(0, 360)); // rotate randomly around y-axis
            transformation *= Matrix4.CreateRotationZ(seed.Next(0, 360)); // rotate randomly around z-axis
            transformation *= Matrix4.CreateTranslation(genPos); // move to genPos
            
            // generate obstacle at position and add to Entity list
            OctreeEntity o = new OctreeEntity(obstacle.Object3d, obstacle.Material, obstacle.MaterialSetting, transformation);
            octree.AddEntity(o);
            
            entityList.Add(o);
            
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
        private bool genIsValid(List<OctreeEntity> entities, Vector3 position, float radius)
        {
            // detect potential collision with other Asteroids
            bool hasCollision = entities.Exists(entity =>
            {
                // calculate distance between point and other octreeEntity and check if it is larger than the sum of both radii
                return Vector3.Distance(entity.Transform.ExtractTranslation(), position) < radius + entity.Object3d.radius;
            });
            // generation is valid if no collision detected
            return !hasCollision;
        }
        
        // function to generate projection matrix to align coordinate system to position and direction
        private Matrix4 getRotation(Vector3 targetForward, Vector3 sourceForward)
        {
            /*/ create rotation to align -z axis with targetForward vector
            Vector3 v = Vector3.Cross(Vector3.UnitX, targetForward.Normalized());
            float c = Vector3.Dot(Vector3.UnitZ * -1, targetForward.Normalized());
            
            Matrix4 vx = new Matrix4(
                new Vector4(        0, -1 * v.Z,      v.Y, 0),
                new Vector4(      v.Z,        0, -1 * v.X, 0),
                new Vector4( -1 * v.Y,      v.X,        0, 0),
                new Vector4(        0,        0,        0, 0));

            // return rotation
            //return Matrix4.Identity + vx + vx * vx * (1 / (1 + c)); // I + vx + vx^2 * 1/(1+c)
            */
            
            Vector3 source = sourceForward.Normalized();
            Vector3 target = targetForward.Normalized();
            
            float angle = (float)Math.Acos(Vector3.Dot(source, target));
            Vector3 axis = Vector3.Cross(source, target);
            
            return Matrix4.CreateFromAxisAngle(axis, angle);
        }
        
        // debug function to draw track control points
        private void debugDrawControlPoints(double[][] points, Octree octree, OctreeEntity entity)
        {
            foreach (double[] point in points)
            {
                entity.Transform = Matrix4.CreateTranslation((float) point[0], (float) point[1], (float) point[2]);
                octree.AddEntity(entity);
            }
        }
        
        // debug function to draw track points
        private void debugDrawTrack(int trackPosition, float[] point, Octree octree, OctreeEntity entity)
        {
            if (trackPosition > debugTrackThreshold && trackPosition % 2 == 0)
            {
                entity.Transform = Matrix4.CreateTranslation(point[0], point[1], point[2]);
                octree.AddEntity(entity);
                debugTrackThreshold = trackPosition;
            }
        }
        
        // test for collisions
        public bool testCollision(OctreeEntity player, List<OctreeEntity> obstacles)
        {
            return obstacles.Exists(entity =>
            {
                // calculate distance between point and other octreeEntity and check if it is larger than the sum of both radii
                return Vector3.Distance(entity.Transform.ExtractTranslation(), player.Transform.ExtractTranslation()) < (player.Object3d.radius + entity.Object3d.radius);
            });
        }
        #endregion --- sub-functions ---
    }
}