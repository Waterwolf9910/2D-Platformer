using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
using UnityEngine.SceneManagement;


public class EntitySelection {

    public Entity Entity {
        get;
    }

    public int Amount {
        get;
    }

    public EntitySelection(Entity entity, int amount) {
        this.Entity = entity;
        this.Amount = amount;
    }
}

public enum BoardSide {
    Left,
    Right,
    Top,
    Bottom,
    BottomLeft,
    BottomRight,
    TopLeft,
    TopRight
}

public class BattleManager : MonoBehaviour {

    //private Entity entity;
    //private GameObject EntityObject;
    private Dictionary<Vector2Int, TileData> Board = new();
    private List<Vector2Int> CompletedTurns = new();
    private Dictionary<int, bool> PrevActiveGO = new();
    private int BoardX = 9;
    private int BoardY = 16;
    private int EnemyCount = 0;
    private int AllyCount = 0;
    private int _id = 0;
    private bool CursorSelected = false;
    private bool CursorReversed = false;
    private bool IsEntityMoving = false;
    private bool AttackMode = false;
    private float cursorFadeSpeed = .5f;
    private BattleTurn Turn = BattleTurn.Ally;
    private Scene PrevScene;
    private Scene BoardScene;
    [SerializeField]
    private Camera Cam;
    private GameObject Cursor;
    private Vector2Int SelectedPos = Vector2Int.zero;
    private static Sprite CustomCursor;
    private static Color CursorColor = Color.yellow;

    private enum BattleTurn {
        Ally,
        Enemy,
        Neutral,
    }

    /// <summary>
    /// Creates a new copy and adds a entity to the board
    /// </summary>
    /// <param name="entity">The entity to copy</param>
    /// <param name="side">Which Side to spawn the entity on</param>
    public void AddEntity(Entity entity, BoardSide side = BoardSide.Left, int maxAttempts = 100) {
        Vector2Int pos = new();
        void GenPos() {
            switch (side) {
                case BoardSide.Left: {
                        pos = new(Random.Range(0, this.BoardX / 2), Random.Range(0, this.BoardY));
                        break;
                    }
                case BoardSide.Right: {
                        pos = new(Random.Range(this.BoardX / 2, this.BoardX), Random.Range(0, this.BoardY));
                        break;
                    }
                case BoardSide.Bottom: {
                        pos = new(Random.Range(0, this.BoardX), Random.Range(0, this.BoardY / 2));
                        break;
                    }
                case BoardSide.Top: {
                        pos = new(Random.Range(0, this.BoardX), Random.Range(this.BoardY / 2, this.BoardY));
                        break;
                    }
                case BoardSide.TopLeft: {
                        pos = new(Random.Range(0, this.BoardX / 2), Random.Range(this.BoardY / 2, this.BoardY));
                        break;
                    }
                case BoardSide.TopRight: {
                        pos = new(Random.Range(this.BoardX / 2, this.BoardX), Random.Range(this.BoardY / 2, this.BoardY));
                        break;
                    }
                case BoardSide.BottomLeft: {
                        pos = new(Random.Range(0, this.BoardX / 2), Random.Range(0, this.BoardY / 2));
                        break;
                    }
                case BoardSide.BottomRight: {
                        pos = new(Random.Range(this.BoardX / 2, this.BoardX), Random.Range(0, this.BoardY / 2));
                        break;
                    }
            }
        }
        GenPos();
        int attempts = 0;
        while (this.Board[pos].entity != null && this.Board[pos].tile.isWalkable(entity)) {
            if (attempts == maxAttempts) {
                Debug.LogException(new System.Exception("Error Finding Placement"));
                return;
            }
            ++attempts;
            GenPos();
        }
        //this.board[pos] = Instantiate(entity, new(pos.x, pos.y), Quaternion.identity);
        this.Board[pos].entity = Instantiate(entity, new(pos.x, pos.y, -0.05f), Quaternion.identity);
        SceneManager.MoveGameObjectToScene(this.Board[pos].entity.gameObject, this.BoardScene);
        this.Board[pos].entity.gameObject.name = $"{this.Board[pos].entity.Name} #{this.Board[pos].entity.Id}";
        //this.board[pos].transform.SetParent(null);
        if (this.Board[pos].entity.Alignment == Entity.EntityAlignment.Enemy) {
            this.EnemyCount++;
        } else if (this.Board[pos].entity.Alignment == Entity.EntityAlignment.Ally) {
            this.AllyCount++;
        }
    }

