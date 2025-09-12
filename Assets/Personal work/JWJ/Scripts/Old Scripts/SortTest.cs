using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SortTest : MonoBehaviour
{
    [SerializeField] private List<string> options;

    private void Start()
    {
        int[] priorityNums = { 1, 3, 5, 7, 9 };

        options.Sort((a, b) =>
        {
            int aNum = int.Parse(a);
            int bNum = int.Parse(b);

            bool aHas = priorityNums.Contains(aNum);
            bool bHas = priorityNums.Contains(bNum);

            if (aHas == bHas) return 0;
            return aHas ? -1 : 1;
        });

        Debug.LogError ("=== 정렬 결과 ===");
        foreach (var opt in options)
        {
            Debug.LogError(opt);
        }
    }
}
