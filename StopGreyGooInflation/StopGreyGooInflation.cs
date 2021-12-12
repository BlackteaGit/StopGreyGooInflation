using HarmonyLib;
using CoOpSpRpG;
using WTFModLoader;
using WTFModLoader.Manager;

using System.Runtime.CompilerServices;


namespace StopGreyGooInflation
{
    public class StopGreyGooInflation : IWTFMod
    {
		public ModLoadPriority Priority => ModLoadPriority.Low;
		public void Initialize()
		{
			Harmony harmony = new Harmony("blacktea.Stop_Grey_Goo_Inflation");
			harmony.PatchAll();
		}

		[HarmonyPatch(typeof(Ship), "aggro")]
		public class Ship_aggro
		{

			[HarmonyPostfix]
			private static void Postfix(Ship __instance)
			{
				if (__instance == PLAYER.currentShip || __instance.faction == 2UL)
				{
					return;
				}
				if (__instance.shipMetric != null && __instance.cosm?.cargoBays != null && __instance.cosm.cargoBays.Count > 0 && __instance.getGooUpdated() == false && (__instance.checkMassFast() / __instance.shipMetric.MaxMass) < 0.8f)
				{
					if (__instance.cosm?.cargoBays != null)
					{
						bool gooupdated = false;
						foreach (var cargobay in __instance.cosm.cargoBays)
						{
							if (cargobay.storage != null && cargobay.storage.countItemByType(InventoryItemType.grey_goo) != 0)
							{
								int amount = cargobay.storage.countItemByType(InventoryItemType.grey_goo);
								if (amount > 20)
								{
									amount = (int)(amount * RANDOM.getRandomNumber(0.001, 0.01));
									cargobay.storage.deleteAllByType(InventoryItemType.grey_goo);
									gooupdated = true;
									for (int i = 0; i < amount; i++)
									{
										cargobay.storage.placeInFirstSlot(new InventoryItem(InventoryItemType.grey_goo));
									}
									if (cargobay.storage.countItemByType(InventoryItemType.grey_goo) == 0)
									{
										var goo = new InventoryItem(InventoryItemType.grey_goo)
										{
											stackSize = (uint)RANDOM.Next(1, 20)
										};
										cargobay.storage.placeInFirstSlot(goo);
									}
								}
								
								__instance.setGooUpdated(true);
							}
						}
						if (gooupdated)
						{
							if (__instance.seenByPlayer)
							{
								SCREEN_MANAGER.widgetChat.AddMessage("Grey goo storage on a nearby ship has become unstabile, a considarable amount of grey goo has been destroyed.", MessageTarget.Ship);
							}

						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(CosmMetaData), "takeOneItem")]
		public class CosmMetaData_takeOneItem
		{

			[HarmonyPostfix]
			private static void Postfix(CosmMetaData __instance, ref InventoryItem __result)
			{
				InventoryItem taken = null;
				if (__result != null && __result.type == InventoryItemType.grey_goo)
				{
					foreach (Storage storage in __instance.storage)
					{
						if (storage != null)
						{
							storage.placeInFirstSlot(__result);
							taken = __result;
							break;
						}
					}
					__result = null;
				}

				foreach (Ship ship in PLAYER.currentSession.allShips.Values)
				{
					if (ship.data == __instance && (ship.getGooUpdated() == true || ship.cosm?.crew == null || (ship.cosm?.crew != null && ship.cosm.crew.IsEmpty)))
					{
						if (__instance.storage != null && taken != null)
						{
							if (ship.getGooUpdated() == false)
							{
								bool gooupdated = false;
								foreach (var cargobaystorage in __instance.storage)
								{
									if (cargobaystorage != null && cargobaystorage.countItemByType(InventoryItemType.grey_goo) != 0)
									{
										int amount = cargobaystorage.countItemByType(InventoryItemType.grey_goo);
										if (amount > 20)
										{
											amount = (int)(amount * RANDOM.getRandomNumber(0.001, 0.01));
											cargobaystorage.deleteAllByType(InventoryItemType.grey_goo);
											gooupdated = true;
											for (int i = 0; i < amount; i++)
											{
												cargobaystorage.placeInFirstSlot(new InventoryItem(InventoryItemType.grey_goo));
											}
											if (cargobaystorage.countItemByType(InventoryItemType.grey_goo) == 0)
											{
												var goo = new InventoryItem(InventoryItemType.grey_goo)
												{
													stackSize = (uint)RANDOM.Next(1, 20)
												};
												cargobaystorage.placeInFirstSlot(goo);
											}
										}
										
										ship.setGooUpdated(true);
									}
								}
								if (gooupdated)
								{
									if (ship.seenByPlayer)
									{
										SCREEN_MANAGER.widgetChat.AddMessage("Grey goo storage on a nearby ship has become unstabile, a considarable amount of grey goo has been destroyed.", MessageTarget.Ship);
									}

								}
							}
							foreach (Storage storage in __instance.storage)
							{
								if (storage != null && storage.countItemByType(InventoryItemType.grey_goo) >= taken.stackSize)
								{
									storage.deleteByType(InventoryItemType.grey_goo, (int)taken.stackSize);
									var item = new InventoryItem(InventoryItemType.grey_goo);
									item.stackSize = taken.stackSize;
									__result = item;
									return;
								}
							}
						}
						__result = null;
						return;
					}
				}
			}
		}

		[HarmonyPatch(typeof(CargoBay), "whenBroken")]
		public class CargoBay_whenBroken
		{

			[HarmonyPrefix]
			private static void Prefix(CargoBay __instance)
			{
				if (__instance.cosm != null && __instance.cosm.ship != null && __instance.storage != null && __instance.cosm.ship.getGooUpdated() == false)
				{
					if (__instance.storage.countItemByType(InventoryItemType.grey_goo) != 0)
					{
						if (__instance.cosm.ship != PLAYER.currentShip)
						{
							int amount = __instance.storage.countItemByType(InventoryItemType.grey_goo);
							if (amount > 20)
							{
								amount = (int)(amount * RANDOM.getRandomNumber(0.001, 0.1));
								__instance.storage.deleteAllByType(InventoryItemType.grey_goo);
								for (int i = 0; i < amount; i++)
								{
									__instance.storage.placeInFirstSlot(new InventoryItem(InventoryItemType.grey_goo));
								}
								if (__instance.cosm.ship.seenByPlayer)
								{
									SCREEN_MANAGER.widgetChat.AddMessage("Grey goo storage on a nearby ship has been damaged, some grey goo has been destroyed.", MessageTarget.Ship);
								}
								if (__instance.storage.countItemByType(InventoryItemType.grey_goo) == 0)
								{
									var goo = new InventoryItem(InventoryItemType.grey_goo)
									{
										stackSize = (uint)RANDOM.Next(1, 20)
									};
									__instance.storage.placeInFirstSlot(goo);
								}
							}

						}
						else
						{
							int amount = __instance.storage.countItemByType(InventoryItemType.grey_goo);
							if (amount > 20)
							{
								amount = (int)(amount * RANDOM.getRandomNumber(0.8, 0.9));
								__instance.storage.deleteAllByType(InventoryItemType.grey_goo);
								for (int i = 0; i < amount; i++)
								{
									__instance.storage.placeInFirstSlot(new InventoryItem(InventoryItemType.grey_goo));
								}
								SCREEN_MANAGER.widgetChat.AddMessage("Grey goo storage on your ship has been damaged, some grey goo has been destroyed.", MessageTarget.Ship);
							}
						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(ITEMBAG), "prepareItems")]
		public class ITEMBAG_prepareItems
		{

			[HarmonyPostfix]
			private static void Postfix()
			{
				ITEMBAG.defaultTip[InventoryItemType.grey_goo].description = ITEMBAG.defaultTip[InventoryItemType.grey_goo].description + " Storage of nanobots reqires highly sophisticated and fragile containers. Be sure to insulate the container from any physical disturbance.";
			}
		}

		[HarmonyPatch(typeof(SpecialtyShop), "activate")]
		public class SpecialtyShop_activate
		{

			[HarmonyPrefix]
			private static bool Prefix()
			{
				if (PLAYER.currentShip != null && PLAYER.currentGame != null && PLAYER.currentShip.id == PLAYER.currentGame.homeBaseId)
				{
					Storage storage = new Storage(24, 6, 4);
					storage.addLoot(new RepairGun());
					storage.addLoot(new Extinguisher());
					storage.addLoot(new Digger());
					storage.addLoot(new Consumable(ConsumableType.patch));
					storage.addLoot(new Consumable(ConsumableType.patch));
					storage.addLoot(new Consumable(ConsumableType.patch));
					var goo = new InventoryItem(InventoryItemType.grey_goo)
					{
						stackSize = 100U
					};
					if (CHARACTER_DATA.credits > 50000)
					{
						goo.refineValue = (uint)(CHARACTER_DATA.credits * 0.2);
					}
					else
					{
						goo.refineValue = goo.refineValue * 40;
					}
					storage.addLoot(goo);
					SCREEN_MANAGER.popupOverlay = new TradeOverlay(storage, null);
					return false;
				}
				return true;
			}
		}
	}
	public static class ShipExtensions
	{
		static readonly ConditionalWeakTable<Ship, GooUpdatedObject> gooupdated = new ConditionalWeakTable<Ship, GooUpdatedObject>();
		public static bool getGooUpdated(this Ship ship) { return gooupdated.GetOrCreateValue(ship).Value; }

		public static void setGooUpdated(this Ship ship, bool gooupdated) { ShipExtensions.gooupdated.GetOrCreateValue(ship).Value = gooupdated; }

		class GooUpdatedObject
		{
			public bool Value = new bool();
		}
	}
}
