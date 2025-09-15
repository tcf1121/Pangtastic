using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class BlockMaterial : MonoBehaviour
{
    [SerializeField] private Shader _shader;

    void Start()
    {
        var _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.material = new Material(_shader);
    }
}
