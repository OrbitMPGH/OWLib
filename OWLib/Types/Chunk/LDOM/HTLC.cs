using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

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

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct UnknownStruct1 {
            public fixed float unknown1[4];
            public fixed float unknown2[4];
            public fixed float unknown3[8];
            public fixed float matrix1[16];
            public fixed float unkonwn4[16];
            public fixed float matrix2[12];
            public fixed float unknown5[4];
            public fixed float unknown6[22];
            public ushort index1;
            public ushort index2;
            public ushort index3;
            public ushort index4;
            public fixed float unknown7[8];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct UnknownStruct2 {
            public fixed float unknown1[4];
            public float weight;
            public ushort index1;
            public ushort index2;
            public ushort index3;
            public ushort index4;
            uint padding;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct UnknownStruct3 {
            public float weight;
            public fixed float unknown1[3];
            public fixed float unknown2[4];
            public ushort index1;
            public ushort index2;
            uint padding1;
            uint padding2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct UnknownStruct4 {
            public fixed float unknown1[4];
            public ushort unknown2;
            public ushort index1;
            public fixed float unknown3[2];
            public fixed float unknown4[4];
            public ushort unknown5;
            public ushort index2;
            public ushort index3;
            public ushort index4;
            ulong padding;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct UnknownStruct6 {
            public fixed float unknown1[4];
            public fixed float unknown2[4];
            public fixed float matrix1[16];
            public fixed float matrix2[16];
            public ushort index1;
            public ushort index2;
            ulong padding1;
            uint padding2;
        }

        private MemoryStream shadow;

        private HeaderInfo header;
        private ClothDesc[] descriptors;
        private ushort[] globsystem;
        
        public HeaderInfo Header => header;
        public ClothDesc[] Descriptors => descriptors;
        public ushort[] GlobalSys => globsystem;
        
        private UnknownStruct1[][] unkstruct1;
        private UnknownStruct2[][] unkstruct2;
        private UnknownStruct3[][] unkstruct3;
        private UnknownStruct4[][] unkstruct4;
        private ushort[][] system;
        private UnknownStruct6[][] unkstruct6;
        private short[][] hierarchy;
        private short[][] unk9;
        
        public UnknownStruct1[][] Unknown1 => unkstruct1;
        public UnknownStruct2[][] Unknown2 => unkstruct2;
        public UnknownStruct3[][] Unknown3 => unkstruct3;
        public UnknownStruct4[][] Unknown4 => unkstruct4;
        public ushort[][] Sys => system;
        public UnknownStruct6[][] Unknown6 => unkstruct6;
        public short[][] Hierarchy => hierarchy;
        public short[][] Unknown9 => unk9;

        public void Parse(Stream input) {
            shadow = new MemoryStream((int)input.Length);
            input.CopyTo(shadow);
            input.Position = 0;
            shadow.Position = 0;
        }

        public void Parse(lksm skeleton) {
            using (BinaryReader reader = new BinaryReader(shadow, System.Text.Encoding.Default, false)) {
                header = reader.Read<HeaderInfo>();

                if (header.count > 0) {
                    descriptors = new ClothDesc[header.count];
                    unkstruct1 = new UnknownStruct1[header.count][];
                    unkstruct2 = new UnknownStruct2[header.count][];
                    unkstruct3 = new UnknownStruct3[header.count][];
                    unkstruct4 = new UnknownStruct4[header.count][];
                    unkstruct6 = new UnknownStruct6[header.count][];
                    unk9 = new short[header.count][];
                    system = new ushort[header.count][];
                    hierarchy = new short[header.count][];
                    shadow.Position = header.desc;
                    for (ulong i = 0; i < header.count; ++i) {
                        descriptors[i] = reader.Read<ClothDesc>();
                    }
                    globsystem = new ushort[skeleton.Data.bonesCloth];
                    for (int i = 0; i < globsystem.Length; ++i) {
                        globsystem[i] = reader.ReadUInt16();
                    }
                    for (ulong i = 0; i < header.count; ++i) {
                        if (descriptors[i].boneCount > 0) {
                            unkstruct1[i] = new UnknownStruct1[descriptors[i].boneCount];
                            if (descriptors[i]._unk1 > 0) {
                                shadow.Position = descriptors[i]._unk1;
                                for (int j = 0; j < descriptors[i].boneCount; ++j) {
                                    unkstruct1[i][j] = reader.Read<UnknownStruct1>();
                                }
                            }

                            system[i] = new ushort[descriptors[i].boneCount];
                            if (descriptors[i].system > 0) {
                                shadow.Position = descriptors[i].system;
                                for (int j = 0; j < descriptors[i].boneCount; ++j) {
                                    system[i][j] = reader.ReadUInt16();
                                }
                            }

                            hierarchy[i] = new short[descriptors[i].boneCount];
                            if (descriptors[i].hierarchy > 0) {
                                shadow.Position = descriptors[i].hierarchy;
                                for (int j = 0; j < descriptors[i].boneCount; ++j) {
                                    hierarchy[i][j] = reader.ReadInt16();
                                }
                            }
                        } else {
                            system[i] = new ushort[0];
                            hierarchy[i] = new short[0];
                        }

                        unkstruct2[i] = new UnknownStruct2[descriptors[i].unk2Count];
                        if (descriptors[i]._unk2 > 0) {
                            shadow.Position = descriptors[i]._unk2;
                            for (int j = 0; j < descriptors[i].unk2Count; ++j) {
                                unkstruct2[i][j] = reader.Read<UnknownStruct2>();
                            }
                        }

                        unkstruct3[i] = new UnknownStruct3[descriptors[i].unk3Count];
                        if (descriptors[i]._unk3 > 0) {
                            shadow.Position = descriptors[i]._unk3;
                            for (int j = 0; j < descriptors[i].unk3Count; ++j) {
                                unkstruct3[i][j] = reader.Read<UnknownStruct3>();
                            }
                        }

                        unkstruct4[i] = new UnknownStruct4[descriptors[i].unk4Count];
                        if (descriptors[i]._unk4 > 0) {
                            shadow.Position = descriptors[i]._unk4;
                            for (int j = 0; j < descriptors[i].unk4Count; ++j) {
                                unkstruct4[i][j] = reader.Read<UnknownStruct4>();
                            }
                        }

                        unkstruct6[i] = new UnknownStruct6[descriptors[i].unk6Count];
                        if (descriptors[i]._unk6 > 0) {
                            shadow.Position = descriptors[i]._unk6;
                            for (int j = 0; j < descriptors[i].unk6Count; ++j) {
                                unkstruct6[i][j] = reader.Read<UnknownStruct6>();
                            }
                        }

                        unk9[i] = new short[descriptors[i].unk9Count];
                        if (descriptors[i]._unk9 > 0) {
                            shadow.Position = descriptors[i]._unk9;
                            for (int j = 0; j < descriptors[i].unk9Count; ++j) {
                                unk9[i][j] = reader.ReadInt16();
                            }
                        }
                    }
                }
            }
        }
    }
}
