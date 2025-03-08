using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using Newtonsoft.Json;

using Nautilus.Json;
using Nautilus.Json.Attributes;
using Nautilus.Options.Attributes;
using Nautilus.Options;
using Nautilus.Commands;

namespace org.efool.subnautica.custom_inventory {

public class OptionsMenu : ModOptions
{
	private static readonly ConfigPerSave _default = new ConfigPerSave();
	private ConfigPerSave _custom = new ConfigPerSave();
	private ConfigPerSave _current;

	public bool inGame { get; set; } = false;

	public OptionsMenu(object inst)
		: base(Info.title)
	{
		foreach ( var field in inst.GetType().GetFields() ) {
			switch ( field.GetCustomAttribute<ModOptionAttribute>() ) {
			case SliderAttribute attr:
				var slider = ModSliderOption.Create(attr.Id, attr.Label, attr.Min, attr.Max, readSliderField(inst, field), defaultValue: attr.DefaultValue, valueFormat: attr.Format, step: attr.Step, tooltip: attr.Tooltip);
				slider.OnChanged += (_, e) => writeSliderField(inst, field, e.Value);
				AddItem(slider);
				break;
			case ToggleAttribute attr:
				var toggle = ModToggleOption.Create(attr.Id, attr.Label, (bool)field.GetValue(inst), tooltip: attr.Tooltip);
				toggle.OnChanged += (_, e) => field.SetValue(inst, e.Value);
				AddItem(toggle);
				break;
			case KeybindAttribute attr:
				var keybind = ModKeybindOption.Create(attr.Id, attr.Label, GameInput.Device.Keyboard, (KeyCode)field.GetValue(inst), tooltip: attr.Tooltip);
				keybind.OnChanged += (_, e) => field.SetValue(inst, e.Value);
				AddItem(keybind);
				break;
			}
		}
	}

	public static SliderLabelMode typeToLabelMode(Type type)
	{
		if ( type == typeof(int) )
			return SliderLabelMode.Int;
		if ( type == typeof(float) )
			return SliderLabelMode.Float;

		return SliderLabelMode.Default;
	}

	public static float readSliderField(object obj, FieldInfo field)
	{
		if ( field.FieldType == typeof(float) )
			return (float)field.GetValue(obj);
		if ( field.FieldType == typeof(int) )
			return (int)field.GetValue(obj);

		return 0;
	}

	public static void writeSliderField(object obj, FieldInfo field, float value)
	{
		if ( field.FieldType == typeof(float) )
			field.SetValue(obj, value);
		else if ( field.FieldType == typeof(int) )
			field.SetValue(obj, (int)value);
	}

	public void loadCurrent()
	{
		if ( _current == null ) {
			_current = new ConfigPerSave();
			_current.copySettings(Plugin.game);
		}
	}

