﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Alex.API.Gui;
using Alex.API.Gui.Graphics;
using Alex.API.Localization;
using Alex.API.Utils;
using Alex.Audio;
using Alex.ResourcePackLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;
using RocketUI.Audio;
using AudioEngine = Alex.Audio.AudioEngine;

namespace Alex.Gui
{
	public class GuiRenderer : IGuiRenderer
	{
		private IFont _font;

		public IFont Font
		{
			get => _font;
			set
			{
				_font = value;
				OnFontChanged();
			}
		}

		public GuiScaledResolution ScaledResolution { get; set; }

		public CultureLanguage Language =
			new CultureLanguage();

		private GraphicsDevice  _graphicsDevice;
		private ResourceManager _resourceManager;

		private readonly Dictionary<GuiTextures, TextureSlice2D>  _textureCache       = new Dictionary<GuiTextures, TextureSlice2D>();
		private readonly Dictionary<string, TextureSlice2D>       _pathedTextureCache = new Dictionary<string, TextureSlice2D>();
		private readonly Dictionary<GuiSoundEffects, ISoundEffect> _soundEffectCache   = new Dictionary<GuiSoundEffects, ISoundEffect>();

		private Texture2D _widgets;
		private Texture2D _icons;
		private Texture2D _scrollbar;
		private Texture2D _inventory;
		private Texture2D _chestInventory;
		private Texture2D _craftingTable;
		private Texture2D _furnace;
		private Texture2D _tabItemSearch;
		
		#region SpriteSheet Definitions

		#region Widgets

		private static readonly Rectangle WidgetHotBar                = new Rectangle(0, 0,  182, 22);
		private static readonly Rectangle WidgetHotBarSelectedOverlay = new Rectangle(0, 22, 24,  24);
		private static readonly Rectangle WidgetButtonDisabled        = new Rectangle(0, 46, 200, 20);
		private static readonly Rectangle WidgetButtonDefault         = new Rectangle(0, 66, 200, 20);
		private static readonly Rectangle WidgetButtonHover           = new Rectangle(0, 86, 200, 20);

		private static readonly Rectangle WidgetHotBarSeparated = new Rectangle(24, 23, 22, 22);

		private static readonly Rectangle WidgetGreen = new Rectangle(208, 0, 15, 15);
		private static readonly Rectangle WidgetGrey = new Rectangle(224, 0, 15, 15);
		#endregion

		#region Icons

		private static readonly Rectangle IconCrosshair = new Rectangle(0, 0, 15, 15);

		private static readonly Rectangle IconServerPing5 = new Rectangle(0, 176, 10, 8);
		private static readonly Rectangle IconServerPing4 = new Rectangle(0, 184, 10, 8);
		private static readonly Rectangle IconServerPing3 = new Rectangle(0, 192, 10, 8);
		private static readonly Rectangle IconServerPing2 = new Rectangle(0, 200, 10, 8);
		private static readonly Rectangle IconServerPing1 = new Rectangle(0, 208, 10, 8);
		private static readonly Rectangle IconServerPing0 = new Rectangle(0, 216, 10, 8);

		private static readonly Rectangle IconServerPingPending1 = new Rectangle(10, 176, 10, 8);
		private static readonly Rectangle IconServerPingPending2 = new Rectangle(10, 184, 10, 8);
		private static readonly Rectangle IconServerPingPending3 = new Rectangle(10, 192, 10, 8);
		private static readonly Rectangle IconServerPingPending4 = new Rectangle(10, 200, 10, 8);
		private static readonly Rectangle IconServerPingPending5 = new Rectangle(10, 208, 10, 8);

		private static readonly Rectangle IconHeartHolder = new Rectangle(16, 0, 9, 9);
		private static readonly Rectangle IconHeart = new Rectangle(52, 0, 9, 9);
		private static readonly Rectangle IconHalfHeart = new Rectangle(69, 0, 9, 9);
		
		#endregion

		#region ScrollBar

