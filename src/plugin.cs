using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using HarmonyLib;

using BepInEx;
using BepInEx.Logging;

using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace org.efool.subnautica.custom_inventory {

[BepInPlugin(FQN, "efool's Custom Inventory", "0.0.1")]
[BepInDependency("com.snmodding.nautilus")]
public class Plugin : BaseUnityPlugin
{
	public const string FQN = "org.efool.subnautica.custom_inventory";
	public static ManualLogSource log { get; } = BepInEx.Logging.Logger.CreateLogSource("efool");

	public static ConfigGlobal config { get; private set;}
	public static OptionsMenu optionsMenu { get; private set; }
	public static ConfigPerSave game { get; } = SaveDataHandler.RegisterSaveDataCache<ConfigPerSave>();

	public static void debug(string txt)
	{
#if DEBUG
		log.LogDebug(txt);
#endif
	}

	private void Awake()
	{
		config = new ConfigGlobal();
		config.Load();

		optionsMenu = new OptionsMenu(config);
		OptionsPanelHandler.RegisterModOptions(optionsMenu);

		SaveUtils.RegisterOnFinishLoadingEvent(() => optionsMenu.inGame = true);
		SaveUtils.RegisterOnQuitEvent(() => optionsMenu.inGame = false);

		ConsoleCommandsHandler.RegisterConsoleCommands(typeof(Commands));

		SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(Patch.evtSceneLoaded);

		new Harmony(FQN).PatchAll();
	}
}

[HarmonyPatch]
class Patch
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.OnSave))]
	public static void uGUI_OptionsPanel_OnSave(uGUI_OptionsPanel __instance, UserStorageUtils.SaveOperation saveOperation)
	{
		Plugin.config.Save();
		Plugin.config.syncScrollPanes();
		Plugin.optionsMenu.applyChanges();
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Inventory), "Awake")]
	public static void Inventory_Awake(Inventory __instance)
	{
		__instance.container.Resize(Plugin.game.inventory_width, Plugin.game.inventory_height);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StorageContainer), "CreateContainer")]
	public static void StorageContainer_CreateContainer(StorageContainer __instance)
	{
		if ( __instance.name.StartsWith("SmallLocker") ) {
			// wall locker
			__instance.width = Plugin.game.smalllocker_width;
			__instance.height = Plugin.game.smalllocker_height;
		}
		else if ( __instance.name.StartsWith("Locker") ) {
			// locker
			__instance.width = Plugin.game.locker_width;
			__instance.height = Plugin.game.locker_height;
		}
		else if ( __instance.prefabRoot.name.StartsWith("SmallStorage") ) {
			// waterproof locker
			__instance.width = Plugin.game.smallstorage_width;
			__instance.height = Plugin.game.smallstorage_height;
		}
		else if ( __instance.GetComponent<SpawnEscapePodSupplies>() ) {
			// lifepod locker
			__instance.width = Plugin.game.lifepodLocker_width;
			__instance.height = Plugin.game.lifepodLocker_height;
		}
		else if ( __instance.name.StartsWith("submarine_locker_01_door") ) {
			// cyclops lockers
			if ( __instance.storageRoot.name.StartsWith("Locker0") ) {
				switch ( __instance.storageRoot.name["Locker0".Length] ) {
				case '1':
					__instance.width = Plugin.game.cyclopsLocker1_width;
					__instance.height = Plugin.game.cyclopsLocker1_height;
					break;
				case '2':
					__instance.width = Plugin.game.cyclopsLocker2_width;
					__instance.height = Plugin.game.cyclopsLocker2_height;
					break;
				case '3':
					__instance.width = Plugin.game.cyclopsLocker3_width;
					__instance.height = Plugin.game.cyclopsLocker3_height;
					break;
				case '4':
					__instance.width = Plugin.game.cyclopsLocker4_width;
					__instance.height = Plugin.game.cyclopsLocker4_height;
					break;
				case '5':
					__instance.width = Plugin.game.cyclopsLocker5_width;
					__instance.height = Plugin.game.cyclopsLocker5_height;
					break;
				default:
					__instance.width = 0;
					break;
				}

				if ( __instance.width == 0 ) {
					__instance.width = Plugin.game.cyclopsLocker_width;
					__instance.height = Plugin.game.cyclopsLocker_height;
				}
			}
			else {
				__instance.width = Plugin.game.cyclopsLocker_width;
				__instance.height = Plugin.game.cyclopsLocker_height;
			}
		}
		else if ( __instance.name.StartsWith("storage") ) {
			// carry-all
			__instance.width = Plugin.game.luggagebag_width;
			__instance.height = Plugin.game.luggagebag_height;
		}
		else if ( __instance.name.StartsWith("Trashcans") ) {
			// trash can
			__instance.width = Plugin.game.trashcans_width;
			__instance.height = Plugin.game.trashcans_height;
		}
		else if ( __instance.name.StartsWith("LabTrashcan") ) {
			// nuclear waste can
			__instance.width = Plugin.game.labtrashcan_width;
			__instance.height = Plugin.game.labtrashcan_height;
		}
		else if ( __instance.name.StartsWith("FiltrationMachine") ) {
			// filtration machine
		}
		else if ( __instance.prefabRoot.name.StartsWith("Exosuit") ) {
			// prawn storage
			__instance.width = Plugin.game.exosuit_width;
			__instance.height = Plugin.game.exosuit_height;
		}
		else if ( __instance.name.StartsWith("MapRoomUpgrades") ) {
			// scanner room upgrades
			__instance.width = Plugin.game.maproomupgrades_width;
			__instance.height = Plugin.game.maproomupgrades_height;
		}
		else if ( __instance.name.StartsWith("PlanterPot3") ) {
			// planter 3
			__instance.width = Plugin.game.planterpot3_width;
			__instance.height = Plugin.game.planterpot3_height;
		}
		else if ( __instance.name.StartsWith("PlanterPot2") ) {
			// planter 2
			__instance.width = Plugin.game.planterpot2_width;
			__instance.height = Plugin.game.planterpot2_height;
		}
		else if ( __instance.name.StartsWith("PlanterPot") ) {
			// planter
			__instance.width = Plugin.game.planterpot_width;
			__instance.height = Plugin.game.planterpot_height;
		}
		else if ( __instance.name.StartsWith("PlanterShelf") ) {
			// shelf planter
			__instance.width = Plugin.game.plantershelf_width;
			__instance.height = Plugin.game.plantershelf_height;
		}
		else if ( __instance.name.StartsWith("FarmingTray") ) {
			// farming tray
			__instance.width = Plugin.game.farmingtray_width;
			__instance.height = Plugin.game.farmingtray_height;
		}
		else if ( __instance.name.StartsWith("PlanterBox") ) {
			// planter box
			__instance.width = Plugin.game.planterbox_width;
			__instance.height = Plugin.game.planterbox_height;
		}
		else if ( __instance.name.StartsWith("planter") ) {
			// alien containment planter
			__instance.width = Plugin.game.planter_width;
			__instance.height = Plugin.game.planter_height;
		}
#if DEBUG
		else {
			Plugin.debug("StorageContainer_CreateContainer Dump Components:");
			foreach (var c in __instance.GetComponents(typeof(Component)))
				Plugin.debug("  Component: " + c.ToString());

			Plugin.debug("CreateContainer Dump: " + Newtonsoft.Json.JsonConvert.SerializeObject(__instance, new Newtonsoft.Json.JsonSerializerSettings
			{
				ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
				Formatting = Newtonsoft.Json.Formatting.Indented,
			}));
		}
#endif
	}

