//   Metric.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils
{
    public struct MeshTransform
    {
        public Mesh m;
        public Transform t;
        public Renderer r;

        public bool Valid => m != null && t != null && r != null && t.gameObject != null;

        public MeshTransform(MeshFilter mesh_filter)
        {
            t = mesh_filter.transform;
            m = mesh_filter.sharedMesh; 
            r = mesh_filter.GetComponent<MeshRenderer>();
        }

        public MeshTransform(SkinnedMeshRenderer skin)
        {
            r = skin;
            t = skin.transform;
            m = new Mesh();
            skin.BakeMesh(m);
        }
    }

    public struct Metric : IConfigNode
    {
        //convex hull
        public ConvexHull3D hull { get; private set; }

        Mesh _hull_mesh;
        public Mesh hull_mesh 
        { 
            get 
            { 
                if(_hull_mesh == null && hull != null) 
                    _hull_mesh = hull.MakeMesh();
                return _hull_mesh;
            }
        }

        public float volume => hull == null ? bounds_volume : hull.Volume;

        public float area => hull == null ? bounds_area : hull.Area;
        //bounds
        public Bounds  bounds { get; private set; }

        public Vector3 center => bounds.center;

        public Vector3 extents => bounds.extents;

        public Vector3 size => bounds.size;
        //physical properties
        public float bounds_volume { get; private set; }

        public float bounds_area { get; private set; }

        public float mass { get; set; }
        //part-vessel properties
        public int CrewCapacity { get; private set; }

        public float cost { get; set; }

        public bool Empty => bounds.size.IsZero();

        static Vector3[] local2local(Transform _from, Transform _to, Vector3[] points)
        {
            if(_from != _to)
                for(int p = 0; p < points.Length; p++)
                    points[p] = _to.InverseTransformPoint(_from.TransformPoint(points[p]));
            return points;
        }

        static Vector3[] local2world(Transform _from, Vector3[] points)
        {
            for(int p = 0; p < points.Length; p++)
                points[p] = _from.TransformPoint(points[p]);
            return points;
        }

        static Vector3[] world2local(Transform _to, Vector3[] points)
        {
            for(int p = 0; p < points.Length; p++)
                points[p]  = _to.InverseTransformPoint(points[p]);
            return points;
        }

        static float boundsVolume(Bounds b) => b.size.x * b.size.y * b.size.z;

        static float boundsArea(Bounds b) => 2 * (b.size.x * b.size.y + b.size.x * b.size.z + b.size.y * b.size.z);

        static Bounds initBounds(Vector3[] edges)
        {
            var b = new Bounds(edges[0], new Vector3());
            for(int i = 1; i < edges.Length; i++)
                b.Encapsulate(edges[i]);
            return b;
        }

        static void updateBounds(ref Bounds b, Vector3[] edges)
        {
            if(b == default(Bounds))
                b = initBounds(edges);
            else
                for(int i = 0; i < edges.Length; i++)
                    b.Encapsulate(edges[i]);
        }

        static void updateBounds(ref Bounds b, Bounds nb)
        {
            if(b == default(Bounds))
                b = nb;
            else
                b.Encapsulate(nb);
        }

        Bounds partsBounds(IList<Part> parts, Transform refT, bool compute_hull, bool exclude_disabled = true)
        {
            //reset metric
            mass = 0;
            cost = 0;
            CrewCapacity = 0;
            Bounds b = default(Bounds);
            if(parts == null || parts.Count == 0)
            {
                Utils.Log("Metric.partsBounds: WARNING! No parts were provided.");
                return b;
            }
            //calculate bounds and convex hull
            float b_size = 0;
            List<Vector3> hull_points = compute_hull ? new List<Vector3>() : null;
            for(int i = 0, partsCount = parts.Count; i < partsCount; i++)
            {
                Part p = parts[i];
                if(p == null)
                    continue;
                //EditorLogic.SortedShipList returns List<Part>{null} when all parts are deleted
                //check for weels; if it's a wheel, get all meshes under the wheel collider
                var wheel = p.Modules.GetModule<ModuleWheelBase>();
                var wheel_transform = wheel != null && wheel.Wheel != null && wheel.Wheel.wheelCollider != null ? wheel.Wheel.wheelCollider.wheelTransform : null;
                //check for asteroids
                var is_asteroid = p.Modules.GetModule<ModuleAsteroid>() != null;
                //check for bad parts
                var pname = p.partInfo != null? p.partInfo.name : p.name;
                var bad_part = Utils.NameMatches(pname, AT_UtilsGlobals.Instance.BadPartsList);
                var part_rot = p.partTransform.rotation;
                if(bad_part) 
                    p.partTransform.rotation = Quaternion.identity;
                foreach(var mesh in p.AllModelMeshes())
                {
                    //skip disabled objects
                    if(!mesh.Valid || exclude_disabled 
                       && (!mesh.r.enabled || !mesh.t.gameObject.activeInHierarchy))
                        continue;
                    //skip meshes from the blacklist
                    if(Utils.NameMatches(mesh.t.name, AT_UtilsGlobals.Instance.MeshesToSkipList))
                        continue;
                    Vector3[] verts;
                    if(bad_part)
                    {
                        verts = Utils.BoundCorners(mesh.r.bounds);
                        for(int j = 0, len = verts.Length; j < len; j++)
                        {
                            var v = p.partTransform.position + part_rot * (verts[j]-p.partTransform.position);
                            if(refT != null)
                                v = refT.InverseTransformPoint(v);
                            verts[j] = v;
                        }
                    }
                    else 
                    {
                        if(is_asteroid || wheel_transform != null && mesh.t.IsChildOf(wheel_transform)
                           || (compute_hull 
                               && Vector3.Scale(mesh.m.bounds.size, mesh.t.lossyScale).sqrMagnitude > b_size / 10))
                            verts = mesh.m.uniqueVertices();
                        else
                            verts = Utils.BoundCorners(mesh.m.bounds);
                        verts = refT != null? local2local(mesh.t, refT, verts) : local2world(mesh.t, verts);
                    }
                    updateBounds(ref b, verts);
                    if(compute_hull)
                    {
                        hull_points.AddRange(verts);
                        b_size = b.size.sqrMagnitude;
                    }
                }
                CrewCapacity += p.CrewCapacity;
                mass += p.TotalMass();
                cost += p.TotalCost();
                if(bad_part) 
                    p.partTransform.rotation = part_rot;
            }
            if(compute_hull && hull_points.Count >= 4)
            {
                hull = new ConvexHull3D(hull_points);
                _hull_mesh = null;
            }
            return b;
        }

        #region Constructors
        //metric copy
        public Metric(Metric m)
            : this()
        {
            hull = m.hull;
            bounds = new Bounds(m.bounds.center, m.bounds.size);
            bounds_volume = m.bounds_volume;
            bounds_area = m.bounds_area;
            mass = m.mass;
            CrewCapacity = m.CrewCapacity;
        }

        //metric from bounds
        void init_with_bounds(Bounds b, float m, int crew_capacity)
        {
            bounds = b;
            bounds_volume = boundsVolume(b);
            bounds_area = boundsArea(b);
            mass = m;
            CrewCapacity = crew_capacity;
        }

        public Metric(Bounds b, float m = 0f, int crew_capacity = 0)
            : this()
        {
            init_with_bounds(b, m, crew_capacity);
        }
        
        //metric from size
        public Metric(Vector3 center, Vector3 size, float m = 0f, int crew_capacity = 0)
            : this(new Bounds(center, size), m, crew_capacity)
        {
        }

        public Metric(Vector3 size, float m = 0f, int crew_capacity = 0)
            : this(Vector3.zero, size, m, crew_capacity)
        {
        }

        //metric from volume
        public Metric(float V, float m = 0f, int crew_capacity = 0)
            : this()
        {
            var a = Mathf.Pow(V, 1 / 3f);
            init_with_bounds(new Bounds(Vector3.zero, new Vector3(a, a, a)), m, crew_capacity);
        }
        
        //metric form vertices
        public Metric(Vector3[] verts, float m = 0f, int crew_capacity = 0, bool compute_hull = false)
            : this()
        {
            if(compute_hull)
                hull = new ConvexHull3D(verts);
            bounds = initBounds(verts);
            bounds_volume = boundsVolume(bounds);
            bounds_area = boundsArea(bounds);
            mass = m;
            CrewCapacity = crew_capacity;
        }
        
        //metric from config node
        public Metric(ConfigNode node)
            : this()
        {
            Load(node);
        }
        
        //mesh metric
        void init_with_mesh(MeshFilter mesh, Transform refT, bool compute_hull)
        {
            Vector3[] verts = Utils.BoundCorners(mesh.sharedMesh.bounds);
            if(refT != null)
                local2local(mesh.transform, refT, verts);
            else
                local2world(mesh.transform, verts);
            if(compute_hull)
            {
                hull = refT != null ? 
                    new ConvexHull3D(local2local(mesh.transform, refT, mesh.sharedMesh.uniqueVertices())) : 
                    new ConvexHull3D(local2world(mesh.transform, mesh.sharedMesh.uniqueVertices()));
            }
            bounds = initBounds(verts);
            bounds_volume = boundsVolume(bounds);
            bounds_area = boundsArea(bounds);
            mass = 0f;
        }

        public Metric(MeshFilter mesh, Transform refT = null, bool compute_hull = false)
            : this()
        {
            init_with_mesh(mesh, refT, compute_hull);
        }

        public Metric(Transform transform, bool compute_hull = false, bool world_space = false)
            : this()
        {
            MeshFilter m = transform.gameObject.GetComponent<MeshFilter>();
            if(m == null)
            {
                Utils.Log("[Metric] {} does not have MeshFilter component", transform.gameObject);
                return;
            }
            init_with_mesh(m, world_space? null : transform, compute_hull);
        }

        public Metric(Part part, string mesh_name, bool compute_hull = false, bool world_space = false)
            : this()
        {
            MeshFilter m = part.FindModelComponent<MeshFilter>(mesh_name);
            if(m == null)
            {
                Utils.Log("[Metric] {} does not have '{}' mesh", part.name, mesh_name);
                return;
            }
            init_with_mesh(m, world_space? null : part.transform, compute_hull);
        }
        
        //part metric
        public Metric(Part part, bool compute_hull = false, bool world_space = false)
            : this()
        {
            var exclude_disabled = part.partInfo != null && part != part.partInfo.partPrefab;
            bounds = partsBounds(new List<Part>{ part }, world_space? null : part.partTransform, compute_hull, exclude_disabled);
            bounds_volume = boundsVolume(bounds);
            bounds_area = boundsArea(bounds);
        }
        
        //vessel metric
        public Metric(Vessel vessel, bool compute_hull = false, bool world_space = false)
            : this()
        {
            bounds = partsBounds(vessel.parts, world_space? null : vessel.vesselTransform, compute_hull);
            bounds_volume = boundsVolume(bounds);
            bounds_area = boundsArea(bounds);
        }
        
        //in-editor vessel metric
        public Metric(List<Part> vessel, bool compute_hull = false, bool world_space = false)
            : this()
        {
            bounds = partsBounds(vessel, world_space? null : vessel[0].partTransform, compute_hull);
            bounds_volume = boundsVolume(bounds);
            bounds_area = boundsArea(bounds);
        }

        public Metric(IShipconstruct vessel, bool compute_hull = false, bool world_space = false)
            : this()
        {
            bounds = partsBounds(vessel.Parts, world_space? null : vessel.Parts[0].partTransform, compute_hull);
            bounds_volume = boundsVolume(bounds);
            bounds_area = boundsArea(bounds);
        }
        #endregion

        //public methods
        public void Clear()
        { 
            bounds = default(Bounds);
            bounds_volume = bounds_area = mass = cost = 0;
            CrewCapacity = 0;
            hull = null;
        }

        public Bounds GetBounds()
        {
            return new Bounds(bounds.center, bounds.size);
        }

        public void Scale(float s)
        {
            bounds = new Bounds(center, size * s);
            bounds_volume = boundsVolume(bounds);
            bounds_area = boundsArea(bounds);
            if(hull != null)
                hull = hull.Scale(s);
        }

        public void Save(ConfigNode node)
        {
            node.AddValue("bounds_center", ConfigNode.WriteVector(bounds.center));
            node.AddValue("bounds_size", ConfigNode.WriteVector(bounds.size));
            node.AddValue("crew_capacity", CrewCapacity);
            node.AddValue("mass", mass);
            node.AddValue("cost", cost);
            if(hull != null)
                hull.Save(node.AddNode("HULL"));
        }

        public void Load(ConfigNode node)
        {
            if(!node.HasValue("bounds_center") ||
               !node.HasValue("bounds_size") ||
               !node.HasValue("crew_capacity") ||
               !node.HasValue("mass") ||
               !node.HasValue("cost"))
                throw new KeyNotFoundException("Metric.Load: not all needed values are present in the config node.");
            Vector3 _center = ConfigNode.ParseVector3(node.GetValue("bounds_center"));
            Vector3 _size = ConfigNode.ParseVector3(node.GetValue("bounds_size"));
            bounds = new Bounds(_center, _size);
            bounds_volume = boundsVolume(bounds);
            bounds_area = boundsArea(bounds);
            CrewCapacity = int.Parse(node.GetValue("crew_capacity"));
            mass = float.Parse(node.GetValue("mass"));
            cost = float.Parse(node.GetValue("cost"));
            if(node.HasNode("HULL"))
                hull = ConvexHull3D.Load(node.GetNode("HULL"));
        }

        #region Fitting
        bool fits_somehow(List<float> _D)
        {
            var D = new List<float>{ size.x, size.y, size.z };
            D.Sort();
            _D.Sort();
            foreach(float d in D)
            {
                if(_D.Count == 0)
                    break;
                int ud = -1;
                for(int i = 0; i < _D.Count; i++)
                { 
                    if(d <= _D[i])
                    {
                        ud = i;
                        break;
                    } 
                }
                if(ud < 0)
                    return false;
                _D.RemoveAt(ud);
            }
            return true;
        }

        public bool FitsSomehow(Metric other)
        {
            var _D = new List<float>{ other.size.x, other.size.y, other.size.z };
            return fits_somehow(_D);
        }

        public bool FitsSomehow(Vector2 node)
        {
            var _D = new List<float>{ node.x, node.y };
            return fits_somehow(_D);
        }

        /// <summary>
        /// Returns true if THIS metric fits inside the OTHER metric.
        /// </summary>
        /// <param name="this_T">Transform of this metric.</param>
        /// <param name="other_T">Transform of the other metric.</param>
        /// <param name="other">Metric acting as a container.</param>
        /// <param name="offset">Places the center of THIS metric at the offset in this_T coordinates</param>
        public bool FitsAligned(Transform this_T, Transform other_T, Metric other, Vector3 offset = default(Vector3))
        {
            offset -= center;
            var verts = hull != null ? hull.Points.ToArray() : Utils.BoundCorners(bounds);
            for(int i = 0; i < verts.Length; i++)
            {
                var v = other_T.InverseTransformPoint(this_T.position + this_T.TransformDirection(verts[i] + offset));
                if(other.hull != null)
                {
                    if(!other.hull.Contains(v))
                        return false;
                }
                else if(!other.bounds.Contains(v))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true if THIS metric fits inside the given CONTAINER mesh.
        /// </summary>
        /// <param name="this_T">Transform of this metric.</param>
        /// <param name="container_T">Transform of the given mesh.</param>
        /// <param name="container">Mesh acting as a container.</param>
        /// <param name="offset">Places the center of THIS metric at the offset in this_T coordinates</param>
        /// Implemeted using algorithm described at
        /// http://answers.unity3d.com/questions/611947/am-i-inside-a-volume-without-colliders.html
        public bool FitsAligned(Transform this_T, Transform container_T, Mesh container, Vector3 offset = default(Vector3))
        {
            offset -= center;
            //get vertices in containers reference frame
            var verts = hull != null ? hull.Points.ToArray() : Utils.BoundCorners(bounds);
            //check each triangle of container
            var c_verts = container.vertices;
            var triangles = container.triangles;
            var ntris = triangles.Length / 3;
            if(ntris > verts.Length)
            {
                for(int i = 0; i < verts.Length; i++)
                    verts[i] = container_T.InverseTransformPoint(this_T.position + this_T.TransformDirection(verts[i] + offset));
                for(int i = 0; i < ntris; i++)
                {
                    int j = i * 3;
                    var V1 = c_verts[triangles[j]];
                    var V2 = c_verts[triangles[j + 1]];
                    var V3 = c_verts[triangles[j + 2]];
                    var P = new Plane(V1, V2, V3);
                    foreach(var v in verts)
                    {
                        if(!P.GetSide(v))
                            return false;
                    }
                }
            }
            else
            {
                var planes = new Plane[triangles.Length / 3];
                for(int i = 0; i < ntris; i++)
                {
                    int j = i * 3;
                    var V1 = c_verts[triangles[j]];
                    var V2 = c_verts[triangles[j + 1]];
                    var V3 = c_verts[triangles[j + 2]];
                    planes[i] = new Plane(V1, V2, V3);
                }
                for(int i = 0; i < verts.Length; i++)
                {
                    var v = container_T.InverseTransformPoint(this_T.position + this_T.TransformDirection(verts[i] + offset));
                    foreach(var P in planes)
                    {
                        if(!P.GetSide(v))
                            return false;
                    }
                }
            }
            return true;
        }
        #endregion

        #region Operators
        public static Metric operator*(Metric m, float scale)
        {
            var _new = new Metric(m);
            _new.Scale(scale);
            return _new;
        }

        public static Metric operator /(Metric m, float scale) => m * (1.0f / scale);

        //convenience functions
        public static float BoundsVolume(Part part) => (new Metric(part)).bounds_volume;

        public static float BoundsVolume(Vessel vessel) => (new Metric(vessel)).bounds_volume;

        public static float Volume(Part part) => (new Metric(part, true)).volume;

        public static float Volume(Vessel vessel) => (new Metric(vessel, true)).volume;
        #endregion

        #if DEBUG
        public void DrawBox(Transform vT) => Utils.GLDrawBounds(bounds, vT, Color.white);

        public void DrawCenter(Transform vT) => Utils.GLDrawPoint(vT.position + vT.TransformDirection(center), Color.white);

        public override string ToString()
        {
            return Utils.Format("hull:    {}\n" +
                                "bounds:  {}\n" +
                                "center:  {}\n" +
                                "extents: {}\n" +
                                "size:    {}\n" +
                                "volume:  {}\n" +
                                "area:    {}\n" +
                                "mass:    {}\n" +
                                "cost:    {}\n" +
                                "CrewCapacity: {}\n" +
                                "Empty:   {}\n", 
                                hull, bounds, center, extents, size, volume, area, mass, cost, CrewCapacity, Empty);
        }
        #endif
    }
}

