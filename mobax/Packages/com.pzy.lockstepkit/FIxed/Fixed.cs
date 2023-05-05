using System;

[Serializable]
/// <summary>
/// 64 位有符号定点数, 其中 1 个符号位，16 个小数位
/// </summary>
public struct Fixed
{
    long buffer64;

    public Fixed(float value)
    {
        buffer64 = (long)(value * 65536);
    }

    public Fixed(int value)
    {
        buffer64 = (long)value << 16;
    }

    public Fixed(double value)
    {
        buffer64 = (long)(value * 65536);
    }

    public Fixed(long value)
    {
        buffer64 = value << 16;
    }

    public float ToFloat()
    {
        var value = (float)((double)buffer64 / 65536);
        return value;
    }

    public int ToInt()
    {
        var value = (int)(buffer64 >> 16);
        return value;
    }

    public double ToDouble()
    {
        var value = ((double)buffer64 / 65536);
        return value;
    }

    public long ToLong()
    {
        var value = buffer64 >> 16;
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
            //var value = long.MaxValue / 65536;
            var value = long.MaxValue >> 16;
            var fixedValue = new Fixed(value);
            return fixedValue;
        }
    }

    public static Fixed MinValue
    {
        get
        {
            //var value = long.MinValue / 65535;
            var value = long.MinValue >> 16;
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
        var aBuffer = a.buffer64;
        var bBuffer = b.buffer64;
        var retBuffer = aBuffer + bBuffer;
        var ret = new Fixed();
        ret.buffer64 = retBuffer;
        return ret;
    }

    public static Fixed operator -(Fixed a, Fixed b)
    {
        var aBuffer = a.buffer64;
        var bBuffer = b.buffer64;
        var retBuffer = aBuffer - bBuffer;
        var ret = new Fixed();
        ret.buffer64 = retBuffer;
        return ret;
    }

    public static Fixed operator *(Fixed a, Fixed b)
    {
        var aBuffer = a.buffer64;
        var bBuffer = b.buffer64;
        var temp = aBuffer * bBuffer;
        var retBuffer = temp >> 16;
        var ret = new Fixed();
        ret.buffer64 = retBuffer;
        return ret;
    }

    public static Fixed operator /(Fixed a, Fixed b)
    {
        var aBuffer = a.buffer64;
        var bBuffer = b.buffer64;
        var temp = aBuffer / bBuffer;
        var retBuffer = temp << 16;
        var ret = new Fixed();
        ret.buffer64 = retBuffer;
        return ret;
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