	public override void BuildModOptions(uGUI_TabbedControlsPanel panel, int modsTabIndex, IReadOnlyCollection<OptionItem> options)
	{
		base.BuildModOptions(panel, modsTabIndex, options);
		if ( !inGame )
			return;

		panel.AddHeading(modsTabIndex, Info.title + " Per Save Options");
		var presetConfigs = new List<ConfigPerSave> {
			_default,
			_custom,
		};
		var presets = new List<string>{
			"Default",
			"Custom",
		};
		//const int defaultPresetIdx = 0;
		const int customPresetIdx = 1;
		
		{
			_custom.copySettings(Plugin.game);

			var presetsPath = Path.Combine(Path.GetDirectoryName(typeof(Plugin).Assembly.Location), "presets.json");
			if ( File.Exists(presetsPath) ) {
				var dict = JsonConvert.DeserializeObject<Dictionary<string, ConfigPerSave>>(File.ReadAllText(presetsPath));
				foreach ( var entry in dict ) {
					presets.Add(entry.Key);
					presetConfigs.Add(entry.Value);
				}
			}
		}

		var presetIdx = 0;
		if ( !Plugin.game.equalSettings(_default) ) {
			presetIdx = presetConfigs.FindIndex(customPresetIdx + 1, x => x.equalSettings(Plugin.game));
			if ( presetIdx == -1 )
				presetIdx = customPresetIdx;
		}

		var fields = ConfigPerSave.fieldsSettings;
		var sliders = new uGUI_SnappingSlider[fields.Count];

		bool suppressChangeEvent = false;
		UnityAction<int> onPreset = (int idx) => {
			if ( suppressChangeEvent )
				return;

			loadCurrent();
			suppressChangeEvent = true;
			for ( int i = 0; i < sliders.Length; ++i )
				sliders[i].value = readSliderField(presetConfigs[idx], fields[i]);
			suppressChangeEvent = false;

			_current.copySettings(presetConfigs[idx]);
		};
		var presetObject = panel.AddChoiceOption(modsTabIndex, "Preset", presets.ToArray(), presetIdx, callback: onPreset).GetComponentInChildren<uGUI_Choice>();

		for ( int fieldIdx = 0; fieldIdx < fields.Count; ++fieldIdx ) {
			switch ( fields[fieldIdx].GetCustomAttribute<ModOptionAttribute>() ) {
			case SliderAttribute attr:
				int sliderIdx = fieldIdx;
				UnityAction<float> onChange = (float x) => {
					if ( suppressChangeEvent )
						return;

					loadCurrent();
					if ( presetObject.value != customPresetIdx ) {
						suppressChangeEvent = true;
						presetObject.value = customPresetIdx;
						suppressChangeEvent = false;
						for ( int i = 0; i < sliders.Length; ++i ) {
							writeSliderField(_custom, fields[i], sliders[i].value);
							writeSliderField(_current, fields[i], sliders[i].value);
						}
					}
					writeSliderField(_custom, fields[sliderIdx], x);
					writeSliderField(_current, fields[sliderIdx], x);
				};
				sliders[sliderIdx] = panel.AddSliderOption(modsTabIndex, attr.Label, readSliderField(_custom, fields[sliderIdx]), attr.Min, attr.Max, attr.DefaultValue, attr.Step, onChange, typeToLabelMode(fields[sliderIdx].FieldType), attr.Format, attr.Tooltip)
					.GetComponentInChildren<uGUI_SnappingSlider>();
				break;
			}
		}
	}

	public void applyChanges()
	{
		if ( _current is null )
			return;

		Plugin.game.copySettings(_current);
		_current = null;

		Plugin.debug("Applying new game settings");

		applyInventory();
		applyStorageContainer();
		applyBioreactor();
		applyFiltrationMachine();
		applySeamoth();
		applyExosuit();
	}

	public static void applyInventory()
	{
		foreach ( var inv in Plugin.FindObjectsOfType<Inventory>() )
			inv.container.Resize(Plugin.game.inventory_width, Plugin.game.inventory_height);

		foreach ( var tab in Plugin.FindObjectsOfType<uGUI_InventoryTab>() )
			tab.inventory.Init(Inventory.main.container);
	}

	public static void applyStorageContainer()
	{
		foreach ( var s in Plugin.FindObjectsOfType<StorageContainer>() ) {
			Patch.StorageContainer_CreateContainer(s);
			s.Resize(s.width, s.height);
		}
	}

	public static void applyBioreactor()
	{
		foreach ( var b in Plugin.FindObjectsOfType<BaseBioReactor>() )
			Patch.BaseBioReactor_Start(b);
	}

	public static void applyFiltrationMachine()
	{
		foreach ( var f in Plugin.FindObjectsOfType<FiltrationMachine>() )
			Patch.FiltrationMachine_Start(f);
	}

