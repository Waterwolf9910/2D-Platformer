using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum WeaponType {
    Spear,
    Dagger,
    Rapier,
    Claymore,
    Broadsword,
    Shortsword,
    Staff,
    Bow,
    Crossbow,
    MagicStaff,
    Wand,
    Fist
}


public abstract class Weapon : Item {

    /// <summary>
    /// The Base Damage of the Weapon
    /// </summary>
    public float Damage {
        get;
        protected set;
    }

    /// <summary>
    /// The Weapon Type
    /// </summary>
    public WeaponType Type {
        get;
        protected set;
    }

    /// <summary>
    /// A base weapon for all entities
    /// </summary>
    public static Weapon Fist {
        get;
    } = new FistWeapon();

    /// <summary>
    /// Override to add bonus damage to a weapon
    /// </summary>
    /// <returns>The weapon's bonus damage</returns>
    public virtual float BonusDamage() {
        return 0;
    }

    /// <summary>
    /// The amount of damage goes through entity resistance
    /// </summary>
    /// <returns>The resistence pen of the weapon</returns>
    public virtual float ResistancePen() {
        return 0;
    }

    /// <summary>
    /// How many tiles the weapon can hit pass
    /// </summary>
    /// <returns>The Range of the weapon</returns>
    public virtual int GetRange() {
        return 1;
    }

    public override string ToString() {
        return $"{{Name: {this.Name}\n    Damage: {this.Damage} + {this.BonusDamage()}\n    ResPen: {this.ResistancePen()}\n    Range: ${this.GetRange()}";
    }


    protected Weapon(string name, float damage, WeaponType type) : base(name, 1, 1) {
        this.Type = type;
        this.Damage = damage;
        this.Type = type;
    }
        
    // There should only be one weapon in one inventory slot
    protected override void SetMaxSize(int size) {
        return;
    }

    private class FistWeapon : Weapon {
            
        public FistWeapon() : base("fist", 1, WeaponType.Dagger) {
        }

    }

}
