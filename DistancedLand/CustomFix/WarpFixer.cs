using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using Menu;
using ModFixerTx;
using MonoMod.RuntimeDetour;
using RWCustom;
using UnityEngine;

public class WarpFixer : ModFixerTx.ModFixerTx
{
    public static FieldInfo WarpModMenu_warpActive;
    public static FieldInfo WarpModMenu_newRegion;
    public static FieldInfo WarpModMenu_newRoom;
    public static FieldInfo WarpModMenu_warpError;

    public static FieldInfo RegionSwitcher_error;
    public static MethodInfo RegionSwitcher_SwitchRegions;
    public static MethodInfo RegionSwitcher_GetErrorText;
    public static Type RegionSwitcherType;

    public static readonly string[] fixRegionName = new string[] {"SSF"};

    public WarpFixer() : base("Warp", "Menu", "warp")
    {
    }

    public override void Apply()
    {
        base.Apply();


        var warpModMenuClass = Assembly.GetType("WarpModMenu", true);
        RegionSwitcherType = Assembly.GetType("RegionSwitcher", true);

        EmgTxCustom.Log(warpModMenuClass.ToString());
        EmgTxCustom.Log(RegionSwitcherType.ToString());

        var origMethod = warpModMenuClass.GetMethod("WarpOverworldUpdate", BindingFlags.Static | BindingFlags.Public);
        var detourMethod = typeof(WarpFixer).GetMethod("WarpOverworldUpdateHook", BindingFlags.Static | BindingFlags.Public);

        WarpModMenu_warpActive = warpModMenuClass.GetField("warpActive", BindingFlags.Static | BindingFlags.Public);
        WarpModMenu_newRegion = warpModMenuClass.GetField("newRegion", BindingFlags.Static | BindingFlags.Public);
        WarpModMenu_newRoom = warpModMenuClass.GetField("newRoom", BindingFlags.Static | BindingFlags.Public);
        WarpModMenu_warpError = warpModMenuClass.GetField("warpError", BindingFlags.Static | BindingFlags.Public);

        RegionSwitcher_error = RegionSwitcherType.GetField("error", BindingFlags.Public | BindingFlags.Instance);
        RegionSwitcher_SwitchRegions = RegionSwitcherType.GetMethod("SwitchRegions", BindingFlags.Public | BindingFlags.Instance);
        RegionSwitcher_GetErrorText = RegionSwitcherType.GetMethod("GetErrorText", BindingFlags.Public | BindingFlags.Instance);

        EmgTxCustom.Log($"{RegionSwitcher_SwitchRegions},{RegionSwitcher_GetErrorText},{RegionSwitcher_error}");

        Hook hook = new Hook(origMethod, detourMethod);
    }

    public static void WarpOverworldUpdateHook(Action<OverWorld, RainWorldGame> orig,OverWorld overWorld,RainWorldGame game)
    {
        if (game.IsStorySession && (!global::ModManager.MSC || !game.rainWorld.safariMode))
        {
            if ((bool)WarpModMenu_warpActive.GetValue(null))
            {
                Player player = (overWorld.game.Players.Count <= 0) ? null : (overWorld.game.Players[0].realizedCreature as Player);
                if (player == null || player.inShortcut)
                {
                    return;
                }

                string newRegion = (string)WarpModMenu_newRegion.GetValue(null);
                if (newRegion != null && fixRegionName.Contains(newRegion) && newRegion != overWorld.activeWorld.region.name)
                {
                    var rs = Activator.CreateInstance(RegionSwitcherType);
                    EmgTxCustom.Log(rs.ToString());
                    try
                    {
                        WarpModMenu_warpError.SetValue(null, "");
                        RegionSwitcher_SwitchRegions.Invoke(rs, new object[] { game, newRegion, WarpModMenu_newRoom.GetValue(null), new IntVector2(0, 0) });
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        Debug.Log("WARP ERROR: " + RegionSwitcher_GetErrorText.Invoke(rs,new object[] {RegionSwitcher_error.GetValue(rs)}));
                        WarpModMenu_warpError.SetValue(null, RegionSwitcher_GetErrorText.Invoke(rs, new object[] { RegionSwitcher_error.GetValue(rs) }));
                        overWorld.game.pauseMenu = new PauseMenu(overWorld.game.manager, overWorld.game);
                    }
                    WarpModMenu_warpActive.SetValue(null, false);
                    return;
                }
            }
        }
        orig.Invoke(overWorld, game);
    }
}