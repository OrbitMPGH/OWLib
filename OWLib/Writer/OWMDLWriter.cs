﻿using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OWLib.Types;
using OWLib.Types.Chunk;
using OWLib.Types.Map;

namespace OWLib.Writer {
    public class OWMDLWriter : IDataWriter {
        public string Format => ".owmdl";
        public char[] Identifier => new char[1] { 'w' };
        public string Name => "OWM Model Format";
        public WriterSupport SupportLevel => (WriterSupport.VERTEX | WriterSupport.UV | WriterSupport.BONE | WriterSupport.POSE | WriterSupport.MATERIAL | WriterSupport.ATTACHMENT | WriterSupport.MODEL);

        public bool Write(Map10 physics, Stream output, object[] data) {
            // Console.Out.WriteLine("Writing OWMDL");
            using (BinaryWriter writer = new BinaryWriter(output)) {
                writer.Write((ushort)1);
                writer.Write((ushort)1);
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((ushort)0);
                writer.Write(1);
                writer.Write(0);

                writer.Write("PhysicsModel");
                writer.Write(0L);
                writer.Write((byte)0);
                writer.Write(physics.Vertices.Length);
                writer.Write(physics.Indices.Length);

                for (int i = 0; i < physics.Vertices.Length; ++i) {
                    writer.Write(physics.Vertices[i].position.x);
                    writer.Write(physics.Vertices[i].position.y);
                    writer.Write(physics.Vertices[i].position.z);
                    writer.Write(0.0f);
                    writer.Write(0.0f);
                    writer.Write(0.0f);
                    writer.Write((byte)0);
                }

                for (int i = 0; i < physics.Indices.Length; ++i) {
                    writer.Write((byte)3);
                    writer.Write(physics.Indices[i].index.v1);
                    writer.Write(physics.Indices[i].index.v2);
                    writer.Write(physics.Indices[i].index.v3);
                }
            }
            return true;
        }

