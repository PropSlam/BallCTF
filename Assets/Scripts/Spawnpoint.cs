using UnityEngine;

public class Spawnpoint : MonoBehaviour {
    public Team team;

    void OnDrawGizmos() {
        Gizmos.color = team == Team.Yellow ? new Color(1, 1, 0) : new Color(0.5f, 0, 1);
        Gizmos.DrawSphere(transform.position, 0.1f);
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
