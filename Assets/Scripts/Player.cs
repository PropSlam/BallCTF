using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour {
    private const float MovementSpeed = 2.5f;

    public TeamReactiveProperty team;
    public StringReactiveProperty alias;
    public ReactiveProperty<bool> alive = new ReactiveProperty<bool>(true);
    public ReactiveProperty<Flag> heldFlag = new ReactiveProperty<Flag>();
    public ReactiveProperty<Vector2> inputVector = new ReactiveProperty<Vector2>();
    public bool local = false;
    public new Rigidbody rigidbody;
    private Text aliasText;
    private NetworkedEntity networkedEntity;
    private Client client;

    void Awake() {
        rigidbody = GetComponent<Rigidbody>();
        networkedEntity = GetComponent<NetworkedEntity>();
        client = FindObjectOfType<Client>();

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
        var playerMove = playerMoveEvent.AsObservable()
            .Select(context => context.ReadValue<Vector2>())
            .Subscribe(input => {
                inputVector.Value = input;
                client.SendPlayerInput(networkedEntity.Id, inputVector.Value);
            });
        gameObject.FixedUpdateAsObservable()
            .WithLatestFrom(inputVector, (_, inputVec) => inputVec)
            .Subscribe(ApplyMovement);

        // Handling death.
        alive.Where(alive => !alive).Subscribe(_ => Destroy(gameObject));
    }

    void ApplyMovement(Vector2 inputVec) {
        var moveVec = new Vector3(inputVec.x, 0, inputVec.y);
        var moveForce = moveVec * MovementSpeed;
        rigidbody.AddForce(moveForce, ForceMode.Acceleration);
    }

    public void SyncState(Vector3 pos, Vector3 vel, Quaternion rot) {
        rigidbody.position = Vector3.Lerp(rigidbody.position, pos, 0.5f);
        rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, vel, 0.5f);
        rigidbody.rotation = Quaternion.Lerp(rigidbody.rotation, rot, 0.5f);
    }

    void OnGUI() {
        // Move alias text to correct position.
        var aliasPos = transform.position + Camera.main.transform.up;
        var aliasScreenPos = Camera.main.WorldToScreenPoint(aliasPos);
        aliasText.rectTransform.position = aliasScreenPos;
    }
}