	public static void applySeamoth()
	{
		foreach ( var s in Resources.FindObjectsOfTypeAll<SeamothStorageContainer>() ) {
			if ( s is null || s.name != "SeamothStorageModule(Clone)" )
				continue;

			Patch.SeamothStorageContainer_Init(s);
			s.container.Resize(s.width, s.height);
		}
	}

	public static void applyExosuit()
	{
		foreach ( var e in Plugin.FindObjectsOfType<Exosuit>() ) {
			Plugin.debug($"Applying to {e.name}");
			Patch.Exosuit_UpdateStorageSize(e);
		}
	}
}

[Menu(Info.title)]
public class ConfigGlobal : ConfigFile
{
	[Slider("Inventory Max View Width", 6, 8, Step = 1, DefaultValue = 6), OnChange(nameof(syncScrollPanes))]
	public int inventoryMaxView_width = 6;

	[Slider("Inventory Max View Height", 6, 8, Step = 1, DefaultValue = 8), OnChange(nameof(syncScrollPanes))]
	public int inventoryMaxView_height = 8;

	[Slider("Storage Max View Width", 6, 8, Step = 1, DefaultValue = 6), OnChange(nameof(syncScrollPanes))]
	public int storageMaxView_width = 6;

	[Slider("Storage Max View Height", 6, 8, Step = 1, DefaultValue = 8), OnChange(nameof(syncScrollPanes))]
	public int storageMaxView_height = 8;

	[Slider("Scroll View Margin", 0, 256, DefaultValue = 256), OnChange(nameof(syncScrollPanes))]
	public float viewMargin = 20.0f;

	[Slider("Scroll Bar Size", 0, 256, DefaultValue = 256), OnChange(nameof(syncScrollPanes))]
	public float scrollbarSize = 20.0f;

	[Toggle("Reset Scroll when PDA Opens")]
	public bool resetInventoryScroll = true;

	[Slider("Inventory Scroll Pane Padding Left", -32, 32, DefaultValue = 0), OnChange(nameof(syncScrollPanes))]
	public float inventoryMaskPadding_left = -7;
	[Slider("Inventory Scroll Pane Padding Bottom", -32, 32, DefaultValue = 0), OnChange(nameof(syncScrollPanes))]
	public float inventoryMaskPadding_bottom = -4;
	[Slider("Inventory Scroll Pane Padding Right", -32, 32, DefaultValue = 0), OnChange(nameof(syncScrollPanes))]
	public float inventoryMaskPadding_right = -4;
	[Slider("Inventory Scroll Pane Padding Top", -32, 32, DefaultValue = 0), OnChange(nameof(syncScrollPanes))]
	public float inventoryMaskPadding_top = -8;
	
	[Slider("Storage Scroll Pane Padding Left", -32, 32, DefaultValue = 0), OnChange(nameof(syncScrollPanes))]
	public float storageMaskPadding_left = -7;
	[Slider("Storage Scroll Pane Padding Bottom", -32, 32, DefaultValue = 0), OnChange(nameof(syncScrollPanes))]
	public float storageMaskPadding_bottom = -4;
	[Slider("Storage Scroll Pane Padding Right", -32, 32, DefaultValue = 0), OnChange(nameof(syncScrollPanes))]
	public float storageMaskPadding_right = -4;
	[Slider("Storage Scroll Pane Padding Top", -32, 32, DefaultValue = 0), OnChange(nameof(syncScrollPanes))]
	public float storageMaskPadding_top = -8;

	[Keybind("Move all items of type")]
	public KeyCode keyMoveAllItemType = KeyCode.LeftControl;
	[Keybind("Move all items")]
	public KeyCode keyMoveAllItems = KeyCode.LeftShift;
	[Keybind("Pin item")]
	public KeyCode keyPinItem = KeyCode.LeftAlt;

