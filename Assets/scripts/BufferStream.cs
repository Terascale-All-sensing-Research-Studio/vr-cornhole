using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public enum BufferStreamType
{
    Head,
    LeftHand,
    RightHand,
    EyeHit,
    ButtonPress,
    HandGrab,
    BagMovement
}

public class BufferStream : IDisposable
{
    private readonly Dictionary<BufferStreamType, List<string>> buffers = new();
    private string baseFolderPath;

    public void CreateDirectory(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        baseFolderPath = path;

        foreach (BufferStreamType type in Enum.GetValues(typeof(BufferStreamType)))
        {
            buffers[type] = new List<string>(capacity: 10000);
        }
    }

    public void SetBasePath(string path)
    {
        baseFolderPath = path;
    }

    public void Enqueue(BufferStreamType type, string line)
    {
        if (buffers.TryGetValue(type, out var list))
        {
            list.Add(line);
        }
    }

    public void StopAndFlush()
    {
        foreach (var pair in buffers)
        {
            BufferStreamType type = pair.Key;
            List<string> lines = pair.Value;
            if (lines.Count == 0) continue;

            string fileName = type switch
            {
                BufferStreamType.Head => "Head.csv",
                BufferStreamType.LeftHand => "LeftHand.csv",
                BufferStreamType.RightHand => "RightHand.csv",
                BufferStreamType.EyeHit => "EyeGaze.csv",
                BufferStreamType.ButtonPress => "ButtonLog.csv",
                BufferStreamType.HandGrab => "BagThrowLog.csv",
                BufferStreamType.BagMovement => "BagMovement.csv",

                _ => throw new ArgumentOutOfRangeException()
            };

            string fullPath = Path.Combine(baseFolderPath, fileName);
            using var stream = new FileStream(fullPath, FileMode.Append, FileAccess.Write, FileShare.Read);
            using var writer = new StreamWriter(new BufferedStream(stream, 2 * 1024 * 1024), Encoding.UTF8);

            string header = type switch
            {
                BufferStreamType.Head => "timeStampNs,gameTime,posX,posY,posZ,rotX,rotY,rotZ,",
                BufferStreamType.LeftHand => "timeStampNs,gameTime,posX,posY,posZ,rotX,rotY,rotZ,",
                BufferStreamType.RightHand => "timeStampNs,gameTime,posX,posY,posZ,rotX,rotY,rotZ,",
                BufferStreamType.EyeHit => "timeStampNs,gameTime,objectName,posX,posY,posZ,",
                BufferStreamType.ButtonPress => "timeStampNs,gameTime,buttonSelection,",
                BufferStreamType.HandGrab => "grabTimestampNs,grabGameTime,bagID,grabPosX,grabPosY,grabPosZ,impactTimestampNs,impactGameTime,destination,impactPosX,impactPosY,impactPosZ,impactSpeed,",
                BufferStreamType.BagMovement => "timeStampNs,gameTime,bagID,posX,posY,posZ,rotX,rotY,rotZ,",
                _ => throw new ArgumentOutOfRangeException()
            };

            writer.WriteLine(header);
            foreach (string line in lines)
            {
                writer.WriteLine(line);
            }
        }
    }

    public void Dispose()
    {
        StopAndFlush();
    }
}