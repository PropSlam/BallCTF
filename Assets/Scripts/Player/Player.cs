using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class Player : MonoBehaviour {
    public TeamReactiveProperty team;
    public StringReactiveProperty alias;
    private Text aliasText;

    internal void Death() {
        Destroy(gameObject);
    }

    void Start() {
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
    }

    void OnGUI() {
        var aliasPos = transform.position + Camera.main.transform.up;
        var aliasScreenPos = Camera.main.WorldToScreenPoint(aliasPos);
        aliasText.rectTransform.position = aliasScreenPos;
    }
}