		public static readonly Rectangle ScrollBarBackgroundDefault  = new Rectangle(0, 0, 10, 10);
		public static readonly Rectangle ScrollBarBackgroundHover    = new Rectangle(0, 0, 10, 10);
		public static readonly Rectangle ScrollBarBackgroundFocus    = new Rectangle(0, 0, 10, 10);
		public static readonly Rectangle ScrollBarBackgroundDisabled = new Rectangle(0, 0, 10, 10);

		public static readonly Rectangle ScrollBarTrackDefault  = new Rectangle(10, 10, 10, 10);
		public static readonly Rectangle ScrollBarTrackHover    = new Rectangle(10, 10, 10, 10);
		public static readonly Rectangle ScrollBarTrackFocus    = new Rectangle(10, 10, 10, 10);
		public static readonly Rectangle ScrollBarTrackDisabled = new Rectangle(10, 10, 10, 10);

		public static readonly Rectangle ScrollBarUpButtonDefault  = new Rectangle(20, 20, 10, 10);
		public static readonly Rectangle ScrollBarUpButtonHover    = new Rectangle(20, 20, 10, 10);
		public static readonly Rectangle ScrollBarUpButtonFocus    = new Rectangle(20, 20, 10, 10);
		public static readonly Rectangle ScrollBarUpButtonDisabled = new Rectangle(20, 20, 10, 10);

		public static readonly Rectangle ScrollBarDownButtonDefault  = new Rectangle(30, 30, 10, 10);
		public static readonly Rectangle ScrollBarDownButtonHover    = new Rectangle(30, 30, 10, 10);
		public static readonly Rectangle ScrollBarDownButtonFocus    = new Rectangle(30, 30, 10, 10);
		public static readonly Rectangle ScrollBarDownButtonDisabled = new Rectangle(30, 30, 10, 10);

		#endregion

		#endregion
		
		public GuiRenderer()
		{
		}


		public void Init(GraphicsDevice graphics, IServiceProvider serviceProvider)
		{
			_graphicsDevice  = graphics;
			_resourceManager = serviceProvider.GetRequiredService<ResourceManager>();
			
			LoadEmbeddedTextures();
			LoadSoundEffects(serviceProvider.GetRequiredService<AudioEngine>());
		}

		private void LoadSoundEffects(AudioEngine audioEngine)
		{
			LoadSoundEffect(GuiSoundEffects.ButtonClick, new RocketSoundEffect(audioEngine, "random.click"));
		}

		private void LoadSoundEffect(GuiSoundEffects guiSoundEffects, ISoundEffect soundEffect)
		{
			_soundEffectCache[guiSoundEffects] = soundEffect;
		}
		public ISoundEffect GetSoundEffect(GuiSoundEffects soundEffects)
		{
			if (_soundEffectCache.TryGetValue(soundEffects, out var soundEffect))
			{
				return soundEffect;//.CreateInstance();
			}

			return null;
		}

		private void OnFontChanged()
		{
		}

		//private CultureInfo Culture { get; set; }
		public bool SetLanguage(string cultureCode)
		{
			cultureCode = cultureCode;

			try
			{
				/*Culture = CultureInfo.GetCultureInfo(cultureCode.Replace("_", "-"));
				CultureInfo.CurrentCulture = Culture;
				CultureInfo.CurrentUICulture = Culture;
				CultureInfo.DefaultThreadCurrentUICulture = Culture;
				CultureInfo.DefaultThreadCurrentUICulture = Culture;
*/
				if (_languages.TryGetValue(cultureCode, out var lng))
				{
					Language = lng;
					ChatParser.Language = lng;
					
					return true;
				}
				
				/*var matchingResults = _resourceManager.ResourcePack.Languages
				   .Where(x => x.Value.CultureCode == cultureCode).Select(x => x.Value).ToArray();

				if (matchingResults.Length <= 0) return false;
			//	var             cultureInfo = CultureInfo.GetCultureInfo(cultureCode.Replace("_", "-"));
				
				CultureLanguage newLanguage = new CultureLanguage();

				foreach (var lang in matchingResults)
				{
					newLanguage.Load(lang);
				}

				Language = newLanguage;

				return true;*/
			}
			catch (CultureNotFoundException)
			{
				
			}

			return false;
		}

