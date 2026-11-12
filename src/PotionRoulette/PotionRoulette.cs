using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace PotionRoulette;

public class PotionRoulette : Mod
{
    internal static bool Eligible(int t) => t == ItemID.LesserHealingPotion || t == ItemID.HealingPotion || t == ItemID.GreaterHealingPotion || t == ItemID.SuperHealingPotion;
    public override void HandlePacket(BinaryReader r, int whoAmI)
    {
        byte kind = r.ReadByte();
        if (kind == 0 && Main.netMode == NetmodeID.Server) {
            int type = r.ReadInt32();
            if (whoAmI < 0 || whoAmI >= Main.maxPlayers) return;
            Player p = Main.player[whoAmI];
            if (p.active && !p.dead && Eligible(type)) Roll(p);
        } else if (kind == 1 && Main.netMode == NetmodeID.MultiplayerClient) {
            int result = r.ReadByte();
            ApplyLocal(Main.LocalPlayer, result);
        }
    }
    internal void Roll(Player p)
    {
        var state = p.GetModPlayer<RoulettePlayer>();
        if (state.Cooldown > 0) return;
        state.Cooldown = 120; // Reject duplicate requests, including same-tick replays.
        int n = Main.rand.Next(RouletteTable.TotalWeight);
        int result = RouletteTable.Select(n);
        if (result == 1) {
            for (int i = 0; i < 10; i++)
                for (int attempt = 0; attempt < 40; attempt++) {
                    Vector2 pos = p.Center + new Vector2(Main.rand.Next(-192,193), Main.rand.Next(-128,129));
                    float distance = Vector2.Distance(pos,p.Center);
                    if (distance < 96 || distance > 192 || pos.X < 32 || pos.Y < 32 || pos.X > Main.maxTilesX*16-64 || pos.Y > Main.maxTilesY*16-64 || Collision.SolidCollision(pos,32,24)) continue;
                    int id = NPC.NewNPC(p.GetSource_Misc("PotionRoulette"),(int)pos.X + 16,(int)pos.Y + 24,NPCID.BlueSlime);
                    if (id < Main.maxNPCs) Main.npc[id].netUpdate = true;
                    break;
                }
        }
        if (result == 2) p.QuickSpawnItem(p.GetSource_Misc("PotionRoulette"),ItemID.Meowmere);
        if (Main.netMode == NetmodeID.Server) {
            var packet = GetPacket(); packet.Write((byte)1); packet.Write((byte)result); packet.Send(p.whoAmI);
        } else ApplyLocal(p,result);
    }
    private static void ApplyLocal(Player p, int result)
    {
        string[] names = {"풀피 회복!","슬라임 습격!","미야우미어 당첨!","좀비 합창!","중력 반전!","동굴 탐험가!","나이트비전!","저항!","전투!"};
        if (result < 0 || result >= names.Length) return;
        Main.NewText("[포션 룰렛] " + names[result],Color.Gold);
        if (result == 0) { p.statLife = p.statLifeMax2; NetMessage.SendData(MessageID.PlayerLifeMana,number:p.whoAmI); }
        if (result == 3) p.GetModPlayer<RoulettePlayer>().SoundTicks = 61;
        int[] buffs = {0,0,0,0,BuffID.Gravitation,BuffID.Spelunker,BuffID.NightOwl,BuffID.Endurance,BuffID.Battle};
        if (result >= 4) p.AddBuff(buffs[result],result == 4 ? 300 : 3600);
        if (result == 4) p.gravDir = -1f;
    }
}
public class PotionHook : GlobalItem
{
    public override void OnConsumeItem(Item item, Player player)
    {
        if (!PotionRoulette.Eligible(item.type) || player.whoAmI != Main.myPlayer) return;
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            var packet = Mod.GetPacket(); packet.Write((byte)0); packet.Write(item.type); packet.Send();
        } else ((PotionRoulette)Mod).Roll(player);
    }
}
public class RoulettePlayer : ModPlayer
{
    internal int Cooldown;
    internal int SoundTicks;
    public override void PostUpdate()
    {
        if (Cooldown > 0) Cooldown--;
        if (SoundTicks > 0 && Player.whoAmI == Main.myPlayer && !Main.dedServ) {
            if (SoundTicks % 30 == 1) SoundEngine.PlaySound(SoundID.Zombie1);
            SoundTicks--;
        }
    }
    public override void UpdateDead() { SoundTicks = 0; if (Cooldown > 0) Cooldown--; }
}
