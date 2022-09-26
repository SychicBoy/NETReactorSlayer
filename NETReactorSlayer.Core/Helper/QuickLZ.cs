// QuickLZ data compression library
// Copyright (C) 2006-2011 Lasse Mikkel Reinhold
// lar@quicklz.com
//
// QuickLZ can be used for free under the GPL 1, 2 or 3 license (where anything 
// released into public must be open source) or under a commercial license if such 
// has been acquired (see http://www.quicklz.com/order.html). The commercial license 
// does not cover derived or ported versions created by third parties under GPL.

// Port of QuickLZ to C# by de4dot@gmail.com. This code is most likely not working now.

using System;

namespace NETReactorSlayer.Core.Helper;

public class QuickLZ : QuickLZBase
{
    public static byte[] Decompress(byte[] inData) => Decompress(inData, DEFAULT_QCLZ_SIG);

    public static byte[] Decompress(byte[] inData, int sig)
    {
        /*int mode =*/
        BitConverter.ToInt32(inData, 4);
        var compressedLength = BitConverter.ToInt32(inData, 8);
        var decompressedLength = BitConverter.ToInt32(inData, 12);
        var isDataCompressed = BitConverter.ToInt32(inData, 16) == 1;
        const int headerLength = 32;
        if (BitConverter.ToInt32(inData, 0) != sig || BitConverter.ToInt32(inData, compressedLength - 4) != sig)
            throw new ApplicationException("No QCLZ sig");

        var outData = new byte[decompressedLength];

        if (!isDataCompressed)
        {
            Copy(inData, headerLength, outData, 0, decompressedLength);
            return outData;
        }

        Decompress(inData, headerLength, outData);
        return outData;
    }

    private const int DEFAULT_QCLZ_SIG = 0x5A4C4351;
}