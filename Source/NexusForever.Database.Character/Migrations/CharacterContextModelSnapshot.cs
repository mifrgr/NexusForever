﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NexusForever.Database.Character;

namespace NexusForever.Database.Character.Migrations
{
    [DbContext(typeof(CharacterContext))]
    partial class CharacterContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterAchievementModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<ushort>("AchievementId");

                    b.Property<uint>("Data0");

                    b.Property<uint>("Data1");

                    b.Property<DateTime?>("DateCompleted")
                        .HasColumnType("datetime");

                    b.HasKey("Id", "AchievementId");

                    b.ToTable("character_achievement");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterActionSetAmpModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<byte>("SpecIndex");

                    b.Property<ushort>("AmpId");

                    b.HasKey("Id", "SpecIndex", "AmpId");

                    b.ToTable("character_action_set_amp");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterActionSetShortcutModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<byte>("SpecIndex");

                    b.Property<ushort>("Location");

                    b.Property<uint>("ObjectId");

                    b.Property<byte>("ShortcutType");

                    b.Property<byte>("Tier");

                    b.HasKey("Id", "SpecIndex", "Location");

                    b.ToTable("character_action_set_shortcut");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterAppearanceModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<byte>("Slot");

                    b.Property<ushort>("DisplayId");

                    b.HasKey("Id", "Slot");

                    b.ToTable("character_appearance");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterBoneModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<byte>("BoneIndex");

                    b.Property<float>("Bone");

                    b.HasKey("Id", "BoneIndex");

                    b.ToTable("character_bone");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterCostumeItemModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<byte>("Index");

                    b.Property<byte>("Slot");

                    b.Property<int>("DyeData");

                    b.Property<uint>("ItemId");

                    b.HasKey("Id", "Index", "Slot");

                    b.ToTable("character_costume_item");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterCostumeModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<byte>("Index");

                    b.Property<uint>("Mask");

                    b.Property<DateTime>("Timestamp")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("Id", "Index");

                    b.ToTable("character_costume");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterCurrencyModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<byte>("CurrencyId");

                    b.Property<ulong>("Amount");

                    b.HasKey("Id", "CurrencyId");

                    b.ToTable("character_currency");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterCustomisationModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<uint>("Label");

                    b.Property<uint>("Value");

                    b.HasKey("Id", "Label");

                    b.ToTable("character_customisation");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterDatacubeModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<byte>("Type");

                    b.Property<ushort>("Datacube");

                    b.Property<uint>("Progress");

                    b.HasKey("Id", "Type", "Datacube");

                    b.ToTable("character_datacube");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterEntitlementModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<byte>("EntitlementId");

                    b.Property<uint>("Amount");

                    b.HasKey("Id", "EntitlementId");

                    b.ToTable("character_entitlement");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterKeybindingModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<ushort>("InputActionId");

                    b.Property<uint>("Code00");

                    b.Property<uint>("Code01");

                    b.Property<uint>("Code02");

                    b.Property<uint>("DeviceEnum00");

                    b.Property<uint>("DeviceEnum01");

                    b.Property<uint>("DeviceEnum02");

                    b.Property<uint>("EventTypeEnum00");

                    b.Property<uint>("EventTypeEnum01");

                    b.Property<uint>("EventTypeEnum02");

                    b.Property<uint>("MetaKeys00");

                    b.Property<uint>("MetaKeys01");

                    b.Property<uint>("MetaKeys02");

                    b.HasKey("Id", "InputActionId");

                    b.ToTable("character_keybinding");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterModel", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<uint>("AccountId");

                    b.Property<sbyte>("ActiveCostumeIndex");

                    b.Property<uint>("ActivePath");

                    b.Property<byte>("ActiveSpec");

                    b.Property<byte>("Class");