		private readonly Dictionary<string, CultureLanguage>          _languages = new Dictionary<string, CultureLanguage>();
		public           IReadOnlyDictionary<string, CultureLanguage> Languages => _languages;
		
		public void LoadLanguages(McResourcePack resourcePack, IProgressReceiver progressReceiver)
		{
			if (resourcePack.Languages == null)
				return;

			var languages = resourcePack.Languages.Count;
			int done      = 0;
			foreach (var lng in resourcePack.Languages)
			{
				if (lng.Value?.CultureCode == null)
					continue;
				var key = lng.Value.CultureCode.ToLower();
				
				progressReceiver?.UpdateProgress(done, languages, "Loading languages...", key);
				try
				{
					CultureLanguage language;
					if (!_languages.TryGetValue(key, out language))
					{
						language = new CultureLanguage()
						{
							Name = lng.Value.Name,
							Code = lng.Value.CultureCode,
							Region = lng.Value.CultureRegion
						};

						if (!string.IsNullOrWhiteSpace(lng.Value.CultureRegion))
						{
							language.DisplayName = $"{lng.Value.CultureName} ({lng.Value.CultureRegion})";
						}
					}

					//if (lng.Value.CultureCode == Culture.Name)
					language.Load(lng.Value);

					_languages[key] = language;
				}catch(CultureNotFoundException){}
			}
		}


		private void LoadEmbeddedTextures()
		{
			LoadTextureFromEmbeddedResource(AlexGuiTextures.AlexLogo, ResourceManager.ReadResource("Alex.Resources.logo2.png"));
			LoadTextureFromEmbeddedResource(AlexGuiTextures.ProgressBar, ResourceManager.ReadResource("Alex.Resources.ProgressBar.png"));
			LoadTextureFromEmbeddedResource(AlexGuiTextures.SplashBackground, ResourceManager.ReadResource("Alex.Resources.Splash.png"));
			LoadTextureFromEmbeddedResource(AlexGuiTextures.GradientBlur, ResourceManager.ReadResource("Alex.Resources.GradientBlur.png"));							
		}
		
