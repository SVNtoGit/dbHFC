using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Bots.DungeonBuddy.Attributes;
using Bots.DungeonBuddy.Helpers;
using Buddy.Coroutines;
using Styx;
using Styx.Common.Helpers;
using Styx.CommonBot;
using Styx.CommonBot.Coroutines;
using Styx.CommonBot.POI;
using Styx.Helpers;
using Styx.Pathing;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Vector2 = Tripper.Tools.Math.Vector2;

// ReSharper disable CheckNamespace
namespace Bots.DungeonBuddy.Raids.WarlordsOfDraenor
// ReSharper restore CheckNamespace
{
    // Class that contains common behavior for all WOD LFRs
	public abstract class HellfireCitadel : Dungeon
	{
		protected static LocalPlayer Me
		{
			get { return StyxWoW.Me; }
		}

        public override void OnEnter()
        {
            if (Me.IsTank())
            {
                Alert.Show(
                    "Tanking Not Supported",
                    string.Format(
                        "Tanking is not supported in the {0} script. If you wish to stay in raid and play manually then press 'Continue'. Otherwise you will automatically leave raid.",
                        Name),
                    30,
                    true,
                    true,
                    null,
                    () => Lua.DoString("LeaveParty()"),
                    "Continue",
                    "Leave");
            }
            else
            {
                Logger.Write(Colors.Gold, "It is highly recommended you do not afk while in a raid and be prepared to intervene if needed in the event something goes wrong or you're asked to perform a certain task.");
            }
            base.OnEnter();
        }

        [EncounterHandler(0, "Root Handler")]
		public virtual async Task<bool> RootBehavior(WoWUnit npc)
		{
			if (await ScriptHelpers.CancelCinematicIfPlaying())
				return true;

			return false;
		}

	}
    
	public class Hellbreach : HellfireCitadel
	{
		#region Overrides of Dungeon
	
		public override uint DungeonId
		{
			get { return 982; }
		}

        public override WoWPoint Entrance
        {
            get { return new WoWPoint(4071.450, -2133.950, 51.149); }
        }

        public override WoWPoint ExitLocation
        {
            get { return new WoWPoint(1491.654, 2952.643, 35.23913); }
        }

        private const uint MobId_TrainingDummy = 93828;
        
        public override void RemoveTargetsFilter(List<WoWObject> units)
        {
			units.RemoveAll(
				ret =>
				{
                    var unit = ret as WoWUnit;
				    if (unit == null)
				        return false;
                    
                    if (unit.Entry == MobId_IronReaver && unit.HasAura(SpellId_FullCharge))
						return true;
                    
                    if (unit.Entry == MobId_TrainingDummy)
                        return true;
                    
					return false;
				});
		}

	    public override void IncludeTargetsFilter(List<WoWObject> incomingunits, HashSet<WoWObject> outgoingunits)
	    {
            foreach (var obj in incomingunits)
            {
                var unit = obj as WoWUnit;
                if (unit != null)
                {
                    if(unit.Entry == MobId_VolatileFirebomb)
                        {
                            outgoingunits.Add(unit);
                        }
                }
            }
	    }
        
        public override void WeighTargetsFilter(List<Targeting.TargetPriority> units)
	    {
            var isMelee = Me.IsMelee();
            var israngedDps = !isMelee && Me.IsDps();
            foreach (var priority in units)
			{
				var unit = priority.Object as WoWUnit;
				if (unit != null)
				{
                    switch (unit.Entry)
                    {
                        case MobId_GraspingHand:
                        case MobId_VolatileFirebomb:
                        case MobId_GoreboundTerror:
							priority.Score += 6500;
							break;
                        case MobId_HulkingBerserker:
							priority.Score += 5000;
							break;
                        case MobId_FelfireDemolisher:
                        case MobId_FelfireArtillery:
                        case MobId_FelfireFlamebelcher:
							priority.Score += 4500;
							break;
                        case MobId_GoreboundFelcaster:
							priority.Score += 2500;
							break;
                    }
				}
			}
		}
        
        
        private const uint MobId_GoreboundTerror = 92522;
        private const uint MobId_HulkingBerserker = 92911;
        private const uint MobId_FelfireDemolisher = 91103;
        private const uint MobId_FelfireFlamebelcher = 90432;
        private const uint MobId_FelfireArtillery = 90485;
        private const uint MobId_GoreboundFelcaster = 90409;
        
        private const uint MobId_VolatileFirebomb = 93717;
        
        private const uint MobId_GraspingHand = 94865;

	    

        #endregion
        
        
		#region Root

        private const uint MobId_FlamethrowerCollision = 242372;
        private const uint MobId_ScaffoldingCollision = 242371;
        private const uint MobId_DrillCollision = 242387;
        private const uint MobId_TransportCollision = 242394;
        private const uint MobId_ScaffoldingCollision2 = 242441;

		[EncounterHandler(0, "Root Handler")]
		public Func<WoWUnit, Task<bool>> RootHandler()
		{
			AddAvoidObject(10, MobId_FlamethrowerCollision);
            AddAvoidObject(10, MobId_ScaffoldingCollision);
            AddAvoidObject(10, MobId_DrillCollision);
            AddAvoidObject(10, MobId_TransportCollision);
            AddAvoidObject(10, MobId_ScaffoldingCollision2);
            AddAvoidObject(30, AreaTriggerId_FoulPool);
            AddAvoidObject(30, AreaTriggerId_FieryPool);
            AddAvoidObject(30, AreaTriggerId_ShadowyPool);
            
			return async boss => false;
		}

		#endregion

        
        #region ReinforcedHellfireDoor
        
        private const uint MobId_ReinforcedHellfireDoor = 95652;
        
        private const uint SpellId_BelchFlame = 186545;
        private const uint SpellId_CapsuleImpact = 187996;
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/hellfire-assault-strategy-guide
		[EncounterHandler((int)MobId_ReinforcedHellfireDoor, "Reinforced Hellfire Door")]
		public Func<WoWUnit, Task<bool>> ReinforcedHellfireDoorEncounter()
		{

            AddAvoidLocation(
				ctx => true,
				5,
				m => ((WoWMissile) m).ImpactPosition,
				() => WoWMissile.InFlightMissiles.Where(m => m.SpellId == SpellId_BelchFlame));
                
            AddAvoidLocation(
				ctx => true,
				5,
				m => ((WoWMissile) m).ImpactPosition,
				() => WoWMissile.InFlightMissiles.Where(m => m.SpellId == SpellId_CapsuleImpact));
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        #endregion
        
        #region IronReaver
        
        private const uint MobId_IronReaver = 90284;
        
        private const int SpellId_Firebomb = 190377;
        private const uint AreaTriggerId_Immolation = 8662;
        private const uint AreaTriggerId_FuelStreak = 9012;
        private const int SpellId_FullCharge = 182055;
        private const int SpellId_Artillery = 182108;
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/iron-reaver-strategy-guide
		[EncounterHandler((int)MobId_IronReaver, "Iron Reaver")]
		public Func<WoWUnit, Task<bool>> IronReaverEncounter()
		{
            // Don't stand directly under him (trying to avoid getting caught in blitz)
            AddAvoidObject(10, MobId_IronReaver);
            // Don't stand in front of Iron Reaver because of frontal cone attack, Barrage.
            AddAvoidObject(10, o => o.Entry == MobId_IronReaver && o.ToUnit().Combat, o => o.Location.RayCast(o.Rotation, 45));
            
            // Immo and Fuel Streak
            AddAvoidObject(3, AreaTriggerId_Immolation);
            AddAvoidObject(3, AreaTriggerId_FuelStreak);
            
            // bomb spawns explosions
            AddAvoidLocation(
                ctx => true,
                10,
                m => ((WoWMissile) m).ImpactPosition,
				() => WoWMissile.InFlightMissiles.Where(m => m.SpellId == SpellId_Firebomb));
                
            // Stay away from anyone targeted by artillery.
            AddAvoidObject(35, o => o is WoWPlayer && !o.IsMe && (o.ToPlayer().HasAura(SpellId_Artillery) || Me.HasAura(SpellId_Artillery)));
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        #endregion
        
        #region KormrokTrash
        
        private const uint AreaTriggerId_OrbofDestruction = 9208;
        private const uint MobId_FelHellweaver = 94806;
        
        [EncounterHandler((int)MobId_FelHellweaver, "Fel Hellweaver")]
		public Func<WoWUnit, Task<bool>> FelHellweaverEncounter()
		{
            
            AddAvoidObject(10, AreaTriggerId_OrbofDestruction);
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        
        private const uint MobId_KeenEyedGronnstalker = 94894;
        private const uint AreaTriggerId_SlagTrap = 9227;
        
        [EncounterHandler((int)MobId_KeenEyedGronnstalker, "Keen-Eyed Gronnstalker")]
		public Func<WoWUnit, Task<bool>> KeenEyedGronnstalkerEncounter()
		{
            
            AddAvoidObject(5, AreaTriggerId_SlagTrap);
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        
        private const uint AreaTriggerId_ResidualShadows = 9136;
        
        private const uint MobId_Togdrov = 94816;
        
        [EncounterHandler((int)MobId_Togdrov, "Togdrov")]
		public Func<WoWUnit, Task<bool>> TogdrovEncounter()
		{
            
            AddAvoidObject(6, AreaTriggerId_ResidualShadows);
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        private const uint MobId_Sovokk = 94779;
        
        [EncounterHandler((int)MobId_Sovokk, "Sovokk")]
		public Func<WoWUnit, Task<bool>> SovokkEncounter()
		{
            
            AddAvoidObject(6, AreaTriggerId_ResidualShadows);
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        private const uint MobId_Morkronn = 94777;
        
        [EncounterHandler((int)MobId_Morkronn, "Morkronn")]
		public Func<WoWUnit, Task<bool>> MorkronnEncounter()
		{
            
            AddAvoidObject(6, AreaTriggerId_ResidualShadows);
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        
        #endregion
        
        #region Kormrok
        
        private const uint MobId_Kormrok = 90435;
        
        private const uint AreaTriggerId_FoulPool = 9097;
        private const uint AreaTriggerId_FieryPool = 9096;
        private const uint AreaTriggerId_ShadowyPool = 8575;
        
        private const uint AreaTriggerId_PurpleWave = 8504;
        private const int SpellId_ExplosiveBurst = 181306;
        
        // explosive runes just ignore?
        private const uint AreaTriggerId_ExplosiveRune = 8511;
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/kormrok-strategy-guide
		[EncounterHandler((int)MobId_Kormrok, "Kormrok")]
		public Func<WoWUnit, Task<bool>> KormrokEncounter()
		{
            
            AddAvoidObject(30, AreaTriggerId_FoulPool);
            AddAvoidObject(30, AreaTriggerId_FieryPool);
            AddAvoidObject(30, AreaTriggerId_ShadowyPool);
            
            AddAvoidObject(2, AreaTriggerId_PurpleWave);
            
            AddAvoidObject(35, o => o is WoWPlayer && !o.IsMe && (o.ToPlayer().HasAura(SpellId_ExplosiveBurst) || Me.HasAura(SpellId_ExplosiveBurst)));
            
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        #endregion
	}


}