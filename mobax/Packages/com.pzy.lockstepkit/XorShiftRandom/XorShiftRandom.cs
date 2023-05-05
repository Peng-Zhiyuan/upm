using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XorShiftRandom
{

    #region Data Members

    private ulong x_;
    private ulong y_;

    #endregion

    #region Constructor

    public XorShiftRandom(ulong seed)
    {
        x_ = seed << 1;
        y_ = seed >> 1;
    }

    public int NextInt32()
    {
        int _;
        ulong temp_x, temp_y;

        temp_x = y_;
        x_ ^= x_ << 23; temp_y = x_ ^ y_ ^ (x_ >> 17) ^ (y_ >> 26);

        _ = (int)(temp_y + y_);

        x_ = temp_x;
        y_ = temp_y;

        return _;
    }

    private const double DOUBLE_UNIT = 1.0 / (int.MaxValue + 1.0);
    public double NextDouble()
    {
        double _;
        ulong temp_x, temp_y, temp_z;

        temp_x = y_;
        x_ ^= x_ << 23; temp_y = x_ ^ y_ ^ (x_ >> 17) ^ (y_ >> 26);

        temp_z = temp_y + y_;
        _ = DOUBLE_UNIT * (0x7FFFFFFF & temp_z);

        x_ = temp_x;
        y_ = temp_y;

        return _;
    }

    public decimal NextDecimal()
    {
        decimal _;
        int l, m, h;
        ulong temp_x, temp_y, temp_z;

        temp_x = y_;
        x_ ^= x_ << 23; temp_y = x_ ^ y_ ^ (x_ >> 17) ^ (y_ >> 26);

        temp_z = temp_y + y_;

        h = (int)(temp_z & 0x1FFFFFFF);
        m = (int)(temp_z >> 16);
        l = (int)(temp_z >> 32);

        _ = new decimal(l, m, h, false, 28);

        x_ = temp_x;
        y_ = temp_y;

        return _;
    }

    // Buffer for optimized bit generation.
    private ulong buffer_;
    private ulong bufferMask_;

    public bool NextBoolean()
    {
        bool _;
        if (bufferMask_ > 0)
        {
            _ = (buffer_ & bufferMask_) == 0;
            bufferMask_ >>= 1;
            return _;
        }

        ulong temp_x, temp_y;
        temp_x = y_;
        x_ ^= x_ << 23; temp_y = x_ ^ y_ ^ (x_ >> 17) ^ (y_ >> 26);

        buffer_ = temp_y + y_;
        x_ = temp_x;
        y_ = temp_y;

        bufferMask_ = 0x8000000000000000;
        return (buffer_ & 0xF000000000000000) == 0;
    }

    public byte NextByte()
    {
        if (bufferMask_ >= 8)
        {
            byte _ = (byte)buffer_;
            buffer_ >>= 8;
            bufferMask_ >>= 8;
            return _;
        }

        ulong temp_x, temp_y;
        temp_x = y_;
        x_ ^= x_ << 23; temp_y = x_ ^ y_ ^ (x_ >> 17) ^ (y_ >> 26);

        buffer_ = temp_y + y_;
        x_ = temp_x;
        y_ = temp_y;

        bufferMask_ = 0x8000000000000;
        return (byte)(buffer_ >>= 8);
    }

    //public unsafe void NextBytes(byte[] buffer)
    //{
    //    // Localize state for stack execution
    //    ulong x = x_, y = y_, temp_x, temp_y, z;

    //    fixed (byte* pBuffer = buffer)
    //    {
    //        ulong* pIndex = (ulong*)pBuffer;
    //        ulong* pEnd = (ulong*)(pBuffer + buffer.Length);

    //        // Fill array in 8-byte chunks
    //        while (pIndex <= pEnd - 1)
    //        {
    //            temp_x = y;
    //            x ^= x << 23; temp_y = x ^ y ^ (x >> 17) ^ (y >> 26);

    //            *(pIndex++) = temp_y + y;

    //            x = temp_x;
    //            y = temp_y;
    //        }

    //        // Fill remaining bytes individually to prevent overflow
    //        if (pIndex < pEnd)
    //        {
    //            temp_x = y;
    //            x ^= x << 23; temp_y = x ^ y ^ (x >> 17) ^ (y >> 26);
    //            z = temp_y + y;

    //            byte* pByte = (byte*)pIndex;
    //            while (pByte < pEnd) *(pByte++) = (byte)(z >>= 8);
    //        }
    //    }

    //    // Store modified state in fields.
    //    x_ = x;
    //    y_ = y;
    //}

    #endregion

}