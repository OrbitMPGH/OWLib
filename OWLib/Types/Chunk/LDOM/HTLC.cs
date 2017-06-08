using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OWLib.Types.Chunk {
    public class HTLC : IChunk {
        public string Identifier => "HTLC"; // CLTH - Cloth
        public string RootIdentifier => "LDOM"; // MODL - Model

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct HeaderInfo {
            public ulong count;
            public long desc;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct ClothDesc {
            public long _unk1;
            public long _unk2;
            public long _unk3;
            public long _unk4;
            public long system;
            public long _unk6;
            public long _unk7;
            public long hierarchy;
            public long _unk9;
            public fixed byte name[32];
            public uint boneCount;
            public uint unk2Count;
            public int unknown1;
            public uint unk3Count;
            public uint unk4Count;
            public uint unknown2;
            public uint unknown3;
            public uint unk9Count;
            public uint unknown5;
            public uint unk6Count;
            public uint unknown6;
            public float unkB;
            public ulong unkD;
            public uint unkE;
            public ushort unkF;
            public ushort unk10;
            public OWRecord unk11;
            public ulong unk12;
            public OWRecord unk13;

            public unsafe string Name {
                get {
                    fixed (byte* n = name) {
                        return Encoding.ASCII.GetString(n, 32).Split('\0')[0];
                    }
                }
            }
        }

        private HeaderInfo header;
        private ClothDesc[] descriptors;
        private ushort[][] system;
        private short[][] hierarchy;
        
        public HeaderInfo Header => header;
        public ClothDesc[] Descriptors => descriptors;
        public ushort[][] Sys => system;
        public short[][] Hierarchy => hierarchy;

        public void Parse(Stream input) {
            using (BinaryReader reader = new BinaryReader(input, System.Text.Encoding.Default, true)) {
                header = reader.Read<HeaderInfo>();

                if (header.count > 0) {
                    descriptors = new ClothDesc[header.count];
                    system = new ushort[header.count][];
                    hierarchy = new short[header.count][];
                    input.Position = header.desc;
                    for (ulong i = 0; i < header.count; ++i) {
                        descriptors[i] = reader.Read<ClothDesc>();
                    }
                    for (ulong i = 0; i < header.count; ++i) {
                        if (descriptors[i].boneCount > 0) {
                            system[i] = new ushort[descriptors[i].boneCount];
                            if (descriptors[i].system > 0) {
                                input.Position = descriptors[i].system;
                                for (int j = 0; j < descriptors[i].boneCount; ++j) {
                                    system[i][j] = reader.ReadUInt16();
                                }
                            }

                            hierarchy[i] = new short[descriptors[i].boneCount];
                            if (descriptors[i].hierarchy > 0) {
                                input.Position = descriptors[i].hierarchy;
                                for (int j = 0; j < descriptors[i].boneCount; ++j) {
                                    hierarchy[i][j] = reader.ReadInt16();
                                }
                            }
                        } else {
                            system[i] = new ushort[0];
                            hierarchy[i] = new short[0];
                        }
                    }
                }
            }
        }
    }
}
