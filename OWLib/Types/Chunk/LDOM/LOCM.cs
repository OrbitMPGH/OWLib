using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OWLib.Types.Chunk {
    public class LOCM : IChunk {
        public string Identifier => "LOCM"; // MCOL - Model Collision
        public string RootIdentifier => "LDOM"; // MODL - Model

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct HeaderInfo {
            public ushort simpleCount;
            public ushort complexCount;
            public ushort descriptorCount;
            public ushort otherCount;
            public long simple;
            public long complex;
            public long descriptor;
            public long other;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct SimpleBox {
            public ulong padding1;
            public Matrix4B matrix;
            public ulong unknown1;
            public uint name_id;
            public uint attachment_id;
            public ushort unknown2;
            public ushort unknown3;
            public ushort unknown4;
            public ushort unknown5;
            public byte unknown6;
            public byte unknown7;
            public uint unknown8;
            public ushort unknown9;
        }

        private HeaderInfo header;
        public HeaderInfo Header => header;

        private SimpleBox[] simple;
        public SimpleBox[] Simple => simple;

        public void Parse(Stream input) {
            using (BinaryReader reader = new BinaryReader(input, System.Text.Encoding.Default, true)) {
                header = reader.Read<HeaderInfo>();

                if (header.simpleCount > 0) {
                    simple = new SimpleBox[header.simpleCount];
                    input.Position = header.simple;
                    for (ushort i = 0; i < header.simpleCount; ++i) {
                        simple[i] = reader.Read<SimpleBox>();
                    }
                }
            }
        }
    }
}
