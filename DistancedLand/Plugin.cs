using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using DistancedLand;
using DistancedLand.CustomFix;
using DistancedLand.CustomObjects;
using DistancedLand.LandScapeExpand;
using UnityEngine;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
[BepInPlugin("distancedland", "DistancedLand", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    static bool inited;
    public void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig.Invoke(self);
        if (inited) return;

        //On.Player.Update += Player_Update;

        FakeWaterHooks.HookOn();
        CustomShelterDoorRule.HookOn();

        ModFixerTx.ModFixerRx.ApplyTreatment(new WarpFixer());
        RegionNameAndFastTravelFix.HookOn();
        LandScapeExpand.HookOn();

        Enums.Register();
        LoadResources(self);
    }

    void LoadResources(RainWorld rainworld)
    {
        string path = AssetManager.ResolveFilePath("AssetBundle/distancedlandbundle");
        AssetBundle ab = AssetBundle.LoadFromFile(path);

        Shader fakeWater = ab.LoadAsset<Shader>("assets/myshader/fakewater.shader");
        rainworld.Shaders.Add("FakeWater", FShader.CreateShader("FakeWater", fakeWater));
    }

    private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig.Invoke(self, eu);
        if (Input.GetKeyDown(KeyCode.Y))
        {
            self.room.game.rainWorld.processManager.RequestMainProcessSwitch(ProcessManager.ProcessID.FastTravelScreen);
        }
    }

}