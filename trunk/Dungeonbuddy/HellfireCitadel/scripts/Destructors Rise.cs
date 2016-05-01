using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
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
	public class DestructorsRise : HellfireCitadel
	{
		#region Overrides of Dungeon
	
		public override uint DungeonId
		{
			get { return 985; }
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
        
        private readonly Vector2[] _hellfireCitadelMannoArea =
	    {
			new Vector2(-2927.520f, -327.4402f),
            new Vector2(-2920.075f, -294.5142f),
            new Vector2(-2929.392f, -254.4315f),
            new Vector2(-2954.585f, -226.5452f),
            new Vector2(-2965.699f, -234.5559f),
            new Vector2(-3026.277f, -216.6467f),
            new Vector2(-3055.833f, -234.8321f),
            new Vector2(-3074.591f, -269.9282f),
            new Vector2(-3071.210f, -325.1456f),
            new Vector2(-3056.161f, -348.6308f),
            new Vector2(-3030.329f, -365.5455f),
            new Vector2(-3000.718f, -371.7056f),
            new Vector2(-2980.411f, -369.2141f),
            new Vector2(-2920.504f, -307.5354f),
		};
        
        private const uint MobId_mannoPort = 95987;

	    public override async Task<bool> HandleMovement(WoWPoint location)
	    {
		    var myLoc = Me.Location;
			var meIsInMannoArea = WoWMathHelper.IsPointInPoly(myLoc, _hellfireCitadelMannoArea) && myLoc.Z > 600.0f;
			var destIsInMannoArea = WoWMathHelper.IsPointInPoly(location, _hellfireCitadelMannoArea) && location.Z > 600.0f;

		    if (!meIsInMannoArea && destIsInMannoArea){
                var mannoPort = ObjectManager.GetObjectsOfType<WoWUnit>().FirstOrDefault(u => u.Entry == MobId_mannoPort);
				if (mannoPort != null && await ScriptHelpers.InteractWithObject(mannoPort))
					return true;
            }
		    
            return await FelLordZakuunDoorHandler(location) || await XhulhoracDoorHandler(location) || await SummonerDoorHandler(location);
	    }
        
        private const uint MobId_VanguardAkkelion = 94185;
        private const uint MobId_Omnus = 94239;
        private const uint MobId_UnstableVoidfiend = 94397;
        private const uint MobId_WildPyromaniac = 94231;
        
        private const uint MobId_DoomLordKazeth = 91241;
        private const uint MobId_FelImp = 91259;
        private const uint MobId_DreadInfernal = 91270;
        private const uint MobId_FelIronSummoner = 91305;
        
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
                        case MobId_UnstableVoidfiend:
                            if(israngedDps){
                                priority.Score += 7500;
                            }
							break;
                        case MobId_VanguardAkkelion:
                        case MobId_Omnus:
                        case MobId_DoomLordKazeth:
                            priority.Score += 6500;
                            break;
                        case MobId_WildPyromaniac:
                            priority.Score += 5500;
                            break;
                        case MobId_FelImp:
                        case MobId_DreadInfernal:
                            if(israngedDps){
                                priority.Score += 6500;
                            }
							break;
                        case MobId_FelIronSummoner:
                            priority.Score += 5000;
                            break;
                    }
				}
			}
		}
        

        #endregion
        
        
		#region Root

        private const uint AreaTriggerId_BloodofMannoroth = 8672;
        private const uint SpellId_FelStreak = 181190;
        private const uint AreaTriggerId_FelLordExit = 241733;



		[EncounterHandler(0, "Root Handler")]
		public Func<WoWUnit, Task<bool>> RootHandler()
		{
			AddAvoidObject(33, AreaTriggerId_BloodofMannoroth);
            AddAvoidObject(9, MobId_DreadInfernal);
            AddAvoidObject(18, AreaTriggerId_FelLordExit);
            
            AddAvoidLocation(
				ctx => true,
				10,
				m => ((WoWMissile) m).ImpactPosition,
				() => WoWMissile.InFlightMissiles.Where(m => m.SpellId == SpellId_FelStreak));
            
			return async boss => false;
		}
        
		[EncounterHandler((int)MobId_DreadInfernal, "Dread Infernal")]
		public Func<WoWUnit, Task<bool>> DreadInfernalEncounter()
		{
            
            AddAvoidObject(9, MobId_DreadInfernal);

            return async boss =>
						 {
							 return false;
						 };
        }
        
        [EncounterHandler((int)MobId_UnstableVoidfiend, "Unstable Voidfiend")]
		public Func<WoWUnit, Task<bool>> UnstableVoidfiendEncounter()
		{
            
            AddAvoidObject(5, MobId_UnstableVoidfiend);

            return async boss =>
						 {
							 return false;
						 };
        }
        
		#endregion
        
        #region FelLordZakuun
        
        private const uint MobId_FelLordZakuun = 89890;
        
        private const uint AreaTriggerId_WakeofDestruction = 8983;
        private const uint AreaTriggerId_WakeofDestruction2 = 8624;
        private const uint AreaTriggerId_RingofDestruction = 8941;
        private const uint AreaTriggerId_FelCrystal = 8433;
        private const uint AreaTriggerId_RumblingFissure = 8418;
        
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/hellfire-high-council-strategy-guide
		[EncounterHandler((int)MobId_FelLordZakuun, "Fel Lord Zakuun")]
		public Func<WoWUnit, Task<bool>> FelLordZakuunEncounter()
		{
            
            AddAvoidObject(4, AreaTriggerId_WakeofDestruction);
            AddAvoidObject(4, AreaTriggerId_WakeofDestruction2);
            AddAvoidObject(8, AreaTriggerId_RumblingFissure);
            AddAvoidObject(10, AreaTriggerId_FelCrystal);
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        #endregion
        
        #region Xhulhorac
        
        private const uint MobId_Xhulhorac = 93068;
        
        private const uint AreaTriggerId_ChaoticFelblaze = 9015;
        private const uint AreaTriggerId_FelOrb = 9221;
        private const uint AreaTriggerId_BlackHole3 = 9093;
        private const uint AreaTriggerId_BlackHole2 = 9092;
        private const uint AreaTriggerId_BlackHole = 9091;
        private const uint AreaTriggerId_CreepingVoid = 9021;
        
        private const int SpellId_FelSurge = 186407;
        private const int SpellId_VoidSurge = 186333;
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/hellfire-high-council-strategy-guide
		[EncounterHandler((int)MobId_Xhulhorac, "Xhul'horac")]
		public Func<WoWUnit, Task<bool>> XhulhoracEncounter()
		{
            AddAvoidObject(4, o => o.Entry == AreaTriggerId_ChaoticFelblaze, ignoreIfBlocking: true);
            AddAvoidObject(4, AreaTriggerId_FelOrb);
            AddAvoidObject(10, AreaTriggerId_BlackHole3);
            AddAvoidObject(10, AreaTriggerId_BlackHole2);
            AddAvoidObject(10, AreaTriggerId_BlackHole);
            AddAvoidObject(5, o => o.Entry == AreaTriggerId_CreepingVoid, ignoreIfBlocking: true);
            
            AddAvoidObject(5, o => o is WoWPlayer && !o.IsMe && (o.ToPlayer().HasAura(SpellId_FelSurge) || Me.HasAura(SpellId_FelSurge)), ignoreIfBlocking: true);
            AddAvoidObject(5, o => o is WoWPlayer && !o.IsMe && (o.ToPlayer().HasAura(SpellId_VoidSurge) || Me.HasAura(SpellId_VoidSurge)), ignoreIfBlocking: true);
            
            return async boss =>
						 {
							 return false;
						 };
        }
        
        #endregion
        
        #region Mannoroth
        
        private const uint MobId_Mannoroth = 91349;
        
        private const uint MissileId_EmpoweredFelHellstorm = 189279;
        private const uint MissileId_FellHellstorm = 181961;
        private const uint MissileId_FellHellstorm2 = 181567;
        
        private const int SpellId_Shadowforce = 181841;
        private const int SpellId_EmpoweredShadowforce = 182088;
        
        // http://www.wowhead.com/guides/raiding/hellfire-citadel/kilrogg-deadeye-strategy-guide
		[EncounterHandler((int)MobId_Mannoroth, "Mannoroth")]
		public Func<WoWUnit, Task<bool>> MannorothEncounter()
		{
            //try not to stand in front of manno
            AddAvoidObject(23, o => o.Entry == MobId_Mannoroth && o.ToUnit().Combat, o => o.Location.RayCast(o.Rotation, 20));
            
            AddAvoidLocation(
				ctx => true,
				5,
				m => ((WoWMissile) m).ImpactPosition,
				() => WoWMissile.InFlightMissiles.Where(m => m.SpellId == MissileId_EmpoweredFelHellstorm));
            
            AddAvoidLocation(
				ctx => true,
				5,
				m => ((WoWMissile) m).ImpactPosition,
				() => WoWMissile.InFlightMissiles.Where(m => m.SpellId == MissileId_FellHellstorm));
                
            AddAvoidLocation(
				ctx => true,
				5,
				m => ((WoWMissile) m).ImpactPosition,
				() => WoWMissile.InFlightMissiles.Where(m => m.SpellId == MissileId_FellHellstorm2));
                
            
            
            return async boss =>
						 {
							 if (!boss.Combat)
					            return false;
                             
                             return await ScriptHelpers.StayAtLocationWhile(() => Me.HasAura(SpellId_Shadowforce) || Me.HasAura(SpellId_EmpoweredShadowforce), boss.Location.RayCast(boss.Rotation, -5), "Shadowforce");
						 };
        }
        
        #endregion
        
        #region FelLordZakuun Gate

        private const uint ObjectId_HellfireRaidGate = 242510;

		private readonly WoWPoint _hellfireDoorRightEdge = new WoWPoint(4209.189, 2554.937, 179.9163);
		private readonly WoWPoint _hellfireDoorLeftEdge = new WoWPoint(4207.504, 2485.602, 179.9164);

		private async Task<bool> FelLordZakuunDoorHandler(WoWPoint location)
		{
			var myLoc = Me.Location;
			if (myLoc.DistanceSqr(_hellfireDoorLeftEdge) > 25*15)
				return false;

			var door = ObjectManager.GetObjectsOfType<WoWGameObject>().FirstOrDefault(g => g.Entry == ObjectId_HellfireRaidGate);
			if (door == null || ((WoWDoor)door.SubObj).IsOpen)
				return false;

			var movePath = ((MeshNavigator) Navigator.NavigationProvider).CurrentMovePath;
			if (movePath == null || movePath.Index >= movePath.Path.Points.Length || movePath.Index < 0)
				return false;

			var waypoint = (WoWPoint)movePath.Path.Points[movePath.Index];
			var isPlayerLeftOfDoor = myLoc.IsPointLeftOfLine(_hellfireDoorLeftEdge, _hellfireDoorRightEdge);
			var isWaypointLeftOfDoor = waypoint.IsPointLeftOfLine(_hellfireDoorLeftEdge, _hellfireDoorRightEdge);

			// player and waypoint are on same side then return.
            if (isPlayerLeftOfDoor == isWaypointLeftOfDoor)
				return false;

			TreeRoot.StatusText = "Waiting on Hellfire gate to open";
			await CommonCoroutines.StopMoving();
			return true;
		}

		#endregion FelLordZakuun Gate

        
        #region Xhulhorac Gate

        private const uint ObjectId_Doodad_6FX_Firewall_DoorFel001 = 243540;

		private readonly WoWPoint _xhulfireDoorRightEdge = new WoWPoint(4132.638, 2201.691, 184.3424);
		private readonly WoWPoint _xhulfireDoorLeftEdge = new WoWPoint(4148.038, 2201.674, 184.3429);

		private async Task<bool> XhulhoracDoorHandler(WoWPoint location)
		{
			var myLoc = Me.Location;
			if (myLoc.DistanceSqr(_xhulfireDoorLeftEdge) > 25*15)
				return false;

			var door = ObjectManager.GetObjectsOfType<WoWGameObject>().FirstOrDefault(g => g.Entry == ObjectId_Doodad_6FX_Firewall_DoorFel001);
			if (door == null || ((WoWDoor)door.SubObj).IsOpen)
				return false;

			var movePath = ((MeshNavigator) Navigator.NavigationProvider).CurrentMovePath;
			if (movePath == null || movePath.Index >= movePath.Path.Points.Length || movePath.Index < 0)
				return false;

			var waypoint = (WoWPoint)movePath.Path.Points[movePath.Index];
			var isPlayerLeftOfDoor = myLoc.IsPointLeftOfLine(_xhulfireDoorLeftEdge, _xhulfireDoorRightEdge);
			var isWaypointLeftOfDoor = waypoint.IsPointLeftOfLine(_xhulfireDoorLeftEdge, _xhulfireDoorRightEdge);

			// player and waypoint are on same side then return.
            if (isPlayerLeftOfDoor == isWaypointLeftOfDoor)
				return false;

			TreeRoot.StatusText = "Waiting on Xhulhorac fire door to open";
			await CommonCoroutines.StopMoving();
			return true;
		}

		#endregion Xhulhorac Gate
        
        #region Summoner Entrance

        private const uint ObjectId_SummonerEntrance = 242407;

		private readonly WoWPoint _summonerDoorRightEdge = new WoWPoint(4154.826, 2281.968, 206.9238);
		private readonly WoWPoint _summonerDoorLeftEdge = new WoWPoint(4125.467, 2282.768, 206.9238);

		private async Task<bool> SummonerDoorHandler(WoWPoint location)
		{
			var myLoc = Me.Location;
			if (myLoc.DistanceSqr(_summonerDoorLeftEdge) > 25*15)
				return false;

			var door = ObjectManager.GetObjectsOfType<WoWGameObject>().FirstOrDefault(g => g.Entry == ObjectId_SummonerEntrance);
			if (door == null || ((WoWDoor)door.SubObj).IsOpen)
				return false;

			var movePath = ((MeshNavigator) Navigator.NavigationProvider).CurrentMovePath;
			if (movePath == null || movePath.Index >= movePath.Path.Points.Length || movePath.Index < 0)
				return false;

			var waypoint = (WoWPoint)movePath.Path.Points[movePath.Index];
			var isPlayerLeftOfDoor = myLoc.IsPointLeftOfLine(_summonerDoorLeftEdge, _summonerDoorRightEdge);
			var isWaypointLeftOfDoor = waypoint.IsPointLeftOfLine(_summonerDoorLeftEdge, _summonerDoorRightEdge);

			// player and waypoint are on same side then return.
            if (isPlayerLeftOfDoor == isWaypointLeftOfDoor)
				return false;

			TreeRoot.StatusText = "Waiting on Summoner Entrance to open";
			await CommonCoroutines.StopMoving();
			return true;
		}

		#endregion Summoner Entrance
        
	}
}