	private static readonly FieldInfo field_uGUI_ItemsContainer_container = typeof(uGUI_ItemsContainer).GetField("container", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
	public void syncItemsContainer(uGUI_ItemsContainer guiItemsContainer)
	{
		var isInventory = guiItemsContainer == guiItemsContainer.inventory.inventory;
		var m = guiItemsContainer.GetComponentInParent<RectMask2D>(true);
		if ( m ) {
			m.padding = isInventory
				? new Vector4(Plugin.config.inventoryMaskPadding_left, Plugin.config.inventoryMaskPadding_bottom, Plugin.config.inventoryMaskPadding_right, Plugin.config.inventoryMaskPadding_top)
				: new Vector4(Plugin.config.storageMaskPadding_left, Plugin.config.storageMaskPadding_bottom, Plugin.config.storageMaskPadding_right, Plugin.config.storageMaskPadding_top);
		}

		var scrollRect = guiItemsContainer.GetComponentInParent<ScrollRect>(true);
		if ( scrollRect ) {
			var r = scrollRect.verticalScrollbar.transform as RectTransform;
			r.sizeDelta = new Vector2(Plugin.config.scrollbarSize, 0);
			scrollRect.verticalScrollbar.enabled = r.sizeDelta.x != 0;
			scrollRect.verticalScrollbar.targetGraphic.gameObject.SetActive(scrollRect.verticalScrollbar.enabled);

			var container = field_uGUI_ItemsContainer_container.GetValue(guiItemsContainer) as ItemsContainer;
			var contentSize = new Vector2int(container.sizeX, container.sizeY);
			var maxViewSize = isInventory
				? new Vector2int(Plugin.config.inventoryMaxView_width, Plugin.config.inventoryMaxView_height)
				: new Vector2int(Plugin.config.storageMaxView_width, Plugin.config.storageMaxView_height);
			ScrollPane.init(guiItemsContainer, scrollRect, contentSize, maxViewSize);
		}
	}

	public void syncScrollPanes()
	{
		foreach ( var tab in Plugin.FindObjectsOfType<uGUI_InventoryTab>() ) {
			syncItemsContainer(tab.inventory);
			syncItemsContainer(tab.storage);
		}
	}

	public void syncScrollPanes(SliderChangedEventArgs evt)
	{
		syncScrollPanes();
	}
}

[FileName(Info.name)]
public class ConfigPerSave : SaveDataCache
{
	public static readonly List<FieldInfo> fieldsSettings = new List<FieldInfo>(
		from x in typeof(ConfigPerSave).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).AsEnumerable()
		where x.GetCustomAttribute<SliderAttribute>() != null
		select x);

	public bool equalSettings(ConfigPerSave a)
	{
		foreach ( var field in fieldsSettings ) {
			if ( !field.GetValue(this).Equals(field.GetValue(a)) )
				return false;
		}

		return true;
	}

	public void copySettings(ConfigPerSave a)
	{
		foreach ( var field in fieldsSettings )
			field.SetValue(this, field.GetValue(a));
	}

	/* The extreme DefaultValue is to mitigate subnautica defect in sliders where it snaps to the default value
	 * For large ranges this snapping prevents selecting values near the default
	 *
	 * High enough not to limit users who want to collect everything
	 * Low enough that the GUI slider has resolution of 1
	 */
	const int max = 256;

	public int version = 1;

	/* 6 storage slots fit comfortably on PDA inventory GUI
	 * 8 can work but looks crammed
	 */

	// inventory

	[Slider("Inventory Width" , 1, 8  , Step = 1, DefaultValue = 6  )] public int inventory_width  = 6;
	[Slider("Inventory Height", 1, max, Step = 1, DefaultValue = max)] public int inventory_height = 8;

	// lockers

