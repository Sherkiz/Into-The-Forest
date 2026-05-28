using ITF.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace ITF.Navigation
{
    /// <summary>
    /// 寻路者,采用分层A*寻路算法
    /// </summary>
    public class PathFinder
    {
        //map pass cost buffer
        List<List<int>> map = new();

        //if the cost is greater than or equal to this value, the area will be considered impassable
        public int maxCost = 9999_9999;

        //if the number of continuous entrances is less than this value, the midpoint will be used as a transition point,
        //otherwise, the two ends will be used as transition points
        public uint maxContinuum = 6;

        //the highest level cluster, usually contains the entire map
        MapCluster mapCluster;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="map">map of costs</param>
        /// <param name="hierarchies">hierarchical division, from low to high, representing the array size of the sub-clusters at this level. Uneven division may cause misalignment errors; length less than 1 may cause array out-of-bounds errors</param>
        /// <param name="defaultCost"></param>
        /// <param name="maxCost">maximum cost, areas with a cost greater than this value are considered impassable</param>
        /// <param name="maxContinuum">maximum number of continuous entrances, if the number of continuous entrances is greater than this value, transition points will be generated at both ends, otherwise a transition point will be generated at the center</param>
        public PathFinder(List<List<int>> map, Vector2Int[] hierarchies, int defaultCost, int maxCost = 9999_9999, uint maxContinuum = 6)
        {
            this.maxContinuum = maxContinuum;
            this.maxCost = maxCost;
            this.map = map;

            MapCluster[][] lowerClusters = SplitMap(map, hierarchies[0], defaultCost, maxCost, maxContinuum);
            for (int i = 1; i < hierarchies.Length; i++) lowerClusters = SplitMap(lowerClusters, hierarchies[i]);
            mapCluster = new(lowerClusters, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPoint">starting index of the update area</param>
        /// <param name="costs">updated costs</param>
        public void UpdateMap(Vector2Int startPoint, int[][] costs)
        {
            Vector2Int size = new(costs.Length, costs[0].Length);
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    map[startPoint.x + i][startPoint.y + j] = costs[i][j];
                }
            }
            //expand the range to update the neighboring clusters
            startPoint -= new Vector2Int(1, 1);
            size += new Vector2Int(1, 1);
            mapCluster.UpdateMap(startPoint, size, maxCost, maxContinuum);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point">point to clear the buffer</param>
        /// <returns>returns whether the buffer was successfully cleared</returns>
        public bool ClearBuff(Vector2Int point)
        {
            return mapCluster.ClearBuff(point);
        }

        public void ClearAllBuff()
        {
            mapCluster.ClearAllBuff();
        }

        /// <summary>
        /// Find path based on hierarchical A* algorithm
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="isAbstract">Whether only find abstract path, false by default</param>
        /// <returns>returns the found path, if the search fails, the returned path will be null</returns>
        public ResultPath FindPath(Vector2Int startPoint, Vector2Int endPoint, bool isAbstract = false)
        {
            if (isAbstract) return mapCluster.GetAbstractPath(startPoint, endPoint);
            else return mapCluster.GetCompletePath(startPoint, endPoint);
        }

        /// <summary>
        /// Find path based on A*
        /// </summary>
        /// <param name="map">map costs</param>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="maxCost">maximum cost, areas with a cost greater than this value are considered impassable</param>
        /// <param name="defaultCost">used to calculate the estimated cost</param>
        /// <param name="x">starting x value of the searchable area</param>
        /// <param name="y">starting y value of the searchable area</param>
        /// <param name="width">width of the searchable area, should be greater than 0</param>
        /// <param name="height">height of the searchable area, should be greater than 0</param>
        /// <returns>returns a list of path waypoints, if no path is found, the list will be empty</returns>
        public static ResultPath FindPath(List<List<int>> map, Vector2Int startPoint, Vector2Int endPoint, int maxCost, int defaultCost = 1, int x = 0, int y = 0, int width = -1, int height = -1)
        {
            if (width < 0) width = map.Count;
            if (height < 0) height = map[0].Count;

            List<Vector2Int> path = new();

            List<PathNode> closeNodes = new();
            List<PathNode> openNodes = new();

            openNodes.Add(new PathNode(startPoint, null, Mathf.Abs(endPoint.x - startPoint.x) + Mathf.Abs(endPoint.y - startPoint.y) * defaultCost, 0));

            //A temp function, used to check the neighboring nodes of a specified node, node is the specified node, index is the index of its neighboring node
            System.Action<PathNode, Vector2Int> CheckNeighbor = new((node, index) =>
            {
                int indexG = map[index.x][index.y];
                if (indexG >= maxCost) return;

                int g = node.g + indexG;
                //Check open list
                int i = openNodes.FindIndex(_ => _.index == index);
                if (i >= 0)
                {
                    PathNode temp = openNodes[i];
                    if (g < temp.g)
                    {
                        temp.f += g - temp.g;
                        temp.g = g;
                        temp.parent = node;
                        openNodes.RemoveAt(i);
                        int j = StaticTools.BinaryIndex(openNodes, temp);
                        openNodes.Insert(j, temp);
                    }
                }
                else
                {
                    //Check close list
                    if (closeNodes.Find(_ => _.index == index) == null)
                    {
                        PathNode temp = new(index, node, g + Mathf.Abs(endPoint.x - index.x) + Mathf.Abs(endPoint.y - index.y), g);
                        int j = StaticTools.BinaryIndex(openNodes, temp);
                        openNodes.Insert(j, temp);
                    }
                }
            });

            //Store end node
            PathNode endNode = null;
            while (openNodes.Count > 0)
            {
                PathNode node = openNodes[0];
                openNodes.RemoveAt(0);
                if (node.index == endPoint)
                {
                    endNode = node;
                    break;
                }
                closeNodes.Add(node);

                //up
                Vector2Int index = new(node.index.x, node.index.y - 1);
                if (index.y >= y) CheckNeighbor(node, index);
                //right
                index = new(node.index.x + 1, node.index.y);
                if (index.x < x + width) CheckNeighbor(node, index);
                //down
                index = new(node.index.x, node.index.y + 1);
                if (index.y < y + height) CheckNeighbor(node, index);
                //left
                index = new(node.index.x - 1, node.index.y);
                if (index.x >= x) CheckNeighbor(node, index);
            }

            int g = 0;
            if (endNode != null)
            {
                g = endNode.g;
                if (endPoint != startPoint) path.Insert(0, endPoint);
                while (endNode.parent != null)
                {
                    Vector2Int lastIndex = path[0];
                    //Check whether the current node is a turning point, if not, skip adding it to the path
                    if (!((lastIndex.x == endNode.index.x && endNode.index.x == endNode.parent.index.x) || (lastIndex.y == endNode.index.y && endNode.index.y == endNode.parent.index.y)))
                    {
                        path.Insert(0, endNode.index);
                    }
                    endNode = endNode.parent;
                }
            }

            return new ResultPath(path, g);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        /// <param name="size">the size of partition, must be divisible by the map size, otherwise misalignment errors will be thrown</param>
        /// <param name="defaultCost">default cost</param>
        /// <param name="maxCost">maximum cost, areas with a cost higher than this are considered impassable</param>
        /// <param name="maxContinuum">maximum number of continuous entrances</param>
        /// <returns>returns the divided clusters</returns>
        static MapCluster[][] SplitMap(List<List<int>> map, Vector2Int size, int defaultCost, int maxCost, uint maxContinuum)
        {
            if (map.Count % size.x != 0 || map[0].Count % size.y != 0) 
                throw new UnalignedException("The size division is not aligned with the map size");

            int width = map.Count / size.x;
            int height = map[0].Count / size.y;

            MapCluster[][] clusters = new MapCluster[width][];
            for (int i = 0; i < width; i++)
            {
                clusters[i] = new MapCluster[height];
                for (int j = 0; j < height; j++)
                {
                    clusters[i][j] = new(map, new(i * size.x, j * size.y), size, defaultCost, maxCost, maxContinuum);
                }
            }
            return clusters;
        }

        /// <summary>
        /// Split the map (cannot be used to generate the highest-level clusters)
        /// </summary>
        /// <param name="subClusters">sub-clusters</param>
        /// <param name="size">the size of the array to split into (misalignment will throw an error)</param>
        /// <returns>returns the divided clusters</returns>
        static MapCluster[][] SplitMap(MapCluster[][] subClusters, Vector2Int size)
        {
            if (subClusters.Length % size.x != 0 || subClusters[0].Length % size.y != 0) 
                throw new UnalignedException("The size division is not aligned with the map size");

            int width = subClusters.Length / size.x;
            int height = subClusters.Length / size.y;

            MapCluster[][] clusters = new MapCluster[width][];
            for (int i = 0; i < width; i++)
            {
                clusters[i] = new MapCluster[height];

                for (int j = 0; j < height; j++)
                {
                    MapCluster[][] pClusters = subClusters[(i * size.x)..((i + 1) * size.x)];
                    for (int k = 0; k < pClusters.Length; k++)
                    {
                        pClusters[k] = pClusters[k][(j * size.y)..((j + 1) * size.y)];
                    }
                    clusters[i][j] = new MapCluster(pClusters);
                }
            }

            return clusters;
        }
    }

    /// <summary>
    /// Store the result of pathfinding
    /// </summary>
    public struct ResultPath
    {
        //Store the path's waypoints, an empty list indicates the destination is unreachable
        public List<Vector2Int> path;
        //Actual cost of the path
        public int g;

        public ResultPath(List<Vector2Int> path, int g)
        {
            this.path = path;
            this.g = g;
        }

        /// <summary>
        /// Inverse the path
        /// </summary>
        /// <param name="map">The map where the path is located</param>
        /// <param name="end">The end point of the path after inversion</param>
        /// <returns>Returns the inverted path</returns>
        public ResultPath Inverse(List<List<int>> map, Vector2Int end)
        {
            if (path == null || path.Count == 0) return new(path, g);

            List<Vector2Int> inversePath = new(path);
            inversePath.Reverse();
            inversePath.Add(end);
            Vector2Int start = inversePath[0];
            inversePath.RemoveAt(0);
            return new(inversePath, g - map[start.x][start.y] + map[end.x][end.y]);
        }
    }

    /// <summary>
    /// Unaligned error, thrown when the map division size is not divisible by the map size
    /// </summary>
    public class UnalignedException : System.ApplicationException
    {
        public UnalignedException(string error) : base(error)
        {
        }
    }

    class PathNode : IComparer<PathNode>
    {
        //This node's index in the map
        public Vector2Int index;
        //Parent node. If null, this node is the root node
        public PathNode parent;
        //Total cost of the path
        public int f;
        //Actual cost of the path
        public int g;

        public PathNode(Vector2Int index, PathNode parent, int f, int g)
        {
            this.index = index;
            this.parent = parent;
            this.f = f;
            this.g = g;
        }

        public static bool operator >(PathNode self, PathNode other)
        {
            return self.f > other.f;
        }

        public static bool operator <(PathNode self, PathNode other)
        {
            return self.f < other.f;
        }

        public int Compare(PathNode self, PathNode other)
        {
            if (self.f == other.f) return 0;
            if (self.f > other.f) return 1;
            return -1;
        }
    }

    class MapCluster
    {
        //Start Index in the map
        Vector2Int startIndex;

        Vector2Int size;

        //The map used by this cluster
        List<List<int>> map;

        //if subClusters is null, this cluster is the lowest-level cluster
        MapCluster[][] subClusters;

        //Transition points and their links for this cluster, null indicates this cluster is the highest-level cluster (usually containing the entire map)
        Dictionary<Vector2Int, List<TransitionLink>> links;

        public int defaultCost = 1;

        public int maxCost = 9999_9999;

        //Store the transition links between target point and the transition points
        Dictionary<Vector2Int, List<TransitionLink>> linksBuff = new();

        /// <summary>
        /// Constructor (constructs the lowest-level cluster)
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="startIndex">The start index in the map</param>
        /// <param name="size">The size of the cluster</param>
        /// <param name="defaultCost"></param>
        /// <param name="maxCost">The maximum cost, values greater than or equal to this are considered impassable</param>
        /// <param name="maxContinuum">The maximum number of consecutive entrances, values greater than this will have their ends as transition points</param>
        public MapCluster(List<List<int>> map, Vector2Int startIndex, Vector2Int size, int defaultCost, int maxCost, uint maxContinuum)
        {
            this.map = map;
            this.startIndex = startIndex;
            this.size = size;
            subClusters = null;
            this.defaultCost = defaultCost;
            this.maxCost = maxCost;

            links = GetTransitions(map, startIndex, size, defaultCost, maxCost, maxContinuum);
        }

        /// <summary>
        /// Constructor (used to construct non-lowest-level clusters)
        /// </summary>
        /// <param name="subClusters"></param>
        /// <param name="isGreatest">Indicates whether this cluster is the highest-level cluster, the highest-level cluster will not create transition points</param>
        public MapCluster(MapCluster[][] subClusters, bool isGreatest = false)
        {
            MapCluster sample = subClusters[0][0];
            map = sample.map;
            startIndex = sample.startIndex;
            size = new(sample.size.x * subClusters.Length, sample.size.y * subClusters[0].Length);
            this.subClusters = subClusters;
            defaultCost = sample.defaultCost;
            maxCost = sample.maxCost;

            if (!isGreatest)
            {
                UpdateTransitionLinks();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="size"></param>
        /// <param name="maxCost">The maximum cost, values greater than or equal to this are considered impassable</param>
        /// <param name="maxContinumm">The maximum number of consecutive entrances, values greater than this will have their ends as transition points</param>
        public void UpdateMap(Vector2Int startIndex, Vector2Int size, int maxCost, uint maxContinumm)
        {
            ClearAllBuff();
            if (subClusters == null)
            {
                links = GetTransitions(map, this.startIndex, this.size, defaultCost, maxCost, maxContinumm);
            }
            else
            {
                Vector2Int sIndex = LocatePoint(startIndex);
                Vector2Int eIndex = LocatePoint(new(startIndex.x + size.x - 1, startIndex.y + size.y - 1));

                for (int x = sIndex.x; x <= eIndex.x; x++)
                {
                    if (x < 0) continue;
                    if (x >= subClusters.Length) break;
                    for (int y = sIndex.y; y <= eIndex.y; y++)
                    {
                        if (y < 0) continue;
                        if (y >= subClusters[x].Length) break;
                        subClusters[x][y].UpdateMap(startIndex, size, maxCost, maxContinumm);
                    }
                }

                if (links != null) UpdateTransitionLinks();
            }
        }

        /// <summary>
        /// Gets the abstract path from the start point to the end point.
        /// </summary>
        /// <param name="startPoint">The start point index in the map.</param>
        /// <param name="endPoint">The end point index in the map.</param>
        /// <returns>If the returned path is null, it indicates that the search failed or the end point is unreachable from the start point.</returns>
        public ResultPath GetAbstractPath(Vector2Int startPoint, Vector2Int endPoint)
        {
            if (!IsInCluster(startPoint) || !IsInCluster(endPoint)) return new(null, 0);
            if (endPoint == startPoint) return new(new(), 0);

            if (links != null)
            {
                if (links.TryGetValue(startPoint, out List<TransitionLink> sLinks))
                {
                    TransitionLink link = sLinks.Find(_ => _.index == endPoint);
                    if (link != null) return new ResultPath(link.path, link.g);
                }
                if (linksBuff.TryGetValue(startPoint, out sLinks))
                {
                    TransitionLink link = sLinks.Find(_ => _.index == endPoint);
                    if (link != null) return new(link.path, link.g);
                }
            }

            //Create the links between the end point and the sub-clusters
            if (subClusters != null)
            {
                Vector2Int childIndex = LocatePoint(endPoint);
                List<TransitionLink> endLinks = subClusters[childIndex.x][childIndex.y].GetTransitionLinks(endPoint);
                if (endLinks.Count == 0) return new(null, 0);

                //Whether the start point and end point are in the same sub-cluster
                Vector2Int subIndex = LocatePoint(startPoint);
                if (subIndex == LocatePoint(endPoint))
                {
                    MapCluster subCluster = subClusters[subIndex.x][subIndex.y];
                    ResultPath path = subCluster.subClusters == null ?
                        PathFinder.FindPath(map, startPoint, endPoint, maxCost, defaultCost, subCluster.startIndex.x, subCluster.startIndex.y, subCluster.size.x, subCluster.size.y)
                        : subCluster.GetAbstractPath(startPoint, endPoint);
                    if (path.path != null && path.path.Count > 0) return path;
                }
            }

            return subClusters == null ? PathFinder.FindPath(map, startPoint, endPoint, maxCost, defaultCost, startIndex.x, startIndex.y, size.x, size.y) : BridgePoints(startPoint, endPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPoint">The start point index in the map.</param>
        /// <param name="endPoint">The end point index in the map.</param>
        /// <returns>Return the found path, if the path does not exist, the path value is null.</returns>
        public ResultPath GetCompletePath(Vector2Int startPoint, Vector2Int endPoint)
        {
            ResultPath abstractPath = GetAbstractPath(startPoint, endPoint);
            if (abstractPath.path == null || subClusters == null) return abstractPath;

            ResultPath resultPath = new(new(), 0);

            Vector2Int lastPoint = startPoint;
            foreach (Vector2Int point in abstractPath.path)
            {
                Vector2Int sIndex = LocatePoint(lastPoint);
                Vector2Int eIndex = LocatePoint(point);
                ResultPath path;
                if (sIndex == eIndex) path = subClusters[sIndex.x][sIndex.y].GetCompletePath(lastPoint, point);
                else path = BridgePoints(lastPoint, point);

                if (path.path == null) return path;

                resultPath.path.AddRange(path.path);
                resultPath.g += path.g;

                lastPoint = point;
            }

            return resultPath;
        }

        /// <summary>
        /// Try to clear the link buffer for the specified point.
        /// </summary>
        /// <param name="point">The index of the point to be removed from the map.</param>
        /// <returns>Returns whether the removal was successful.</returns>
        public bool ClearBuff(Vector2Int point)
        {
            //The transition points cannot be cleared using this method
            if (links.ContainsKey(point)) return false;

            if (linksBuff.Remove(point))
            {
                //Record the empty links
                List<Vector2Int> emptyLinks = new();
                foreach (var pair in linksBuff)
                {
                    int index = pair.Value.FindIndex(_ => _.index == point);
                    if (index >= 0)
                    {
                        pair.Value.RemoveAt(index);
                        if (pair.Value.Count == 0) emptyLinks.Add(pair.Key);
                    }
                }

                //Remove the empty links
                foreach (Vector2Int p in emptyLinks)
                {
                    linksBuff.Remove(p);
                }

                //Clear the buffer of sub-cluster
                if (subClusters != null)
                {
                    Vector2Int subIndex = LocatePoint(point);
                    subClusters[subIndex.x][subIndex.y].ClearBuff(point);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearAllBuff()
        {
            linksBuff.Clear();

            //Clear the buffers of sub-clusters
            if (subClusters != null)
            {
                foreach (var clusters in subClusters)
                {
                    foreach (MapCluster cluster in clusters)
                    {
                        cluster.ClearAllBuff();
                    }
                }
            }
        }

        /// <summary>
        /// Is the point in this cluster
        /// </summary>
        /// <param name="point">The index of the point to be checked in the map.</param>
        /// <returns>Returns whether the point is in this cluster.</returns>
        public bool IsInCluster(Vector2Int point)
        {
            return point.x >= startIndex.x && point.y >= startIndex.y && point.x < startIndex.x + size.x && point.y < startIndex.y + size.y;
        }

        /// <summary>
        /// Get the shortest path from the specified point to all transition points in the current cluster. The found paths will be cached.
        /// </summary>
        /// <param name="point">The start point to search from.</param>
        /// <returns>Returns a list of transition links forming paths through sub-cluster transitions. If it is the lowest-level cluster, the paths consist of waypoints.</returns>
        List<TransitionLink> GetTransitionLinks(Vector2Int point)
        {
            List<TransitionLink> result = new();

            //Indicates whether the point is a transition point
            bool isTransition = false;
            if (links != null)
            {
                if (links.TryGetValue(point, out List<TransitionLink> tLinks))
                {
                    result.AddRange(tLinks);
                    isTransition = true;
                }
            }
            if (linksBuff.TryGetValue(point, out List<TransitionLink> transLinks))
            {
                result.AddRange(transLinks);
                return result;
            }

            if (isTransition) return result;

            List<TransitionLink> buffLinks = new();
            if (subClusters == null)
            {
                foreach (var pair in links)
                {
                    ResultPath path = PathFinder.FindPath(map, point, pair.Key, maxCost, defaultCost, startIndex.x, startIndex.y, size.x, size.y);
                    if (path.path.Count > 0) buffLinks.Add(new TransitionLink(pair.Key, path.path, path.g));

                    ResultPath reverse = path.Inverse(map, point);
                    if (linksBuff.TryGetValue(pair.Key, out List<TransitionLink> value)) value.Add(new TransitionLink(point, reverse.path, reverse.g));
                    else linksBuff.Add(pair.Key, new() { new(point, reverse.path, reverse.g) });
                }
            }
            else if (links != null)
            {
                foreach (var pair in links)
                {
                    ResultPath path = BridgePoints(point, pair.Key);
                    if (path.path.Count == 0) continue;
                    buffLinks.Add(new(pair.Key, path.path, path.g));

                    ResultPath reverse = path.Inverse(map, point);
                    if (linksBuff.TryGetValue(pair.Key, out List<TransitionLink> value)) value.Add(new TransitionLink(point, reverse.path, reverse.g));
                    else linksBuff.Add(pair.Key, new() { new(point, reverse.path, reverse.g) });
                }
            }

            linksBuff.Add(point, buffLinks);
            result.AddRange(buffLinks);
            return result;
        }

        /// <summary>
        /// Update transition points and their links (cannot be used for the highest or lowest-level clusters)
        /// </summary>
        void UpdateTransitionLinks()
        {
            if (subClusters == null) return;

            //Store transition points and their sub-cluster links
            Dictionary<Vector2Int, List<TransitionLink>> transitions = new();
            Rect rect = new(subClusters[0][0].startIndex, new(subClusters[0][0].size.x * subClusters.Length, subClusters[0][0].size.y * subClusters[0].Length));
            foreach (MapCluster[] clusters in subClusters)
            {
                foreach (MapCluster cluster in clusters)
                {
                    foreach (var pair in cluster.links)
                    {
                        Vector2Int p = pair.Key;
                        //Check if the point is on the edge
                        if (p.x == rect.x || p.x == rect.xMax - 1 || p.y == rect.y || p.y == rect.yMax - 1)
                        {
                            transitions.Add(pair.Key, pair.Value);
                        }
                    }
                }
            }

            links = new();
            foreach (var pair in transitions)
            {
                List<TransitionLink> tLinks = new();
                foreach (TransitionLink link in pair.Value)
                {
                    if (rect.Contains(link.index)) break;
                    else tLinks.Add(link);
                }
                if (tLinks.Count > 0) links.Add(pair.Key, tLinks);
            }

            //Establish links with other transition points
            List<Vector2Int> visited = new();
            foreach (var pair in links)
            {
                visited.Add(pair.Key);
                foreach (var other in links)
                {
                    if (visited.Contains(other.Key)) continue;
                    ResultPath path = BridgePoints(pair.Key, other.Key);
                    if (path.path.Count == 0) continue;
                    if (path.path.Count > 0)
                    {
                        pair.Value.Add(new(other.Key, path.path, path.g));
                        ResultPath otherPath = path.Inverse(map, pair.Key);
                        other.Value.Add(new(pair.Key, otherPath.path, otherPath.g));
                    }
                }
            }
        }

        /// <summary>
        /// Bridge two points using a node-based A* algorithm
        /// </summary>
        /// <param name="startPoint">The index of the start point.</param>
        /// <param name="endPoint">The index of the end point.</param>
        /// <returns>Returns the shortest path found.</returns>
        ResultPath BridgePoints(Vector2Int startPoint, Vector2Int endPoint)
        {
            List<PathNode> openNodes = new();
            List<PathNode> closeNodes = new();

            openNodes.Add(new(startPoint, null, (Mathf.Abs(endPoint.x - startPoint.x) + Mathf.Abs(endPoint.y - startPoint.y)) * defaultCost, 0));
            PathNode endNode = null;
            while (openNodes.Count > 0)
            {
                PathNode node = openNodes[0];
                openNodes.RemoveAt(0);
                if (node.index == endPoint)
                {
                    endNode = node;
                    break;
                }
                closeNodes.Add(node);

                Vector2Int clusterIndex = LocatePoint(node.index);
                List<TransitionLink> links = subClusters[clusterIndex.x][clusterIndex.y].GetTransitionLinks(node.index);

                foreach (TransitionLink link in links)
                {
                    Vector2Int index = link.index;
                    if (!IsInCluster(index)) continue;

                    int g = node.g + link.g;
                    //Check open list
                    int i = openNodes.FindIndex(_ => _.index == index);
                    if (i >= 0)
                    {
                        PathNode temp = openNodes[i];
                        if (g < temp.g)
                        {
                            temp.f += g - temp.g;
                            temp.g = g;
                            temp.parent = node;
                            openNodes.RemoveAt(i);
                            int j = StaticTools.BinaryIndex(openNodes, temp);
                            openNodes.Insert(j, temp);
                        }
                    }
                    else
                    {
                        //Check close list
                        if (closeNodes.Find(_ => _.index == index) == null)
                        {
                            PathNode temp = new(index, node, g + Mathf.Abs(endPoint.x - index.x) + Mathf.Abs(endPoint.y - index.y), g);
                            int j = StaticTools.BinaryIndex(openNodes, temp);
                            openNodes.Insert(j, temp);
                        }
                    }
                }
            }

            int cost = 0;
            List<Vector2Int> path = new();
            if (endNode != null)
            {
                cost = endNode.g;
                if (endPoint != startPoint) path.Insert(0, endPoint);
                while (endNode.parent != null)
                {
                    endNode = endNode.parent;
                    path.Insert(0, endNode.index);
                }
            }

            return new(path, cost);
        }

        /// <summary>
        /// Get transition points and their links for the specified area.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="startPoint">The starting point of the cluster.</param>
        /// <param name="size">The size of the cluster.</param>
        /// <param name="defaultCost"></param>
        /// <param name="maxCost">The maximum cost. Any cost greater than or equal to this value is considered impassable.</param>
        /// <param name="maxContinuum">The maximum continuum. If the continuous entrance is greater than or equal to this value, two transition points will be recorded; otherwise, the endpoint will be used as the transition point.</param>
        /// <returns>Returns the found transition points and all their links.</returns>
        static Dictionary<Vector2Int, List<TransitionLink>> GetTransitions(List<List<int>> map, Vector2Int startPoint, Vector2Int size, int defaultCost, int maxCost, uint maxContinuum)
        {
            Dictionary<Vector2Int, List<TransitionLink>> links = new();

            //Check transition points, parameters are start point, traversal direction, neighbor direction, iteration count
            System.Action<Vector2Int, Vector2Int, Vector2Int, int> CheckTransition = new((start, dir, nei, count) =>
            {
                RectInt rect = new(0, 0, map.Count, map[0].Count);
                //Record the last continuous entrance boundary point
                Vector2Int lastEntry = start;
                //Indicates whether the previous point was an entrance
                bool last = false;
                //The point currently being traversed
                Vector2Int cPoint = start;
                for (int i = 0; i <= count; i++)
                {
                    if (i < count && map[cPoint.x][cPoint.y] < maxCost && rect.Contains(cPoint + nei) && map[cPoint.x + nei.x][cPoint.y + nei.y] < maxCost)
                    {
                        //Is an entrance
                        if (!last) lastEntry = cPoint;
                        last = true;
                    }
                    else
                    {
                        if (last)
                        {
                            List<Vector2Int> points = new();
                            if (Mathf.Abs(cPoint.x - lastEntry.x) < maxContinuum && Mathf.Abs(cPoint.y - lastEntry.y) < maxContinuum)
                            {
                                points.Add((cPoint + lastEntry) / 2);

                            }
                            else
                            {
                                points.Add(lastEntry);
                                points.Add(cPoint - dir);
                            }
                            foreach (Vector2Int point in points)
                            {
                                if (links.TryGetValue(point, out List<TransitionLink> value))
                                {
                                    Vector2Int nPoint = point + nei;
                                    value.Add(new(nPoint, new() { nPoint }, map[nPoint.x][nPoint.y]));
                                }
                                else
                                {
                                    Vector2Int nPoint = point + nei;
                                    links.Add(point, new() { new(nPoint, new() { nPoint }, map[nPoint.x][nPoint.y]) });
                                }
                            }
                        }
                        last = false;
                    }

                    cPoint += dir;
                }
            });

            //up
            CheckTransition(startPoint, new(1, 0), new(0, -1), size.x);
            //right
            CheckTransition(new(startPoint.x + size.x - 1, startPoint.y), new(0, 1), new(1, 0), size.y);
            //down
            CheckTransition(new(startPoint.x, startPoint.y + size.y - 1), new(1, 0), new(0, 1), size.x);
            //left
            CheckTransition(startPoint, new(0, 1), new(-1, 0), size.y);

            //Store the points that have already been visited
            List<Vector2Int> visited = new();
            //Establish links with other transition points
            foreach (var pair in links)
            {
                visited.Add(pair.Key);
                foreach (var other in links)
                {
                    if (visited.Contains(other.Key)) continue;
                    ResultPath path = PathFinder.FindPath(map, pair.Key, other.Key, maxCost, defaultCost, startPoint.x, startPoint.y, size.x, size.y);
                    if (path.path.Count > 0)
                    {
                        pair.Value.Add(new(other.Key, path.path, path.g));
                        ResultPath otherPath = path.Inverse(map, pair.Key);
                        other.Value.Add(new(pair.Key, otherPath.path, otherPath.g));
                    }
                }
            }

            return links;
        }

        /// <summary>
        /// Gets the position of the specified point within a sub-cluster.
        /// </summary>
        /// <param name="point">The index of the point to locate.</param>
        /// <returns>Returns the index of the sub-cluster it belongs to. If there are no sub-clusters, returns its relative coordinates to the starting index.</returns>
        Vector2Int LocatePoint(Vector2Int point)
        {
            if (subClusters == null) return point - startIndex;

            point -= startIndex;
            return new(point.x / subClusters[0][0].size.x, point.y / subClusters[0][0].size.y);
        }

        /// <summary>
        /// Transition link, stores link information with other transition points.
        /// </summary>
        class TransitionLink
        {
            //The index of this transition in the map
            public Vector2Int index;

            //The actual cost of the path between this transition
            public int g;

            /// <summary>
            /// The cached path between this transition; can be null, in which case the path will be searched each time.
            /// For the lowest-level clusters, this value stores the actual path's turning points; for other levels, it stores transition indices.
            /// </summary>
            public List<Vector2Int> path;

            public TransitionLink(Vector2Int index, List<Vector2Int> path, int g)
            {
                this.index = index;
                this.path = path;
                this.g = g;
            }
        }
    }
}