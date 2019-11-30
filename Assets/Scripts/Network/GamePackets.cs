/*
PlayerInput
    client => server
    - id: int
    - input_vec: v2

PlayerSpawn
    server => client
    - id: int
    - pos: v3
    - team: Team
    - alias: str

PlayerSyncState
    server => client
    id: int
    pos: v3
    vel: v3
    rot: quat ?
    rotVel: quat ?
*/