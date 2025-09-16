using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResidentState
{
    void Enter();
    void Update(float deltaTime);
    void Exit();
}