#if DEBUG
	[HarmonyPrefix]
	[HarmonyPatch(typeof(StorageContainer), nameof(StorageContainer.Open), typeof(Transform))]
	public static void StorageContainer_Open(StorageContainer __instance, Transform useTransform)
	{
		Plugin.debug("StorageContainer_Open Dump Components:");
		foreach (var c in __instance.GetComponents(typeof(Component)))
			Plugin.debug("  Component: " + c.ToString());

		Plugin.debug("Open Dump: " + Newtonsoft.Json.JsonConvert.SerializeObject(__instance, new Newtonsoft.Json.JsonSerializerSettings
		{
			ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
			Formatting = Newtonsoft.Json.Formatting.Indented,
		}));
	}
#endif

	[HarmonyPrefix]
	[HarmonyPatch(typeof(BaseBioReactor), "Start")]
	public static void BaseBioReactor_Start(BaseBioReactor __instance)
	{
		(AccessTools.PropertyGetter(typeof(BaseBioReactor), "container").Invoke(__instance, null) as ItemsContainer).Resize(Plugin.game.basebioreactor_width, Plugin.game.basebioreactor_height);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(FiltrationMachine), "Start")]
	public static void FiltrationMachine_Start(FiltrationMachine __instance)
	{
		__instance.maxSalt = Plugin.game.basefiltrationMachine_maxSalt;
		__instance.maxWater = Plugin.game.basefiltrationMachine_maxWater;

		int maxWidth = Plugin.config.storageMaxView_width;
		int total = __instance.maxSalt + __instance.maxWater;
		int sqroot = (int)Math.Ceiling(Math.Sqrt(total));
		if ( sqroot <= maxWidth ) {
			__instance.storageContainer.width = sqroot;
			__instance.storageContainer.height = sqroot;
		}
		else {
			__instance.storageContainer.width = maxWidth;
			__instance.storageContainer.height = (total + maxWidth - 1) / maxWidth;
		}
		__instance.storageContainer.Resize(__instance.storageContainer.width, __instance.storageContainer.height);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SeamothStorageContainer), "Init")]
	public static void SeamothStorageContainer_Init(SeamothStorageContainer __instance)
	{
		Plugin.debug($"Init SeaMoth storage: {__instance.name}");
		if ( __instance.name.StartsWith("SeamothStorageModule") ) {
			__instance.width = Plugin.game.seamoth_width;
			__instance.height = Plugin.game.seamoth_height;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Exosuit), "UpdateStorageSize")]
	public static void Exosuit_UpdateStorageSize(Exosuit __instance)
	{
		__instance.storageContainer.Resize(
			Plugin.game.exosuit_width,
			Plugin.game.exosuit_height
				+ (Plugin.game.exosuitStorageModule_height * __instance.modules.GetCount(TechType.VehicleStorageModule)));
	}

	static class ScrollPane
	{
		public static Vector2 viewportSize(Vector2int contentSize, Vector2int maxViewSize)
		{
			var w = Math.Min(contentSize.x, maxViewSize.x);
			var h = Math.Min(contentSize.y, maxViewSize.y);
			return new Vector2(w * uGUI_ItemsContainer.CellWidth + Plugin.config.viewMargin, h * uGUI_ItemsContainer.CellHeight + Plugin.config.viewMargin);
		}

		public static void DumpRectTransform(string name, RectTransform r)
		{
			Plugin.debug(name + ": "
				+ "  anchorMin=" + r.anchorMin
				+ "  anchorMax=" + r.anchorMax
				+ "  pivot=" + r.pivot
				+ "  anchoredPosition=" + r.anchoredPosition
				+ "  sizeDelta=" + r.sizeDelta
				+ "  localPosition=" + r.localPosition
				+ "  localRotation=" + r.localRotation
				+ "  localScale=" + r.localScale
				);
		}

		public static void DumpRect(string name, Rect r)
		{
			Plugin.debug(name + ": x=" + r.x + " y=" + r.y + " w=" + r.width + " h=" + r.height);
		}

		public static ScrollRect inject(uGUI_ItemsContainer __instance, string name)
		{
			Plugin.debug("Installing ScrollPane");

			GameObject scrollObject = new GameObject() { name = name };
			scrollObject.transform.SetParent(__instance.transform.parent);
			var viewport = scrollObject.AddComponent<RectTransform>();
			viewport.CopyLocals(__instance.rectTransform);
			__instance.transform.SetParent(scrollObject.transform);

			{
				var m = scrollObject.AddComponent<RectMask2D>();
				m.padding = __instance == __instance.inventory
					? new Vector4(Plugin.config.inventoryMaskPadding_left, Plugin.config.inventoryMaskPadding_bottom, Plugin.config.inventoryMaskPadding_right, Plugin.config.inventoryMaskPadding_top)
					: new Vector4(Plugin.config.storageMaskPadding_left, Plugin.config.storageMaskPadding_bottom, Plugin.config.storageMaskPadding_right, Plugin.config.storageMaskPadding_top);
			}

			var scrollRect = scrollObject.AddComponent<ScrollRect>();
			scrollRect.movementType = ScrollRect.MovementType.Clamped;
			scrollRect.horizontal = true;
			scrollRect.viewport = viewport;
			scrollRect.content = __instance.rectTransform;
			scrollRect.scrollSensitivity = uGUI_ItemsContainer.CellHeight;

			scrollRect.verticalScrollbarSpacing = 0;
			scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
			scrollRect.verticalScrollbar = UnityEngine.Object.Instantiate<GameObject>((Player.main.GetPDA().ui.tabEncyclopedia as uGUI_EncyclopediaTab).contentScrollRect.verticalScrollbar.gameObject, scrollRect.viewport).GetComponent<Scrollbar>();
			{
				var r = scrollRect.verticalScrollbar.transform as RectTransform;
				r.anchorMin        = new Vector2(1, 0);
				r.anchorMax        = new Vector2(1, 1);
				r.pivot            = new Vector2(1, 1);
				r.sizeDelta        = new Vector2(Plugin.config.scrollbarSize, 0);
				r.anchoredPosition = new Vector2(0, 0);

				scrollRect.verticalScrollbar.enabled = r.sizeDelta.x != 0;
				scrollRect.verticalScrollbar.targetGraphic.gameObject.SetActive(scrollRect.verticalScrollbar.enabled);
			}

			return scrollRect;
		}

		public static void init(uGUI_ItemsContainer __instance, ScrollRect scrollRect, Vector2int contentSize, Vector2int maxViewSize)
		{
			scrollRect.viewport.sizeDelta = viewportSize(contentSize, maxViewSize);
			__instance.rectTransform.anchorMin        = new Vector2(0.5f, 1);
			__instance.rectTransform.anchorMax        = new Vector2(0.5f, 1);
			__instance.rectTransform.pivot            = new Vector2(0.5f, 1);
			__instance.rectTransform.anchoredPosition = new Vector2(0, 0);

			scrollRect.verticalNormalizedPosition = 1;
			scrollRect.horizontalNormalizedPosition = 0;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(uGUI_InventoryTab), nameof(uGUI_InventoryTab.OnOpenPDA))]
	public static void uGUI_InventoryTab_OnOpenPDA(uGUI_InventoryTab __instance)
	{
		if ( Plugin.config.resetInventoryScroll ) {
			var scrollRect = __instance.inventory.GetComponentInParent<ScrollRect>(true);
			if ( scrollRect ) {
				scrollRect.verticalNormalizedPosition = 1;
				scrollRect.horizontalNormalizedPosition = 0;
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(uGUI_ItemsContainer), nameof(uGUI_ItemsContainer.Init))]
	public static void uGUI_ItemsContainer_Init(uGUI_ItemsContainer __instance, ItemsContainer ___container)
	{
		var isInventory = __instance == __instance.inventory.inventory;
		var isStorage = __instance == __instance.inventory.storage;
		if ( isInventory || isStorage ) {
			var contentSize = new Vector2int(___container.sizeX, ___container.sizeY);
			var maxViewSize = isInventory
				? new Vector2int(Plugin.config.inventoryMaxView_width, Plugin.config.inventoryMaxView_height)
				: new Vector2int(Plugin.config.storageMaxView_width, Plugin.config.storageMaxView_height);

			var scrollRect = __instance.GetComponentInParent<ScrollRect>(true);
			if ( scrollRect is null )
				scrollRect = ScrollPane.inject(__instance, "ScrollPane");
			else
				scrollRect.gameObject.SetActive(true);

			ScrollPane.init(__instance, scrollRect, contentSize, maxViewSize);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(uGUI_ItemsContainer), nameof(uGUI_ItemsContainer.Uninit))]
	public static void uGUI_ItemsContainer_Uninit(uGUI_ItemsContainer __instance, ItemsContainer ___container)
	{
		__instance.GetComponentInParent<ScrollRect>(true)?.gameObject.SetActive(false);
	}

	private static string getId(InventoryItem item)
	{
		return item.item.GetComponent<UniqueIdentifier>().Id;
	}

	private static bool isPinned(InventoryItem item)
	{
		return Plugin.game.inventoryPinnedItems.Contains(getId(item));
	}

	private static void pin(InventoryItem item)
	{
		Plugin.game.inventoryPinnedItems.Add(getId(item));
	}

	private static void unpin(InventoryItem item)
	{
		Plugin.game.inventoryPinnedItems.Remove(getId(item));
	}

	private static GameObject _pinReference;
	public static void evtSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if ( _pinReference == null && Player.main != null ) {
			var tab = Player.main.GetPDA().ui.tabJournal as uGUI_BlueprintsTab;
			var pin = tab.prefabPin.gameObject.FindChild("Pin");
			var container = (Player.main.GetPDA().ui.tabInventory as uGUI_InventoryTab).inventory;
			_pinReference = UnityEngine.Object.Instantiate<GameObject>(pin);
			_pinReference.name = "Pin";
			var x = _pinReference.transform as RectTransform;
			x.anchorMin     = new Vector2(0.1f, 1);
			x.anchorMax     = new Vector2(0.1f, 1);
			x.pivot         = new Vector2(0.1f, 1);
			x.offsetMin     = new Vector2(0, -30);
			x.offsetMax     = new Vector2(35, 0);
			x.localPosition = new Vector3(0, 0, 0);
			_pinReference.SetActive(false);
		}
	}

	private static void applyEffectPin(uGUI_ItemsContainer container, InventoryItem item, bool on = true)
	{
		if ( _pinReference == null )
			return;

		var icon = container.GetIcon(item);
		if ( icon == null )
			return;

		if ( on ) {
			RectTransform rectTransform = UnityEngine.Object.Instantiate<GameObject>(_pinReference, icon.rectTransform).GetComponent<RectTransform>();
			rectTransform.gameObject.name = "Pin";
			rectTransform.gameObject.SetActive(true);
		}
		else {
			var pin = icon.gameObject.FindChild("Pin");
			if ( pin != null )
				GameObject.Destroy(pin);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(uGUI_ItemsContainer), "OnAddItem")]
	public static void uGUI_ItemsContainer_OnAddItem(uGUI_ItemsContainer __instance, InventoryItem item)
	{
		applyEffectPin(__instance, item, isPinned(item));
	}

	private static void handlePinItemAction(InventoryItem item)
	{
		var container = item.container as ItemsContainer;
		if ( item.container is Equipment )
			return;

		var itemPinned = isPinned(item);
		var targetItems = new List<InventoryItem>();
		if ( Plugin.config.keyMoveAllItems.GetKeyHeld() ) {
			foreach ( var e in container )
				if ( isPinned(e) == itemPinned )
					targetItems.Add(e);
		}
		else if ( Plugin.config.keyMoveAllItemType.GetKeyHeld() ) {
			foreach ( var e in container.GetItems(item.item.GetTechType()) )
				if ( isPinned(e) == itemPinned )
					targetItems.Add(e);
		}
		else {
			targetItems.Add(item);
		}

		// subnautica APIs don't allow for multi-processing lists
		if ( itemPinned ) {
			foreach ( var e in targetItems ) {
				unpin(e);
				foreach ( var c in Plugin.FindObjectsOfType<uGUI_ItemsContainer>() )
					applyEffectPin(c, e, false);
			}
		}
		else {
			foreach ( var e in targetItems ) {
				pin(e);
				foreach ( var c in Plugin.FindObjectsOfType<uGUI_ItemsContainer>() )
					applyEffectPin(c, e, true);
			}
		}
	}

	private static void handleItemAction(InventoryItem item, ItemAction itemAction)
	{
		var targetItems = new List<InventoryItem>();
		if ( Plugin.config.keyMoveAllItems.GetKeyHeld() ) {
			foreach ( var e in item.container )
				if ( !isPinned(e) )
					targetItems.Add(e);
		}
		else {
			var container = item.container as ItemsContainer;
			if ( container != null && Plugin.config.keyMoveAllItemType.GetKeyHeld() ) {
				foreach ( var e in container.GetItems(item.item.GetTechType()) )
					if ( !isPinned(e) )
						targetItems.Add(e);
			}
			else {
				targetItems.Add(item);
			}
		}

		foreach ( var e in targetItems )
			Inventory.main.ExecuteItemAction(itemAction, e);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(uGUI_InventoryTab), nameof(uGUI_InventoryTab.OnPointerClick))]
	public static bool uGUI_InventoryTab_OnPointerClick(uGUI_InventoryTab __instance, InventoryItem item, int button)
	{
		if ( ItemDragManager.isDragging )
			return false;

		var itemAction = Inventory.main.GetItemAction(item, button);
		if ( Plugin.config.keyPinItem.GetKeyHeld() ) {
			if ( button == 0 )
				handlePinItemAction(item);
		}
		else if ( itemAction != ItemAction.None && !isPinned(item) ) {
			handleItemAction(item, itemAction);
		}

		__instance.equipment.ExtinguishSlots();

		if ( itemAction != ItemAction.None || GameInput.GetPrimaryDevice() != GameInput.Device.Controller || button != 1 )
			return false;

		Player.main.GetPDA().Close();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(InventoryItem), nameof(InventoryItem.CanDrag))]
	public static bool InventoryItem_CanDrag(ref bool __result, InventoryItem __instance, bool verbose)
	{
		if ( isPinned(__instance) ) {
			__result = false;
			return false;
		}

		return true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Inventory), nameof(Inventory.GetAllItemActions))]
	public static void Inventory_GetAllItemActions(ref ItemAction __result, Inventory __instance, InventoryItem item)
	{
		const ItemAction disallowed = ItemAction.Use
			| ItemAction.Eat
			| ItemAction.Equip
			| ItemAction.Unequip
			| ItemAction.Switch
			| ItemAction.Swap
			| ItemAction.Drop;
		if ( isPinned(item) )
			__result = __result & ~disallowed;
	}
}

} // namespace org.efool.subnautica.custom_inventory