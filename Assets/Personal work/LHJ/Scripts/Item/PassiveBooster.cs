using KDJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveBooster : MonoBehaviour
{
    [SerializeField] private BoardManager _board;

    // 특수블록 ID 매핑
    [SerializeField] private int _rollerVId;
    [SerializeField] private int _rollerHId;
    [SerializeField] private int _ovenId;
    [SerializeField] private int _donutId;

    [SerializeField] private int _manhattanDist;
    private Coroutine _applyCo;

    // 켜짐/꺼짐 토글
    public bool _enabled;

    private void OnEnable()
    {
        if (_enabled && _applyCo == null)
            _applyCo = StartCoroutine(WaitAndApplyRoutine());
    }

    private void OnDisable()
    {
        if (_applyCo != null)
        {
            StopCoroutine(_applyCo);
            _applyCo = null;
        }
    }
    private IEnumerator WaitAndApplyRoutine()
    {
        if (_board == null) yield break;

        var sp = _board.Spawner;
        // 스포너/플레이트/배열 준비 될 때까지 대기
        while (sp == null || sp.BlockPlate == null || sp.BlockArray == null)
        {
            sp = _board.Spawner;
            yield return null;
        }

        // 보드가 실제로 "채워질" 때까지 대기
        int w = sp.BlockPlate.BlockPlateWidth;
        int h = sp.BlockPlate.BlockPlateHeight;

        bool filled = false;
        while (!filled)
        {
            filled = true;
            for (int y = 0; y < h && filled; y++)
            {
                for (int x = 0; x < w && filled; x++)
                {
                    var cell = sp.BlockArray[y, x];
                    if (cell == null || cell.BlockInstance == null)
                        filled = false;
                }
            }
            yield return null;
        }
        ApplyOnStageStart();
    }
    public void ApplyOnStageStart()
    {
        if (!_enabled || _board == null) return;

        var sp = _board.Spawner;
        int w = sp.BlockPlate.BlockPlateWidth;
        int h = sp.BlockPlate.BlockPlateHeight;

        var picked = new List<Vector2Int>();

        // 1) 밀대 1개 (가로/세로 50%)
        int rollerId = (Random.value < 0.5f) ? _rollerHId : _rollerVId;
        TryPlaceSpecial(sp, w, h, picked, rollerId);

        // 2) 도넛상자 1개
        TryPlaceSpecial(sp, w, h, picked, _donutId);

        // 3) 오븐 1개
        TryPlaceSpecial(sp, w, h, picked, _ovenId);
    }

    private bool TryPlaceSpecial(BlockSpawner sp, int w, int h, List<Vector2Int> picked, int blockNum)
    {
        for (int t = 0; t < 200; t++)
        {
            int x = Random.Range(0, w);
            int y = Random.Range(0, h);

            var cell = sp.BlockArray[y, x];
            // 일반 블록만 대상(특수/장애물 제외), 시각 오브젝트가 있는 칸만
            if (cell == null || cell.BlockInstance == null) continue;
            if (cell.IsObstacle) continue;
            if (cell.GemType > GemType.Sugar) continue;

            // 특수블록 거리 보장
            bool ok = true;
            for (int i = 0; i < picked.Count; i++)
            {
                int dist = Mathf.Abs(x - picked[i].x) + Mathf.Abs(y - picked[i].y);
                if (dist < _manhattanDist) { ok = false; break; }
            }
            if (!ok) continue;

            // 해당 칸을 특수블록으로 교체 (스포너 경로 사용)
            Object.Destroy(cell.BlockInstance);
            sp.BlockArray[y, x] = null;
            sp.SpawnBlock(x, y, blockNum);

            picked.Add(new Vector2Int(x, y));
            return true;
        }
        return false;
    }
}
