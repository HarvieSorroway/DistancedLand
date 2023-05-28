using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistancedLand.CustomFix
{
    public class RegionNameAndFastTravelFix
    {
        public static void HookOn()
        {
            On.Menu.SlugcatSelectMenu.SlugcatPageContinue.ctor += SlugcatPageContinue_ctor;
            On.PlayerProgression.MiscProgressionData.ConditionalShelterData.GetShelterRegion += ConditionalShelterData_GetShelterRegion;
        }

        private static string ConditionalShelterData_GetShelterRegion(On.PlayerProgression.MiscProgressionData.ConditionalShelterData.orig_GetShelterRegion orig, PlayerProgression.MiscProgressionData.ConditionalShelterData self)
        {
            string result = orig.Invoke(self);
            if (WarpFixer.fixRegionName.Contains(self.shelterName.Split('_')[0]))
                result = self.shelterName.Split('_')[0];
            return result;
        }

        private static void SlugcatPageContinue_ctor(On.Menu.SlugcatSelectMenu.SlugcatPageContinue.orig_ctor orig, Menu.SlugcatSelectMenu.SlugcatPageContinue self, Menu.Menu menu, Menu.MenuObject owner, int pageIndex, SlugcatStats.Name slugcatNumber)
        {
            orig.Invoke(self, menu, owner, pageIndex, slugcatNumber);
            string text = "";

            if (WarpFixer.fixRegionName.Contains(self.saveGameData.shelterName.Split('_')[0]))
            {
                if (self.saveGameData.shelterName != null && self.saveGameData.shelterName.Length > 2)
                {
                    text = Region.GetRegionFullName(self.saveGameData.shelterName.Split('_')[0], slugcatNumber);
                    if (text.Length > 0)
                    {
                        text = menu.Translate(text);
                        text = string.Concat(new string[]
                        {
                            text,
                            " - ",
                            menu.Translate("Cycle"),
                            " ",
                            ((slugcatNumber == SlugcatStats.Name.Red) ? (RedsIllness.RedsCycles(self.saveGameData.redsExtraCycles) - self.saveGameData.cycle) : self.saveGameData.cycle).ToString()
                        });
                        if (ModManager.MMF)
                        {
                            TimeSpan timeSpan = TimeSpan.FromSeconds((double)self.saveGameData.gameTimeAlive + (double)self.saveGameData.gameTimeDead);
                            text = text + " (" + SpeedRunTimer.TimeFormat(timeSpan) + ")";
                        }
                    }
                }
                self.regionLabel.text = text;
            }

        }
    }
}
