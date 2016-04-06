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
        
        
        private readonly Vector2[] _hellfireCitadelIskarArea =
	    {
			new Vector2(4094.083f, 2596.255f),
            new Vector2(4086.359f, 2496.549f),
            new Vector2(4044.514f, 2474.065f),
            new Vector2(4037.263f, 2474.67f),
            new Vector2(3993.114f, 2498.634f),
            new Vector2(3964.972f, 2535.576f),
            new Vector2(3964.707f, 2541.975f),
            new Vector2(3988.533f, 2575.518f),
            new Vector2(4049.016f, 2598.57f),
		};
        
        
        /*
        
        // does not work, can't get path through door :(
        // left here for improvement
        
        private WoWPoint IskarExit = new WoWPoint(4035.819f, 2445.393f, 206.2967f);

	    public override async Task<bool> HandleMovement(WoWPoint location)
	    {
		    var myLoc = Me.Location;
			var meIsInIskarArea = WoWMathHelper.IsPointInPoly(myLoc, _hellfireCitadelIskarArea);
			var destIsInIskarArea = WoWMathHelper.IsPointInPoly(location, _hellfireCitadelIskarArea);

		    if (meIsInIskarArea && !destIsInIskarArea){
                //Try to move via entrance door, otherwise we run into crates/walls
                return await ScriptHelpers.StayAtLocationWhile(() => meIsInIskarArea && !destIsInIskarArea, IskarExit, "Leaving Iskars Room");
                
            }
		    return false;
	    }
        */
        

        #endregion
        
        
		#region Root

		#endregion
        
        #region ShadowLordIskar
        
        private const uint MobId_ShadowLordIskar = 90316;
        
        private const uint AreaTriggerId_FelIncineration = 8699;
        private const int SpellId_PhantasmalWinds = 181957;
        private WoWPoint AwayFromLedge = new WoWPoint(4038.455f, 2502.004f, 210.8059f); //need to get exact coord middle of room
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/hellfire-high-council-strategy-guide
		[EncounterHandler((int)MobId_ShadowLordIskar, "Shadow-Lord Iskar")]
		public Func<WoWUnit, Task<bool>> ShadowLordIskarEncounter()
		{
            
            AddAvoidObject(5, o => o.Entry == AreaTriggerId_FelIncineration, ignoreIfBlocking: true);
            
            return async boss =>
						 {
							 //if (!boss.Combat)
					            return false;
                             
                             //return await ScriptHelpers.StayAtLocationWhile(() => Me.HasAura(SpellId_PhantasmalWinds), AwayFromLedge, "Phantasmal Winds");
						 };
        }
        
        #endregion
        
        #region SoulboundConstruct
        
        private const uint MobId_SoulboundConstruct = 90296;
        
        private const uint AreaTriggerId_VolatileFelOrb = 8500;
        private const uint AreaTriggerId_FelblazeCharge = 8674;
        private const uint AreaTriggerId_FelPrison = 8514; //may have another id
        private const int SpellId_GiftoftheManari = 184125;
        
        private const int SpellId_FelblazeCharge = 184247;
        
        private WoWUnit _construct;
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/hellfire-high-council-strategy-guide
		[EncounterHandler((int)MobId_SoulboundConstruct, "Soulbound Construct")]
		public Func<WoWUnit, Task<bool>> SoulboundConstructEncounter()
		{
            
            AddAvoidObject(15, o => o.Entry == AreaTriggerId_VolatileFelOrb);
            AddAvoidObject(10, o => o.Entry == AreaTriggerId_FelblazeCharge);
            AddAvoidObject(15, o => o.Entry == AreaTriggerId_FelPrison);
            
            // Felblaze Charge line
			AddAvoidLine(ctx => ScriptHelpers.IsViable(_construct) && _construct.CastingSpellId == SpellId_FelblazeCharge,
				() => 5,
				() => _construct.Location,
				() => _construct.CurrentTarget.Location);
            
            AddAvoidObject(10, o => o is WoWPlayer && !o.IsMe && (o.ToPlayer().HasAura(SpellId_GiftoftheManari) || Me.HasAura(SpellId_GiftoftheManari)));
            
            return async boss =>
						 {
                             _construct = boss;
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
        
        private const uint MissileId_SearingBlaze = 183089;
        private const uint AreaTriggerId_EnforcersOnslaught = 8502;
        private const uint AreaTriggerId_DespoiledGround = 8533;
        private const int SpellId_FontofCorruption = 180526;
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/gorefiend-strategy-guide
		[EncounterHandler((int)MobId_TyrantVelhari, "Tyrant Velhari")]
		public Func<WoWUnit, Task<bool>> TyrantVelhariEncounter()
		{
            
            AddAvoidLocation(
				ctx => true,
				3,
				m => ((WoWMissile) m).ImpactPosition,
				() => WoWMissile.InFlightMissiles.Where(m => m.SpellId == MissileId_SearingBlaze));
            
            AddAvoidObject(5, o => o.Entry == AreaTriggerId_EnforcersOnslaught);
            AddAvoidObject(8, o => o.Entry == AreaTriggerId_DespoiledGround, ignoreIfBlocking: true);
            
            if(!Me.IsMelee()){
                AddAvoidObject(5, o => o is WoWPlayer && !o.IsMe && (o.ToPlayer().HasAura(SpellId_FontofCorruption) || Me.HasAura(SpellId_FontofCorruption)));
            }
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        #endregion
        
	}
}