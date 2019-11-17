using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {
    private const float MovementSpeed = 1.8f;

    public TeamReactiveProperty team;
    public StringReactiveProperty alias;
    private new Rigidbody rigidbody;
    private Text aliasText;

    internal void Death() {
        Destroy(gameObject);
    }

    void Start() {
        rigidbody = GetComponent<Rigidbody>();

        // Instantiate Alias text into main UI canvas and clean up with OnDestroy
        aliasText = GameObject.Instantiate(Resources.Load<Text>("Prefabs/Player/Alias"));
        aliasText.transform.SetParent(GameObject.FindGameObjectWithTag("UI").transform);
        this.OnDestroyAsObservable().Where(_ => aliasText)
            .Subscribe(_ => Destroy(aliasText.gameObject));

        // Subscribe to team changes.
        team.Subscribe(newTeam => {
            var teamName = newTeam.ToString("G");
            var playerMaterial = Resources.Load<Material>($"Materials/Player/{teamName}");
            GetComponent<Renderer>().material = playerMaterial;
        });

        // Subscribe to alias changes.
        alias.Subscribe(newAlias => aliasText.text = newAlias);

        // Subscribe to inputs.
        var actionEvents = GetComponent<PlayerInput>().actionEvents;

        // Movement inputs.
        var playerMoveEvent = actionEvents[actionEvents.IndexOf(action => action.actionName == "Player/Move")];
        var playerMove = playerMoveEvent.AsObservable().Select(context => context.ReadValue<Vector2>());
        gameObject.UpdateAsObservable()
            .WithLatestFrom(playerMove, (_, inputVec) => inputVec)
            .Subscribe(inputVec => {
                var moveVec = new Vector3(inputVec.x, 0, inputVec.y);
                var moveForce = moveVec * MovementSpeed;
                rigidbody.AddForce(moveForce, ForceMode.Acceleration);
            });
    }

    void OnGUI() {
        // Move alias text to correct position.
        var aliasPos = transform.position + Camera.main.transform.up;
        var aliasScreenPos = Camera.main.WorldToScreenPoint(aliasPos);
        aliasText.rectTransform.position = aliasScreenPos;
    }
}