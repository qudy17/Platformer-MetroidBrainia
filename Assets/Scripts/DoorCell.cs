using UnityEngine;
using System;

// Просто контейнер для позиции тайла
[Serializable]
public struct DoorCell
{
    public Vector3Int cellPosition; // Позиция в координатах Tilemap
}