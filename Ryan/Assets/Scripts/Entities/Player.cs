using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity {

    private GameObject _goDel;

    // Use this for initialization
    void Start() {
        Utils.GetBattleManager().OnBattleEnd += (won) => {
            if (!won) {
                // Run Respawn or smth like that
            }
            if (_goDel != null) {
                Destroy(_goDel);
                _goDel = null;
            }
        };
    }

    // Update is called once per frame
    void Update() {

    }

    public Player() : base("player", 50, 3, 3) {
        this.Alignment = EntityAlignment.Ally;
    }

    public void ActivateBattle(GameObject initialGO, List<EntitySelection> enemies) {
        _goDel = initialGO;
        var battle = Utils.GetBattleManager();
        var dic = new Dictionary<EntitySelection, BoardSide> {
            { new(this, 1), BoardSide.Left }
        };
        enemies.ForEach(e => {
            dic.Add(e, BoardSide.Right);
        });
        battle.CreateNew(dic);
        battle.StartBattle(Tile.GetTile("grass"), Tile.GetTile("water"));
    }
}