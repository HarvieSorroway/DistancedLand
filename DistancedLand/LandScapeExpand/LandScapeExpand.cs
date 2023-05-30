using Menu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DistancedLand.LandScapeExpand
{
    public class LandScapeExpand
    {
        public static void HookOn()
        {
            On.Region.GetRegionLandscapeScene += Region_GetRegionLandscapeScene;
            On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;
        }

        private static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, MenuScene self)
        { 
            if(self.sceneID == LandScapeEnum.Landscape_SSF)
            {
                self.sceneFolder = "Scenes" + Path.DirectorySeparatorChar.ToString() + "Landscape - SSF";

                if(self.flatMode)
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "landscape - ssf - flat", new Vector2(683f, 384f), false, true));
                else
                {
                    self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "ssf_landscape-5", new Vector2(85f, 91f), 8f, MenuDepthIllustration.MenuShader.Normal));
                    self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "ssf_landscape-4", new Vector2(85f, 91f), 4f, MenuDepthIllustration.MenuShader.Normal));
                    self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "ssf_landscape-3", new Vector2(85f, 91f), 5f, MenuDepthIllustration.MenuShader.Normal));
                    self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "ssf_landscape-2", new Vector2(85f, 91f), 4f, MenuDepthIllustration.MenuShader.Normal));
                    self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "ssf_landscape-1", new Vector2(85f, 91f), 3f, MenuDepthIllustration.MenuShader.Normal));
                }

                if (self.menu.ID == ProcessManager.ProcessID.FastTravelScreen || self.menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, "", "title_ssf_shadow", new Vector2(0.01f, 13f), true, false));
                    self.AddIllustration(new MenuIllustration(self.menu, self, "", "title_ssf", new Vector2(0.01f, 0.01f), true, false));
                    self.flatIllustrations[self.flatIllustrations.Count - 1].sprite.shader = self.menu.manager.rainWorld.Shaders["MenuText"];
                }
            }
            orig.Invoke(self);
        }

        private static MenuScene.SceneID Region_GetRegionLandscapeScene(On.Region.orig_GetRegionLandscapeScene orig, string regionAcro)
        {
            if(regionAcro == "SSF")
            {
                return LandScapeEnum.Landscape_SSF;
            }
            return orig.Invoke(regionAcro);
        }
    }

    public class LandScapeEnum
    {
        public static MenuScene.SceneID Landscape_SSF = new MenuScene.SceneID("Landscape_SSF", true);
    }
}
