using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public abstract class Tile {


    private static readonly List<string> CheckedAssemblies = new();

    private static Dictionary<string, Tile> Tiles = new();

    public enum Side {
        Up,
        Down,
        Left,
        Right,
        All
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

    protected Dictionary<string, float> allowedNeighbors {
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

    public bool IsAllowedNeighbor(Tile? tile) {
        if (tile == null) {
            return true;
        }
        return tile.IsAllowedNeighbor(this) || this.allowedNeighbors.ContainsKey(tile.Name); 
    }

    protected void AddAllowedNeighbor(Tile tile, Side sides = Side.Up | Side.Right | Side.Left | Side.Down) {
        //TODO: ()
        throw new System.Exception("Not Implemented");
        //this.allowedNeighbors[tile.Name] = weight;
    }

    protected void RemoveAllowedNeighbor(Tile tile) {
        this.allowedNeighbors.Remove(tile.Name);
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
        if (Tiles.ContainsKey(name)) {
            throw new System.InvalidOperationException($"Only one tile and be registered once with this name: {name}\nTiles are usually registered");
        }
        Tiles.Add(name, this);
        this.AddAllowedNeighbor(this);
            
    }

    private static void RegisterAssemblyTiles() {
        var assembly = Assembly.GetExecutingAssembly();
        if (CheckedAssemblies.Contains(assembly.FullName)) {
            return;
        }
        foreach (System.Type type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Tile)))) {
            try {
                System.Activator.CreateInstance(type, true);
            }
            catch { }
        }
        CheckedAssemblies.Add(assembly.FullName);
    }

    static Tile() {
        RegisterAssemblyTiles();
    }
}

