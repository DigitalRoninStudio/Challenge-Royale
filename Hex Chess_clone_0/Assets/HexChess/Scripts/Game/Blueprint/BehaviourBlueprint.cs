﻿using System;
using UnityEngine;

//[CreateAssetMenu(fileName = "BehaviourData", menuName = "BehaviourData/BehaviourData")]
public abstract class BehaviourBlueprint : ScriptableObject
{
    public string Id;
    public string Name;

    public BehaviourVisual Visual;
    public abstract Behaviour CreateBehaviour();
}