using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 64 位有符号定点数, 其中 1 个符号位，16 个小数位
/// </summary>
public struct Fixed
{
    long buffer64;

    public Fixed(float value)
    {
        var max = MaxValue;
        if (value > max)
        {
            throw new Exception($"[Fixed] value overflow max value: " + max);
        }
        buffer64 = (long)(value * 65536);
    }

    public Fixed(int value)
    {
        buffer64 = value * 65536;
    }

    public Fixed(double value)
    {
        var max = MaxValue;
        if (value > max)
        {
            throw new Exception($"[Fixed] value overflow max value: " + max);
        }
        var min = MinValue;
        if (value < min)
        {
            throw new Exception($"[Fixed] value overflow min value: " + max);
        }
        buffer64 = (long)(value * 65536);
    }

    public Fixed(long value)
    {
        var max = MaxValue;
        if (value > max)
        {
            throw new Exception($"[Fixed] value overflow max value: " + max);
        }
        var min = MinValue;
        if(value < min)
        {
            throw new Exception($"[Fixed] value overflow min value: " + max);
        }
        buffer64 = value * 65536;
    }

    public float ToFloat()
    {
        var value = (float)((double)buffer64 / 65536);
        return value;
    }

    public int ToInt()
    {
        var value = (int)(buffer64 / 65536);
        return value;
    }

    public double ToDouble()
    {
        var value = ((double)buffer64 / 65536);
        return value;
    }

    public long ToLong()
    {
        var value = buffer64 / 65536;
        return value;
    }

    public override string ToString()
    {
        var floatValue = ToFloat();
        var floatStirng = floatValue.ToString();
        return floatStirng;
    }

    public static implicit operator float(Fixed @fixed)
    {
        return @fixed.ToFloat();
    }

    public static implicit operator int(Fixed @fixed)
    {
        return @fixed.ToInt();
    }

    public static implicit operator double(Fixed @fixed)
    {
        return @fixed.ToDouble();
    }

    public static implicit operator long(Fixed @fixed)
    {
        return @fixed.ToLong();
    }

    public static implicit operator Fixed(float value)
    {
        return new Fixed(value);
    }

    public static implicit operator Fixed(int value)
    {
        return new Fixed(value);
    }

    public static implicit operator Fixed(double value)
    {
        return new Fixed(value);
    }

    public static implicit operator Fixed(long value)
    {
        return new Fixed(value);
    }

    public static Fixed MaxValue
    {
        get
        {
            var value = long.MaxValue / 65536;
            var fixedValue = new Fixed(value);
            return fixedValue;
        }
    }

    public static Fixed MinValue
    {
        get
        {
            var value = long.MinValue / 65535;
            var fixedValue = new Fixed(value);
            return fixedValue;
        }
    }
    public static bool operator ==(Fixed a, Fixed b)
    {
        var aBuffer = a.buffer64;
        var bBuffer = b.buffer64;
        return aBuffer == bBuffer;
    }

    public static bool operator !=(Fixed a, Fixed b)
    {
        var aBuffer = a.buffer64;
        var bBuffer = b.buffer64;
        return aBuffer != bBuffer;
    }

    public static Fixed operator +(Fixed a, Fixed b)
    {
        var aLongValue = a.ToLong();
        var bLongValue = b.ToLong();
        var totalValue = aLongValue + bLongValue;
        return totalValue;
    }

    public static Fixed operator -(Fixed a, Fixed b)
    {
        var aLongValue = a.ToLong();
        var bLongValue = b.ToLong();
        var totalValue = aLongValue - bLongValue;
        return totalValue;
    }

    public static Fixed operator *(Fixed a, Fixed b)
    {
        var aLongValue = a.ToLong();
        var bLongValue = b.ToLong();
        var totalValue = aLongValue * bLongValue;
        return totalValue;
    }

    public static Fixed operator /(Fixed a, Fixed b)
    {
        var aLongValue = a.ToLong();
        var bLongValue = b.ToLong();
        var totalValue = aLongValue / bLongValue;
        return totalValue;
    }

    public override bool Equals(object obj)
    {
        var fixedValue = (Fixed)obj;
        var b = this == fixedValue;
        return b;
    }

    public override int GetHashCode()
    {
        throw new Exception("[Fixed] cannot get HashCode");
    }
}
