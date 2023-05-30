using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DistancedLand.CustomCreatures
{
    static class LizardHooks
    {
        public static void HookOn()
        {
            On.Lizard.ctor += Lizard_ctor;
            On.Lizard.SpearStick += Lizard_SpearStick;
            On.LizardJumpModule.InitiateJump += LizardJumpModule_InitiateJump;
            On.LizardJumpModule.RunningUpdate += LizardJumpModule_RunningUpdate;
            On.LizardAI.ctor += LizardAI_ctor;

            On.LizardGraphics.DynamicBodyColor += LizardGraphics_DynamicBodyColor;
        }

        private static Color LizardGraphics_DynamicBodyColor(On.LizardGraphics.orig_DynamicBodyColor orig, LizardGraphics self, float f)
        {
            if(self.lizard.Template.type == Enums.NavyLizard)
                return Color.Lerp(self.palette.blackColor, self.whiteCamoColor, self.whiteCamoColorAmount);
            return orig(self, f);
        }

        private static void LizardAI_ctor(On.LizardAI.orig_ctor orig, LizardAI self, AbstractCreature creature, World world)
        {
            orig(self,creature, world);
            if (self.lizard.Template.type == Enums.NavyLizard)
            {
                self.lurkTracker = new LizardAI.LurkTracker(self, self.lizard);
                self.AddModule(self.lurkTracker);
                self.utilityComparer.AddComparedModule(self.lurkTracker, null, Mathf.Lerp(0.4f, 0.3f, creature.personality.energy), 1f);
            }
        }

        private static void LizardJumpModule_RunningUpdate(On.LizardJumpModule.orig_RunningUpdate orig, LizardJumpModule self)
        {
            if (self.lizard.Submersion < 1 && self.lizard.Template.type == Enums.NavyLizard)
                return;
            orig(self);
        }

        private static void LizardJumpModule_InitiateJump(On.LizardJumpModule.orig_InitiateJump orig, LizardJumpModule self, LizardJumpModule.JumpFinder jump, bool chainJump)
        {
            orig(self,jump, chainJump);
            //cd
            if (self.lizard.Template.type == Enums.NavyLizard)
            {
                self.lizard.timeToRemainInAnimation /=2;
            }
        }

        private static bool Lizard_SpearStick(On.Lizard.orig_SpearStick orig, Lizard self, Weapon source, float dmg, BodyChunk chunk, PhysicalObject.Appendage.Pos onAppendagePos, UnityEngine.Vector2 direction)
        {
           var re = orig(self,source,dmg,chunk,onAppendagePos,direction);
           //漏气 也可以删掉
           if (re)
           {
               if (source != null && self.Template.type == CreatureTemplate.Type.CyanLizard && !self.dead && !(chunk.index == 0 && self.HitInMouth(direction)) && 
                   self.jumpModule.gasLeakPower > 0f && self.jumpModule.gasLeakSpear == null && source is Spear spear && chunk.index < 2 &&
                   (self.animation == Lizard.Animation.Jumping || self.animation == Lizard.Animation.PrepareToJump || Random.value < ((chunk.index == 1) ? 0.5f : 0.25f)))
               {
                   self.jumpModule.gasLeakSpear = spear;
               }
           }
           return re;
        }

        private static void Lizard_ctor(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.Template.type == Enums.NavyLizard)
            {
                //颜色你改下.jpg
                self.effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.49f, 0.04f, 0.6f), 1f, Custom.ClampedRandomVariation(0.5f, 0.15f, 0.1f));
                self.jumpModule = new LizardJumpModule(self);
            }
        }
    }

}
