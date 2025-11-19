using System;
using System.Collections.Generic;
using UnityEngine;
using ModeFeature;
using System.Linq;

public class DataMode : DataBase
{
    public readonly string core;
    public readonly Dictionary<Type, ModeFeatureBase> features;

    public DataMode(SmartXmlNode e) : base(e)
    {
        this.core = $"ModeComponent.{e.GetChildText("Core")}";
        this.features = e.GetChildren("Feature")
            .Select(x => (Type: Type.GetType($"ModeFeature.{x.GetAttributeText("Type")}"), Xml: x))
            .ToDictionary(x => x.Type, x => Activator.CreateInstance(x.Type, x.Xml) as ModeFeatureBase);
    }

    public override void Verify(DataManager manager)
    {
        if (Type.GetType(core) == null)
        {
            Debug.Log($"core type error({core})");
        }

        foreach (var feature in features.Values)
        {
            feature.Verify(manager);
        }
    }

    public bool TryGetFeature<T>(out T result) where T : ModeFeatureBase
    {
        result = null;
        if (features.TryGetValue(typeof(T), out var v))
        {
            result = v as T;
        }

        return result != null;
    }
}
