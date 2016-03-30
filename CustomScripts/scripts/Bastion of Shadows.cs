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
	public class BastionofShadows : HellfireCitadel
	{
		#region Overrides of Dungeon
	
		public override uint DungeonId
		{
			get { return 984; }
		}

        public override WoWPoint Entrance
        {
            get { return new WoWPoint(4071.450, -2133.950, 51.149); }
        }

        public override WoWPoint ExitLocation
        {
            get { return new WoWPoint(1491.654, 2952.643, 35.23913); }
        }


        private const uint MobId_CarrionSwarm = 95656;

        public override void RemoveTargetsFilter(List<WoWObject> units)
        {
			units.RemoveAll(
				ret =>
				{
                    var unit = ret as WoWUnit;
				    if (unit == null)
				        return false;
                        
                    if (unit.Entry == MobId_CarrionSwarm)
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

                }
            }
	    }
        
        private const uint MobId_SargereiDominator = 92767;
        private const uint MobId_SargereiShadowcaller = 91941;
        
        private const uint MobId_AncientEnforcer = 90270;
        private const uint MobId_AncientHarbinger = 90271;
        private const uint MobId_AncientSovereign = 90272;
        
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
                        case MobId_AncientHarbinger:
                        case MobId_SargereiDominator:
                        case MobId_AncientEnforcer:
                        case MobId_AncientSovereign:
                            priority.Score += 7500;
                            break;
                        case MobId_SargereiShadowcaller:
                            if(israngedDps){
                                priority.Score += 5500;
                            }
                            break;
                    }
				}
			}
		}
        

        #endregion
        
        
		#region Root

		#endregion
        
        #region ShadowLordIskar
        
        private const uint MobId_ShadowLordIskar = 90316;
        
        private const uint AreaTriggerId_FelIncineration = 8699;
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/hellfire-high-council-strategy-guide
		[EncounterHandler((int)MobId_ShadowLordIskar, "Shadow-Lord Iskar")]
		public Func<WoWUnit, Task<bool>> ShadowLordIskarEncounter()
		{
            
            AddAvoidObject(5, o => o.Entry == AreaTriggerId_FelIncineration);
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        #endregion
        
        #region SoulboundConstruct
        
        private const uint MobId_SoulboundConstruct = 90296;
        
        private const uint AreaTriggerId_VolatileFelOrb = 8500;
        private const uint AreaTriggerId_FelblazeCharge = 8674;
        private const uint AreaTriggerId_FelPrison = 8514; //may have another id
        private const int SpellId_GiftoftheManari = 184125;
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/hellfire-high-council-strategy-guide
		[EncounterHandler((int)MobId_SoulboundConstruct, "Soulbound Construct")]
		public Func<WoWUnit, Task<bool>> SoulboundConstructEncounter()
		{
            
            AddAvoidObject(15, o => o.Entry == AreaTriggerId_VolatileFelOrb);
            AddAvoidObject(10, o => o.Entry == AreaTriggerId_FelblazeCharge);
            AddAvoidObject(15, o => o.Entry == AreaTriggerId_FelPrison);
            
            AddAvoidObject(10, o => o is WoWPlayer && !o.IsMe && (o.ToPlayer().HasAura(SpellId_GiftoftheManari) || Me.HasAura(SpellId_GiftoftheManari)));
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        #endregion
        
        #region SoulofSocrethar
        
        private const uint MobId_SoulofSocrethar = 92330;
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/kilrogg-deadeye-strategy-guide
		[EncounterHandler((int)MobId_SoulofSocrethar, "Soul of Socrethar")]
		public Func<WoWUnit, Task<bool>> SoulofSocretharEncounter()
		{
            
            AddAvoidObject(15, o => o.Entry == AreaTriggerId_VolatileFelOrb);
            AddAvoidObject(10, o => o.Entry == AreaTriggerId_FelblazeCharge);
            AddAvoidObject(15, o => o.Entry == AreaTriggerId_FelPrison);
            
            AddAvoidObject(10, o => o is WoWPlayer && !o.IsMe && (o.ToPlayer().HasAura(SpellId_GiftoftheManari) || Me.HasAura(SpellId_GiftoftheManari)));
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        #endregion
        
        #region TyrantTrash
        
        private const uint MobId_EredarFaithbreaker = 93156;
        
        private const uint AreaTriggerId_HellfireBlast = 8933;
        
        [EncounterHandler((int)MobId_EredarFaithbreaker, "Eredar Faithbreaker")]
		public Func<WoWUnit, Task<bool>> EredarFaithbreakerEncounter()
		{
            AddAvoidObject(5, o => o.Entry == AreaTriggerId_HellfireBlast);
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        #endregion
        
        #region TyrantVelhari
        
        private const uint MobId_TyrantVelhari = 90269;
        
        private const uint SpellId_SearingBlaze = 183089; //missile
        private const uint AreaTriggerId_EnforcersOnslaught = 8502; //5 yards
        private const uint AreaTriggerId_DespoiledGround = 8533; //10 yards?
        private const int SpellId_FontofCorruption = 180526;
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/gorefiend-strategy-guide
		[EncounterHandler((int)MobId_TyrantVelhari, "Tyrant Velhari")]
		public Func<WoWUnit, Task<bool>> TyrantVelhariEncounter()
		{
            
            AddAvoidLocation(
				ctx => true,
				3,
				m => ((WoWMissile) m).ImpactPosition,
				() => WoWMissile.InFlightMissiles.Where(m => m.SpellId == SpellId_SearingBlaze));
            
            AddAvoidObject(5, o => o.Entry == AreaTriggerId_EnforcersOnslaught);
            AddAvoidObject(8, o => o.Entry == AreaTriggerId_DespoiledGround);
            
            AddAvoidObject(5, o => o is WoWPlayer && !o.IsMe && (o.ToPlayer().HasAura(SpellId_FontofCorruption) || Me.HasAura(SpellId_FontofCorruption)));
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        #endregion
        
	}
}