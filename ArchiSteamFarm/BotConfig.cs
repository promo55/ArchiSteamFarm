﻿//     _                _      _  ____   _                           _____
//    / \    _ __  ___ | |__  (_)/ ___| | |_  ___   __ _  _ __ ___  |  ___|__ _  _ __  _ __ ___
//   / _ \  | '__|/ __|| '_ \ | |\___ \ | __|/ _ \ / _` || '_ ` _ \ | |_  / _` || '__|| '_ ` _ \
//  / ___ \ | |  | (__ | | | || | ___) || |_|  __/| (_| || | | | | ||  _|| (_| || |   | | | | | |
// /_/   \_\|_|   \___||_| |_||_||____/  \__|\___| \__,_||_| |_| |_||_|   \__,_||_|   |_| |_| |_|
// 
// Copyright 2015-2018 Łukasz "JustArchi" Domeradzki
// Contact: JustArchi@JustArchi.net
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArchiSteamFarm.Json;
using ArchiSteamFarm.Localization;
using Newtonsoft.Json;
using SteamKit2;

namespace ArchiSteamFarm {
	[SuppressMessage("ReSharper", "ClassCannotBeInstantiated")]
	public sealed class BotConfig {
		private const bool DefaultAcceptGifts = false;
		private const bool DefaultAutoSteamSaleEvent = false;
		private const EBotBehaviour DefaultBotBehaviour = EBotBehaviour.None;
		private const string DefaultCustomGamePlayedWhileFarming = null;
		private const string DefaultCustomGamePlayedWhileIdle = null;
		private const bool DefaultEnabled = false;
		private const byte DefaultHoursUntilCardDrops = 3;
		private const bool DefaultIdlePriorityQueueOnly = false;
		private const bool DefaultIdleRefundableGames = true;
		private const EPersonaState DefaultOnlineStatus = EPersonaState.Online;
		private const ArchiCryptoHelper.ECryptoMethod DefaultPasswordFormat = ArchiCryptoHelper.ECryptoMethod.PlainText;
		private const bool DefaultPaused = false;
		private const ERedeemingPreferences DefaultRedeemingPreferences = ERedeemingPreferences.None;
		private const bool DefaultSendOnFarmingFinished = false;
		private const byte DefaultSendTradePeriod = 0;
		private const bool DefaultShutdownOnFarmingFinished = false;
		private const string DefaultSteamLogin = null;
		private const ulong DefaultSteamMasterClanID = 0;
		private const string DefaultSteamParentalCode = null;
		private const string DefaultSteamPassword = null;
		private const string DefaultSteamTradeToken = null;
		private const ETradingPreferences DefaultTradingPreferences = ETradingPreferences.None;
		private const bool DefaultUseLoginKeys = true;

		private static readonly ImmutableList<EFarmingOrder> DefaultFarmingOrders = ImmutableList<EFarmingOrder>.Empty;
		private static readonly ImmutableHashSet<uint> DefaultGamesPlayedWhileIdle = ImmutableHashSet<uint>.Empty;
		private static readonly ImmutableHashSet<Steam.Asset.EType> DefaultLootableTypes = ImmutableHashSet.Create(Steam.Asset.EType.BoosterPack, Steam.Asset.EType.FoilTradingCard, Steam.Asset.EType.TradingCard);
		private static readonly ImmutableHashSet<Steam.Asset.EType> DefaultMatchableTypes = ImmutableHashSet.Create(Steam.Asset.EType.TradingCard);
		private static readonly ImmutableDictionary<ulong, EPermission> DefaultSteamUserPermissions = ImmutableDictionary<ulong, EPermission>.Empty;
		private static readonly ImmutableHashSet<Steam.Asset.EType> DefaultTransferableTypes = ImmutableHashSet.Create(Steam.Asset.EType.BoosterPack, Steam.Asset.EType.FoilTradingCard, Steam.Asset.EType.TradingCard);

