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
	public class TheBlackGate : HellfireCitadel
	{
		#region Overrides of Dungeon
	
		public override uint DungeonId
		{
			get { return 986; }
		}

        public override WoWPoint Entrance
        {
            get { return new WoWPoint(4071.450, -2133.950, 51.149); }
        }

        public override WoWPoint ExitLocation
        {
            get { return new WoWPoint(1491.654, 2952.643, 35.23913); }
        }

        public override void RemoveTargetsFilter(List<WoWObject> units)
        {
	        
            var isMelee = Me.IsMelee();

			units.RemoveAll(
				ret =>
				{
                    var unit = ret as WoWUnit;
				    if (unit == null)
				        return false;
                    
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
        
        private const uint MobId_PortaltoDraenor = 95432;

	    public override async Task<bool> HandleMovement(WoWPoint location)
	    {
		    var myLoc = Me.Location;
			var meIsInArchiArea = myLoc.Z > 100.0f;
			var destIsInArchiArea = location.Z < 130.0f;

		    if (!meIsInArchiArea && destIsInArchiArea){
                var archiPort = ObjectManager.GetObjectsOfType<WoWUnit>().FirstOrDefault(u => u.Entry == MobId_PortaltoDraenor);
				if (archiPort != null && await ScriptHelpers.InteractWithObject(archiPort))
					return true;
            }
		    return false;
	    }
        
        private const uint MobId_FelborneOverfiend = 93615;
        private const uint MobId_Dreadstalker = 93616;
        private const uint MobId_DoomfireSpirit = 92208;
        private const uint MobId_InfernalDoombringer = 94412;
        private const uint MobId_ShadowedNetherwalker = 94695;
        private const uint MobId_HellfireDeathcaller = 92740;

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
                        case MobId_ShadowedNetherwalker:
                        case MobId_InfernalDoombringer:
                            priority.Score += 6500;
                            break;
                        case MobId_Dreadstalker:
                            if(israngedDps){
                                priority.Score += 6500;
                            }
                            break;
                        case MobId_HellfireDeathcaller:
						case MobId_FelborneOverfiend:
							priority.Score += 5000;
							break;
                        case MobId_DoomfireSpirit:
							priority.Score += 4000;
							break;
                    }
				}
			}
		}

        #endregion
        
        
		#region Root


		#endregion
        
        #region Archimonde
        
        private const uint MobId_VoidStar = 95775;
        
        [EncounterHandler((int)MobId_VoidStar, "Void Star")]
        public Func<WoWUnit, Task<bool>> VoidStarEncounter()
        {
            
            AddAvoidObject(8, MobId_VoidStar);

            return async boss =>
                {
                    return false;
                };
        }
        
        [EncounterHandler((int)MobId_Dreadstalker, "Dreadstalker")]
        public Func<WoWUnit, Task<bool>> DreadstalkerEncounter()
        {
            
            AddAvoidObject(5, MobId_Dreadstalker);

            return async boss =>
                {
                    return false;
                };
        }
        
        private const uint MobId_Archimonde = 91331;
        
        private const uint AreaTriggerId_Doomfire = 8732;
        private const uint MobId_Pillar = 241689;
        private const uint MissileId_Pillar = 241689;
        private const uint MissileId_RainofChaos = 190053;
        private const uint MissileId_RainofChaos2 = 187107;
        private const uint AreaTriggerId_NetherPortal = 9120;
        private const uint AreaTriggerId_LightoftheNaaru = 8892;
        private const int SpellId_ShackledTorment = 185005;
        private const int SpellId_ShackledTorment2 = 184964;
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/archimonde-strategy-guide
		[EncounterHandler((int)MobId_Archimonde, "Archimonde")]
		public Func<WoWUnit, Task<bool>> ArchimondeEncounter()
		{
            var isMelee = Me.IsMelee();
            var israngedDps = !isMelee && Me.IsDps();
            
            AddAvoidObject(5, AreaTriggerId_Doomfire);
            AddAvoidObject(10, MobId_Pillar);
            AddAvoidObject(5, AreaTriggerId_LightoftheNaaru); //avoid falling doomfire pools??

            AddAvoidObject(3, MobId_Archimonde); //occasional facing issue with melee, this might fix?

            AddAvoidLocation(
				ctx => true,
				10,
				m => ((WoWMissile) m).ImpactPosition,
				() => WoWMissile.InFlightMissiles.Where(m => m.SpellId == MissileId_Pillar));
            
            AddAvoidLocation(
                ctx => true,
                10,
                m => ((WoWMissile) m).ImpactPosition,
                () => WoWMissile.InFlightMissiles.Where(m => m.SpellId == MissileId_RainofChaos));
            
            AddAvoidLocation(
                ctx => true,
                10,
                m => ((WoWMissile) m).ImpactPosition,
                () => WoWMissile.InFlightMissiles.Where(m => m.SpellId == MissileId_RainofChaos2));
            
            AddAvoidObject(25, AreaTriggerId_NetherPortal);
            
            AddAvoidObject(50, o => o.Entry == MobId_Archimonde && Me.HasAura(SpellId_ShackledTorment));
            AddAvoidObject(50, o => o.Entry == MobId_Archimonde && Me.HasAura(SpellId_ShackledTorment2));
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        #endregion
	}


}