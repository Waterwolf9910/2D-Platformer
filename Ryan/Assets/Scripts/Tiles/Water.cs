using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Water : Tile {

    public Water() : base("water") {
    }

    protected override void RunFirstAdds() {
        this.AddAllowedNeighbor(Tile.GetTile("grass"));
        this.AddAllowedNeighbor(this);
    }

    public override bool isWalkable(Entity entity) {
        return false; // Might Change
    }
}
