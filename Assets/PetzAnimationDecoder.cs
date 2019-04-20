/*
 * PetzAnimationDecoder code from https://github.com/thenickdude/petz-file-formats
 * Adapted to C# by Kyle Toom
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class PetzAnimationDecoder
{
    public int numBalls;
    public int[] ballSizes;
    public List<uint[]> animationFrameOffsets;
    public List<PetzBHT> animations;
    public PetzAnimationDecoder(byte[] bhdRaw)
    {
        var parsed = new PetzBHD(bhdRaw);
        numBalls = parsed.numBalls;
        ballSizes = parsed.ballSizes;
        animationFrameOffsets = parsed.animations;
        animations = new List<PetzBHT>();
    }

    public void addAnimation(int animIndex, byte[] bhtRaw)
    {
        while (animations.Count <= animIndex)
        {
            animations.Add(null);
        }
        animations[animIndex] = new PetzBHT(this, animIndex, bhtRaw);
    }
}

public class PetzBHD
{
    public List<uint[]> animations;

    public int framesOffset, version, numBalls, animationCount;
    public int[] ballSizes, animationEndOffset;
    public PetzBHD(byte[] bhdRaw)
    {
        var header = new BHDHeader(bhdRaw,0);

        framesOffset = header.framesOffset;
        version = header.version;
        numBalls = header.numBalls;
        animationCount = header.animationCount;
        ballSizes = header.ballSizes;
        animationEndOffset = header.animationEndOffset;

        animations = new List<uint[]>();

        for(int i = 0; i < header.animationCount; i++)
        {
            int animationStartOffset, animationLength;
            uint[] animFrameOffsets;
            
            if(i==0)
            {
                animationStartOffset = 0;
            } else
            {
                animationStartOffset = header.animationEndOffset[i - 1];
            }

            animationLength = header.animationEndOffset[i] - animationStartOffset;
            animFrameOffsets = new uint[animationLength];

            for(int j = 0; j < animationLength; j++)
            {
                animFrameOffsets[j] = BitConverter.ToUInt32(bhdRaw, header.framesOffset + animationStartOffset * 4 + j * 4);
            }
            animations.Add(animFrameOffsets);
        }
    }
}

public class PetzBHT
{
    public List<BHTFrameHeader> frames;
    public PetzBHT(PetzAnimationDecoder bhd, int animIndex, byte[] bhtRaw)
    {
        frames = new List<BHTFrameHeader>();

        foreach(var frameOffset in bhd.animationFrameOffsets[animIndex])
        {
            frames.Add(new BHTFrameHeader(bhtRaw, (int)frameOffset,bhd.numBalls));
        }
    }
}

public class BHTFrameHeader
{
    public int minX, minY, minZ, maxX, maxY, maxZ, tag;
    public BHTBallPosition[] balls;
    public BHTFrameHeader(byte[] bhtRaw, int frameOffset, int numBalls)
    {
        balls = new BHTBallPosition[numBalls];
        minX = BitConverter.ToInt16(bhtRaw, frameOffset);
        minY = BitConverter.ToInt16(bhtRaw, frameOffset + 2);
        minZ = BitConverter.ToInt16(bhtRaw, frameOffset + 4);
        maxX = BitConverter.ToInt16(bhtRaw, frameOffset + 6);
        maxY = BitConverter.ToInt16(bhtRaw, frameOffset + 8);
        maxZ = BitConverter.ToInt16(bhtRaw, frameOffset + 10);
        tag = BitConverter.ToInt16(bhtRaw, frameOffset + 12);
        for (int i = 0; i < numBalls; i++)
            balls[i] = new BHTBallPosition(bhtRaw, frameOffset+14+i*10);
    }
}

public class BHTBallPosition
{
    public int x, y, z;
    public BHTBallPosition(byte[] bhtRaw, int offset)
    {
        x = BitConverter.ToInt16(bhtRaw, offset);
        y = BitConverter.ToInt16(bhtRaw, offset+2);
        z = BitConverter.ToInt16(bhtRaw, offset+4);
    }
}

public class BHTHeader
{
    public uint fileLength, version;
    public string copyright;
    public BHTHeader(byte [] data, int offset)
    {
        fileLength = BitConverter.ToUInt32(data, offset);
        version = BitConverter.ToUInt32(data, offset + 4);
        copyright = BitConverter.ToString(data, offset + 6);
    }
}

public class BHDHeader
{
    public int framesOffset, version, numBalls,animationCount;
    public int[] ballSizes, animationEndOffset;

    public BHDHeader(byte[] data,int offset)
    {
        framesOffset = BitConverter.ToUInt16(data, offset);
        version = BitConverter.ToUInt16(data, offset + 4);
        numBalls = BitConverter.ToUInt16(data, offset + 6);
        ballSizes = new int[numBalls];

        offset += 38;
        for (int i = 0; i <numBalls; i++)
        {
            ballSizes[i] = BitConverter.ToUInt16(data, offset + i * 2);
        }
        offset += numBalls * 2;

        animationCount = BitConverter.ToUInt16(data, offset);
        offset += 2;
        animationEndOffset = new int[animationCount];
        for(int i = 0; i < animationCount; i++)
        {
            animationEndOffset[i] = BitConverter.ToUInt16(data, offset + i * 2);
        }
    }
}
