using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

public struct CaptureContext {
    public Player player;
}

[System.Serializable]
public class CapturedEvent : UnityEvent<CaptureContext> {}

public class Flag : MonoBehaviour {
    public TeamReactiveProperty team;
    public ReactiveProperty<Player> heldBy = new ReactiveProperty<Player>();
    public CapturedEvent captured = new CapturedEvent();

    void Awake() {
        // Subscribe to team changes.
        team.Subscribe(newTeam => {
            var teamName = newTeam.ToString("G");
            var playerMaterial = Resources.Load<Material>($"Materials/Flag/{teamName}");
            GetComponent<Renderer>().material = playerMaterial;
        });

        var positionConstraint = GetComponent<PositionConstraint>();
        var originalPosition = transform.position;

        // When a player enters this un-held flag trigger.
        var playerEnters = this.OnTriggerEnterAsObservable()
            .Where(_ => !heldBy.Value)
            .Select(other => other.GetComponent<Player>())
            .Where(player => player);

        // Grab flag when player on other team enters the trigger.
        playerEnters
            .Where(player => player.team.Value != team.Value)
            .Subscribe(GrabBy);

        // Capture flag when player on same team enters the trigger with other team's flag.
        playerEnters
            .Where(player => player.team.Value == team.Value && player.heldFlag.Value)
            .Subscribe(player => {
                player.heldFlag.Value.captured.Invoke(new CaptureContext {
                    player = player
                });
            });

        // When this flag is grabbed...
        var grabbed = heldBy.Where(player => player);
        grabbed.Subscribe(player => {
            // Set position constraint to follow player.
            positionConstraint.AddSource(new ConstraintSource {
                sourceTransform = player.transform,
                weight = 1f
            });
            positionConstraint.constraintActive = true;

            // Drop when player dies.
            player.alive
                .Where(alive => !alive)
                .Subscribe(_ => Drop());
        });

        // When this flag is dropped...
        var dropped = heldBy.SkipUntil(grabbed).Where(player => player == null);
        dropped.Subscribe(_ => {
            // Reset position constraint.
            positionConstraint.RemoveSource(0);
            positionConstraint.constraintActive = false;
            transform.position = originalPosition;
        });

        // When this flag is captured...
        captured.AsObservable().Subscribe(capture => {
            Debug.Log(team.Value + " flag captured by: " + capture.player.alias);
            Drop();
        });
    }

    public void GrabBy(Player player) {
        heldBy.Value = player;
        player.heldFlag.Value = this;
    }

    public void Drop() {
        heldBy.Value.heldFlag.Value = null;
        heldBy.Value = null;
    }
}
