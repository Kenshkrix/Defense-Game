using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    List<Room> rooms;
    System.Random random;

    public Terrain terrain;
    public GameObject wallPrefab;
    public GameObject floorPrefab;

    public int Seed;
    public int mapWidth, mapLength;
    public int maxHeight;
    public int roomAttempts, roomMinCount;
    public int roomSizeMin, roomSizeMax;
    int BreakOut = 1000;

    public MapCell[,] mapCells;

    private void Awake() {
        mapCells = new MapCell[mapWidth, mapLength];
        random = new System.Random(Seed);
        rooms = new List<Room>();
    }
    private void Start() {
        return; //TODO: Make this actually work
        int r1;// = random.Next(roomSizeMin, roomSizeMax);
        int r2;// = random.Next(roomSizeMin, roomSizeMax);
        int rPosX;// = random.Next(0, mapWidth);
        int rPosZ;// = random.Next(0, mapLength);
        Room room;
        while (rooms.Count < roomMinCount) {
            for (int i = 0; i < roomAttempts; i++) {
                rPosX = random.Next(0, mapWidth);
                rPosZ = random.Next(0, mapLength);
                r1 = random.Next(roomSizeMin, roomSizeMax);
                r2 = random.Next(roomSizeMin, roomSizeMax);

                if (rPosX < 30 || rPosX > 200 || rPosZ < 30 || rPosZ > 200) { continue; }
                bool valid = true;
                Vector3 pos = new Vector3(rPosX * 1.0f, 0.1f, rPosZ * 1.0f);
                room = new Room(pos, r1, 5, r2);
                foreach (Room r in rooms) {
                    for (int w = 0; w < room.width; w++) {
                        for (int l = 0; l < room.length; l++) {
                            if (GetPointInRoom(new Vector3(pos.x + w, 0.1f, pos.z + l), r)) {
                                valid = false;
                            }
                        }
                    }
                }
                if (valid) {
                    rooms.Add(room);
                }

            }
            BreakOut--;
            if (BreakOut <= 0) {
                break;
            }
        }
        foreach (Room r in rooms) {
            GameObject floor = Instantiate(floorPrefab);
            Vector3 scale = floor.transform.localScale;
            scale.Set(r.width, r.length, 1.0f);
            floor.transform.position = r.corner;
            floor.transform.position += Vector3.right * r.width;
            floor.transform.position += Vector3.forward * r.length;
            floor.transform.localScale = scale;
        }
    }




    public bool GetPointInRoom(Vector3 point, Room room) {
        return room.GetPointInRoom(point);
    }
    public bool GetPointOnWall(Vector3 point, Room room) {
        return room.GetPointOnWall(point);
    }
    public static Vector3[] directions = new Vector3[] {
        new Vector3(1,0,0) //east
        , new Vector3(0,0,1) //north
        , new Vector3(-1,0,0) //west
        , new Vector3(0,0,-1) //south
    };
}

public class MapCell : MonoBehaviour {
    public Vector3 coordinates;

    [SerializeField]
    MapCell[] neighbors;
    private void Awake() {
        neighbors = new MapCell[4];
    }

    public MapCell GetNeighbor(Dir dir) {
        return neighbors[(int)dir];
    }
    public void SetNeighbor(Dir dir, MapCell cell) {
        neighbors[(int)dir] = cell;
        cell.neighbors[(int)dir.Opposite()] = this;
    }
}

public enum Dir {
    north, east, south, west
}
public static class DirExtensions {
    public static Dir Opposite(this Dir dir) {
        return (int)dir < 2 ? dir + 2 : dir - 2;
    }
    public static Dir Next(this Dir dir) {
        return dir == Dir.west ? Dir.north : dir + 1;
    }
    public static Dir Previous(this Dir dir) {
        return dir == Dir.north ? Dir.west : dir - 1;
    }
}

public class Room {
    public Vector3 corner;
    public int width;
    public int height;
    public int length;

    public Room(Vector3 Corner, int Width, int Height, int Length) {
        corner = Corner;
        width = Width;
        height = Height;
        length = Length;
    }

    public bool GetPointInRoom(Vector3 point) {
        if (
               point.x >= corner.x
            && point.x <= corner.x + width
            && point.y >= corner.y
            && point.y <= corner.y + height
            && point.z >= corner.z
            && point.z <= corner.z + length
           ) {
            return true;
        }
        return false;
    }
    public bool GetPointOnWall(Vector3 point) {
        if (GetPointInRoom(point)){
            if (point.x == corner.x || point.x == corner.x + width
             || point.y == corner.y || point.y == corner.y + height
             || point.z == corner.z || point.z == corner.z + length) {
                return true;
            }
        }
        return false;
    }
}