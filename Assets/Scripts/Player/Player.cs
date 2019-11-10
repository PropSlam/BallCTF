using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {
    public Team team;
    public string alias;
    private Canvas canvas;
    private Text aliasText;

    void Start() {
        var teamName = team.ToString("G");
        var playerMaterial = Resources.Load<Material>($"Materials/Player/{teamName}");
        GetComponent<Renderer>().material = playerMaterial;

        canvas = GetComponentInChildren<Canvas>();
        aliasText = canvas.transform.Find("Alias").GetComponent<Text>();
        aliasText.text = alias;
    }

    void OnGUI() {
        var aliasPos = transform.position + Camera.main.transform.up;
        var aliasScreenPos = Camera.main.WorldToScreenPoint(aliasPos);
        aliasText.rectTransform.position = aliasScreenPos;
    }
}
