using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public List<EnemyBase> enemies = new List<EnemyBase>();
    public List<RoomDoor> doorsToOpen;
    public Portal portal;
    public bool isBossRoom = false;
    public bool isCleared = false;

    void Start()
    {
        // Auto-find enemies that are children
        EnemyBase[] found = GetComponentsInChildren<EnemyBase>();
        foreach (var e in found)
        {
            enemies.Add(e);
            e.room = this;
        }
    }

    public void OnEnemyDied(EnemyBase enemy)
    {
        enemies.Remove(enemy);
        if (enemies.Count == 0)
        {
            CompleteRoom();
        }
    }

    public void OnBossDefeated(EnemyBase boss)
    {
        enemies.Remove(boss);
        CompleteRoom();
    }

    void CompleteRoom()
    {
        if (isCleared) return;
        isCleared = true;

        if (doorsToOpen != null)
        {
            foreach (var door in doorsToOpen)
            {
                if (door != null) door.Open();
            }
        }

        if (isBossRoom && portal != null)
        {
            portal.Activate();
        }
    }
}
