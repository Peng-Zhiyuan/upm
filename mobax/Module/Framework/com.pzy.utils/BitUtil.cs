using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
public static class BitUtil 
{
    public static bool IsBitSet(ulong bitBucket, int indexBaseFrom0)
    {
        if(indexBaseFrom0 >= 64)
        {
            throw new Exception("[BitUtil] bit index out of range (0 ~ 63)");
        }
        return (bitBucket & (1UL << indexBaseFrom0)) != 0;
    }

    public static bool IsBitSet(uint bitBucket, int indexBaseFrom0)
    {
        if (indexBaseFrom0 >= 32)
        {
            throw new Exception("[BitUtil] bit index out of range (0 ~ 32)");
        }
        return (bitBucket & (1UL << indexBaseFrom0)) != 0;
    }

    public static ulong SetBit(ulong bitBucket, int indexBaseFrom0)
    {
        bitBucket |= (1UL << indexBaseFrom0);
        return bitBucket;
    }

    public static uint SetBit(uint bitBucket, int indexBaseFrom0)
    {
        bitBucket |= (1U << indexBaseFrom0);
        return bitBucket;
    }

    public static bool IsBitSet(ulong[] bitBucket, int indexBaseFrom0)
    {
        if(bitBucket == null)
        {
            return false;
        }
        var index = indexBaseFrom0 / 64;
        var indexInBitBucket = indexBaseFrom0 % 64;
        if(index >= bitBucket.Length)
        {
            return false;
        }
        var bitBucketValue = bitBucket[index];
        return IsBitSet(bitBucketValue, indexInBitBucket);
    }

    public static bool IsBitSet(uint[] bitBucket, int indexBaseFrom0)
    {
        if (bitBucket == null)
        {
            return false;
        }
        var index = indexBaseFrom0 / 32;
        var indexInBitBucket = indexBaseFrom0 % 32;
        if (index >= bitBucket.Length)
        {
            return false;
        }
        var bitBucketValue = bitBucket[index];
        return IsBitSet(bitBucketValue, indexInBitBucket);
    }

    public static ulong SetBit(ulong[] bitBucket, int indexBaseFrom0)
    {
        var index = indexBaseFrom0 / 64;
        var indexInBitBucket = indexBaseFrom0 % 64;
        var bitBucketValue = bitBucket[index];
        bitBucketValue = SetBit(bitBucketValue, indexInBitBucket);
        bitBucket[index] = bitBucketValue;
        return bitBucketValue;
    }

    public static ulong SetBit(uint[] bitBucket, int indexBaseFrom0)
    {
        var index = indexBaseFrom0 / 32;
        var indexInBitBucket = indexBaseFrom0 % 32;
        var bitBucketValue = bitBucket[index];
        bitBucketValue = SetBit(bitBucketValue, indexInBitBucket);
        bitBucket[index] = bitBucketValue;
        return bitBucketValue;
    }

    public static Int32 ToInt32(byte[] buffer, int startIndex, bool isBigHead = true)
    {
        if (isBigHead)
        {
            var length = 4;
            Reverse(buffer, startIndex, length);
        }
        var value = BitConverter.ToInt32(buffer, startIndex);
        return value;
    }

    public static UInt32 ToUInt32(byte[] buffer, int startIndex, bool isBigHead = true)
    {
        if (isBigHead)
        {
            var length = 4;
            Reverse(buffer, startIndex, length);
        }
        var value = BitConverter.ToUInt32(buffer, startIndex);
        return value;
    }

    public static UInt16 ToUInt16(byte[] buffer, int startIndex, bool isBigHead = true)
    {
        if (isBigHead)
        {
            var length = 2;
            Reverse(buffer, startIndex, length);
        }
        var value = BitConverter.ToUInt16(buffer, startIndex);
        return value;
    }

    public static Int16 ToInt16(byte[] buffer, int startIndex, bool isBigHead = true)
    {
        if (isBigHead)
        {
            var length = 2;
            Reverse(buffer, startIndex, length);
        }
        var value = BitConverter.ToInt16(buffer, startIndex);
        return value;
    }

    public static UInt64 ToUInt64(byte[] buffer, int startIndex, bool isBigHead = true)
    {
        if (isBigHead)
        {
            var length = 4;
            Reverse(buffer, startIndex, length);
        }
        var value = BitConverter.ToUInt64(buffer, startIndex);
        return value;
    }

    public static byte[] GetBytes(int value, bool isBigHead = true)
    {
        var ret = BitConverter.GetBytes(value);
        if(isBigHead)
        {
            Reverse(ret, 0, ret.Length);
        }
        return ret;
    }

    public static byte[] GetBytes(long value, bool isBigHead = true)
    {
        var ret = BitConverter.GetBytes(value);
        if (isBigHead)
        {
            Reverse(ret, 0, ret.Length);
        }
        return ret;
    }

    public static byte[] GetBytes(uint value, bool isBigHead = true)
    {
        var ret = BitConverter.GetBytes(value);
        if (isBigHead)
        {
            Reverse(ret, 0, ret.Length);
        }
        return ret;
    }

    public static byte[] GetBytes(ulong value, bool isBigHead = true)
    {
        var ret = BitConverter.GetBytes(value);
        if (isBigHead)
        {
            Reverse(ret, 0, ret.Length);
        }
        return ret;
    }

    public static byte[] GetBytes(string value)
    {
        var ret = Encoding.Default.GetBytes(value);
        return ret;
    }

    public static byte[] GetBytes(short value, bool isBigHead = true)
    {
        var ret = BitConverter.GetBytes(value);
        if (isBigHead)
        {
            Reverse(ret, 0, ret.Length);
        }
        return ret;
    }

    public static byte[] GetBytes(ushort value, bool isBigHead = true)
    {
        var ret = BitConverter.GetBytes(value);
        if (isBigHead)
        {
            Reverse(ret, 0, ret.Length);
        }
        return ret;
    }

    static void Reverse(byte[] buffer, int startIndex, int length)
    {
        for (int i = 0; i < length/2; i++)
        {
            var headIndex = startIndex + i;
            var tailIndex = startIndex + length - i - 1;
            var temp = buffer[headIndex];
            var temp2 = buffer[tailIndex];
            buffer[headIndex] = temp2;
            buffer[tailIndex] = temp;
        }
    }

    public static byte[] Combine(byte[] buffer1, byte[] buffer2)
    {
        var ret = new byte[buffer1.Length + buffer2.Length];
        for(int i = 0; i < buffer1.Length; i++)
        {
            ret[i] = buffer1[i];
        }
        for(int i = 0; i < buffer2.Length; i++)
        {
            ret[i + buffer1.Length] = buffer2[i];
        }
        return ret;
    }
}
