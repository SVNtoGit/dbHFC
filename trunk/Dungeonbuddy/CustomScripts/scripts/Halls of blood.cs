using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
	public class HallsofBlood : HellfireCitadel
	{
		#region Overrides of Dungeon
	
		public override uint DungeonId
		{
			get { return 983; }
		}

        public override WoWPoint Entrance
        {
            get { return new WoWPoint(4071.450, -2133.950, 51.149); }
        }

        public override WoWPoint ExitLocation
        {
            get { return new WoWPoint(1491.654, 2952.643, 35.23913); }
        }

        private const uint MobId_Gragga = 94995;
        
        public override void RemoveTargetsFilter(List<WoWObject> units)
        {
			units.RemoveAll(
				ret =>
				{
                    var unit = ret as WoWUnit;
				    if (unit == null)
				        return false;
                        
                        switch (unit.Entry)
                        {
                            case MobId_Gragga:
                                return true;
                            break;
                        }
                    
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
                        case MobId_SalivatingBloodthirster:
                        case MobId_BloodSplatter:
                        case MobId_BloodGlobule:
                            if (israngedDps){
                                priority.Score += 7500;
                            }
                            break;
                        case MobId_GoreboundSpirit: // shared id with constructs :(
                            if (israngedDps){
                                priority.Score += 6500;
                            }
                            break;
                        case MobId_GurtoggBloodboil:
                        case MobId_GoreboundBrute:
                            priority.Score += 6500;
							break;
                        case MobId_HulkingTerror:
                        case MobId_BlademasterJubeithos:
                            priority.Score += 5000;
							break;
                    }
				}
			}
		}
        

        #endregion
        
        
		#region Root

		#endregion
        
        #region GurtoggBloodboil
        
        private const uint MobId_GurtoggBloodboil = 92146;
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/hellfire-high-council-strategy-guide
		[EncounterHandler((int)MobId_GurtoggBloodboil, "Gurtogg Bloodboil")]
		public Func<WoWUnit, Task<bool>> GurtoggBloodboilEncounter()
		{
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        #endregion
        
        #region BlademasterJubeithos
        
        private const uint MobId_BlademasterJubeithos = 92142;
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/hellfire-high-council-strategy-guide
		[EncounterHandler((int)MobId_BlademasterJubeithos, "Blademaster Jubei'thos")]
		public Func<WoWUnit, Task<bool>> BlademasterJubeithosEncounter()
		{
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        #endregion
        
        #region DiaDarkwhisper
        
        private const uint MobId_DiaDarkwhisper = 92144;
        private const uint AreaTriggerId_Reap = 9213;
        
        //looks too weird to avoid mark of the necromancer debuffs
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/hellfire-high-council-strategy-guide
		[EncounterHandler((int)MobId_DiaDarkwhisper, "Dia Darkwhisper")]
		public Func<WoWUnit, Task<bool>> DiaDarkwhisperEncounter()
		{
            AddAvoidObject(9, o => o.Entry == AreaTriggerId_Reap);
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        #endregion
        
        #region KilroggDeadeye
        
        private const uint MobId_KilroggDeadeye = 90378;
        
        private const uint MobId_SalivatingBloodthirster = 90521;
        private const uint MobId_BloodSplatter = 95227;
        private const uint MobId_BloodGlobule = 90477;
        private const uint SpellId_DeathThroes = 182381;
        private const uint AreaTriggerId_BloodSplatter = 9264;
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/kilrogg-deadeye-strategy-guide
		[EncounterHandler((int)MobId_KilroggDeadeye, "Kilrogg Deadeye")]
		public Func<WoWUnit, Task<bool>> KilroggDeadeyeEncounter()
		{
            
            AddAvoidLocation(
				ctx => true,
				5,
				m => ((WoWMissile) m).ImpactPosition,
				() => WoWMissile.InFlightMissiles.Where(m => m.SpellId == SpellId_DeathThroes));
                
                AddAvoidObject(5, o => o.Entry == AreaTriggerId_BloodSplatter);
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        #endregion
        
        #region Gorefiend
        
        private const uint MobId_Gorefiend = 90199;
        
        private const uint MobId_GoreboundSpirit = 91554;
        private const uint MobId_GoreboundBrute = 92961;
        private const uint MobId_HulkingTerror = 93369;
        
        private const uint SpellId_CrushingDarkness = 181534;
        private const int SpellId_TouchofDoom = 179978;
        
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/gorefiend-strategy-guide
		[EncounterHandler((int)MobId_Gorefiend, "Gorefiend")]
		public Func<WoWUnit, Task<bool>> GorefiendEncounter()
		{
            AddAvoidLocation(
				ctx => true,
				5,
				m => ((WoWMissile) m).ImpactPosition,
				() => WoWMissile.InFlightMissiles.Where(m => m.SpellId == SpellId_CrushingDarkness));
            
            AddAvoidObject(9, o =>
			{
				var player = o as WoWPlayer;
				return player != null && !player.IsMe && (player.HasAura(SpellId_TouchofDoom) || Me.HasAura(SpellId_TouchofDoom));
			});
                
                
            return async boss =>
						 {
							 return false;
						 };
        }
        
        #endregion
        
	}
}