        public bool Write(Chunked chunked, Stream output, List<byte> LODs, Dictionary<ulong, List<ImageLayer>> layers, object[] data) {
            IChunk chunk = chunked.FindNextChunk("MNRM").Value;
            if (chunk == null) {
                return false;
            }
            MNRM model = (MNRM)chunk;
            chunk = chunked.FindNextChunk("CLDM").Value;
            CLDM materials = null;
            if (chunk != null) {
                materials = (CLDM)chunk;
            }
            chunk = chunked.FindNextChunk("lksm").Value;
            lksm skeleton = null;
            if (chunk != null) {
                skeleton = (lksm)chunk;
            }
            chunk = chunked.FindNextChunk("PRHM").Value;
            PRHM hardpoints = null;
            if (chunk != null) {
                hardpoints = (PRHM)chunk;
            }
            chunk = chunked.FindNextChunk("LOCM").Value;
            LOCM modelbox = null;
            if (chunk != null) {
                modelbox = (LOCM)chunk;
            }

            //Console.Out.WriteLine("Writing OWMDL");
            using (BinaryWriter writer = new BinaryWriter(output)) {
                writer.Write((ushort)2); // version major
                writer.Write((ushort)0); // version minor

                if (data.Length > 1 && data[1] != null && data[1].GetType() == typeof(string) && ((string)data[1]).Length > 0) {
                    writer.Write((string)data[1]);
                } else {
                    writer.Write((byte)0);
                }

                if (data.Length > 2 && data[2] != null && data[2].GetType() == typeof(string) && ((string)data[2]).Length > 0) {
                    writer.Write((string)data[2]);
                } else {
                    writer.Write((byte)0);
                }

                writer.Write(14); // header size. ushort + uint + uint + uint

                if (skeleton == null) {
                    writer.Write((ushort)0); // number of bones
                } else {
                    writer.Write(skeleton.Data.bonesAbs);
                }

                Dictionary<byte, List<int>> LODMap = new Dictionary<byte, List<int>>();
                uint sz = 0;
                uint lookForLod = 0;
                bool lodOnly = false;
                if (data.Length > 3 && data[3] != null && data[3].GetType() == typeof(bool) && (bool)data[3] == true) {
                    lodOnly = true;
                }
                for (int i = 0; i < model.Submeshes.Length; ++i) {
                    SubmeshDescriptor submesh = model.Submeshes[i];
                    if (data.Length > 4 && data[4] != null && data[4].GetType() == typeof(bool) && (bool)data[4] == true) {
                        if ((SubmeshFlags)submesh.flags == SubmeshFlags.COLLISION_MESH) {
                            continue;
                        }
                    }
                    if (LODs != null && !LODs.Contains(submesh.lod)) {
                        continue;
                    }
                    if (lodOnly && lookForLod > 0 && submesh.lod != lookForLod) {
                        continue;
                    }
                    if (!LODMap.ContainsKey(submesh.lod)) {
                        LODMap.Add(submesh.lod, new List<int>());
                    }
                    lookForLod = submesh.lod;
                    sz++;
                    LODMap[submesh.lod].Add(i);
                }

                writer.Write(sz);

                if (hardpoints != null) {
                    writer.Write(hardpoints.HardPoints.Length);
                } else {
                    writer.Write((int)0); // number of attachments
                }

                if (modelbox != null) {
                    writer.Write(modelbox.Simple.Length);
                } else {
                    writer.Write((int)0); // number of hitboxes
                }

                if (skeleton != null) {
                    for (int i = 0; i < skeleton.Data.bonesAbs; ++i) {
                        writer.Write(IdToString("bone", skeleton.IDs[i]));
                        short parent = skeleton.Hierarchy[i];
                        if (parent == -1) {
                            parent = (short)i;
                        }
                        writer.Write(parent);
                        
                        Matrix3x4 bone = skeleton.Matrices34[i];
                        Quaternion rot = new Quaternion(bone[0, 0], bone[0, 1], bone[0, 2], bone[0, 3]);
                        Vector3 scl = new Vector3(bone[1, 0], bone[1, 1], bone[1, 2]);
                        Vector3 pos = new Vector3(bone[2, 0], bone[2, 1], bone[2, 2]);
                        writer.Write(pos.X);
                        writer.Write(pos.Y);
                        writer.Write(pos.Z);
                        writer.Write(scl.X);
                        writer.Write(scl.X);
                        writer.Write(scl.X);
                        writer.Write(rot.X);
                        writer.Write(rot.Y);
                        writer.Write(rot.Z);
                        writer.Write(rot.W);
                    }
                }

                foreach (KeyValuePair<byte, List<int>> kv in LODMap) {
                    //Console.Out.WriteLine("Writing LOD {0}", kv.Key);
                    foreach (int i in kv.Value) {
                        SubmeshDescriptor submesh = model.Submeshes[i];
                        ModelVertex[] vertex = model.Vertices[i];
                        ModelVertex[] normal = model.Normals[i];
                        ModelUV[][] uv = model.TextureCoordinates[i];
                        ModelIndice[] index = model.Indices[i];
                        ModelBoneData[] bones = model.Bones[i];
                        writer.Write($"Submesh_{i}.{kv.Key}.{materials.Materials[submesh.material]:X16}");
                        writer.Write(materials.Materials[submesh.material]);
                        writer.Write((byte)uv.Length);
                        writer.Write(vertex.Length);
                        writer.Write(index.Length);
                        for (int j = 0; j < vertex.Length; ++j) {
                            writer.Write(vertex[j].x);
                            writer.Write(vertex[j].y);
                            writer.Write(vertex[j].z);
                            writer.Write(-normal[j].x);
                            writer.Write(-normal[j].y);
                            writer.Write(-normal[j].z);
                            for (int k = 0; k < uv.Length; ++k) {
                                writer.Write((float)uv[k][j].u);
                                writer.Write((float)uv[k][j].v);
                            }
                            if (skeleton != null && bones != null && bones[j].boneIndex != null && bones[j].boneWeight != null) {
                                writer.Write((byte)4);
                                writer.Write(skeleton.Lookup[bones[j].boneIndex[0]]);
                                writer.Write(skeleton.Lookup[bones[j].boneIndex[1]]);
                                writer.Write(skeleton.Lookup[bones[j].boneIndex[2]]);
                                writer.Write(skeleton.Lookup[bones[j].boneIndex[3]]);
                                writer.Write(bones[j].boneWeight[0]);
                                writer.Write(bones[j].boneWeight[1]);
                                writer.Write(bones[j].boneWeight[2]);
                                writer.Write(bones[j].boneWeight[3]);
                            } else {
                                // bone -> size + index + weight
                                writer.Write((byte)0);
                            }
                        }
                        for (int j = 0; j < index.Length; ++j) {
                            writer.Write((byte)3);
                            writer.Write((int)index[j].v1);
                            writer.Write((int)index[j].v2);
                            writer.Write((int)index[j].v3);
                        }
                    }
                }
                if (hardpoints != null) {
                    // attachments
                    for (int i = 0; i < hardpoints.HardPoints.Length; ++i) {
                        PRHM.HardPoint hp = hardpoints.HardPoints[i];
                        writer.Write(IdToString("attachment_", hp.id));
                        Matrix4 mat = hp.matrix.ToOpenTK();
                        Vector3 pos = mat.ExtractTranslation();
                        Quaternion rot = mat.ExtractRotation();
                        writer.Write(pos.X);
                        writer.Write(pos.Y);
                        writer.Write(pos.Z);
                        writer.Write(rot.X);
                        writer.Write(rot.Y);
                        writer.Write(rot.Z);
                        writer.Write(rot.W);
                    }
                    // extension 1.1
                    for (int i = 0; i < hardpoints.HardPoints.Length; ++i) {
                        PRHM.HardPoint hp = hardpoints.HardPoints[i];
                        writer.Write(IdToString("bone", hp.id));
                    }
                }

                if (modelbox != null) {
                    // hitboxes
                    for (int i = 0; i < modelbox.Simple.Length; ++i) {
                        LOCM.SimpleBox box = modelbox.Simple[i];
                        writer.Write(box.name_id);
                        writer.Write(box.attachment_id);
                        unsafe
                        {
                            writer.Write(box.matrix.Value[8]);
                            writer.Write(box.matrix.Value[9]);
                            writer.Write(box.matrix.Value[10]);
                            writer.Write(box.matrix.Value[4]);
                            writer.Write(box.matrix.Value[5]);
                            writer.Write(box.matrix.Value[6]);
                            writer.Write(box.matrix.Value[0]);
                            writer.Write(box.matrix.Value[1]);
                            writer.Write(box.matrix.Value[2]);
                            writer.Write(box.matrix.Value[3]);
                        }
                    }
                }
            }
            return true;
        }

        public static string IdToString(string prefix, uint id) {
            return $"{prefix}_{id:X4}";
        }

        public bool Write(Animation anim, Stream output, object[] data) {
            return false;
        }

        public Dictionary<ulong, List<string>>[] Write(Stream output, Map map, Map detail1, Map detail2, Map props, Map lights, string name, IDataWriter modelFormat) {
            return null;
        }
    }
}
