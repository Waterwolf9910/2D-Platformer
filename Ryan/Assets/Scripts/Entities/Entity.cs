using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;

public abstract class Entity : MonoBehaviour {

    [SerializeField]
    protected SpriteRenderer _renderer;
    public enum EntityAlignment {
        Ally,
        Enemy,
        Neutral
    }

    /// <summary>
    /// Name of the Entity
    /// </summary>
    public string Name {
        get;
        protected set;
    }

    /// <summary>
    /// The Entity's Max Health
    /// </summary>
    public float MaxHealth { // Health of the Entity
        get;
        protected set;
    }

    /// <summary>
    /// The Entity's Health
    /// </summary>
    public float Health { // Health of the Entity
        get;
        protected set;
    }

    /// <summary>
    /// The Entity's Base Damage
    /// </summary>
    public float BaseDamage { // Entities Base Damage
        get;
        protected set;
    }

    /// <summary>
    /// How much damage the entity can resist
    /// </summary>
    public float Resistance { // How much damage the entity can resist
        get;
        protected set;
    }

    /// <summary>
    /// The Entity's Weapon
    /// </summary>
    public Weapon SelectedWeapon { // The Selected Weapon
        get;
        protected set;
    }

    /// <summary>
    /// What side the entity is on
    /// </summary>
    public EntityAlignment Alignment {
        get;
        protected set;
    } = EntityAlignment.Neutral;

    public long Id {
        get;
    } = _id++;

    public int MaxInventorySize {
        get;
        private set;
    } = 32;

    public int InventorySize => Inventory.Count;

    private static long _id = 0;

    private string spritePath;

    private static Dictionary<string, Sprite> Sprites = new();

    private Dictionary<string, Item> Inventory = new();

    /// <summary>
    /// Adds a weapon to the inventory
    /// </summary>
    /// <param name="weapon">The Weapon to add</param>
    /// <returns>True if the weapon was able to be added</returns>
    public bool AddItem(Weapon weapon) {
        if (this.InventorySize >= this.MaxInventorySize) {
            return false;
        }
        this.Inventory.Add(weapon.Name, weapon);
        this.SelectedWeapon ??= this.SelectedWeapon = weapon;
        return true;
    }

    public bool AddItem(Item item) {
        if (item is Weapon weapon) {
            return this.AddItem(weapon);
        }
        if (this.MaxInventorySize >= this.InventorySize) {
            return false;
        }
        if (Inventory.ContainsKey(name)) {
            this.Inventory[name].AddSize(item.Size);
        }
        this.Inventory.Add(item.Name, item);
        return true;
    }

    public bool RemoveItem(Weapon weapon) {
        if (SelectedWeapon.Name == weapon.Name) {
            this.SelectedWeapon = null;
            foreach (Item item in Inventory.Values) {
                if (item is Weapon _weapon && weapon.Name != _weapon.Name) {
                    this.SelectedWeapon = _weapon;
                }
            }
        }
        return this.Inventory.Remove(weapon.Name);
    }

    public bool RemoveItem(Item item) {
        if (item is Weapon weapon) {
            return this.RemoveItem(weapon);
        }
        return this.Inventory.Remove(item.Name);
    }

    public bool RemoveItem(string name) {
        if (!Inventory.ContainsKey(name)) {
            return false;
        }
        return this.RemoveItem(Inventory[name]);
    }

    /// <summary>
    /// Subtracts health from an entity
    /// Note: if completely overriding make sure to destroy the game object
    /// </summary>
    /// <param name="entity">The entity attacking</param>
    /// <returns>Health Remaining</returns>
    public virtual float HandleDamage(Entity entity) {
        float calculatedDamage = ( ( entity.BaseDamage + entity.SelectedWeapon.Damage + entity.SelectedWeapon.BonusDamage() ) - ( Resistance - entity.SelectedWeapon.ResistancePen() ) );
        if (calculatedDamage < 0) {
            return this.MaxHealth;
        }

        this.MaxHealth -= calculatedDamage;

        if (this.MaxHealth <= 0) {
            Destroy(this.gameObject);
        }


        return this.MaxHealth;
    }

    public virtual bool CanDamage(Entity entity) {
        float calculatedDamage = ( ( entity.BaseDamage + entity.SelectedWeapon.Damage + entity.SelectedWeapon.BonusDamage() ) - ( Resistance - entity.SelectedWeapon.ResistancePen() ) );
        return this.MaxHealth - calculatedDamage <= 0;
    }

    /// <summary>
    /// Toggles state of sprite and returns the renderer
    /// </summary>
    /// <returns>The SpriteRenderer for the Entity</returns>
    public SpriteRenderer ToggleSprite() {
        _renderer.enabled = !_renderer.enabled;
        return _renderer;
    }

