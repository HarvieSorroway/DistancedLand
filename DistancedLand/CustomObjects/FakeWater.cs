using DevInterface;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DistancedLand.CustomObjects
{
    public class FakeWater : CosmeticSprite
    {
        public RectangularDynamicSoundLoop waterSounds;
        public RectangularDynamicSoundLoop upsetWaterSounds;
        public Water.WaterSoundObject waterSoundObject;
        public PlacedObject placedObject;
        SurfacePoint[,] surface;
        FSprite firstWaterSprite;
        List<Water.BubbleEmitter> bubbleEmitters;
        List<Water.RippleWave> rippleWaves;

        float triangleWidth = 20f;

        public bool culled = false;

        int pointsToRender;

        public float leftMargin;
        public float rightMargin;
        private float sinCounter;

        public float originalWaterLevel;
        public float fWaterLevel;

        private float dx;
        private float dt;
        private float C;
        private float R;
        public float cosmeticSurfaceDisplace;
        private RoomPalette palette;
        public float cosmeticLowerBorder = -1f;


        public float[,] camerasOutOfBreathFac;

        public float viscosity;

        public bool WaterIsLethal;

        private float waveAmplitude => Mathf.Lerp(1f, 40f, this.room.roomSettings.WaveAmplitude);
        private float waveSpeed => Mathf.Lerp(-0.033333335f, 0.033333335f, this.room.roomSettings.WaveSpeed);
    
        private float waveLength => Mathf.Lerp(50f, 750f, this.room.roomSettings.WaveLength);

        private float rollBackLength => Mathf.Lerp(2f, 0f, this.room.roomSettings.SecondWaveLength);
        private float rollBackAmp => room.roomSettings.SecondWaveAmplitude;

        public IntRect rect;
        public FloatRect GetFloatRect => new FloatRect((float)this.rect.left * 20f, (float)this.rect.bottom * 20f, (float)this.rect.right * 20f + 20f, (float)this.rect.top * 20f + 20f);

        public FakeWater(Room room, PlacedObject placedObject)
        {
            this.placedObject = placedObject;
            this.rect = (placedObject.data as PlacedObject.GridRectObjectData).Rect;
            this.room = room;
            this.originalWaterLevel = GetFloatRect.top - GetFloatRect.bottom;
            if (ModManager.MSC && room.roomRain != null && room.roomRain.globalRain != null && (room.roomSettings.DangerType == RoomRain.DangerType.Flood || room.roomSettings.DangerType == RoomRain.DangerType.FloodAndRain))
            {
                this.fWaterLevel = this.originalWaterLevel + room.roomRain.globalRain.flood;
            }
            this.fWaterLevel = this.originalWaterLevel;
            this.dx = 0.0005f * this.triangleWidth;
            this.dt = 0.0045f;
            this.C = 1f;
            this.R = this.C * this.dt / this.dx;
            this.leftMargin = 220f;
            this.rightMargin = 220f;
            float num = 0f;
            float num2 = GetFloatRect.right - GetFloatRect.left;
            this.camerasOutOfBreathFac = new float[room.game.cameras.Length, 4];
            int num3 = (int)((num2 - num) / this.triangleWidth) + 1;
            this.surface = new SurfacePoint[num3, 2];
            for (int i = 0; i < this.surface.GetLength(0); i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    this.surface[i, j] = new SurfacePoint(new Vector2(num + ((float)i + ((j == 0) ? 0f : 0.5f)) * this.triangleWidth, this.originalWaterLevel));
                }
            }
            this.pointsToRender = Custom.IntClamp((int)((GetFloatRect.right - GetFloatRect.left) / this.triangleWidth) + 2, 0, this.surface.GetLength(0));
            this.bubbleEmitters = new List<Water.BubbleEmitter>();
            this.rippleWaves = new List<Water.RippleWave>();
            this.waterSoundObject = new Water.WaterSoundObject();
            room.AddObject(this.waterSoundObject);
            if (ModManager.MSC && room.waterInverted)
            {
                this.waterSounds = new RectangularDynamicSoundLoop(this.waterSoundObject, new FloatRect(0f, room.PixelHeight - (float)(room.defaultWaterLevel - 1) * 20f, room.PixelWidth, room.PixelHeight - (float)room.defaultWaterLevel * 20f), room);
            }
            else
            {
                this.waterSounds = new RectangularDynamicSoundLoop(this.waterSoundObject, new FloatRect(0f, (float)(room.defaultWaterLevel - 1) * 20f, room.PixelWidth, (float)room.defaultWaterLevel * 20f), room);
            }
            this.waterSounds.sound = SoundID.Water_Surface_Calm_LOOP;
            if (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.LethalWater) > 0f)
            {
                this.WaterIsLethal = true;
            }
            cosmeticLowerBorder = 0f;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];

            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["GravityDisruptor"];
            sLeaser.sprites[0].scale = 37.5f;

            //TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[pointsToRender * 2];
            //for (int i = 0; i < pointsToRender; i++)
            //{
            //    int num = i * 2;
            //    array[num] = new TriangleMesh.Triangle(num, num + 1, num + 2);
            //    array[num + 1] = new TriangleMesh.Triangle(num + 1, num + 2, num + 3);
            //}
            //sLeaser.sprites[1] = new WaterTriangleMesh("Futile_White", array, true);
            //sLeaser.sprites[1].shader = this.room.game.rainWorld.Shaders["WaterSurface"];
            //TriangleMesh.Triangle[] array2 = new TriangleMesh.Triangle[pointsToRender * 2];
            //for (int j = 0; j < pointsToRender; j++)
            //{
            //    int num2 = j * 2;
            //    array2[num2] = new TriangleMesh.Triangle(num2, num2 + 1, num2 + 2);
            //    array2[num2 + 1] = new TriangleMesh.Triangle(num2 + 1, num2 + 2, num2 + 3);
            //}
            sLeaser.sprites[1] = new CustomFSprite("Futile_White");
            sLeaser.sprites[1].color = new Color(0f, 0f, 0f);
            sLeaser.sprites[1].shader = room.game.rainWorld.Shaders["FakeWater"];
            firstWaterSprite = sLeaser.sprites[1];
            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            sLeaser.sprites[0].RemoveFromContainer();
            sLeaser.sprites[1].RemoveFromContainer();
            //sLeaser.sprites[2].RemoveFromContainer();

            rCam.ReturnFContainer("HUD").AddChild(sLeaser.sprites[0]);
            rCam.ReturnFContainer("HUD").AddChild(sLeaser.sprites[1]);
            //rCam.ReturnFContainer("HUD").AddChild(sLeaser.sprites[2]);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            this.palette = palette;
        }


        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (!this.culled != sLeaser.sprites[0].isVisible)
            {
                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    sLeaser.sprites[i].isVisible = !this.culled;
                }
            }
            if (this.culled)
            {
                return;
            }


            sLeaser.sprites[0].x = 1000000f;

            float y = placedObject.pos.y - camPos.y;

            int num = Custom.IntClamp(this.PreviousSurfacePoint(camPos.x - 30f), 0, this.surface.GetLength(0) - 1);
            int num2 = Custom.IntClamp(num + this.pointsToRender, 0, this.surface.GetLength(0) - 1);

            Vector2 ButtomLeft = placedObject.pos;
            Vector2 TopLeft = placedObject.pos + new Vector2(0f, GetFloatRect.top - GetFloatRect.bottom);
            Vector2 TopRight = placedObject.pos + new Vector2(GetFloatRect.right - GetFloatRect.left, GetFloatRect.top - GetFloatRect.bottom);
            Vector2 ButtonRight = placedObject.pos + new Vector2(GetFloatRect.right - GetFloatRect.left, 0f);

            (sLeaser.sprites[1] as CustomFSprite).MoveVertice(0, ButtomLeft - camPos);
            (sLeaser.sprites[1] as CustomFSprite).MoveVertice(1, TopLeft - camPos);
            (sLeaser.sprites[1] as CustomFSprite).MoveVertice(2, TopRight - camPos);
            (sLeaser.sprites[1] as CustomFSprite).MoveVertice(3, ButtonRight - camPos);

            for(int i = 0;i < 4; i++)
            {
                (sLeaser.sprites[1] as CustomFSprite).verticeColors[i] = Color.Lerp(this.palette.waterSurfaceColor2, this.palette.waterShineColor, (1f - this.palette.fogAmount));
            }

            //for (int i = num; i < num2; i++)
            //{
            //    int num3 = (i - num) * 2;
            //    Vector2 vector = this.surface[i, 0].defaultPos + Vector2.Lerp(this.surface[i, 0].lastPos, this.surface[i, 0].pos, timeStacker) - camPos + new Vector2(0f, this.cosmeticSurfaceDisplace) + placedObject.pos + (GetFloatRect.top - GetFloatRect.bottom) * Vector2.up;
            //    Vector2 vector2 = this.surface[i, 1].defaultPos + Vector2.Lerp(this.surface[i, 1].lastPos, this.surface[i, 1].pos, timeStacker) - camPos + new Vector2(0f, this.cosmeticSurfaceDisplace) + placedObject.pos + (GetFloatRect.top - GetFloatRect.bottom) * Vector2.up;
            //    Vector2 vector3 = this.surface[i + 1, 0].defaultPos + Vector2.Lerp(this.surface[i + 1, 0].lastPos, this.surface[i + 1, 0].pos, timeStacker) - camPos + new Vector2(0f, this.cosmeticSurfaceDisplace) + placedObject.pos + (GetFloatRect.top - GetFloatRect.bottom) * Vector2.up;
            //    Vector2 vector4 = this.surface[i + 1, 1].defaultPos + Vector2.Lerp(this.surface[i + 1, 1].lastPos, this.surface[i + 1, 1].pos, timeStacker) - camPos + new Vector2(0f, this.cosmeticSurfaceDisplace) + placedObject.pos + (GetFloatRect.top - GetFloatRect.bottom) * Vector2.up;
            //    //vector = Custom.ApplyDepthOnVector(vector, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * 0.6666667f), -10f);
            //    //vector2 = Custom.ApplyDepthOnVector(vector2, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * 0.6666667f), 30f);
            //    //vector3 = Custom.ApplyDepthOnVector(vector3, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * 0.6666667f), -10f);
            //    //vector4 = Custom.ApplyDepthOnVector(vector4, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * 0.6666667f), 30f);
            //    //if (i == num)
            //    //{
            //    //    vector2.x -= 100f;
            //    //}
            //    //else if (i == num2 - 1)
            //    //{
            //    //    vector2.x += 100f;
            //    //}
            //    Vector2 zero = Vector2.zero;
            //    //if (ModManager.MSC && this.room.waterInverted)
            //    //{
            //    //    zero = new Vector2(0f, -40f);
            //    //}
            //    (sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(num3, vector);
            //    (sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(num3 + 1, vector2 + zero);
            //    (sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(num3 + 2, vector3);
            //    (sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(num3 + 3, vector4 + zero);
            //    float num4 = rCam.room.WaterShinyness(Vector2.Lerp(this.surface[i, 0].LastRoomPos, this.surface[i, 0].RoomPos, timeStacker), timeStacker);
            //    float num5 = Vector2.Dot((this.surface[i + 1, 0].RoomPos - this.surface[i, 0].RoomPos).normalized, Custom.DegToVec(60f));
            //    if (i > 0)
            //    {
            //        num5 = Mathf.Lerp(num5, Vector2.Dot((this.surface[i, 0].RoomPos - this.surface[i - 1, 0].RoomPos).normalized, Custom.DegToVec(60f)), 0.5f);
            //    }
            //    num4 = Mathf.Pow(num4, 0.1f) * Mathf.InverseLerp(0.9f - num4 * 0.1f, 0.98f - num4 * 0.05f, num5);
            //    Color color = Color.Lerp(this.palette.waterSurfaceColor1, this.palette.waterShineColor, num4);
            //    Vector2 vector5 = this.surface[i, 0].defaultPos + Vector2.Lerp(this.surface[i, 0].lastPos, this.surface[i, 0].pos, timeStacker);
            //    if (this.room.Darkness(vector5) > 0f)
            //    {
            //        for (int j = 0; j < this.room.lightSources.Count; j++)
            //        {
            //            float num6 = Mathf.InverseLerp(vector5.y + 500f, vector5.y, this.room.lightSources[j].Pos.y);
            //            if (this.room.lightSources[j].Pos.y < vector5.y)
            //            {
            //                num6 *= Mathf.InverseLerp(vector5.y - this.room.lightSources[j].Rad * 0.7f, vector5.y, this.room.lightSources[j].Pos.y);
            //            }
            //            color = Custom.Screen(color, this.room.lightSources[j].color * this.room.lightSources[j].Alpha * num6 * Mathf.InverseLerp(10f + 160f * num6, 10f, Mathf.Abs(Custom.DistanceToLine(this.room.lightSources[j].Pos, vector5, vector5 - Custom.PerpendicularVector((vector - vector3).normalized) + new Vector2(0f, 1f - num6)))) * this.room.Darkness(vector5));
            //        }
            //        for (int k = 0; k < this.room.cosmeticLightSources.Count; k++)
            //        {
            //            float num7 = Mathf.InverseLerp(vector5.y + 500f, vector5.y, this.room.cosmeticLightSources[k].Pos.y);
            //            if (this.room.cosmeticLightSources[k].Pos.y < vector5.y)
            //            {
            //                num7 *= Mathf.InverseLerp(vector5.y - this.room.cosmeticLightSources[k].Rad * 0.7f, vector5.y, this.room.cosmeticLightSources[k].Pos.y);
            //            }
            //            color = Custom.Screen(color, this.room.cosmeticLightSources[k].color * this.room.cosmeticLightSources[k].Alpha * num7 * Mathf.InverseLerp(10f + 160f * num7, 10f, Mathf.Abs(Custom.DistanceToLine(this.room.cosmeticLightSources[k].Pos, vector5, vector5 - Custom.PerpendicularVector((vector - vector3).normalized) + new Vector2(0f, 1f - num7)))) * this.room.Darkness(vector5));
            //        }
            //    }
            //    (sLeaser.sprites[1] as WaterTriangleMesh).verticeColors[num3] = color;
            //    (sLeaser.sprites[1] as WaterTriangleMesh).verticeColors[num3 + 1] = Color.Lerp(this.palette.waterSurfaceColor2, this.palette.waterShineColor, num4 * (1f - this.palette.fogAmount));
            //    (sLeaser.sprites[2] as WaterTriangleMesh).MoveVertice(num3, new Vector2(vector.x, y));
            //    (sLeaser.sprites[2] as WaterTriangleMesh).MoveVertice(num3 + 1, vector);
            //    (sLeaser.sprites[2] as WaterTriangleMesh).MoveVertice(num3 + 2, new Vector2(vector3.x, y));
            //    (sLeaser.sprites[2] as WaterTriangleMesh).MoveVertice(num3 + 3, vector3);
            //}
            //if (ModManager.MSC)
            //{
            //    for (int l = (num2 - num) * 2; l < (sLeaser.sprites[1] as WaterTriangleMesh).vertices.Length; l++)
            //    {
            //        (sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(l, new Vector2(3400f, this.fWaterLevel - camPos.y + this.cosmeticSurfaceDisplace));
            //        (sLeaser.sprites[2] as WaterTriangleMesh).MoveVertice(l, new Vector2(3400f, this.fWaterLevel - camPos.y + this.cosmeticSurfaceDisplace));
            //    }
            //}
            //(sLeaser.sprites[2] as WaterTriangleMesh).MoveVertice(0, new Vector2(placedObject.pos.x, y));
            //(sLeaser.sprites[2] as WaterTriangleMesh).MoveVertice(1, new Vector2(placedObject.pos.x, placedObject.pos.y + (GetFloatRect.top - GetFloatRect.bottom) - camPos.y));
            //(sLeaser.sprites[2] as WaterTriangleMesh).MoveVertice((sLeaser.sprites[2] as WaterTriangleMesh).vertices.Length - 2, new Vector2(GetFloatRect.right - GetFloatRect.left + placedObject.pos.x, placedObject.pos.y + (GetFloatRect.top - GetFloatRect.bottom) - camPos.y));
            //(sLeaser.sprites[2] as WaterTriangleMesh).MoveVertice((sLeaser.sprites[2] as WaterTriangleMesh).vertices.Length - 1, new Vector2(GetFloatRect.right - GetFloatRect.left + placedObject.pos.x, y));
            float t = Mathf.Lerp(this.camerasOutOfBreathFac[rCam.cameraNumber, 1], this.camerasOutOfBreathFac[rCam.cameraNumber, 0], timeStacker);
            float b = Mathf.Lerp(1f, 0.75f + 0.25f * Mathf.Sin(Mathf.Lerp(this.camerasOutOfBreathFac[rCam.cameraNumber, 3], this.camerasOutOfBreathFac[rCam.cameraNumber, 2], timeStacker) * 2f), t);
            if (rCam.followAbstractCreature != null && rCam.followAbstractCreature.realizedCreature != null && rCam.followAbstractCreature.realizedCreature.room == this.room && !rCam.followAbstractCreature.realizedCreature.dead)
            {
                Vector2 vector6 = Vector2.Lerp(rCam.followAbstractCreature.realizedCreature.mainBodyChunk.lastPos, rCam.followAbstractCreature.realizedCreature.mainBodyChunk.pos, timeStacker) - camPos;
                (sLeaser.sprites[1] as CustomFSprite).color = new Color(Mathf.InverseLerp(0f, rCam.sSize.x, vector6.x), Mathf.InverseLerp(0f, rCam.sSize.y, vector6.y), b);
            }
            else
            {
                (sLeaser.sprites[1] as CustomFSprite).color = new Color(0f, 0f, 0f);
            }
            //(sLeaser.sprites[1] as WaterTriangleMesh).verticeColors[(sLeaser.sprites[1] as WaterTriangleMesh).verticeColors.Length - 2] = this.palette.waterSurfaceColor1;
            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void Update()
        {
            this.culled = true;
            int num = 0;
            while (num < 4 && this.culled)
            {
                if (this.room.ViewedByAnyCamera(this.GetFloatRect.GetCorner(num), 40f))
                {
                    this.culled = false;
                }
                num++;
            }
            if (this.culled)
            {
                return;
            }

            this.waterSounds.Update();
            if (this.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SilenceWater) > 0f)
            {
                this.waterSounds.Volume = 1f - this.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SilenceWater);
            }
            this.viscosity = this.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterViscosity);
            for (int i = 0; i < this.camerasOutOfBreathFac.GetLength(0); i++)
            {
                this.camerasOutOfBreathFac[i, 1] = this.camerasOutOfBreathFac[i, 0];
                this.camerasOutOfBreathFac[i, 3] = this.camerasOutOfBreathFac[i, 2];
                if (this.room.game.cameras[i].followAbstractCreature != null && this.room.game.cameras[i].followAbstractCreature.realizedCreature != null && this.room.game.cameras[i].followAbstractCreature.realizedCreature is Player)
                {
                    if (this.room.game.cameras[i].followAbstractCreature.realizedCreature.dead)
                    {
                        this.camerasOutOfBreathFac[i, 0] = Custom.LerpAndTick(this.camerasOutOfBreathFac[i, 0], 0f, 0.2f, 0.05f);
                    }
                    else
                    {
                        this.camerasOutOfBreathFac[i, 0] = Mathf.InverseLerp(0.5f, 0.2f, (this.room.game.cameras[i].followAbstractCreature.realizedCreature as Player).airInLungs);
                    }
                }
                this.camerasOutOfBreathFac[i, 2] += 1f / Mathf.Lerp(40f, 4f, this.camerasOutOfBreathFac[i, 0]);
            }
            bool flag = ModManager.MSC && this.room.waterInverted;
            IntVector2 intVector = new IntVector2(Random.Range(0, this.room.TileWidth), Random.Range(0, this.room.defaultWaterLevel));
            if (flag)
            {
                intVector = new IntVector2(Random.Range(0, this.room.TileWidth), Random.Range(this.room.defaultWaterLevel, this.room.TileHeight * 20));
            }
            if (this.room.GetTile(intVector).Terrain == Room.Tile.TerrainType.Air && this.room.GetTile(intVector + new IntVector2(0, -1)).Terrain == Room.Tile.TerrainType.Solid)
            {
                this.room.AddObject(new Bubble(this.room.MiddleOfTile(intVector) + new Vector2(Mathf.Lerp(-10f, 10f, Random.value), -10f), new Vector2(0f, 0f), true, false));
            }
            this.sinCounter -= this.waveSpeed * Mathf.Pow(1f - this.viscosity, 2f);
            
            for (int n = this.bubbleEmitters.Count - 1; n >= 0; n--)
            {
                if (this.bubbleEmitters[n].amount <= 0f)
                {
                    this.bubbleEmitters.RemoveAt(n);
                }
                else
                {
                    this.bubbleEmitters[n].Update();
                }
            }
            for (int num6 = this.rippleWaves.Count - 1; num6 >= 0; num6--)
            {
                if (this.rippleWaves[num6].life < 0f)
                {
                    this.rippleWaves.RemoveAt(num6);
                }
                else
                {
                    this.rippleWaves[num6].Update();
                }
            }
            float num7 = 0f;
            //for (int num8 = 0; num8 < this.surface.GetLength(0); num8++)
            //{
            //    if (num8 == 0)
            //    {
            //        this.surface[num8, 0].nextHeight = (2f * this.surface[num8, 0].height + (this.R - 1f) * this.surface[num8, 0].lastHeight + 2f * Mathf.Pow(this.R, 2f) * (this.surface[num8 + 1, 0].height - this.surface[num8, 0].height)) / (1f + this.R);
            //    }
            //    else if (num8 == this.surface.GetLength(0) - 1)
            //    {
            //        this.surface[num8, 0].nextHeight = (2f * this.surface[num8, 0].height + (this.R - 1f) * this.surface[num8, 0].lastHeight + 2f * Mathf.Pow(this.R, 2f) * (this.surface[num8 - 1, 0].height - this.surface[num8, 0].height)) / (1f + this.R);
            //    }
            //    else
            //    {
            //        this.surface[num8, 0].nextHeight = Mathf.Pow(this.R, 2f) * (this.surface[num8 - 1, 0].height + this.surface[num8 + 1, 0].height) + 2f * (1f - Mathf.Pow(this.R, 2f)) * this.surface[num8, 0].height - this.surface[num8, 0].lastHeight;
            //        if (this.room.GetTile(this.surface[num8, 0].defaultPos + new Vector2(0f, this.surface[num8, 0].height)).Terrain == Room.Tile.TerrainType.Solid)
            //        {
            //            this.surface[num8, 0].nextHeight *= (this.room.waterInFrontOfTerrain ? 0.95f : 0.75f);
            //        }
            //    }
            //    this.surface[num8, 0].nextHeight += Mathf.Lerp(-this.waveAmplitude, this.waveAmplitude, Random.value) * 0.005f * (1f - this.viscosity);
            //    this.surface[num8, 0].nextHeight *= 0.99f * (0.1f * (1f - this.viscosity) + 0.9f);
            //    //if (this.room.roomSettings.DangerType != RoomRain.DangerType.None && (!ModManager.MSC || this.room.roomSettings.DangerType != MoreSlugcatsEnums.RoomRainDangerType.Blizzard))
            //    //{
            //    //    this.surface[num8, 0].nextHeight += Mathf.Lerp(-1f, 1f, Random.value) * this.room.world.rainCycle.ScreenShake * this.room.roomSettings.RumbleIntensity * (1f - this.viscosity);
            //    //}
            //    num7 += this.surface[num8, 0].height;
            //    for (int num9 = 0; num9 < 2; num9++)
            //    {
            //        float num10 = -(float)num8 * this.triangleWidth / this.waveLength;
            //        this.surface[num8, num9].lastPos = this.surface[num8, num9].pos;
            //        this.surface[num8, num9].defaultPos.y = this.fWaterLevel;
            //        float num11 = this.surface[num8, num9].height * 3f;
            //        float num12 = 3f;
            //        for (int num13 = -1; num13 < 2; num13 += 2)
            //        {
            //            if (num8 + num13 * 2 > 0 && num8 + num13 * 2 < this.surface.GetLength(0) && Mathf.Abs(this.surface[num8, num9].height - this.surface[num8 + num13, num9].height) > Mathf.Abs(this.surface[num8, num9].height - this.surface[num8 + num13 * 2, num9].height))
            //            {
            //                num11 += this.surface[num8 + num13, num9].height;
            //                num12 += 1f;
            //            }
            //        }
            //        this.surface[num8, num9].pos = new Vector2(0f, num11 / num12);
            //        this.surface[num8, num9].pos += Custom.DegToVec((num10 + this.sinCounter * ((num9 == 1) ? 1f : -1f)) * 360f) * this.waveAmplitude;
            //        //if (this.room.roomSettings.DangerType != RoomRain.DangerType.None && (!ModManager.MSC || this.room.roomSettings.DangerType != MoreSlugcatsEnums.RoomRainDangerType.Blizzard))
            //        //{
            //        //    this.surface[num8, num9].pos += Custom.DegToVec(Random.value * 360f) * this.room.world.rainCycle.MicroScreenShake * 4f * this.room.roomSettings.RumbleIntensity * Mathf.Pow(1f - this.viscosity, 3f);
            //        //}
            //        this.surface[num8, num9].pos += Custom.DegToVec((num10 + this.sinCounter * ((num9 == 1) ? -1f : 1f)) * 360f * this.rollBackLength) * this.waveAmplitude * this.rollBackAmp * Mathf.Pow(1f - this.viscosity, 3f);
            //    }
            //}
            //num7 /= (float)this.surface.GetLength(0) * 1.5f;
            //for (int num14 = 0; num14 < this.surface.GetLength(0); num14++)
            //{
            //    this.surface[num14, 0].lastHeight = this.surface[num14, 0].height;
            //    float num15 = this.surface[num14, 0].nextHeight - num7;
            //    if (num14 > 0 && num14 < this.surface.GetLength(0) - 1)
            //    {
            //        num15 = Mathf.Lerp(num15, Mathf.Lerp(this.surface[num14 - 1, 0].nextHeight, this.surface[num14 + 1, 0].nextHeight, 0.5f), 0.01f);
            //    }
            //    this.surface[num14, 0].height = Mathf.Clamp(num15, -40f, 40f);
            //}
        }


        public int PreviousSurfacePoint(float horizontalPosition)
        {
            int num = Mathf.Clamp(Mathf.FloorToInt((horizontalPosition + 250f) / triangleWidth) + 2, 0, surface.GetLength(0) - 1);
            while (num > 0 && horizontalPosition < surface[num, 0].defaultPos.x + surface[num, 0].pos.x)
            {
                num--;
            }
            return num;
        }
        public float DetailedWaterLevel(float horizontalPosition)
        {
            int num = this.PreviousSurfacePoint(horizontalPosition);
            int num2 = Custom.IntClamp(num + 1, 0, this.surface.GetLength(0) - 1);
            float t = Mathf.InverseLerp(this.surface[num, 0].defaultPos.x + this.surface[num, 0].pos.x, this.surface[num2, 0].defaultPos.x + this.surface[num2, 0].pos.x, horizontalPosition);
            return Mathf.Lerp(this.surface[num, 0].defaultPos.y + this.surface[num, 0].pos.y, this.surface[num2, 0].defaultPos.y + this.surface[num2, 0].pos.y, t);
        }

        private class SurfacePoint
        {
            public Vector2 RoomPos
            {
                get
                {
                    return this.defaultPos + this.pos;
                }
            }

            public Vector2 LastRoomPos
            {
                get
                {
                    return this.defaultPos + this.lastPos;
                }
            }

            public SurfacePoint(Vector2 defaultPos)
            {
                this.defaultPos = defaultPos;
                this.lastPos = new Vector2(0f, 0f);
                this.pos = new Vector2(0f, 0f);
                this.height = 0f;
                this.lastHeight = 0f;
                this.nextHeight = 0f;
            }

            public Vector2 defaultPos;

            public Vector2 lastPos;

            public Vector2 pos;

            public float height;

            public float lastHeight;

            public float nextHeight;
        }
    }

    public class FakeWaterHooks
    {
        public static void HookOn()
        {
            FakeWaterEnums.Registry();

            On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRep;
            On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += ObjectsPage_DevObjectGetCategoryFromPlacedType;
            On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData;

            On.Room.Loaded += Room_Loaded;
        }

        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            orig.Invoke(self);
            try
            {
                foreach (var pObject in self.roomSettings.placedObjects)
                {
                    if (pObject.type == FakeWaterEnums.FakeWater)
                    {
                        self.AddObject(new FakeWater(self, pObject));
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self)
        {
            orig.Invoke(self);
            if(self.type == FakeWaterEnums.FakeWater)
            {
                self.data = new PlacedObject.GridRectObjectData(self);
            }
        }

        private static ObjectsPage.DevObjectCategories ObjectsPage_DevObjectGetCategoryFromPlacedType(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, ObjectsPage self, PlacedObject.Type type)
        {
            var result = orig.Invoke(self,type);
            if(type == FakeWaterEnums.FakeWater)
            {
                result = ObjectsPage.DevObjectCategories.Decoration;
            }
            return result;
        }

        private static void ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, DevInterface.ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj)
        {   
            if(tp == FakeWaterEnums.FakeWater)
            {
                if (pObj == null)
                {
                    pObj = new PlacedObject(tp, null);
                    pObj.pos = self.owner.room.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new Vector2(-683f, 384f), 0.25f) + Custom.DegToVec(Random.value * 360f) * 0.2f;
                    self.RoomSettings.placedObjects.Add(pObj);
                }

                var placedObjectRepresentation = new GridRectObjectRepresentation(self.owner, tp.ToString() + "_Rep", self, pObj, tp.ToString());

                self.tempNodes.Add(placedObjectRepresentation);
                self.subNodes.Add(placedObjectRepresentation);
                return;
            }
            orig.Invoke(self, tp, pObj);
        }
    }

    public class FakeWaterEnums
    {
        public static PlacedObject.Type FakeWater;

        public static void Registry()
        {
            FakeWater = new PlacedObject.Type("FakeWater", true);
        }
    }
}