		public void LoadResourcePackTextures(ResourceManager resourceManager, IProgressReceiver progressReceiver)
		{
			//progressReceiver?.UpdateProgress(0, null, "gui/widgets");
			//LoadTextureFromResourcePack(GuiTextures.AlexLogo, resourcePack, "");

			// First load Widgets
			progressReceiver?.UpdateProgress(0, null, "gui/widgets");
			if (resourceManager.TryGetBitmap("gui/widgets", out var widgetsBmp))
			{
				_widgets = TextureUtils.BitmapToTexture2D(_graphicsDevice, widgetsBmp);
				LoadWidgets(_widgets);
			}

			progressReceiver?.UpdateProgress(25, null, "gui/icons");
			if (resourceManager.TryGetBitmap("gui/icons", out var icons))
			{
				_icons = TextureUtils.BitmapToTexture2D(_graphicsDevice, icons);
				LoadIcons(_icons);
			}

			if (_scrollbar == null)
			{
				_scrollbar = TextureUtils.ImageToTexture2D(_graphicsDevice,
					ResourceManager.ReadResource("Alex.Resources.ScrollBar.png"));
				LoadScrollBar(_scrollbar);
			}

			// Backgrounds
			progressReceiver?.UpdateProgress(50, null, "gui/options_background");
			LoadTextureFromResourcePack(AlexGuiTextures.OptionsBackground, resourceManager, "gui/options_background", 2f);

			// Load Gui Containers
			{
				progressReceiver?.UpdateProgress(0, null, "gui/container/inventory");
				
				if (resourceManager.TryGetBitmap("gui/container/inventory", out var bmp))
				{
					_inventory = TextureUtils.BitmapToTexture2D(_graphicsDevice, bmp);
					LoadTextureFromSpriteSheet(AlexGuiTextures.InventoryPlayerBackground, _inventory, new Rectangle(0, 0, 176, 166), IconSize);
				}

				if (resourceManager.TryGetBitmap("gui/container/generic_54", out var genericInvBmp))
				{
					_chestInventory = TextureUtils.BitmapToTexture2D(_graphicsDevice, genericInvBmp);
					LoadTextureFromSpriteSheet(AlexGuiTextures.InventoryChestBackground, _chestInventory, new Rectangle(0, 0, 175, 221), IconSize);
				}

				if (resourceManager.TryGetBitmap("gui/container/crafting_table", out var craftingTable))
				{
					_craftingTable = TextureUtils.BitmapToTexture2D(_graphicsDevice, craftingTable);
					LoadTextureFromSpriteSheet(AlexGuiTextures.InventoryCraftingTable, _craftingTable, new Rectangle(0, 0, 175, 165), IconSize);
				}
				
				if (resourceManager.TryGetBitmap("gui/container/furnace", out var furnace))
				{
					_furnace = TextureUtils.BitmapToTexture2D(_graphicsDevice, furnace);
					LoadTextureFromSpriteSheet(AlexGuiTextures.InventoryFurnace, _furnace, new Rectangle(0, 0, 175, 165), IconSize);
				}

				if (resourceManager.TryGetBitmap("gui/container/creative_inventory/tab_item_search", out var tabImage))
				{
					_tabItemSearch = TextureUtils.BitmapToTexture2D(_graphicsDevice, tabImage);
					LoadTextureFromSpriteSheet(AlexGuiTextures.InventoryCreativeItemSearch, _tabItemSearch, new Rectangle(0, 0, 194, 135), IconSize);
				}
				//LoadTextureFromSpriteSheet(GuiTextures.InventoryChestBackground, _inventory, new Rectangle(0, 0, 175, 221), IconSize);
			}

			progressReceiver?.UpdateProgress(75, null, "gui/title/background");
			
			// Panorama
			LoadTextureFromResourcePack(AlexGuiTextures.Panorama0, resourceManager, "gui/title/background/panorama_0");
			LoadTextureFromResourcePack(AlexGuiTextures.Panorama1, resourceManager, "gui/title/background/panorama_1");
			LoadTextureFromResourcePack(AlexGuiTextures.Panorama2, resourceManager, "gui/title/background/panorama_2");
			LoadTextureFromResourcePack(AlexGuiTextures.Panorama3, resourceManager, "gui/title/background/panorama_3");
			LoadTextureFromResourcePack(AlexGuiTextures.Panorama4, resourceManager, "gui/title/background/panorama_4");
			LoadTextureFromResourcePack(AlexGuiTextures.Panorama5, resourceManager, "gui/title/background/panorama_5");

			// Other
			LoadTextureFromResourcePack(AlexGuiTextures.DefaultServerIcon, resourceManager, "misc/unknown_server");
			
			progressReceiver?.UpdateProgress(100, null, "");
		}

		private void LoadWidgets(Texture2D spriteSheet)
		{
			LoadTextureFromSpriteSheet(AlexGuiTextures.Inventory_HotBar, spriteSheet, WidgetHotBar, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.Inventory_HotBar_SelectedItemOverlay, spriteSheet,
									   WidgetHotBarSelectedOverlay, IconSize);

			LoadTextureFromSpriteSheet(AlexGuiTextures.ButtonDefault,  spriteSheet, WidgetButtonDefault, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ButtonHover,    spriteSheet, WidgetButtonHover, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ButtonFocused,  spriteSheet, WidgetButtonHover, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ButtonDisabled, spriteSheet, WidgetButtonDisabled, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.PanelGeneric, spriteSheet, WidgetHotBarSeparated,
									   new Thickness(5), IconSize);
			
			LoadTextureFromSpriteSheet(AlexGuiTextures.GreenCheckMark, spriteSheet, WidgetGreen, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.GreyCheckMark, spriteSheet, WidgetGrey, IconSize);
		}