	[Slider("Locker Width"            , 1, 8  , Step = 1, DefaultValue = 6  )] public int locker_width        = 6;
	[Slider("Locker Height"           , 1, max, Step = 1, DefaultValue = max)] public int locker_height       = 8;
	[Slider("Wall Locker Width"       , 1, 8  , Step = 1, DefaultValue = 5  )] public int smalllocker_width   = 5;
	[Slider("Wall Locker Height"      , 1, max, Step = 1, DefaultValue = max)] public int smalllocker_height  = 6;
	[Slider("Waterproof Locker Width" , 1, 8  , Step = 1, DefaultValue = 4  )] public int smallstorage_width  = 4;
	[Slider("Waterproof Locker Height", 1, max, Step = 1, DefaultValue = max)] public int smallstorage_height = 4;
	[Slider("Carry-all Width"         , 1, 8  , Step = 1, DefaultValue = 3  )] public int luggagebag_width    = 3;
	[Slider("Carry-all Height"        , 1, max, Step = 1, DefaultValue = max)] public int luggagebag_height   = 3;

	// vehicles

	[Slider("Lifepod 5 Storage Width"              , 1, 8  , Step = 1, DefaultValue = 4  )] public int lifepodLocker_width  = 4;
	[Slider("Lifepod 5 Storage Height"             , 1, max, Step = 1, DefaultValue = max)] public int lifepodLocker_height = 8;
	[Slider("Seamoth Storage Module Width"         , 1, 8  , Step = 1, DefaultValue = 6  )] public int seamoth_width  = 6;
	[Slider("Seamoth Storage Module Height"        , 1, max, Step = 1, DefaultValue = max)] public int seamoth_height = 6;
	[Slider("Prawn Suit Storage Width"             , 1, 8  , Step = 1, DefaultValue = 6  )] public int exosuit_width               = 6;
	[Slider("Prawn Suit Storage Height"            , 1, max, Step = 1, DefaultValue = max)] public int exosuit_height              = 4;
	[Slider("Prawn Suit Height Per Storage Module" , 1, max, Step = 1, DefaultValue = max)] public int exosuitStorageModule_height = 1;
	[Slider("Cyclops Storage Locker Default Width" , 1, 8  , Step = 1, DefaultValue = 3  )] public int cyclopsLocker_width = 3;
	[Slider("Cyclops Storage Locker Default Height", 1, max, Step = 1, DefaultValue = max)] public int cyclopsLocker_height = 6;
	[Slider("Cyclops Storage Locker 1 Width"       , 0, 8  , Step = 1, DefaultValue = 0  , Tooltip = "Set to 0 to use default")] public int cyclopsLocker1_width  = 0;
	[Slider("Cyclops Storage Locker 1 Height"      , 1, max, Step = 1, DefaultValue = max                                     )] public int cyclopsLocker1_height = 6;
	[Slider("Cyclops Storage Locker 2 Width"       , 0, 8  , Step = 1, DefaultValue = 0  , Tooltip = "Set to 0 to use default")] public int cyclopsLocker2_width  = 0;
	[Slider("Cyclops Storage Locker 2 Height"      , 1, max, Step = 1, DefaultValue = max                                     )] public int cyclopsLocker2_height = 6;
	[Slider("Cyclops Storage Locker 3 Width"       , 0, 8  , Step = 1, DefaultValue = 0  , Tooltip = "Set to 0 to use default")] public int cyclopsLocker3_width  = 0;
	[Slider("Cyclops Storage Locker 3 Height"      , 1, max, Step = 1, DefaultValue = max                                     )] public int cyclopsLocker3_height = 6;
	[Slider("Cyclops Storage Locker 4 Width"       , 0, 8  , Step = 1, DefaultValue = 0  , Tooltip = "Set to 0 to use default")] public int cyclopsLocker4_width  = 0;
	[Slider("Cyclops Storage Locker 4 Height"      , 1, max, Step = 1, DefaultValue = max                                     )] public int cyclopsLocker4_height = 6;
	[Slider("Cyclops Storage Locker 5 Width"       , 0, 8  , Step = 1, DefaultValue = 0  , Tooltip = "Set to 0 to use default")] public int cyclopsLocker5_width  = 0;
	[Slider("Cyclops Storage Locker 5 Height"      , 1, max, Step = 1, DefaultValue = max                                     )] public int cyclopsLocker5_height = 6;