    public void MoveTo(Vector3 pos, bool animate = true, Action onComplete = null) {
        onComplete ??= () => { };
        bool xfirst = Vector3.Distance(new(this.transform.position.x, 0), new(pos.x, 0)) < Vector3.Distance(new(0, this.transform.position.y), new(0, pos.y));

        if (animate) {
            this.transform.position = new(this.transform.position.x, this.transform.position.y, pos.z);
            if (xfirst) {
                StartCoroutine(this.XMove(pos, onComplete));
            } else {
                StartCoroutine(this.YMove(pos, onComplete));
            }
        } else {
            this.transform.position = pos;
        }
    }

    private IEnumerator XMove(Vector3 pos, Action onComplete, float speed = .1f, float pause = .01f) {
        this.transform.position = new(Mathf.Round(this.transform.position.x), Mathf.Round(this.transform.position.y), Mathf.Round(this.transform.position.z)); // this should be an int
        while (this.transform.position.x != pos.x) {
            if (this.transform.position.x < pos.x) {
                this.transform.position = new(MathF.Round(this.transform.position.x + speed, 3), this.transform.position.y, this.transform.position.z);
            } else {
                this.transform.position = new(MathF.Round(this.transform.position.x - speed, 3), this.transform.position.y, this.transform.position.z);
            }
            yield return new WaitForSeconds(pause);
        }
        if (this.transform.position.y != pos.y) {
            StartCoroutine(this.YMove(pos, onComplete));
        } else {
            onComplete();
        }
        yield break;
    }

    private IEnumerator YMove(Vector3 pos, Action onComplete, float speed = .1f, float pause = .01f) {
        this.transform.position = new(Mathf.Round(this.transform.position.x), Mathf.Round(this.transform.position.y), Mathf.Round(this.transform.position.z)); // this should be an int
        while (this.transform.position.y != pos.y) {
            if (this.transform.position.y < pos.y) {
                this.transform.position = new(this.transform.position.x, MathF.Round(this.transform.position.y + speed, 3), this.transform.position.z);
            } else {
                this.transform.position = new(this.transform.position.x, MathF.Round(this.transform.position.y - speed, 3), this.transform.position.z);
            }
            yield return new WaitForSeconds(pause);
        }
        if (this.transform.position.x != pos.x) {
            StartCoroutine(this.XMove(pos, onComplete));
        } else {
            onComplete();
        }
        yield break;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="maxHealth"></param>
    /// <param name="baseDamage"></param>
    /// <param name="resistance"></param>
    /// <param name="customSpritePath"></param>
    protected Entity(string name, float maxHealth, float baseDamage, float resistance, string customSpritePath = null) {
        this.Name = name;
        this.MaxHealth = maxHealth;
        this.BaseDamage = baseDamage;
        this.Resistance = resistance;
        this.spritePath = customSpritePath ?? $"Entities/{name}";
        this.AddItem(Weapon.Fist);
        //this.spriterenderer = new() {
        //    sprite = sprites[name] ? sprites[name] = Resources.Load<Sprite>($"Entities/{name}") : sprites[name],
        //    drawMode = SpriteDrawMode.Simple,
        //    enabled = true,
        //    rendererPriority = 5
        //};
    }


    protected Entity(string name, float health, float maxHealth, float baseDamage, float resistance, string customSpritePath = null): this(name, maxHealth, baseDamage, resistance, customSpritePath) {
        this.MaxHealth = health;
    }

    void Start() {

    }

    /// <summary>
    /// Gets a sprite from the cache or loads the sprite from storage
    /// </summary>
    /// <returns>The sprite for the entity</returns>
    //private Sprite GetSprite() {
    //    if (!Sprites.ContainsKey(this.Name)) {
    //        Sprites[this.Name] = Resources.Load<Sprite>(this.spritePath);
    //    } else if (Sprites[this.Name] == null) { // Try to Load Asset Again
    //        Sprites[this.Name] = Resources.Load<Sprite>(spritePath);
    //    }
    //    //var rec = sprites[this.Name].textureRect;
    //    //sprites[this.Name].textureRect.Set(rec.x, rec.y, 256, 256);
    //    return Sprites[this.Name];
    //}

    bool hasRenderer = false;
    void Update() {
        //if (!hasRenderer) {
        //    this.gameObject.TryGetComponent(out SpriteRenderer renderer);
        //    var srenderer = renderer ?? this.gameObject.AddComponent<SpriteRenderer>();
        //    srenderer.enabled = false;
        //    srenderer.sprite = GetSprite();
        //    srenderer.drawMode = SpriteDrawMode.Simple;
        //    srenderer.rendererPriority = 5;
        //    hasRenderer = true;
        //}

    }

}

