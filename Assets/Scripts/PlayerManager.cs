using System.Collections.Generic;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

class PlayerManager {
    public static readonly List<Player> players = new List<Player>();

    private static Player Spawn(Vector3 pos, Team team) {
        var playerPrefab = Resources.Load<GameObject>("Prefabs/Player/Player");
        var playerObj = Object.Instantiate(playerPrefab, pos, Quaternion.identity);
        var player = playerObj.GetComponent<Player>();
        player.team.Value = team;
        players.Add(player);
        return player;
    }

    public static Player SpawnServer(Vector3 pos, Team team) {
        var player = Spawn(pos, team);
        var playerId = player.GetComponent<NetworkedEntity>().Id;
        player.alias.Value = $"Player #{playerId}";
        return player;
    }

    public static Player SpawnClient(int id, bool local, Vector3 pos, Team team, string alias) {
        var player = Spawn(pos, team);
        player.GetComponent<NetworkedEntity>().Id = id;
        player.alias.Value = alias;
        player.local = local;

        if (local) {
            Camera.main.GetComponent<LookAtConstraint>().AddSource(new ConstraintSource {
                sourceTransform = player.transform,
                weight = 1
            });

            Camera.main.GetComponent<PositionConstraint>().AddSource(new ConstraintSource {
                sourceTransform = player.transform,
                weight = 1
            });
        } else {
            Object.Destroy(player.GetComponent<PlayerInput>());
        }

        return player;
    }

    public static void HandlePlayerLeave(Player player) {
        players.Remove(player);
        Object.Destroy(player.gameObject);
    }
}