	// todo: neptune escape rocket?

	// bases

	[Slider("Scanner Room Upgrades Width" , 1, 8  , Step = 1, DefaultValue = 4  )] public int maproomupgrades_width          = 2;
	[Slider("Scanner Room Upgrades Height", 1, max, Step = 1, DefaultValue = max)] public int maproomupgrades_height         = 2;
	[Slider("Bioreactor Width"            , 1, 8  , Step = 1, DefaultValue = 4  )] public int basebioreactor_width           = 4;
	[Slider("Bioreactor Height"           , 1, max, Step = 1, DefaultValue = max)] public int basebioreactor_height          = 4;
	[Slider("Filtration Machine Max Salt" , 1, max, Step = 1, DefaultValue = max)] public int basefiltrationMachine_maxSalt  = 2;
	[Slider("Filtration Machine Max Water", 1, max, Step = 1, DefaultValue = max)] public int basefiltrationMachine_maxWater = 2;

	// trash

	[Slider("Nuclear Waste Disposal Width" , 1, 8  , Step = 1, DefaultValue = 4  )] public int labtrashcan_width  = 3;
	[Slider("Nuclear Waste Disposal Height", 1, max, Step = 1, DefaultValue = max)] public int labtrashcan_height = 4;
	[Slider("Trash Can Width"              , 1, 8  , Step = 1, DefaultValue = 4  )] public int trashcans_width    = 4;
	[Slider("Trash Can Height"             , 1, max, Step = 1, DefaultValue = max)] public int trashcans_height   = 5;

	// planters

	[Slider("Basic Plant Pot Width"           , 1, 8  , Step = 1, DefaultValue = 4  )] public int planterpot_width    = 2;
	[Slider("Basic Plant Pot Height"          , 1, max, Step = 1, DefaultValue = max)] public int planterpot_height   = 2;
	[Slider("Composite Plant Pot Width"       , 1, 8  , Step = 1, DefaultValue = 4  )] public int planterpot2_width   = 2;
	[Slider("Composite Plant Pot Height"      , 1, max, Step = 1, DefaultValue = max)] public int planterpot2_height  = 2;
	[Slider("Chic Plant Pot Width"            , 1, 8  , Step = 1, DefaultValue = 4  )] public int planterpot3_width   = 2;
	[Slider("Chic Plant Pot Height"           , 1, max, Step = 1, DefaultValue = max)] public int planterpot3_height  = 2;
	[Slider("Plant Shelf Width"               , 1, 8  , Step = 1, DefaultValue = 4  )] public int plantershelf_width  = 1;
	[Slider("Plant Shelf Height"              , 1, max, Step = 1, DefaultValue = max)] public int plantershelf_height = 1;
	[Slider("Exterior Growbed Width"          , 1, 8  , Step = 1, DefaultValue = 4  )] public int farmingtray_width   = 4;
	[Slider("Exterior Growbed Height"         , 1, max, Step = 1, DefaultValue = max)] public int farmingtray_height  = 6;
	[Slider("Indoor Growbed Width"            , 1, 8  , Step = 1, DefaultValue = 4  )] public int planterbox_width    = 4;
	[Slider("Indoor Growbed Height"           , 1, max, Step = 1, DefaultValue = max)] public int planterbox_height   = 4;
	[Slider("Alien Containment Planter Width" , 1, 8  , Step = 1, DefaultValue = 4  )] public int planter_width       = 4;
	[Slider("Alien Containment Planter Height", 1, max, Step = 1, DefaultValue = max)] public int planter_height      = 4;

	// pinned inventory

