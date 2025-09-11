using KDJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveBooster : MonoBehaviour
{
    [SerializeField] private BoardManager _board;

    // 패시브 아이템에 사용되는 특수블록
    [SerializeField]
    private List<GemType> _specialTypes = new List<GemType>
    {
        GemType.Roller_v,
        GemType.Roller_h,
        GemType.DonutBox,
        GemType.Oven
    };

    [SerializeField] private int _manhattanDist = 3;
    private Coroutine _applyCo;

    [Header("선택 여부")]
    [SerializeField] private bool _useRoller;
    [SerializeField] private bool _useDonut;
    [SerializeField] private bool _useOven;

    private void OnEnable()
    {
        SetPassiveItem(Manager.Stage.GetUseItem());
        if (_applyCo == null)
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

    private void SetPassiveItem(List<bool> useItem)
    {
        if (useItem[0]) _useRoller = useItem[0];
        if (useItem[1]) _useDonut = useItem[1];
        if (useItem[2]) _useOven = useItem[2];
    }

    private IEnumerator WaitAndApplyRoutine()
    {
        if (_board == null) yield break;

        var sp = _board.Spawner;
        while (sp == null || sp.GameBoardData == null || sp.GameBoardData.BlockPlate == null || sp.GameBoardData.BlockArray == null)
        {
            sp = _board.Spawner;
            yield return null;
        }

        int w = sp.GameBoardData.BlockPlate.BlockPlateWidth;
        int h = sp.GameBoardData.BlockPlate.BlockPlateHeight;

        bool filled = false;
        while (!filled)
        {
            filled = true;
            for (int y = 0; y < h && filled; y++)
            {
                for (int x = 0; x < w && filled; x++)
                {
                    var cell = sp.GameBoardData.BlockArray[y, x];
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
        if (_board == null) return;

        var sp = _board.Spawner;
        int w = sp.GameBoardData.BlockPlate.BlockPlateWidth;
        int h = sp.GameBoardData.BlockPlate.BlockPlateHeight;

        var picked = new List<Vector2Int>();

        // Roller (H/V 랜덤)
        if (_useRoller)
        {
            GemType rollerType = (Random.value < 0.5f) ? _specialTypes[0] : _specialTypes[1];
            TryPlaceSpecial(sp, w, h, picked, rollerType);
        }

        if (_useDonut)
            TryPlaceSpecial(sp, w, h, picked, _specialTypes[2]);

        if (_useOven)
            TryPlaceSpecial(sp, w, h, picked, _specialTypes[3]);
    }

    private bool TryPlaceSpecial(BlockSpawner sp, int w, int h, List<Vector2Int> picked, GemType gemType)
    {
        for (int t = 0; t < 200; t++)
        {
            int x = Random.Range(0, w);
            int y = Random.Range(0, h);

            var cell = sp.GameBoardData.BlockArray[y, x];
            if (cell == null || cell.BlockInstance == null) continue;
            if (cell.IsObstacle) continue;
            if (cell.GemType > GemType.Sugar) continue;

            bool ok = true;
            for (int i = 0; i < picked.Count; i++)
            {
                int dist = Mathf.Abs(x - picked[i].x) + Mathf.Abs(y - picked[i].y);
                if (dist < _manhattanDist) { ok = false; break; }
            }
            if (!ok) continue;

            Object.Destroy(cell.BlockInstance);
            sp.GameBoardData.BlockArray[y, x] = null;
            sp.SpawnBlock(x, y, gemType, BoardManager.Instance.BlockMover);

            picked.Add(new Vector2Int(x, y));
            return true;
        }
        return false;
    }
}
