using System;
using UniRx;

public enum Team {
    Yellow,
    Purple
}

[Serializable]
public class TeamReactiveProperty : ReactiveProperty<Team> {
    public TeamReactiveProperty() : base() { }
    public TeamReactiveProperty(Team initialValue) : base(initialValue) { }
}

#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(TeamReactiveProperty))]
public class ExtendInspectorDisplayDrawer : InspectorDisplayDrawer { }
#endif