		private Size IconSize { get; } = new Size(256, 256);
		private void LoadIcons(Texture2D spriteSheet)
		{
			LoadTextureFromSpriteSheet(AlexGuiTextures.Crosshair,   spriteSheet, IconCrosshair, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ServerPing0, spriteSheet, IconServerPing0, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ServerPing1, spriteSheet, IconServerPing1, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ServerPing2, spriteSheet, IconServerPing2, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ServerPing3, spriteSheet, IconServerPing3, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ServerPing4, spriteSheet, IconServerPing4, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ServerPing5, spriteSheet, IconServerPing5, IconSize);

			LoadTextureFromSpriteSheet(AlexGuiTextures.ServerPingPending1, spriteSheet, IconServerPingPending1, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ServerPingPending2, spriteSheet, IconServerPingPending2, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ServerPingPending3, spriteSheet, IconServerPingPending3, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ServerPingPending4, spriteSheet, IconServerPingPending4, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ServerPingPending5, spriteSheet, IconServerPingPending5, IconSize);
			
			LoadTextureFromSpriteSheet(AlexGuiTextures.HealthPlaceholder, spriteSheet, IconHeartHolder, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.HealthHeart, spriteSheet, IconHeart, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.HealthHalfHeart, spriteSheet, IconHalfHeart, IconSize);
			
			LoadTextureFromSpriteSheet(AlexGuiTextures.HungerPlaceholder, spriteSheet, new Rectangle(16, 27, 9, 9), IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.HungerFull, spriteSheet, new Rectangle(52, 27, 9, 9), IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.HungerHalf, spriteSheet, new Rectangle(61, 27, 9, 9), IconSize);
		}

		private Size ScrollbarSize { get; } = new Size(40,40);
		private void LoadScrollBar(Texture2D spriteSheet)
		{
			LoadTextureFromSpriteSheet(AlexGuiTextures.ScrollBarBackground, spriteSheet, ScrollBarBackgroundDefault, ScrollbarSize);

			LoadTextureFromSpriteSheet(AlexGuiTextures.ScrollBarTrackDefault,  spriteSheet, ScrollBarTrackDefault, ScrollbarSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ScrollBarTrackHover,    spriteSheet, ScrollBarTrackHover, ScrollbarSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ScrollBarTrackFocused,  spriteSheet, ScrollBarTrackFocus, ScrollbarSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ScrollBarTrackDisabled, spriteSheet, ScrollBarTrackDisabled, ScrollbarSize);

			LoadTextureFromSpriteSheet(AlexGuiTextures.ScrollBarUpButtonDefault,  spriteSheet, ScrollBarUpButtonDefault, ScrollbarSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ScrollBarUpButtonHover,    spriteSheet, ScrollBarUpButtonHover, ScrollbarSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ScrollBarUpButtonFocused,  spriteSheet, ScrollBarUpButtonFocus, ScrollbarSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ScrollBarUpButtonDisabled, spriteSheet, ScrollBarUpButtonDisabled, ScrollbarSize);

			LoadTextureFromSpriteSheet(AlexGuiTextures.ScrollBarDownButtonDefault,  spriteSheet, ScrollBarDownButtonDefault, ScrollbarSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ScrollBarDownButtonHover,    spriteSheet, ScrollBarDownButtonHover, ScrollbarSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ScrollBarDownButtonFocused,  spriteSheet, ScrollBarDownButtonFocus, ScrollbarSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ScrollBarDownButtonDisabled, spriteSheet, ScrollBarDownButtonDisabled, ScrollbarSize);
		}


		private TextureSlice2D LoadTextureFromEmbeddedResource(GuiTextures guiTexture, byte[] resource)
		{
			_textureCache[guiTexture] = TextureUtils.ImageToTexture2D( _graphicsDevice, resource);
			return _textureCache[guiTexture];
		}

		private void LoadTextureFromResourcePack(GuiTextures guiTexture, ResourceManager resources, string path,
												 float       scale = 1f)
		{
			if (resources.TryGetBitmap(path, out var texture))
			{
				_textureCache[guiTexture] = TextureUtils.BitmapToTexture2D(_graphicsDevice, texture);
			}
		}

