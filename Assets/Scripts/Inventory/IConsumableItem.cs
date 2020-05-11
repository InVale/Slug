using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IConsumableItem
{
    int CurrentValue { get; }
    int MaxValue { get; }
}
