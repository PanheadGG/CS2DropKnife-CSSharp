using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;

namespace DropKnife;
public class DropKnife : BasePlugin
{
    public override string ModuleName => "Drop Knife Plugin";

    public override string ModuleVersion => "0.0.1";

    public override string ModuleAuthor => "PanheadGG";
    private static bool drop_knife_only_one_time = true;
    private static List<int> dropedPlayerSlots = [];
    public override void Load(bool hotReload)
    {
        Console.WriteLine("Drop Knife Plugin Loaded!");
    }
    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        dropedPlayerSlots.Clear();
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerChat(EventPlayerChat @event, GameEventInfo @info)
    {
        string message = @event.Text;
        if (message.Equals("!drop")||
            message.Equals(".drop")||
            message.Equals(".d") ){
            int playerSlot = @event.Userid;
            try
            {
                CCSPlayerController player = Utilities.GetPlayerFromSlot(playerSlot)!;
                if (player == null || !player.IsValid || player.IsBot || player.IsHLTV)
                {
                    return HookResult.Continue;
                }

                string chat_message = @event.Text.ToLower();
                if (chat_message.Equals("!drop")
                || chat_message.Equals("/drop")
                || chat_message.Equals(".drop")
                || chat_message.Equals("!d")
                || chat_message.Equals("/d")
                || chat_message.Equals(".d"))
                {
                    DoDropKnife(player);
                }
            }
            catch (System.Exception)
            {
                return HookResult.Continue;
            }
        }
        

        return HookResult.Continue;
    }

    public void DoDropKnife(CCSPlayerController sender)
    {
        if(drop_knife_only_one_time){
            foreach (int playerSlot in dropedPlayerSlots)
            {
                if (playerSlot == sender.UserId) return;
            }
        }
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            if(player.PawnIsAlive && player.Team == sender.Team) {
                nint knife_pointer = sender.GiveNamedItem("weapon_knife");
                CBasePlayerWeapon knife = new(knife_pointer);
                // 获取玩家当前位置  
                var playerPosition = player.PlayerPawn.Value!.AbsOrigin;
                if (playerPosition == null) return;

                // 创建新位置（在玩家上方50单位）  
                var newPosition = new CounterStrikeSharp.API.Modules.Utils.Vector(
                    playerPosition.X,
                    playerPosition.Y,
                    playerPosition.Z + 50.0f  // 高度增加50单位  
                );
                knife.Teleport(newPosition);
            }
        }
        dropedPlayerSlots.Add((int)sender.UserId!);
    }

    [ConsoleCommand("drop_knife_only_one_time", "Drop times controll")]
    [CommandHelper(minArgs: 0, usage: "[boolen]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if(caller == null) return;
        if (command.ArgCount == 1) { caller.PrintToConsole("drop_knife_only_one_time = " + (drop_knife_only_one_time ? "true" : "false")); return; }
        else if(command.ArgCount >= 2)
        {
            if (command.ArgByIndex(1).Equals("0")||
                command.ArgByIndex(1).Equals("false")) drop_knife_only_one_time = false;
            else drop_knife_only_one_time = true;
        }
    }
}