		private void LoadTextureFromSpriteSheet(GuiTextures guiTexture, Texture2D spriteSheet, Rectangle sliceRectangle,
												Thickness   ninePatchThickness, Size originalSize)
		{
			var widthScaler = spriteSheet.Width / originalSize.Width;
			var heightScaler = spriteSheet.Height / originalSize.Height;
			
			_textureCache[guiTexture] = new NinePatchTexture2D(Texture2DExtensions.Slice(spriteSheet, new Rectangle(sliceRectangle.X * widthScaler,
				sliceRectangle.Y * heightScaler, sliceRectangle.Width * widthScaler,
				sliceRectangle.Height * heightScaler)), ninePatchThickness);
		}

		private void LoadTextureFromSpriteSheet(GuiTextures guiTexture, Texture2D spriteSheet, Rectangle sliceRectangle, Size originalSize)
		{
			var widthScaler = spriteSheet.Width / originalSize.Width;
			var heightScaler = spriteSheet.Height / originalSize.Height;

			_textureCache[guiTexture] = Texture2DExtensions.Slice(spriteSheet, new Rectangle(sliceRectangle.X * widthScaler,
				sliceRectangle.Y * heightScaler, sliceRectangle.Width * widthScaler,
				sliceRectangle.Height * heightScaler));
		}
		
		public TextureSlice2D GetTexture(GuiTextures guiTexture)
		{
			if (_textureCache.TryGetValue(guiTexture, out var texture))
			{
				return texture;
			}

			return (TextureSlice2D) RocketUI.GpuResourceManager.CreateTexture2D( 1, 1);
		}

		public TextureSlice2D GetTexture(string texturePath)
		{
			texturePath = texturePath.ToLowerInvariant();
            
			if (!_pathedTextureCache.TryGetValue(texturePath, out TextureSlice2D texture))
			{
				texture = Texture2D.FromFile(_graphicsDevice, texturePath);
				_pathedTextureCache.Add(texturePath, texture);
			}

			return texture;
		}

		public Texture2D GetTexture2D(GuiTextures guiTexture)
		{
			return GetTexture(guiTexture).Texture;
		}

		public string GetTranslation(string key)
		{
			return Language[key];
		}

		public Vector2 Project(Vector2 point)
		{
			return Vector2.Transform(point, ScaledResolution.TransformMatrix);
		}

		public Vector2 Unproject(Vector2 screen)
		{
			return Vector2.Transform(screen, ScaledResolution.InverseTransformMatrix);
		}

		public GraphicsContext CreateGuiSpriteBatchContext(GraphicsDevice graphics)
		{
			return GraphicsContext.CreateContext(graphics, BlendState.NonPremultiplied, DepthStencilState.None, RasterizerState.CullNone, SamplerState.PointClamp);
		}
		
		public IStyle[] ResolveStyles(Type elementType, string[] classNames)
		{
			if (elementType.IsAssignableFrom(typeof(StackMenuItem)))
			{
				return new[]
				{
					new Style()
					{
						Name = nameof(StackMenuItem),
						TargetType = typeof(StackMenuItem),
						Setters = new ObservableCollection<Setter>()
						{
							new Setter(
								nameof(Button.Background), 
								new GuiTexture2D() {Color = Color.Transparent}),
							new Setter(
								nameof(Button.DisabledBackground),
								new GuiTexture2D() {Color = Color.Transparent}),
							new Setter(
								nameof(Button.FocusedBackground),
								new GuiTexture2D() {Color = Color.Transparent}),
							new Setter(
								nameof(Button.HighlightedBackground),
								new GuiTexture2D() {Color = new Color(Color.Black * 0.8f, 0.5f)}),
							new Setter(
								nameof(Button.HighlightColor),
								new GuiTexture2D() {Color = (Color) TextColor.Cyan}),
							new Setter(
								nameof(Button.DefaultColor),
								new GuiTexture2D() {Color = (Color) TextColor.White})
						},
					}
				};
			}
			else if (elementType.IsAssignableFrom(typeof(Button)))
			{
				return new[]
				{
					new Style()
					{
						Name = nameof(Button),
						TargetType = typeof(Button),
						Setters = new ObservableCollection<Setter>()
						{
							new Setter(
								nameof(Button.Background),
								new GuiTexture2D() {TextureResource = AlexGuiTextures.ButtonDefault})
						}
					}
				};
			}
			return null;
		}
	}
}