    /// <summary>
    /// Creates a new copy and adds a entity to the board
    /// </summary>
    /// <param name="entity">The entity to copy</param>
    /// <param name="pos">Where to spawn the entity</param>
    public void AddEntity(Entity entity, Vector2Int pos) {
        if (this.Board[pos].entity != null) {
            Debug.LogException(new System.Exception("There is already an entity here"));
            return;
        }
        //this.board[pos] = Instantiate(entity, new(pos.x, pos.y), Quaternion.identity);
        this.Board[pos].entity = Instantiate(entity, new(pos.x, pos.y, -0.05f), Quaternion.identity);
        SceneManager.MoveGameObjectToScene(this.Board[pos].entity.gameObject, this.BoardScene);
        this.Board[pos].entity.gameObject.name = $"{this.Board[pos].entity.Name}  # {this.Board[pos].entity.Id}";
        //this.board[pos].transform.SetParent(null);
        if (this.Board[pos].entity.Alignment == Entity.EntityAlignment.Enemy) {
            this.EnemyCount++;
        } else if (this.Board[pos].entity.Alignment == Entity.EntityAlignment.Ally) {
            this.AllyCount++;
        }
    }

    /// <summary>
    /// Moves the Entity across the board
    /// </summary>
    /// <param name="fromx">The X from coordinate</param>
    /// <param name="fromy">the X from coordinate</param>
    /// <param name="tox">The X to coordinate</param>
    /// <param name="toy">The Y to coordinate</param>
    /// <param name="attack">To attack the entity (if there is one and opposite alignment)</param>
    public void MoveEntity(int fromx, int fromy, int tox, int toy, bool attack) {
        this.MoveEntity(new(fromx, fromy), new(tox, toy), attack);
    }

    /// <summary>
    /// Moves the Entity across the board
    /// </summary>
    /// <param name="from">The from coordinate </param>
    /// <param name="to">The to coordinate</param>
    /// <param name="attack">To attack the at the position (if there is one and opposite alignment)</param>
    public void MoveEntity(Vector2Int from, Vector2Int to, bool attack) {
        Vector2Int pos = to;
        if (this.Board[to].entity != null) {
            pos = Direction(from, to);
        }
        this.IsEntityMoving = true;
        this.Board[from].entity.MoveTo(new(pos.x, pos.y, this.Board[from].entity.transform.position.z), true, () => {
            var _toInfo = this.Board[to];
            var _fromInfo = this.Board[from];
            if (pos != to) {
                if (attack) {
                    if (_toInfo.entity.HandleDamage(_fromInfo.entity) <= 0) {
                        if (_toInfo.entity.Alignment == Entity.EntityAlignment.Enemy) {
                            this.EnemyCount--;
                        } else if (_toInfo.entity.Alignment == Entity.EntityAlignment.Ally) {
                            this.AllyCount--;
                        }
                        _toInfo.entity = null;

                    }
                }
            }
            if (pos != from) {
                this.Board[pos].entity = this.Board[from].entity;
                this.Board[from].entity = null;
            }
            this.CompletedTurns.Add(pos);
            this.IsEntityMoving = false;
        });
    }

    /// <summary>
    /// Get entity movement information
    /// </summary>
    /// <param name="entity">The entity that will move</param>
    /// <param name="x">The X coordinate to check</param>
    /// <param name="y">The Y coordinate to check</param>
    /// <returns>If the tile is free or attackable</returns>
    public (bool free, bool attack) CanMove(Entity entity, int x, int y) {
        return this.CanMove(entity, new(x, y));
    }

    /// <summary>
    /// Get entity movement information
    /// </summary>
    /// <param name="entity">The entity that will move</param>
    /// <param name="x">The X coordinate to check</param>
    /// <param name="y">The Y coordinate to check</param>
    /// <returns>If the tile is free or attackable</returns>
    public (bool free, bool attack) CanMove(Entity entity, Vector2Int pos) {
        var _info = this.Board[pos];
        return (free: _info.entity == null && _info.tile.isWalkable(entity), attack: _info.entity != null && _info.entity.Alignment != entity.Alignment);
    }

