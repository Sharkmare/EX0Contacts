using System;
using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;
using SkyFrost.Base;

namespace EX0Contacts;

public class EX0Contacts : ResoniteMod
{
    public override string Name => "EX0Contacts";
    public override string Author => "Sharkmare / EX0";
    public override string Version => "1.0.0";
    public override string Link => "https://github.com/Sharkmare/EX0Contacts";

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> KEY_ONLINE = new(
        "OnlineOrder", "Sort priority for Online status (lower = higher in list)",
        () => 0);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> KEY_SOCIABLE = new(
        "SociableOrder", "Sort priority for Sociable status",
        () => 0);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> KEY_AWAY = new(
        "AwayOrder", "Sort priority for Away status",
        () => 3);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> KEY_BUSY = new(
        "BusyOrder", "Sort priority for Busy status",
        () => 4);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> KEY_OFFLINE = new(
        "OfflineOrder", "Sort priority for Offline/Invisible",
        () => 6);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> KEY_HEADLESS = new(
        "HeadlessOrder", "Sort priority for Headless session accounts",
        () => 5);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> KEY_REQUESTED = new(
        "RequestedOrder", "Sort priority for incoming contact requests",
        () => 2);

    internal static ModConfiguration? Config;

    public override void OnEngineInit()
    {
        Config = GetConfiguration();
        Config.Save(true);

        Harmony harmony = new("com.ex0.contacts");
        harmony.PatchAll();

        Msg("EX0Contacts loaded - contact ordering is now configurable");
    }

    [HarmonyPatch(typeof(ContactsDialog), "GetOrderNumber")]
    private static class Patch_GetOrderNumber
    {
        static bool Prefix(ContactItem item, ref int __result)
        {
            if (Config == null)
                return true;

            if (item.Contact.IsPartiallyMigrated)
            {
                __result = 10;
                return false;
            }

            if (item.Contact.ContactStatus == ContactStatus.Requested)
            {
                __result = Config.GetValue(KEY_REQUESTED);
                return false;
            }

            var sessionType = item.Data?.CurrentStatus?.SessionType;
            if (sessionType == UserSessionType.Headless)
            {
                __result = Config.GetValue(KEY_HEADLESS);
                return false;
            }

            __result = (item.Data?.CurrentStatus?.OnlineStatus).GetValueOrDefault() switch
            {
                OnlineStatus.Online   => Config.GetValue(KEY_ONLINE),
                OnlineStatus.Sociable => Config.GetValue(KEY_SOCIABLE),
                OnlineStatus.Away     => Config.GetValue(KEY_AWAY),
                OnlineStatus.Busy     => Config.GetValue(KEY_BUSY),
                _                     => Config.GetValue(KEY_OFFLINE),
            };

            return false; // skip original method
        }
    }
}
