﻿using System.IO;
using System.Runtime.InteropServices;

namespace OWLib.Types.STUD {
  public class SoundBindingReference : ISTUDInstance {
    public ulong Key => 0x50C19D3121E41AA6;
    public uint Id => 0x31B1E932;
    public string Name => "Sound Binding:Reference";

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct ReferenceData {
      public STUDInstanceInfo instance;
      public OWRecord unk1;
      public OWRecord sound;
      public uint unk2;
    }

    private ReferenceData reference;
    public ReferenceData Reference => reference;

    public void Read(Stream input) {
      using(BinaryReader reader = new BinaryReader(input, System.Text.Encoding.Default, true)) {
        reference = reader.Read<ReferenceData>();
      }
    }
  }
}