    /// <summary>
    /// Creates a new battle scenario
    /// </summary>
    /// <param name="initEntities">The entities to add</param>
    /// <param name="x">The horizontal size of the board</param>
    /// <param name="y">The vertical size of the board</param>
    public void CreateNew(Dictionary<EntitySelection, BoardSide> initEntities, int x = 16, int y = 9) {
        this.Board.Clear();
        for (var _y = 0; _y < y; _y++) {
            for (var _x = 0; _x < x; _x++) {
                this.Board[new(_x, _y)] = new(null, null);
            }
        }
        this.BoardScene = SceneManager.CreateScene($"BattleScene {_id++}");
        this.BoardY = y;
        this.BoardX = x;
        SceneManager.MoveGameObjectToScene(this.gameObject, this.BoardScene);
        this.gameObject.transform.position = new(x / 2.13f, y / 2.3f, -20);
        foreach (var placement in initEntities) {
            int i = 0;
            while (placement.Key.Amount > i) {
                i++;
                this.AddEntity(placement.Key.Entity, placement.Value);
            }
        }
    }

    /// <summary>
    /// Creates a new battle scenario
    /// </summary>
    /// <param name="initEntities">The entites and their positions on the board</param>
    /// <param name="x">The horizontal size of the board</param>
    /// <param name="y">The vertical size of the board</param>
    public void CreateNew(Dictionary<Vector2Int, Entity> initEntities, int x = 16, int y = 9) {
        this.Board.Clear();
        this.BoardScene = SceneManager.CreateScene($"BattleScene {_id++}");
        this.BoardY = y;
        this.BoardX = x;
        for (var _y = 0; _y < y; _y++) {
            for (var _x = 0; _x < x; _x++) {
                this.Board[new(_x, _y)] = new(null, null);
            }
        }
        SceneManager.MoveGameObjectToScene(this.gameObject, this.BoardScene);
        this.gameObject.transform.position = new(x / 2.13f, y / 2.3f, -20);
        foreach (var placement in initEntities) {
            if (placement.Key.x > x - 1 && placement.Key.y > y - 1) {
                Debug.LogException(new System.InvalidOperationException("Inital entity placements are not allowed to be outside of board"));
                return;
            }
            this.AddEntity(pos: placement.Key, entity: placement.Value);
        }
    }

    /// <summary>
    /// <para></para>
    /// Call <see cref="CreateNew">CreateNew</see> Before Running
    /// </summary>
    public void StartBattle(params Tile[] tiles) {
        this.PrevScene = SceneManager.GetActiveScene();

        foreach (GameObject go in this.PrevScene.GetRootGameObjects()) {
            this.PrevActiveGO[go.GetInstanceID()] = go.activeSelf;
            go.SetActive(false);
        }

        //Misc.Utils.LoadAsync(boardScene, prevScene, () => {
        this.Cam.enabled = true;
        //cam.fieldOfView = 30;
        this.InBattle = true;
        var loadedTiles = new Dictionary<string, (GameObject normal, Tile tile, GameObject alt)>();
        foreach (var tile in tiles) {
            var go1 = new GameObject();
            var sr1 = go1.AddComponent<SpriteRenderer>();
            var go2 = new GameObject();
            var sr2 = go2.AddComponent<SpriteRenderer>();

            go1.transform.position = Vector3.zero;
            go1.name = tile.Name;
            sr1.enabled = false;
            sr1.sortingOrder = 0;
            sr1.rendererPriority = int.MaxValue;
            sr1.sprite = Resources.Load<Sprite>(tile.Path);
            sr1.rendererPriority = 5;
            sr1.drawMode = SpriteDrawMode.Simple;

            go2.transform.position = Vector3.zero;
            go2.name = tile.Name + "_alt";
            sr2.enabled = false;
            sr2.sortingOrder = 0;
            sr2.rendererPriority = int.MaxValue;
            sr2.sprite = Resources.Load<Sprite>(tile.AltPath ?? tile.Path);
            sr2.rendererPriority = 5;
            sr2.drawMode = SpriteDrawMode.Simple;

            loadedTiles.Add(tile.Name, (go1, tile, go2));
        }
        SceneManager.SetActiveScene(this.BoardScene);
        var random = new System.Random();
        var _lastTile = tiles[random.Next(0, (loadedTiles.Count / 2) + 1)];
        //await Task.Run(() => {
        for (int x = 0; x < this.BoardX; x++) {
            for (int y = 0; y < this.BoardY; y++) {
                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                Tile.Side side;
                if (y == 0) {
                    side = x == 0 ? Tile.Side.Null : Tile.Side.Right;
                } else {
                    side = Tile.Side.Up;
                }
                var _allowed = _lastTile.GetAllowedNeighbors(side, loadedTiles.Select(kv => kv.Key));
                (GameObject normal, Tile _tile, GameObject alt) = x == 0 ? loadedTiles[_lastTile.Name] : loadedTiles[_allowed[random.Next(0, _allowed.Count)]];

                var tile = Instantiate(isOffset ? alt : normal, new Vector3(x, y), Quaternion.identity);

                var sr = tile.GetComponent<SpriteRenderer>();
                sr.enabled = true;
                tile.name = $"{tile.name} ({x}, {y})";
                _lastTile = _tile;
                this.Board[new(x, y)].tile = _tile;
            }
        }

        foreach (var tile in loadedTiles) {
            Destroy(tile.Value.normal);
            Destroy(tile.Value.alt);
        }

        foreach (var tileInfo in this.Board) {
            var entity = tileInfo.Value.entity;
            if (entity == null) {
                continue;
            }
            var sr = entity.GetSpriteRenderer(false);
            var calcX = entity.transform.position.x - (1 - sr.sprite.textureRect.width / 256);
            var calcY = entity.transform.position.y - (1 - sr.sprite.textureRect.height / 256);
            entity.transform.position = new Vector3(calcX > 0 && entity.transform.position.x > .5 ? calcX : entity.transform.position.x, calcY > 0 && entity.transform.position.y > .5 ? calcY : entity.transform.position.y, entity.transform.position.z);
        }
        //});
    }