                    b.Property<DateTime>("CreateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<DateTime?>("DeleteTime")
                        .HasColumnType("datetime");

                    b.Property<ushort>("FactionId");

                    b.Property<byte>("InnateIndex");

                    b.Property<sbyte>("InputKeySet");

                    b.Property<byte>("Level");

                    b.Property<float>("LocationX");

                    b.Property<float>("LocationY");

                    b.Property<float>("LocationZ");

                    b.Property<string>("Name")
                        .HasMaxLength(50);

                    b.Property<string>("OriginalName")
                        .HasMaxLength(50);

                    b.Property<DateTime>("PathActivatedTimestamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<byte>("Race");

                    b.Property<byte>("Sex");

                    b.Property<uint>("TimePlayedLevel");

                    b.Property<uint>("TimePlayedTotal");

                    b.Property<ushort>("Title");

                    b.Property<ushort>("WorldId");

                    b.Property<ushort>("WorldZoneId");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.ToTable("character");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterPathModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<byte>("Path");

                    b.Property<byte>("LevelRewarded");

                    b.Property<uint>("TotalXp");

                    b.Property<byte>("Unlocked");

                    b.HasKey("Id", "Path");

                    b.ToTable("character_path");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterPetCustomisationModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<byte>("Type");

                    b.Property<uint>("ObjectId");

                    b.Property<ulong>("FlairIdMask");

                    b.Property<string>("Name")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasDefaultValue("");

                    b.HasKey("Id", "Type", "ObjectId");

                    b.ToTable("character_pet_customisation");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterPetFlairModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<uint>("PetFlairId");

                    b.HasKey("Id", "PetFlairId");

                    b.ToTable("character_pet_flair");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterQuestModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<ushort>("QuestId");

                    b.Property<byte>("Flags");

                    b.Property<DateTime?>("Reset")
                        .HasColumnType("datetime");

                    b.Property<byte>("State");

                    b.Property<uint?>("Timer");

                    b.HasKey("Id", "QuestId");

                    b.ToTable("character_quest");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterQuestObjectiveModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<ushort>("QuestId");

                    b.Property<byte>("Index");

                    b.Property<uint>("Progress");

                    b.Property<uint?>("Timer");

                    b.HasKey("Id", "QuestId", "Index");

                    b.ToTable("character_quest_objective");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterSpellModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<uint>("Spell4BaseId");

                    b.Property<byte>("Tier");

                    b.HasKey("Id", "Spell4BaseId");

                    b.ToTable("character_spell");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterStatModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<byte>("Stat");

                    b.Property<float>("Value");

                    b.HasKey("Id", "Stat");

                    b.ToTable("character_stats");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterTitleModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<ushort>("Title");

                    b.Property<byte>("Revoked");

                    b.Property<uint>("TimeRemaining");

                    b.HasKey("Id", "Title");

                    b.ToTable("character_title");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterZonemapHexgroupModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<ushort>("ZoneMap");

                    b.Property<ushort>("HexGroup");

                    b.HasKey("Id", "ZoneMap", "HexGroup");

                    b.ToTable("character_zonemap_hexgroup");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.ItemModel", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<uint>("BagIndex");

                    b.Property<uint>("Charges");

                    b.Property<float>("Durability");

                    b.Property<uint>("ExpirationTimeLeft");

                    b.Property<uint>("ItemId");

                    b.Property<ushort>("Location");

                    b.Property<ulong?>("OwnerId");

                    b.Property<uint>("StackCount");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("item");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.MailAttachmentModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<uint>("Index");

                    b.Property<ulong>("ItemGuid");

                    b.HasKey("Id", "Index");

                    b.HasIndex("Id");

                    b.HasIndex("ItemGuid")
                        .IsUnique();

                    b.ToTable("mail_attachment");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.MailModel", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<uint>("CreatureId");

                    b.Property<ulong>("CurrencyAmount");

                    b.Property<byte>("CurrencyType");

                    b.Property<byte>("DeliveryTime");

                    b.Property<byte>("Flags");

                    b.Property<byte>("HasPaidOrCollectedCurrency");

                    b.Property<byte>("IsCashOnDelivery");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(2000);

                    b.Property<ulong>("RecipientId");

                    b.Property<ulong>("SenderId");

                    b.Property<byte>("SenderType");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<uint>("TextEntryMessage");

                    b.Property<uint>("TextEntrySubject");

                    b.HasKey("Id");

                    b.HasIndex("RecipientId");

                    b.ToTable("mail");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.ResidenceDecorModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<ulong>("DecorId");

                    b.Property<ushort>("ColourShiftId");

                    b.Property<uint>("DecorInfoId");

                    b.Property<ulong>("DecorParentId");

                    b.Property<uint>("DecorType");

                    b.Property<float>("Qw");

                    b.Property<float>("Qx");

                    b.Property<float>("Qy");

                    b.Property<float>("Qz");

                    b.Property<float>("Scale");

                    b.Property<float>("X");

                    b.Property<float>("Y");

                    b.Property<float>("Z");

                    b.HasKey("Id", "DecorId");

                    b.ToTable("residence_decor");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.ResidenceModel", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<ushort>("DoorDecorInfoId");

                    b.Property<ushort>("EntrywayDecorInfoId");

                    b.Property<ushort>("Flags");

                    b.Property<byte>("GardenSharing");

                    b.Property<ushort>("GroundWallpaperId");

                    b.Property<ushort>("MusicId");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<ulong>("OwnerId");

                    b.Property<byte>("PrivacyLevel");

                    b.Property<byte>("PropertyInfoId");

                    b.Property<byte>("ResourceSharing");

                    b.Property<ushort>("RoofDecorInfoId");

                    b.Property<ushort>("SkyWallpaperId");

                    b.Property<ushort>("WallpaperId");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId")
                        .IsUnique();

                    b.ToTable("residence");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.ResidencePlotModel", b =>
                {
                    b.Property<ulong>("Id");

                    b.Property<byte>("Index");

                    b.Property<byte>("BuildState");

                    b.Property<ushort>("PlotInfoId");

                    b.Property<byte>("PlugFacing");

                    b.Property<ushort>("PlugItemId");

                    b.HasKey("Id", "Index");

                    b.ToTable("residence_plot");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterAchievementModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("Achievements")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterActionSetAmpModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("ActionSetAmps")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterActionSetShortcutModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("ActionSetShortcuts")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterAppearanceModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("Appearance")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterBoneModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("Bones")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterCostumeItemModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterCostumeModel", "Costume")
                        .WithMany("Items")
                        .HasForeignKey("Id", "Index")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterCostumeModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("Costumes")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterCurrencyModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("Currencies")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterCustomisationModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("Customisations")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterDatacubeModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("Datacubes")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterEntitlementModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("Entitlements")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterKeybindingModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("Keybindings")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterPathModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("Paths")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterPetCustomisationModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("PetCustomisations")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterPetFlairModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("PetFlairs")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterQuestModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("Quests")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterQuestObjectiveModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterQuestModel", "Quest")
                        .WithMany("Objectives")
                        .HasForeignKey("Id", "QuestId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterSpellModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("Spells")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterStatModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("Stats")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterTitleModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("Titles")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.CharacterZonemapHexgroupModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("ZonemapHexgroups")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.ItemModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Character")
                        .WithMany("Items")
                        .HasForeignKey("OwnerId");
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.MailAttachmentModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.MailModel", "Mail")
                        .WithMany("Attachments")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("NexusForever.Database.Character.Model.ItemModel", "Item")
                        .WithOne("Attachment")
                        .HasForeignKey("NexusForever.Database.Character.Model.MailAttachmentModel", "ItemGuid")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.MailModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Recipient")
                        .WithMany("Mail")
                        .HasForeignKey("RecipientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.ResidenceDecorModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.ResidenceModel", "Residence")
                        .WithMany("Decor")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.ResidenceModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.CharacterModel", "Owner")
                        .WithOne("Residence")
                        .HasForeignKey("NexusForever.Database.Character.Model.ResidenceModel", "OwnerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NexusForever.Database.Character.Model.ResidencePlotModel", b =>
                {
                    b.HasOne("NexusForever.Database.Character.Model.ResidenceModel", "Residence")
                        .WithMany("Plots")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
