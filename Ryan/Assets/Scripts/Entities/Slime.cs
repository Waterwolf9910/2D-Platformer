using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Entities {
    internal class Slime : Entity {
        public Slime() : base("Slime", 4, 1, 0) {
            this.Alignment = EntityAlignment.Enemy;

        }
    }
}