    public void SetAttackMode(bool enabled) {
        this.AttackMode = enabled;
    }

    public event System.Action<bool> OnBattleEnd = (bool won) => { };

    //private GameObject cursor = new();

    // Start is called before the first frame update
    void Start() {
        //this.Cam = this.gameObject.GetComponent<Camera>();
        //entity.GetComponent<SpriteRenderer>().enabled = false;
    }

    public bool InBattle {
        get;
        private set;
    } = false;

    private void EndBattle() {
        if (this.InBattle) {
            this.InBattle = false;
            this.Turn = BattleTurn.Ally;
            this.AttackMode = false;
            this.AllyCount = 0;
            this.EnemyCount = 0;
            this.Cam.enabled = false;
            //foreach(var entityInfo in this.board) {
            //    Destroy(entityInfo.Value.gameObject);
            //}
            this.Board.Clear();
            SceneManager.MoveGameObjectToScene(this.gameObject, PrevScene);
            SceneManager.UnloadSceneAsync(this.BoardScene);
            SceneManager.SetActiveScene(this.PrevScene);
            foreach (var go in this.PrevScene.GetRootGameObjects()) {
                if (!this.PrevActiveGO.ContainsKey(go.GetInstanceID())) {
                    continue;
                }
                go.SetActive(this.PrevActiveGO[go.GetInstanceID()]);
            }
            Turn = BattleTurn.Ally;
        }
    }

