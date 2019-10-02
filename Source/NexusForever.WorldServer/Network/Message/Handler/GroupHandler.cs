using NexusForever.Shared.Game.Events;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NLog;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class GroupHandler
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        [MessageHandler(GameMessageOpcode.ClientGroupInvite)]
        public static void HandleGroupInvite(WorldSession session, ClientGroupInvite request)
        {
            session.EnqueueEvent(new TaskGenericEvent<Character>(CharacterDatabase.GetCharacterByName(request.PlayerName),
                character =>
            {
                InviteResult inviteResult = InviteResult.Sent;

                if (character == null)
                    inviteResult = InviteResult.PlayerNotFound;

                //if (groupResult == GroupResult.Sent)
                    // Send request via group manager

                session.EnqueueMessageEncrypted(new ServerGroupInviteResult
                {
                    GroupId = 0,
                    PlayerName = request.PlayerName,
                    Result = inviteResult
                });

                WorldSession targetSession = NetworkManager<WorldSession>.GetSession(s => s.Player?.CharacterId == character.Id);
                if (targetSession != null)
                {
                    targetSession.EnqueueMessageEncrypted(new ServerGroupInviteReceived
                    {
                        GroupId = 1,
                        Unknown0 = 0,
                        Unknown1 = 0,
                        GroupMembers = new System.Collections.Generic.List<GroupMember>
                        {
                            new GroupMember
                            {
                                Name = session.Player.Name,
                                Faction = session.Player.Faction1,
                                Race = session.Player.Race,
                                Class = session.Player.Class,
                                Path = session.Player.Path,
                                Level = (byte)session.Player.Level,
                                GroupMemberId = 1
                            }
                        }
                    });
                }
            }));
        }

        [MessageHandler(GameMessageOpcode.ClientGroupInviteResponse)]
        public static void HandleGroupInviteResponse(WorldSession session, ClientGroupInviteResponse clientGroupInviteResponse)
        {
            log.Info($"{clientGroupInviteResponse.GroupId}, {clientGroupInviteResponse.Response}, {clientGroupInviteResponse.Unknown0}");

            WorldSession targetSession = NetworkManager<WorldSession>.GetSession(s => s.Player?.CharacterId == 1);
            if (clientGroupInviteResponse.Response != 0)
            {
                session.EnqueueMessageEncrypted(new ServerGroupJoin
                {
                    PlayerJoined = new Model.Shared.TargetPlayerIdentity
                    {
                        RealmId = WorldServer.RealmId,
                        CharacterId = session.Player.Guid
                    },
                    GroupId = clientGroupInviteResponse.GroupId,
                    Unknown0 = 257,
                    Unknown1 = 5,
                    Unknown3 = 1,
                    Unknown4 = 2,
                    Unknown5 = 3,
                    Unknown6 = 0,
                    GroupMembers = new System.Collections.Generic.List<ServerGroupJoin.GroupMemberInfo>
                    {
                        new ServerGroupJoin.GroupMemberInfo
                        {
                            MemberIdentity = new Model.Shared.TargetPlayerIdentity
                            {
                                RealmId = WorldServer.RealmId,
                                CharacterId = 2
                            },
                            Unknown7 = 8198,
                            GroupMember = new Model.Shared.GroupMember
                            {
                                Name = session.Player.Name,
                                Faction = session.Player.Faction1,
                                Race = session.Player.Race,
                                Class = session.Player.Class,
                                Path = session.Player.Path,
                                Level = (byte)session.Player.Level,
                                GroupMemberId = 1,
                                Realm = WorldServer.RealmId,
                                WorldZoneId = 51,
                                Unknown25 = 2725,
                                Unknown26 = 1,
                                Unknown27 = true
                            },
                            GroupIndex = 1
                        },
                        new ServerGroupJoin.GroupMemberInfo
                        {
                            MemberIdentity = new TargetPlayerIdentity
                            {
                                RealmId = WorldServer.RealmId,
                                CharacterId = session.Player.Guid
                            },
                            Unknown7 = 8192,
                            GroupMember = new GroupMember
                            {
                                Name = targetSession.Player.Name,
                                Faction = targetSession.Player.Faction1,
                                Race = targetSession.Player.Race,
                                Class = targetSession.Player.Class,
                                Path = targetSession.Player.Path,
                                Level = (byte)targetSession.Player.Level,
                                GroupMemberId = 2,
                                Realm = WorldServer.RealmId,
                                WorldZoneId = 51,
                                Unknown25 = 2725,
                                Unknown26 = 1,
                                Unknown27 = true
                            },
                            GroupIndex = 2
                        }
                    },
                    LeaderIdentity = new Model.Shared.TargetPlayerIdentity
                    {
                        RealmId = WorldServer.RealmId,
                        CharacterId = targetSession.Player.Guid
                    },
                    Realm = WorldServer.RealmId
                });

                targetSession.EnqueueMessageEncrypted(new ServerGroupInviteResult
                {
                    GroupId = clientGroupInviteResponse.GroupId,
                    PlayerName = session.Player.Name,
                    Result = InviteResult.Accepted
                });
            }
            else
            {
                targetSession.EnqueueMessageEncrypted(new ServerGroupInviteResult
                {
                    GroupId = clientGroupInviteResponse.GroupId,
                    PlayerName = session.Player.Name,
                    Result = InviteResult.Declined
                });
            }
        }
    }
}
