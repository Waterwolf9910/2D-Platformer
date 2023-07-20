using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class Item {

    public string Name {
        get;
        protected set;
    }

    public string Description {
        get;
        protected set;
    } = "_no-description_";

    public int Size {
        get;
        protected set;
    }

    public int MaxSize {
        get;
        private set;
    }

    /// <summary>
    /// Creates a copy of the item with the new count
    /// </summary>
    /// <param name="item">The item to copy</param>
    /// <param name="size">The size of the new item</param>
    /// <param name="remainder">The remainder of the amount that could not be set</param>
    /// <returns>The copy of the item</returns>
    public static Item WithSize(Item item, int size, out int remainder) {
        var _item = item.MemberwiseClone() as Item;
        _item.Size = Math.Min(size, item.MaxSize);
        remainder = Math.Max(0, size - item.MaxSize);
        return _item;
    }

    /// <summary>
    /// See <see cref="WithSize(Item, int, ref int)">Item#WithSize(Item, int, ref int)</see>
    /// </summary>
    public static Item WithSize(Item item, int size) {
        return Item.WithSize(item, size, out _);
    }

    public int SetSize(int size) {
        this.Size = Math.Min(size, MaxSize);
        return Math.Max(0, size - this.MaxSize);
    }

    public int AddSize(int size) {
        return this.SetSize(this.Size + size);
    }

    public Item TakeSize(int size) {
        return Item.WithSize(this, Math.Min(size, this.Size));
    }

    protected Item(string name, int size, int maxSize) {
        this.Name = name;
        this.Size = size;
        this.MaxSize = maxSize;
    }

    protected Item(string name, int size) : this(name, size, 48) {

    }

    protected Item(string name) : this(name, 1, 48) {
        
    }

    protected virtual void SetMaxSize(int size) {
        MaxSize = size;
    }

    public override string ToString() {
        return $"{{\n    \"Component\": \"{Name}\", \n    \"Description\": \"{( Description == "_no-description_" ? "No Description" : Description )}\"\n    \"Size\": \"{Size}/{MaxSize}\"\n}}";
    }
}