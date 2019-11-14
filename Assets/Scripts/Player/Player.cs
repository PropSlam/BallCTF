using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

[ExecuteInEditMode]
public class Player : MonoBehaviour {
    public TeamReactiveProperty team;
    public StringReactiveProperty alias;
    private Canvas canvas;
    private Text aliasText;

    internal void Death(ReactOnTouch reactOnTouch) {
        Destroy(this.gameObject);
    }

    void Start() {
        canvas = GetComponentInChildren<Canvas>();
        aliasText = canvas.transform.Find("Alias").GetComponent<Text>();

        team.Subscribe(newTeam => {
            var teamName = newTeam.ToString("G");
            var playerMaterial = Resources.Load<Material>($"Materials/Player/{teamName}");
            GetComponent<Renderer>().material = playerMaterial;
        });
        alias.Subscribe(newAlias => aliasText.text = newAlias);
    }

    void OnGUI() {
        var aliasPos = transform.position + Camera.main.transform.up;
        var aliasScreenPos = Camera.main.WorldToScreenPoint(aliasPos);
        aliasText.rectTransform.position = aliasScreenPos;
    }
}
