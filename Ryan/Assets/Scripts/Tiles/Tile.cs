using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public abstract class Tile {


    private static readonly List<string> CheckedAssemblies = new();

    private static Dictionary<string, Tile> Tiles = new();

    [System.Flags]
    public enum Side {
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8,
        Null = 0
    }

    //private static int _id = 0;

    public string Name {
        get;
    }

    public string Path {
        get;
    }

    public string AltPath {
        get;
    }

    //public int id { get; }  = _id++;

    protected Dictionary<string, Side> allowedNeighbors {
        get;
    } = new();

    /// <summary>
    /// Gets a tile (if has been regisitered)
    /// </summary>
    /// <param name="name">The Name of the tile</param>
    /// <returns>null if there are no registered tile with the name</returns>
    public static Tile? GetTile(string name) {
        RegisterAssemblyTiles();
        if (Tiles.ContainsKey(name.ToLower())) {
            return Tiles[name.ToLower()];
        }
        return null;
    }

    /// <summary>
    /// ONLY ONE SIDE HERE
    /// </summary>
    /// <param name="tile">The Tile to check against</param>
    /// <param name="side">The Side the "tile" gets placed</param>
    /// <returns></returns>
    public bool IsAllowedNeighbor(Tile? tile, Side side) {
        if (tile == null) {
            return false;
        }

        if (!this.allowedNeighbors.ContainsKey(tile.Name)) {
            return false;
        }

        if (!tile.allowedNeighbors.ContainsKey(this.Name)) {
            return false;
        }

        return (tile.allowedNeighbors[tile.Name] & OppositeSide(side)) == OppositeSide(side) && (this.allowedNeighbors[tile.Name] & side) == side; 
    }

    // The names of the allowed tiles
    public List<string> GetAllowedNeighbors(Side side, IEnumerable<string> tiles) {
        return new List<string>(this.allowedNeighbors.Where(kv => { Debug.Log($"({this.Name} -> {kv.Key} | {side} to {kv.Value}): {(kv.Value & side) == side}"); return side == Side.Null || (kv.Value & side) == side; }).Select(a => a.Key).Where(tiles.Contains));
    }

    public virtual bool isWalkable(Entity entity) {
        return true;
    }

    protected void AddAllowedNeighbor(Tile tile, Side sides = Side.Up | Side.Right | Side.Left | Side.Down) {
        if (!this.allowedNeighbors.ContainsKey(tile.Name)) {
            this.allowedNeighbors[tile.Name] = sides;
        }
        this.allowedNeighbors[tile.Name] |= sides;
    }

    protected void RemoveAllowedNeighbor(Tile tile) {
        this.allowedNeighbors.Remove(tile.Name);
    }

    // Remove a side from the allowed sides
    protected void RemoveAllowedNeighbor(Tile tile, Side side) {
        if (!this.allowedNeighbors.ContainsKey(tile.Name)) {
            Debug.LogWarning($"Tile: {this.Name} does not allow the neightbor: {tile.Name}");
            return;
        }
        this.allowedNeighbors[tile.Name] &= ~side;
    }

    protected Tile(string name, string path = null, string altPath = null) {
        this.Name = name.ToLower();
        this.Path = path ?? $"Tiles/{name}";
        if (Resources.Load(altPath) != null) {
            this.AltPath = altPath;
        } else if (Resources.Load(this.Path + "_alt") != null) {
            this.AltPath = this.Path + "_alt";
        } else {
            this.AltPath = this.Path;
        }
        //this.altPath = Resources.Load(altPath) != null ? altPath : null;
        if (!Tiles.ContainsKey(name)) {
            Tiles.Add(name, this);
            //throw new System.InvalidOperationException($"Only one tile and be registered once with the name: {name}\n\"Most\" tiles are usually registered at the start of the program");
        }
            
    }

    private static void RegisterAssemblyTiles() {
        var assembly = Assembly.GetExecutingAssembly();
        if (CheckedAssemblies.Contains(assembly.FullName)) {
            return;
        }
        List<Tile> objs = new();
        foreach (System.Type type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Tile)))) {
            try {
                objs.Add((Tile) System.Activator.CreateInstance(type, true));
            }
            catch { }
        }
        CheckedAssemblies.Add(assembly.FullName);
        foreach(Tile tile in objs) {
            tile.RunFirstAdds();
        }
    }

    protected virtual void RunFirstAdds() { }

    private static Side OppositeSide(Side side) {
        switch (side) {
            case Side.Left: {
                return Side.Right;
            }
            case Side.Right: {
                return Side.Left;
            }
            case Side.Up: {
                return Side.Down;
            }
            case Side.Down: {
                return Side.Up;
            }
            default: {
                throw new System.Exception("An invalid side has bee given");
            }
        }
    }

    static Tile() {
        RegisterAssemblyTiles();
    }
}

