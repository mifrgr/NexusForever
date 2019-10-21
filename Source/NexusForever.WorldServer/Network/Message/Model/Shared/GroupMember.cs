using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model.Shared
{
    public class Member : IWritable
    {
        public class UnknownStruct0 : IWritable
        {
            public ushort Unknown6 { get; set; } = 0;
            public byte Unknown7 { get; set; } = 48;

            public void Write(GamePacketWriter writer)
            {
                writer.Write(Unknown6);
                writer.Write(Unknown7);
            }
        }

        public class UnknownStruct1 : IWritable
        {
            public ushort Unknown30 { get; set; } // 15
            public ushort Unknown31 { get; set; } // 16

            public void Write(GamePacketWriter writer)
            {
                writer.Write(Unknown30, 15u);
                writer.Write(Unknown31);
            }
        }

        public string Name { get; set; }
        public Faction Faction { get; set; } // 14
        public Race Race { get; set; } // 14
        public Class Class { get; set; } // 14
        public Sex Sex { get; set; } // 2 -- 
        public byte Level { get; set; } // 7
        public byte EffectiveLevel { get; set; } // 7
        public Path Path { get; set; } // 3
        public uint Unknown4 { get; set; } // 17
        public ushort GroupMemberId { get; set; } // 16

        public List<UnknownStruct0> UnknownStruct0List { get; set; } = new List<UnknownStruct0> { new UnknownStruct0(), new UnknownStruct0(), new UnknownStruct0(), new UnknownStruct0(), new UnknownStruct0() };

        public TargetPlayerIdentity MentoringTarget { get; set; } // probably
        
        public uint Unknown10 { get; set; }
        public ushort Unknown11 { get; set; }
        public ushort Unknown12 { get; set; }
        public ushort Unknown13 { get; set; }
        public ushort Unknown14 { get; set; }
        public ushort Unknown15 { get; set; }
        public ushort Unknown16 { get; set; }
        public ushort Unknown17 { get; set; }
        public ushort Unknown18 { get; set; }
        public ushort Unknown19 { get; set; }
        public ushort Unknown20 { get; set; }
        public ushort Unknown21 { get; set; }
        public ushort Unknown22 { get; set; }

        public ushort Realm { get; set; } // 14
        public ushort WorldZoneId { get; set; } // 15
        public uint Unknown25 { get; set; } // phases?
        public uint Unknown26 { get; set; } // phases?
        public bool SyncedToGroup { get; set; }

        public uint Unknown28 { get; set; }
        public uint Unknown29 { get; set; }

        public List<UnknownStruct1> UnknownStruct1List { get; set; } = new List<UnknownStruct1>();

        public void Write(GamePacketWriter writer)
        {
            writer.WriteStringWide(Name);
            writer.Write(Faction, 14u);
            writer.Write(Race, 14u);
            writer.Write(Class, 14u);
            writer.Write(Sex, 2u);
            writer.Write(Level, 7u);
            writer.Write(EffectiveLevel, 7u);
            writer.Write(Path, 3u);
            writer.Write(Unknown4, 17u);
            writer.Write(GroupMemberId);

            UnknownStruct0List.ForEach(i => i.Write(writer));

            if (MentoringTarget is null)
            {
                writer.Write((ushort)0, 14u);
                writer.Write((ulong)0);
            }
            else
            {
                MentoringTarget.Write(writer);
            }

            writer.Write(Unknown10);
            writer.Write(Unknown11);
            writer.Write(Unknown12);
            writer.Write(Unknown13);
            writer.Write(Unknown14);
            writer.Write(Unknown15);
            writer.Write(Unknown16);
            writer.Write(Unknown17);
            writer.Write(Unknown18);
            writer.Write(Unknown19);
            writer.Write(Unknown20);
            writer.Write(Unknown21);
            writer.Write(Unknown22);
            writer.Write(Realm, 14u);
            writer.Write(WorldZoneId, 15u);
            writer.Write(Unknown25);
            writer.Write(Unknown26);
            writer.Write(SyncedToGroup);
            writer.Write(Unknown28);
            writer.Write(Unknown29);

            writer.Write(UnknownStruct1List.Count, 32u);
            UnknownStruct1List.ForEach(i => i.Write(writer));
        }
    }
}
