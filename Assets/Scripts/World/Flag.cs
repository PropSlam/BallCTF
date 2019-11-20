using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Animations;

public class Flag : MonoBehaviour {
    public TeamReactiveProperty team;
    public ReactiveProperty<Player> heldBy = new ReactiveProperty<Player>();

    void Awake() {
        // Subscribe to team changes.
        team.Subscribe(newTeam => {
            var teamName = newTeam.ToString("G");
            var playerMaterial = Resources.Load<Material>($"Materials/Flag/{teamName}");
            GetComponent<Renderer>().material = playerMaterial;
        });

        var positionConstraint = GetComponent<PositionConstraint>();
        var originalPosition = transform.position;

        // Set heldBy when another team's player enters the trigger.
        this.OnTriggerEnterAsObservable()
            .Select(other => other.GetComponent<Player>())
            .Where(player => player && player.team.Value != team.Value && !heldBy.Value)
            .Subscribe(player => heldBy.Value = player);

        // Whenever a player grabs the flag...
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
                .Subscribe(_ => heldBy.Value = null);
        });

        // Whenever the flag is dropped...
        var dropped = heldBy
            .SkipUntil(grabbed)
            .Where(player => player == null);
        dropped.Subscribe(_ => {
            // Reset position constraint.
            positionConstraint.RemoveSource(0);
            positionConstraint.constraintActive = false;
            transform.position = originalPosition;
        });
    }
}