		private static readonly SemaphoreSlim WriteSemaphore = new SemaphoreSlim(1, 1);

		[JsonProperty(Required = Required.DisallowNull)]
		internal readonly bool AcceptGifts = DefaultAcceptGifts;

		[JsonProperty(Required = Required.DisallowNull)]
		internal readonly bool AutoSteamSaleEvent = DefaultAutoSteamSaleEvent;

		[JsonProperty(Required = Required.DisallowNull)]
		internal readonly EBotBehaviour BotBehaviour = DefaultBotBehaviour;

		[JsonProperty]
		internal readonly string CustomGamePlayedWhileFarming = DefaultCustomGamePlayedWhileFarming;

		[JsonProperty]
		internal readonly string CustomGamePlayedWhileIdle = DefaultCustomGamePlayedWhileIdle;

		[JsonProperty(Required = Required.DisallowNull)]
		internal readonly bool Enabled = DefaultEnabled;

		[JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace, Required = Required.DisallowNull)]
		internal readonly ImmutableList<EFarmingOrder> FarmingOrders = DefaultFarmingOrders;

		[JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace, Required = Required.DisallowNull)]
		internal readonly ImmutableHashSet<uint> GamesPlayedWhileIdle = DefaultGamesPlayedWhileIdle;

		[JsonProperty(Required = Required.DisallowNull)]
		internal readonly byte HoursUntilCardDrops = DefaultHoursUntilCardDrops;

		[JsonProperty(Required = Required.DisallowNull)]
		internal readonly bool IdlePriorityQueueOnly = DefaultIdlePriorityQueueOnly;

		[JsonProperty(Required = Required.DisallowNull)]
		internal readonly bool IdleRefundableGames = DefaultIdleRefundableGames;

		[JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace, Required = Required.DisallowNull)]
		internal readonly ImmutableHashSet<Steam.Asset.EType> LootableTypes = DefaultLootableTypes;

		[JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace, Required = Required.DisallowNull)]
		internal readonly ImmutableHashSet<Steam.Asset.EType> MatchableTypes = DefaultMatchableTypes;

		[JsonProperty(Required = Required.DisallowNull)]
		internal readonly EPersonaState OnlineStatus = DefaultOnlineStatus;

		[JsonProperty(Required = Required.DisallowNull)]
		internal readonly ArchiCryptoHelper.ECryptoMethod PasswordFormat = DefaultPasswordFormat;

		[JsonProperty(Required = Required.DisallowNull)]
		internal readonly bool Paused = DefaultPaused;

		[JsonProperty(Required = Required.DisallowNull)]
		internal readonly ERedeemingPreferences RedeemingPreferences = DefaultRedeemingPreferences;

		[JsonProperty(Required = Required.DisallowNull)]
		internal readonly bool SendOnFarmingFinished = DefaultSendOnFarmingFinished;

		[JsonProperty(Required = Required.DisallowNull)]
		internal readonly byte SendTradePeriod = DefaultSendTradePeriod;

		[JsonProperty(Required = Required.DisallowNull)]
		internal readonly bool ShutdownOnFarmingFinished = DefaultShutdownOnFarmingFinished;

		[JsonProperty]
		internal readonly string SteamTradeToken = DefaultSteamTradeToken;

		[JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace, Required = Required.DisallowNull)]
		internal readonly ImmutableDictionary<ulong, EPermission> SteamUserPermissions = DefaultSteamUserPermissions;

		[JsonProperty(Required = Required.DisallowNull)]
		internal readonly ETradingPreferences TradingPreferences = DefaultTradingPreferences;

		[JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace, Required = Required.DisallowNull)]
		internal readonly ImmutableHashSet<Steam.Asset.EType> TransferableTypes = DefaultTransferableTypes;

		[JsonProperty(Required = Required.DisallowNull)]
		internal readonly bool UseLoginKeys = DefaultUseLoginKeys;

		internal string DecryptedSteamPassword {
			get {
				if (string.IsNullOrEmpty(SteamPassword)) {
					return null;
				}

				if (PasswordFormat == ArchiCryptoHelper.ECryptoMethod.PlainText) {
					return SteamPassword;
				}

				string decryptedPassword = ArchiCryptoHelper.Decrypt(PasswordFormat, SteamPassword);
				if (string.IsNullOrEmpty(decryptedPassword)) {
					ASF.ArchiLogger.LogGenericError(string.Format(Strings.ErrorIsInvalid, nameof(SteamPassword)));
					return null;
				}

				return decryptedPassword;
			}
			set {
				if (string.IsNullOrEmpty(value)) {
					ASF.ArchiLogger.LogNullError(nameof(value));
					return;
				}

				SteamPassword = PasswordFormat == ArchiCryptoHelper.ECryptoMethod.PlainText ? value : ArchiCryptoHelper.Encrypt(PasswordFormat, value);
			}
		}

		internal bool IsSteamLoginSet { get; private set; }
		internal bool IsSteamParentalCodeSet { get; private set; }
		internal bool IsSteamPasswordSet { get; private set; }
		internal bool ShouldSerializeEverything { private get; set; } = true;
		internal bool ShouldSerializeHelperProperties { private get; set; } = true;

		[JsonProperty]
		internal string SteamLogin {
			get => _SteamLogin;
			set {
				IsSteamLoginSet = true;
				_SteamLogin = value;
			}
		}

		[JsonProperty(Required = Required.DisallowNull)]
		internal ulong SteamMasterClanID { get; private set; } = DefaultSteamMasterClanID;

		[JsonProperty]
		internal string SteamParentalCode {
			get => _SteamParentalCode;
			set {
				IsSteamParentalCodeSet = true;
				_SteamParentalCode = value;
			}
		}

		[JsonProperty]
		internal string SteamPassword {
			get => _SteamPassword;
			set {
				IsSteamPasswordSet = true;
				_SteamPassword = value;
			}
		}

		private string _SteamLogin = DefaultSteamLogin;
		private string _SteamParentalCode = DefaultSteamParentalCode;
		private string _SteamPassword = DefaultSteamPassword;
		private bool ShouldSerializeSensitiveDetails = true;

		[JsonProperty(PropertyName = SharedInfo.UlongCompatibilityStringPrefix + nameof(SteamMasterClanID), Required = Required.DisallowNull)]
		private string SSteamMasterClanID {
			get => SteamMasterClanID.ToString();
			set {
				if (string.IsNullOrEmpty(value) || !ulong.TryParse(value, out ulong result)) {
					ASF.ArchiLogger.LogGenericError(string.Format(Strings.ErrorIsInvalid, nameof(SSteamMasterClanID)));
					return;
				}

				SteamMasterClanID = result;
			}
		}

		[JsonProperty]
		private string SteamParentalPIN {
			set {
				if (string.IsNullOrEmpty(value) || (value != "0")) {
					ASF.ArchiLogger.LogGenericWarning(string.Format(Strings.WarningDeprecated, nameof(SteamParentalPIN), nameof(SteamParentalCode)));
					SteamParentalCode = string.IsNullOrEmpty(value) ? "0" : value;
				}
			}
		}

		internal (bool Valid, string ErrorMessage) CheckValidation() {
			if (BotBehaviour > EBotBehaviour.All) {
				return (false, string.Format(Strings.ErrorConfigPropertyInvalid, nameof(BotBehaviour), BotBehaviour));
			}

			foreach (EFarmingOrder farmingOrder in FarmingOrders) {
				if (!Enum.IsDefined(typeof(EFarmingOrder), farmingOrder)) {
					return (false, string.Format(Strings.ErrorConfigPropertyInvalid, nameof(FarmingOrders), farmingOrder));
				}
			}

			if (GamesPlayedWhileIdle.Count > ArchiHandler.MaxGamesPlayedConcurrently) {
				return (false, string.Format(Strings.ErrorConfigPropertyInvalid, nameof(GamesPlayedWhileIdle), GamesPlayedWhileIdle.Count + " > " + ArchiHandler.MaxGamesPlayedConcurrently));
			}

			foreach (Steam.Asset.EType lootableType in LootableTypes) {
				if (!Enum.IsDefined(typeof(Steam.Asset.EType), lootableType)) {
					return (false, string.Format(Strings.ErrorConfigPropertyInvalid, nameof(LootableTypes), lootableType));
				}
			}

			foreach (Steam.Asset.EType matchableType in MatchableTypes) {
				if (!Enum.IsDefined(typeof(Steam.Asset.EType), matchableType)) {
					return (false, string.Format(Strings.ErrorConfigPropertyInvalid, nameof(MatchableTypes), matchableType));
				}
			}

			if ((OnlineStatus < EPersonaState.Offline) || (OnlineStatus >= EPersonaState.Max)) {
				return (false, string.Format(Strings.ErrorConfigPropertyInvalid, nameof(OnlineStatus), OnlineStatus));
			}

			if (!Enum.IsDefined(typeof(ArchiCryptoHelper.ECryptoMethod), PasswordFormat)) {
				return (false, string.Format(Strings.ErrorConfigPropertyInvalid, nameof(PasswordFormat), PasswordFormat));
			}

			if (RedeemingPreferences > ERedeemingPreferences.All) {
				return (false, string.Format(Strings.ErrorConfigPropertyInvalid, nameof(RedeemingPreferences), RedeemingPreferences));
			}

			if ((SteamMasterClanID != 0) && !new SteamID(SteamMasterClanID).IsClanAccount) {
				return (false, string.Format(Strings.ErrorConfigPropertyInvalid, nameof(SteamMasterClanID), SteamMasterClanID));
			}

			if (!string.IsNullOrEmpty(SteamParentalCode) && (SteamParentalCode != "0") && (SteamParentalCode.Length != 4)) {
				return (false, string.Format(Strings.ErrorConfigPropertyInvalid, nameof(SteamParentalCode), SteamParentalCode));
			}

			foreach (EPermission permission in SteamUserPermissions.Values.Where(permission => !Enum.IsDefined(typeof(EPermission), permission))) {
				return (false, string.Format(Strings.ErrorConfigPropertyInvalid, nameof(SteamUserPermissions), permission));
			}

			return TradingPreferences <= ETradingPreferences.All ? (true, null) : (false, string.Format(Strings.ErrorConfigPropertyInvalid, nameof(TradingPreferences), TradingPreferences));
		}

		internal static async Task<BotConfig> Load(string filePath) {
			if (string.IsNullOrEmpty(filePath)) {
				ASF.ArchiLogger.LogNullError(nameof(filePath));
				return null;
			}

			if (!File.Exists(filePath)) {
				return null;
			}

			BotConfig botConfig;

			try {
				botConfig = JsonConvert.DeserializeObject<BotConfig>(await RuntimeCompatibility.File.ReadAllTextAsync(filePath).ConfigureAwait(false));
			} catch (Exception e) {
				ASF.ArchiLogger.LogGenericException(e);
				return null;
			}

			if (botConfig == null) {
				ASF.ArchiLogger.LogNullError(nameof(botConfig));
				return null;
			}

			(bool valid, string errorMessage) = botConfig.CheckValidation();
			if (!valid) {
				ASF.ArchiLogger.LogGenericError(errorMessage);
				return null;
			}

			botConfig.ShouldSerializeEverything = false;
			botConfig.ShouldSerializeSensitiveDetails = false;
			return botConfig;
		}

		internal static async Task<bool> Write(string filePath, BotConfig botConfig) {
			if (string.IsNullOrEmpty(filePath) || (botConfig == null)) {
				ASF.ArchiLogger.LogNullError(nameof(filePath) + " || " + nameof(botConfig));
				return false;
			}

			string json = JsonConvert.SerializeObject(botConfig, Formatting.Indented);
			string newFilePath = filePath + ".new";

			await WriteSemaphore.WaitAsync().ConfigureAwait(false);

			try {
				await RuntimeCompatibility.File.WriteAllTextAsync(newFilePath, json).ConfigureAwait(false);

				if (File.Exists(filePath)) {
					File.Replace(newFilePath, filePath, null);
				} else {
					File.Move(newFilePath, filePath);
				}
			} catch (Exception e) {
				ASF.ArchiLogger.LogGenericException(e);
				return false;
			} finally {
				WriteSemaphore.Release();
			}

			return true;
		}

		[Flags]
		internal enum EBotBehaviour : byte {
			[SuppressMessage("ReSharper", "UnusedMember.Global")]
			None = 0,

			RejectInvalidFriendInvites = 1,
			RejectInvalidTrades = 2,
			RejectInvalidGroupInvites = 4,
			DismissInventoryNotifications = 8,
			MarkReceivedMessagesAsRead = 16,
			All = RejectInvalidFriendInvites | RejectInvalidTrades | RejectInvalidGroupInvites | DismissInventoryNotifications | MarkReceivedMessagesAsRead
		}

		internal enum EFarmingOrder : byte {
			Unordered,
			AppIDsAscending,
			AppIDsDescending,
			CardDropsAscending,
			CardDropsDescending,
			HoursAscending,
			HoursDescending,
			NamesAscending,
			NamesDescending,
			Random,
			BadgeLevelsAscending,
			BadgeLevelsDescending,
			RedeemDateTimesAscending,
			RedeemDateTimesDescending,
			MarketableAscending,
			MarketableDescending
		}

		internal enum EPermission : byte {
			None,
			FamilySharing,
			Operator,
			Master
		}

		[Flags]
		internal enum ERedeemingPreferences : byte {
			[SuppressMessage("ReSharper", "UnusedMember.Global")]
			None = 0,

			Forwarding = 1,
			Distributing = 2,
			KeepMissingGames = 4,
			All = Forwarding | Distributing | KeepMissingGames
		}

		[Flags]
		internal enum ETradingPreferences : byte {
			[SuppressMessage("ReSharper", "UnusedMember.Global")]
			None = 0,

			AcceptDonations = 1,
			SteamTradeMatcher = 2,
			MatchEverything = 4,
			DontAcceptBotTrades = 8,
			All = AcceptDonations | SteamTradeMatcher | MatchEverything | DontAcceptBotTrades
		}

		// ReSharper disable UnusedMember.Global
		public bool ShouldSerializeAcceptGifts() => ShouldSerializeEverything || (AcceptGifts != DefaultAcceptGifts);
		public bool ShouldSerializeAutoSteamSaleEvent() => ShouldSerializeEverything || (AutoSteamSaleEvent != DefaultAutoSteamSaleEvent);
		public bool ShouldSerializeBotBehaviour() => ShouldSerializeEverything || (BotBehaviour != DefaultBotBehaviour);
		public bool ShouldSerializeCustomGamePlayedWhileFarming() => ShouldSerializeEverything || (CustomGamePlayedWhileFarming != DefaultCustomGamePlayedWhileFarming);
		public bool ShouldSerializeCustomGamePlayedWhileIdle() => ShouldSerializeEverything || (CustomGamePlayedWhileIdle != DefaultCustomGamePlayedWhileIdle);
		public bool ShouldSerializeEnabled() => ShouldSerializeEverything || (Enabled != DefaultEnabled);
		public bool ShouldSerializeFarmingOrders() => ShouldSerializeEverything || ((FarmingOrders != DefaultFarmingOrders) && !FarmingOrders.SequenceEqual(DefaultFarmingOrders));
		public bool ShouldSerializeGamesPlayedWhileIdle() => ShouldSerializeEverything || ((GamesPlayedWhileIdle != DefaultGamesPlayedWhileIdle) && !GamesPlayedWhileIdle.SetEquals(DefaultGamesPlayedWhileIdle));
		public bool ShouldSerializeHoursUntilCardDrops() => ShouldSerializeEverything || (HoursUntilCardDrops != DefaultHoursUntilCardDrops);
		public bool ShouldSerializeIdlePriorityQueueOnly() => ShouldSerializeEverything || (IdlePriorityQueueOnly != DefaultIdlePriorityQueueOnly);
		public bool ShouldSerializeIdleRefundableGames() => ShouldSerializeEverything || (IdleRefundableGames != DefaultIdleRefundableGames);
		public bool ShouldSerializeLootableTypes() => ShouldSerializeEverything || ((LootableTypes != DefaultLootableTypes) && !LootableTypes.SetEquals(DefaultLootableTypes));
		public bool ShouldSerializeMatchableTypes() => ShouldSerializeEverything || ((MatchableTypes != DefaultMatchableTypes) && !MatchableTypes.SetEquals(DefaultMatchableTypes));
		public bool ShouldSerializeOnlineStatus() => ShouldSerializeEverything || (OnlineStatus != DefaultOnlineStatus);
		public bool ShouldSerializePasswordFormat() => ShouldSerializeEverything || (PasswordFormat != DefaultPasswordFormat);
		public bool ShouldSerializePaused() => ShouldSerializeEverything || (Paused != DefaultPaused);
		public bool ShouldSerializeRedeemingPreferences() => ShouldSerializeEverything || (RedeemingPreferences != DefaultRedeemingPreferences);
		public bool ShouldSerializeSendOnFarmingFinished() => ShouldSerializeEverything || (SendOnFarmingFinished != DefaultSendOnFarmingFinished);
		public bool ShouldSerializeSendTradePeriod() => ShouldSerializeEverything || (SendTradePeriod != DefaultSendTradePeriod);
		public bool ShouldSerializeShutdownOnFarmingFinished() => ShouldSerializeEverything || (ShutdownOnFarmingFinished != DefaultShutdownOnFarmingFinished);
		public bool ShouldSerializeSSteamMasterClanID() => ShouldSerializeEverything || (ShouldSerializeHelperProperties && (SteamMasterClanID != DefaultSteamMasterClanID));
		public bool ShouldSerializeSteamLogin() => ShouldSerializeSensitiveDetails && (ShouldSerializeEverything || (SteamLogin != DefaultSteamLogin));
		public bool ShouldSerializeSteamMasterClanID() => ShouldSerializeEverything || (SteamMasterClanID != DefaultSteamMasterClanID);
		public bool ShouldSerializeSteamParentalCode() => ShouldSerializeSensitiveDetails && (ShouldSerializeEverything || (SteamParentalCode != DefaultSteamParentalCode));
		public bool ShouldSerializeSteamPassword() => ShouldSerializeSensitiveDetails && (ShouldSerializeEverything || (SteamPassword != DefaultSteamPassword));
		public bool ShouldSerializeSteamTradeToken() => ShouldSerializeEverything || (SteamTradeToken != DefaultSteamTradeToken);
		public bool ShouldSerializeSteamUserPermissions() => ShouldSerializeEverything || ((SteamUserPermissions != DefaultSteamUserPermissions) && ((SteamUserPermissions.Count != DefaultSteamUserPermissions.Count) || SteamUserPermissions.Except(DefaultSteamUserPermissions).Any()));
		public bool ShouldSerializeTradingPreferences() => ShouldSerializeEverything || (TradingPreferences != DefaultTradingPreferences);
		public bool ShouldSerializeTransferableTypes() => ShouldSerializeEverything || ((TransferableTypes != DefaultTransferableTypes) && !TransferableTypes.SetEquals(DefaultTransferableTypes));
		public bool ShouldSerializeUseLoginKeys() => ShouldSerializeEverything || (UseLoginKeys != DefaultUseLoginKeys);

		// ReSharper restore UnusedMember.Global
	}
}
