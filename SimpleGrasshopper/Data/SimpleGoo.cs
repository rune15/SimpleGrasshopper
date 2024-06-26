﻿using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using SimpleGrasshopper.Util;

namespace SimpleGrasshopper.Data;

/// <summary>
/// A simple type of the <see cref="GH_Goo{T}"/>. It just contains the basic of the goo members.
/// </summary>
/// <typeparam name="T"></typeparam>
public class SimpleGoo<T> : GH_Goo<T>
{
    /// <inheritdoc/>
    public override bool IsValid => !GetValue(nameof(IsValid), out bool v) || v;

    /// <inheritdoc/>
    public override string IsValidWhyNot => GetValue(nameof(IsValidWhyNot), out string v) ? v : base.IsValidWhyNot;

    /// <inheritdoc/>
    public override string TypeName => GetValue(nameof(TypeName), out string v) ? v : typeof(T).Name;

    /// <inheritdoc/>
    public override string TypeDescription => GetValue(nameof(TypeDescription), out string v) ? v : TypeName;

    /// <inheritdoc/>
    public SimpleGoo(T value) : base(value) { }

    /// <inheritdoc/>
    public SimpleGoo() : base() { }

    private bool GetValue<TP>(in string propertyName, out TP value)
    {
        var property = typeof(T).GetRuntimeProperty(propertyName);
        if (property == null || !property.CanRead)
        {
            value = default!;
            return false;
        }
        value = (TP)property.GetValue(Value);
        return true;
    }

    /// <inheritdoc/>
    public override IGH_Goo Duplicate() => new SimpleGoo<T>(Value);

    /// <inheritdoc/>
    public override string ToString()
    {
        var str = Value?.ToString();
        if (!string.IsNullOrEmpty(str))
        {
            var format = "{0}";

            foreach (var func in AssemblyPriority.TypeStringFormats)
            {
                if (!func(typeof(T), out var fo)) continue;
                format = fo;
                break;
            }
            return string.Format(format, str);
        }

        return TypeName + " <Null>";
    }

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
        else if (source is IGH_Goo
            && sType.GetRuntimeProperty("Value") is PropertyInfo property)
        {
            var v = property.GetValue(source);

            if (type.IsAssignableFrom(v.GetType()))
            {
                Value = (T)v;
                return true;
            }
        }

        try
        {
            if (SimpleUtils.GetOperatorCast(type, type, sType) is MethodInfo method)
            {
                Value = (T)method.Invoke(null, [source]);
                return true;
            }

            if (source is IGH_Goo
                && sType.GetRuntimeProperty("Value") is PropertyInfo property
                && SimpleUtils.GetOperatorCast(type, type, property.PropertyType) is MethodInfo method1)
            {
                var v = property.GetValue(source);
                Value = (T)method1.Invoke(null, [v]);
                return true;
            }

            if (AssemblyPriority.CastFromDict.TryGetValue(type, out var dele))
            {
                var o = source;
                if (source is IGH_Goo
                && sType.GetRuntimeProperty("Value") is PropertyInfo prop)
                {
                    o = prop.GetValue(o);
                }

                if (dele(o, out var v))
                {
                    Value = (T)v;
                    return true;
                }
            }

            Value = (T)source.ChangeType(type);
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
        else if (target is IGH_Goo
            && QType.GetRuntimeProperty("Value") is PropertyInfo property)
        {
            if (property.PropertyType.IsAssignableFrom(type))
            {
                property.SetValue(target, Value);
                return true;
            }
        }

        try
        {
            if (SimpleUtils.GetOperatorCast(type, QType, type) is MethodInfo method)
            {
                target = (Q)method.Invoke(null, [Value]);
                return true;
            }

            if (target is IGH_Goo
                && QType.GetRuntimeProperty("Value") is PropertyInfo property
                && SimpleUtils.GetOperatorCast(type, property.PropertyType, type) is MethodInfo method1)
            {
                var v = method1.Invoke(null, [Value]);
                property.SetValue(target, v);
                return true;
            }

            var t = QType;

            if (target is IGH_Goo
                && QType.GetRuntimeProperty("Value") is PropertyInfo prop)
            {
                t = prop.PropertyType;
            }

            if (AssemblyPriority.CastToDict.TryGetValue(t, out var dele)
                && dele(Value!, out var v1))
            {
                if (target is IGH_Goo
                    && QType.GetRuntimeProperty("Value") is PropertyInfo p)
                {
                    p.SetValue(target, v1);
                    return true;
                }
                else
                {
                    target = (Q)v1;
                    return true;
                }
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
        if (reader.Read<T>("Value", out var value))
        {
            m_value = value;
        }
        return base.Read(reader);
    }

    /// <inheritdoc/>
    public override bool Write(GH_IWriter writer)
    {
        if (m_value != null)
        {
            writer.Write("Value", m_value);
        }
        return base.Write(writer);
    }
}