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
            orig.Invoke(self);
            if(self.sceneID == LandScapeEnum.Landscape_SSF)
            {
                self.sceneFolder = "Scenes" + Path.DirectorySeparatorChar.ToString() + "Landscape - SSF";

                self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "Landscape - SSF - Flat", new Vector2(683f, 384f), false, true));
            }
            
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
