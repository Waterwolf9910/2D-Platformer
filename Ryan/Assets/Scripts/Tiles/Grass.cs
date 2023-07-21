using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Grass : Tile {

    protected Grass() : base("grass") {
    }

    protected override void RunFirstAdds() {
        this.AddAllowedNeighbor(Tile.GetTile("water"), Side.Left | Side.Right);
        this.AddAllowedNeighbor(this);
    }
}