	public HashSet<string> inventoryPinnedItems = new HashSet<string>();
}

public static class Commands
{
	[ConsoleCommand("set_inventory_width"               )] public static void set_inventory_width               (int x) { Plugin.game.inventory_width                = x; OptionsMenu.applyInventory(); }
	[ConsoleCommand("set_inventory_height"              )] public static void set_inventory_height              (int x) { Plugin.game.inventory_height               = x; OptionsMenu.applyInventory(); }
	[ConsoleCommand("set_locker_width"                  )] public static void set_locker_width                  (int x) { Plugin.game.locker_width                   = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_locker_height"                 )] public static void set_locker_height                 (int x) { Plugin.game.locker_height                  = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_smalllocker_width"             )] public static void set_smalllocker_width             (int x) { Plugin.game.smalllocker_width              = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_smalllocker_height"            )] public static void set_smalllocker_height            (int x) { Plugin.game.smalllocker_height             = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_smallstorage_width"            )] public static void set_smallstorage_width            (int x) { Plugin.game.smallstorage_width             = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_smallstorage_height"           )] public static void set_smallstorage_height           (int x) { Plugin.game.smallstorage_height            = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_luggagebag_width"              )] public static void set_luggagebag_width              (int x) { Plugin.game.luggagebag_width               = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_luggagebag_height"             )] public static void set_luggagebag_height             (int x) { Plugin.game.luggagebag_height              = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_lifepodLocker_width"           )] public static void set_lifepodLocker_width           (int x) { Plugin.game.lifepodLocker_width            = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_lifepodLocker_height"          )] public static void set_lifepodLocker_height          (int x) { Plugin.game.lifepodLocker_height           = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_seamoth_width"                 )] public static void set_seamoth_width                 (int x) { Plugin.game.seamoth_width                  = x; OptionsMenu.applySeamoth(); }
	[ConsoleCommand("set_seamoth_height"                )] public static void set_seamoth_height                (int x) { Plugin.game.seamoth_height                 = x; OptionsMenu.applySeamoth(); }
	[ConsoleCommand("set_exosuit_width"                 )] public static void set_exosuit_width                 (int x) { Plugin.game.exosuit_width                  = x; OptionsMenu.applyExosuit(); }
	[ConsoleCommand("set_exosuit_height"                )] public static void set_exosuit_height                (int x) { Plugin.game.exosuit_height                 = x; OptionsMenu.applyExosuit(); }
	[ConsoleCommand("set_exosuitStorageModule_height"   )] public static void set_exosuitStorageModule_height   (int x) { Plugin.game.exosuitStorageModule_height    = x; OptionsMenu.applyExosuit(); }
	[ConsoleCommand("set_cyclopsLocker_width"           )] public static void set_cyclopsLocker_width           (int x) { Plugin.game.cyclopsLocker_width            = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_cyclopsLocker_height"          )] public static void set_cyclopsLocker_height          (int x) { Plugin.game.cyclopsLocker_height           = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_cyclopsLocker1_width"          )] public static void set_cyclopsLocker1_width          (int x) { Plugin.game.cyclopsLocker1_width           = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_cyclopsLocker1_height"         )] public static void set_cyclopsLocker1_height         (int x) { Plugin.game.cyclopsLocker1_height          = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_cyclopsLocker2_width"          )] public static void set_cyclopsLocker2_width          (int x) { Plugin.game.cyclopsLocker2_width           = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_cyclopsLocker2_height"         )] public static void set_cyclopsLocker2_height         (int x) { Plugin.game.cyclopsLocker2_height          = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_cyclopsLocker3_width"          )] public static void set_cyclopsLocker3_width          (int x) { Plugin.game.cyclopsLocker3_width           = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_cyclopsLocker3_height"         )] public static void set_cyclopsLocker3_height         (int x) { Plugin.game.cyclopsLocker3_height          = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_cyclopsLocker4_width"          )] public static void set_cyclopsLocker4_width          (int x) { Plugin.game.cyclopsLocker4_width           = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_cyclopsLocker4_height"         )] public static void set_cyclopsLocker4_height         (int x) { Plugin.game.cyclopsLocker4_height          = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_cyclopsLocker5_width"          )] public static void set_cyclopsLocker5_width          (int x) { Plugin.game.cyclopsLocker5_width           = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_cyclopsLocker5_height"         )] public static void set_cyclopsLocker5_height         (int x) { Plugin.game.cyclopsLocker5_height          = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_maproomupgrades_width"         )] public static void set_maproomupgrades_width         (int x) { Plugin.game.maproomupgrades_width          = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_maproomupgrades_height"        )] public static void set_maproomupgrades_height        (int x) { Plugin.game.maproomupgrades_height         = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_basebioreactor_width"          )] public static void set_basebioreactor_width          (int x) { Plugin.game.basebioreactor_width           = x; OptionsMenu.applyBioreactor(); }
	[ConsoleCommand("set_basebioreactor_height"         )] public static void set_basebioreactor_height         (int x) { Plugin.game.basebioreactor_height          = x; OptionsMenu.applyBioreactor(); }
	[ConsoleCommand("set_basefiltrationMachine_maxSalt" )] public static void set_basefiltrationMachine_maxSalt (int x) { Plugin.game.basefiltrationMachine_maxSalt  = x; OptionsMenu.applyFiltrationMachine(); }
	[ConsoleCommand("set_basefiltrationMachine_maxWater")] public static void set_basefiltrationMachine_maxWater(int x) { Plugin.game.basefiltrationMachine_maxWater = x; OptionsMenu.applyFiltrationMachine(); }
	[ConsoleCommand("set_labtrashcan_width"             )] public static void set_labtrashcan_width             (int x) { Plugin.game.labtrashcan_width              = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_labtrashcan_height"            )] public static void set_labtrashcan_height            (int x) { Plugin.game.labtrashcan_height             = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_trashcans_width"               )] public static void set_trashcans_width               (int x) { Plugin.game.trashcans_width                = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_trashcans_height"              )] public static void set_trashcans_height              (int x) { Plugin.game.trashcans_height               = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planterpot_width"              )] public static void set_planterpot_width              (int x) { Plugin.game.planterpot_width               = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planterpot_height"             )] public static void set_planterpot_height             (int x) { Plugin.game.planterpot_height              = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planterpot2_width"             )] public static void set_planterpot2_width             (int x) { Plugin.game.planterpot2_width              = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planterpot2_height"            )] public static void set_planterpot2_height            (int x) { Plugin.game.planterpot2_height             = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planterpot3_width"             )] public static void set_planterpot3_width             (int x) { Plugin.game.planterpot3_width              = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planterpot3_height"            )] public static void set_planterpot3_height            (int x) { Plugin.game.planterpot3_height             = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_plantershelf_width"            )] public static void set_plantershelf_width            (int x) { Plugin.game.plantershelf_width             = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_plantershelf_height"           )] public static void set_plantershelf_height           (int x) { Plugin.game.plantershelf_height            = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_farmingtray_width"             )] public static void set_farmingtray_width             (int x) { Plugin.game.farmingtray_width              = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_farmingtray_height"            )] public static void set_farmingtray_height            (int x) { Plugin.game.farmingtray_height             = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planterbox_width"              )] public static void set_planterbox_width              (int x) { Plugin.game.planterbox_width               = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planterbox_height"             )] public static void set_planterbox_height             (int x) { Plugin.game.planterbox_height              = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planter_width"                 )] public static void set_planter_width                 (int x) { Plugin.game.planter_width                  = x; OptionsMenu.applyStorageContainer(); }
	[ConsoleCommand("set_planter_height"                )] public static void set_planter_height                (int x) { Plugin.game.planter_height                 = x; OptionsMenu.applyStorageContainer(); }
}

} // namespace org.efool.subnautica.custom_inventory