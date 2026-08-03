using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class LadderManager : MonoBehaviour
{
    [Header("참조")]
    public Tilemap ladderTilemap;

    [Header("상/하 버튼 UI (생성된 Trigger에 자동 전달)")]
    public GameObject upButton;
    public GameObject downButton;

    [Header("레이어 설정")]
    public string playerLayerName = "Player";
    public string ladderTopLayerName = "LadderTop";

    private void Awake()
    {
        if (ladderTilemap == null)
            ladderTilemap = GetComponent<Tilemap>();

        GenerateLadderTriggers();
    }

    private void GenerateLadderTriggers()
    {
        // X열별로 타일 Y좌표 묶기
        Dictionary<int, List<int>> columnTiles = new Dictionary<int, List<int>>();

        BoundsInt bounds = ladderTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!ladderTilemap.HasTile(pos)) continue;

            if (!columnTiles.ContainsKey(pos.x))
                columnTiles[pos.x] = new List<int>();

            columnTiles[pos.x].Add(pos.y);
        }

        // 각 X열에서 연속된 Y구간을 하나의 사다리로 묶기
        foreach (var column in columnTiles)
        {
            int x = column.Key;
            List<int> yList = column.Value;
            yList.Sort();

            List<(int minY, int maxY)> ladderRanges = new List<(int, int)>();
            int startY = yList[0];
            int prevY = yList[0];

            for (int i = 1; i < yList.Count; i++)
            {
                if (yList[i] == prevY + 1)
                {
                    // 연속된 타일
                    prevY = yList[i];
                }
                else
                {
                    // 끊김 → 하나의 사다리 완성
                    ladderRanges.Add((startY, prevY));
                    startY = yList[i];
                    prevY = yList[i];
                }
            }
            ladderRanges.Add((startY, prevY)); // 마지막 사다리

            // 각 사다리마다 Trigger 오브젝트 생성
            foreach (var range in ladderRanges)
                CreateLadderTrigger(x, range.minY, range.maxY);
        }
    }

    private void CreateLadderTrigger(int tileX, int minTileY, int maxTileY)
    {
        // 타일 좌표 → 월드 좌표 변환
        Vector3 bottomWorld = ladderTilemap.CellToWorld(new Vector3Int(tileX, minTileY, 0));
        Vector3 topWorld = ladderTilemap.CellToWorld(new Vector3Int(tileX, maxTileY, 0));

        Vector3 cellSize = ladderTilemap.cellSize;

        // 사다리 중앙 위치
        float centerX = bottomWorld.x + cellSize.x * 0.5f;
        float centerY = (bottomWorld.y + topWorld.y + cellSize.y) * 0.5f;
        float height = (maxTileY - minTileY + 1) * cellSize.y;

        // 빈 오브젝트 생성
        GameObject obj = new GameObject($"LadderTrigger_X{tileX}_Y{minTileY}~{maxTileY}");
        obj.transform.position = new Vector3(centerX, centerY, 0f);
        obj.transform.SetParent(transform);

        // BoxCollider2D 추가
        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(cellSize.x, height);

        // LadderTrigger 컴포넌트 추가
        LadderTrigger ladder = obj.AddComponent<LadderTrigger>();
        ladder.upButton = upButton;
        ladder.downButton = downButton;
        ladder.playerLayerName = playerLayerName;
        ladder.ladderTopLayerName = ladderTopLayerName;

        Debug.Log($"LadderTrigger 생성 - X:{centerX} / Y:{centerY} / 높이:{height}");
    }
}