    public void SetCustomCursor(Sprite sprite) {
        if (sprite == null) {
            sprite = Resources.Load<Sprite>("tilecursors/Square");
        }
        this.Cursor.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    // Update is called once per frame
    void Update() {

        if (Input.GetKeyDown(KeyCode.Comma)) {
            //if (entity) Object.Destroy(entity.gameObject);
            //if (EntityObject) Object.Destroy(EntityObject);
            //EntityObject = new();
            //var entity = EntityObject.AddComponent<Hello>();
            //var e = GameObject.Instantiate(entity);
            //e.transform.position = new(0, 0);
            //e.ToggleSprite();

            //entity = Instantiate<Entities.Entity>(entity, this.transform);

            var go = new GameObject();
            var e = go.AddComponent<Hello>();
            var e2 = go.AddComponent<World>();
            var dic = new Dictionary<EntitySelection, BoardSide>() {
                [new(e, 3)] = BoardSide.Left,
                [new(e2, 3)] = BoardSide.Right,
            };
            this.EndBattle();
            this.CreateNew(dic);
            try {
                this.AddEntity(e, pos: new(15, 8));
            } catch {
                try {
                    this.AddEntity(e, pos: new(0, 8));
                } catch { }
            }

            this.StartBattle(Tile.GetTile("grass"));
            Destroy(go);
        }
        if (!this.InBattle) {
            return;
        }
        //if (this.Board.Count == 0) { // Board gets cleared when rebuilding
        //    foreach (GameObject go in this.BoardScene.GetRootGameObjects()) {
        //        if (go.TryGetComponent<Entity>(out var entity)) {
        //            this.Board.Add(new(Mathf.FloorToInt(entity.transform.position.x), Mathf.FloorToInt(entity.transform.position.y)), entity);
        //        }
        //    }
        //}
        SpriteRenderer cr;
        if (this.Cursor == null) {
            this.Cursor = new();
            cr = this.Cursor.AddComponent<SpriteRenderer>();
            cr.enabled = true;
            cr.sprite = CustomCursor != null ? CustomCursor : Resources.Load<Sprite>("tilecursors/Square");
            cr.color = CursorColor;
            cr.drawMode = SpriteDrawMode.Simple;
            cr.rendererPriority = 4;
            cr.sortingOrder = 4;
        } else {
            cr = this.Cursor.GetComponent<SpriteRenderer>();
        }
        this.CameraActions();
        if (this.Turn == BattleTurn.Ally) {
            this.BattleInputActions();
        }
        if (this.CursorSelected) {
            if (cr.color.a > 0.25f && !this.CursorReversed) {
                cr.color = new(cr.color.r, cr.color.g, cr.color.b, cr.color.a - (cursorFadeSpeed * Time.deltaTime));
                cr.material.color = cr.color;
            } else if (cr.color.a < 1 && this.CursorReversed) {
                cr.color = new(cr.color.r, cr.color.g, cr.color.b, cr.color.a + (cursorFadeSpeed * Time.deltaTime));
                cr.material.color = cr.color;
            } else {
                this.CursorReversed = !this.CursorReversed;
            }
        } else {
            var _pos = this.Cam.ScreenToWorldPoint(Input.mousePosition);
            this.Cursor.transform.position = new(Mathf.RoundToInt(_pos.x), Mathf.RoundToInt(_pos.y), this.Cursor.transform.position.z);
            cr.color = new(cr.color.r, cr.color.g, cr.color.b, 1f);
            cr.material.color = cr.color;
        }
        if (this.EnemyCount < 1) {
            this.OnBattleEnd.Invoke(true);
            this.EndBattle();
        } else if (this.AllyCount < 1) {
            this.OnBattleEnd.Invoke(false);
            this.EndBattle();
        }

    }

    private Vector2Int Direction(Vector2Int from, Vector2Int to) {
        bool up = from.y > to.y;
        bool down = from.y < to.y;
        bool right = from.x > to.x;
        bool left = from.x < to.x;
        Vector2Int pos = new(to.x, to.y);
        Vector2Int pos2 = new(to.x, to.y);

        if (up) {
            pos.y += 1;
        } else if (down) {
            pos.y -= 1;
        }

        if (right && (pos.y == to.y - 1 || pos.y == to.y + 1)) {
            pos2.x += 1;
        } else if (right) {
            pos.x += 1;
        } else if (left && (pos.y == to.y - 1 || pos.y == to.y + 1)) {
            pos2.x -= 1;
        } else if (left) {
            pos.x -= 1;
        }

        if (pos2 != to) {
            pos = Vector2Int.Distance(pos2, to) > Vector2Int.Distance(pos, to) ? pos : pos2;
        }
        return pos;
    }

    private void BattleInputActions() {
        //var cr = this.Cursor.GetComponent<SpriteRenderer>();
        if (Input.GetKeyDown(KeyCode.E)) {
            EnemyTurn();
            this.CompletedTurns.Clear();
        }
        if (Input.GetMouseButtonDown(0)) {
            if (this.IsEntityMoving) {
                return;
            }
            var subject = this.Board[this.SelectedPos].entity;
            var _pos = this.Cam.ScreenToWorldPoint(Input.mousePosition);
            var pos = new Vector2Int(Mathf.RoundToInt(_pos.x), Mathf.RoundToInt(_pos.y));
            //new(Input.mousePosition.x, Input.mousePosition.y, this.gameObject.transform.position.z)
            // Debug.Log($"{Input.mousePosition} {cam.ScreenToWorldPoint(Input.mousePosition)} {Camera.main.ScreenToWorldPoint(Input.mousePosition)}");
            // Debug.Log($"({pos.x}) ({pos.y})");
            //Debug.Log(this.Board[pos]);
            if (!this.CursorSelected) {
                this.Cursor.transform.position = new(pos.x, pos.y);
                this.CursorSelected = true;
                this.SelectedPos = pos;
            } else if (pos.x > this.BoardX || pos.x < 0 || pos.y > this.BoardY || pos.y < 0 || this.SelectedPos == pos) {
                this.CursorSelected = false;
                //this.SelectedPos = Vector2Int.zero;
            } else if (this.CursorSelected) {
                if (this.Turn == BattleTurn.Ally && subject != null && subject.Alignment == Entity.EntityAlignment.Ally && !this.CompletedTurns.Contains(SelectedPos)) {
                    var (free, attack) = this.CanMove(subject, pos);
                    if (free) {
                        this.MoveEntity(this.SelectedPos, pos, false);
                    } else if (attack && this.AttackMode) {
                        this.MoveEntity(this.SelectedPos, pos, true);
                    } else {
                        return;
                    }
                    this.CursorSelected = false;
                } else {
                    this.Cursor.transform.position = new(pos.x, pos.y);
                    this.SelectedPos = pos;
                }
                //this.SelectedPos = Vector2Int.zero;
            }
        }
        if (Input.GetKeyDown(KeyCode.A)) {
            this.AttackMode = !this.AttackMode;
        }
        // TODO: Control GUI
        if (Input.GetKey(KeyCode.Slash)) {
        }
    }
    private void EnemyTurn() {
        var allies = this.Board.Where(dic => dic.Value.entity != null).Where(dic => dic.Value.entity.Alignment == Entity.EntityAlignment.Ally).ToDictionary(a => a.Key, a => a.Value);
        var enemies = this.Board.Where(dic => dic.Value.entity != null).Where(dic => dic.Value.entity.Alignment == Entity.EntityAlignment.Enemy).ToList();
        foreach (var enemy in enemies) {
            EnemyAttack(enemy, allies);
        }
    }

    // TODO: Check if in inpassable tile
    private void EnemyAttack(KeyValuePair<Vector2Int, TileData> entityInfo, Dictionary<Vector2Int, TileData> allies) {
        var to = allies.OrderBy(val => Vector2Int.Distance(entityInfo.Key, val.Key)).First().Key;
        MoveEntity(entityInfo.Key, to, true);
    }

    private void CameraActions() {
        if (Input.GetKeyDown(KeyCode.R)) {
            this.transform.position = new(this.BoardX / 2.13f, this.BoardY / 2.3f, -20);
        }
        
        if ((Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus)) && this.Cam.orthographicSize > 0.55) {
            this.Cam.orthographicSize -= .05f;
            //this.transform.position = new(this.transform.position.x, this.transform.position.y, this.transform.position.z + .1f);
        }
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus) && this.Cam.orthographicSize < 10) {
            this.Cam.orthographicSize += .05f;
            //this.transform.position = new(this.transform.position.x, this.transform.position.y, this.transform.position.z - .1f);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.Keypad6)) {
            this.transform.position = new(this.transform.position.x + .05f, this.transform.position.y, this.transform.position.z);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Keypad4)) {
            this.transform.position = new(this.transform.position.x - .05f, this.transform.position.y, this.transform.position.z);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Keypad8)) {
            this.transform.position = new(this.transform.position.x, this.transform.position.y + .05f, this.transform.position.z);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.Keypad2)) {
            this.transform.position = new(this.transform.position.x, this.transform.position.y - .05f, this.transform.position.z);
        }
    }

    private class TileData {
        public Entity entity;
        public Tile tile;

        public TileData(Entity entity, Tile tile) {
            this.entity = entity;
            this.tile = tile;
        }

    }

    class Hello : Entity {

        public Hello() : base("Triangle", 1, 0, 0) {
            this.Alignment = EntityAlignment.Enemy;
        }
    }

    class World : Entity {
        public World() : base("Circle", 1, 0, 0) {
            this.Alignment = EntityAlignment.Ally;
        }
    }
}

