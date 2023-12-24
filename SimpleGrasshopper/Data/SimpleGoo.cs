﻿using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using SimpleGrasshopper.Util;

namespace SimpleGrasshopper.Data;

/// <summary>
/// A simple type of the <see cref="GH_Goo{T}"/>. It just contains the basic of the goo members.
/// </summary>
/// <typeparam name="T"></typeparam>
public class SimpleGoo<T> : GH_Goo<T>
{
    /// <inheritdoc/>
    public override bool IsValid => true;

    /// <inheritdoc/>
    public override string TypeName => typeof(T).Name;

    /// <inheritdoc/>
    public override string TypeDescription => TypeName;

    /// <inheritdoc/>
    public SimpleGoo(T value) : base(value) { }

    /// <inheritdoc/>
    public SimpleGoo() : base() { }

    /// <inheritdoc/>
    public override IGH_Goo Duplicate() => new SimpleGoo<T>(Value);

    /// <inheritdoc/>
    public override string ToString() => Value?.ToString() ?? TypeName + " <Null>";

    /// <inheritdoc/>
    public override bool CastFrom(object source)
    {
        var type = typeof(T);
        var sType = source.GetType();

        if (type.IsAssignableFrom(sType))
        {
            Value = (T)source;
            return true;
        }

        try
        {
            if (Utils.GetOperatorCast(type, type, sType) is MethodInfo method)
            {
                Value = (T)method.Invoke(null, [source]);
                return true;
            }

            if (source is IGH_Goo
                && sType.GetRuntimeProperty("Value") is PropertyInfo property
                && Utils.GetOperatorCast(type, type, property.PropertyType) is MethodInfo method1)
            {
                var v = property.GetValue(source);
                Value = (T)method1.Invoke(null, [v]);
                return true;
            }

            Value = (T)source.ChangeType(typeof(T));
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override bool CastTo<Q>(ref Q target)
    {
        var type = typeof(T);
        var QType = typeof(Q);

        if (QType.IsAssignableFrom(type))
        {
            target = (Q)(object)Value!;
            return true;
        }

        try
        {
            if (Utils.GetOperatorCast(type, QType, type) is MethodInfo method)
            {
                target = (Q)method.Invoke(null, [Value]);
                return true;
            }

            if (target is IGH_Goo
                && QType.GetRuntimeProperty("Value") is PropertyInfo property
                && Utils.GetOperatorCast(type, property.PropertyType, type) is MethodInfo method1)
            {
                var v = method1.Invoke(null, [Value]);
                property.SetValue(target, v);
                return true;
            }

            target = (Q)Value!.ChangeType(QType);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override bool Read(GH_IReader reader)
    {
        string str = string.Empty;
        if (reader.TryGetString(nameof(m_value), ref str))
        {
            try
            {
                m_value = JsonConvert.DeserializeObject<T>(str)!;
            }
            catch
            {

            }
        }
        return base.Read(reader);
    }

    /// <inheritdoc/>
    public override bool Write(GH_IWriter writer)
    {
        writer.SetString(nameof(m_value), JsonConvert.SerializeObject(m_value));
        return base.Write(writer);
    }
}