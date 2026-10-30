using System.Diagnostics;
using Steamworks;

internal static class Program
{
    private const uint AmongUsAppId = 945360;
    private static bool statsReceived;
    private static bool statsSucceeded;
    private static bool storeReceived;
    private static bool storeSucceeded;
    private static Callback<UserStatsReceived_t>? statsCallback;
    private static Callback<UserStatsStored_t>? storeCallback;

    private static int Main(string[] args)
    {
        Console.WriteLine("Among Us - Steam Achievement Unlocker (experimental)");
        Console.WriteLine("No gameplay modification. Only works if Steam authorizes the app context.\n");
        if (args.Length > 1 || (args.Length == 1 && args[0] is not ("--list" or "--unlock")))
        {
            Console.Error.WriteLine("Usage: AmongUsAchievementUnlocker [--list|--unlock]");
            return 2;
        }

        if (!SteamAPI.Init())
        {
            Console.Error.WriteLine("SteamAPI.Init failed. Ensure Steam is running, you own Among Us, and steam_appid.txt is beside this executable.");
            return 1;
        }

        try
        {
            var actualAppId = SteamUtils.GetAppID().m_AppId;
            if (actualAppId != AmongUsAppId)
            {
                Console.Error.WriteLine($"Incorrect Steam app context: {actualAppId}; expected {AmongUsAppId}. No changes made.");
                return 1;
            }

            statsCallback = Callback<UserStatsReceived_t>.Create(result =>
            {
                if (result.m_nGameID == AmongUsAppId)
                {
                    statsReceived = true;
                    statsSucceeded = result.m_eResult == EResult.k_EResultOK;
                }
            });
            storeCallback = Callback<UserStatsStored_t>.Create(result =>
            {
                if (result.m_nGameID == AmongUsAppId)
                {
                    storeReceived = true;
                    storeSucceeded = result.m_eResult == EResult.k_EResultOK;
                }
            });

            if (!SteamUserStats.RequestCurrentStats() || !Poll(() => statsReceived, TimeSpan.FromSeconds(20)) || !statsSucceeded)
            {
                Console.Error.WriteLine("Steam did not return successful user stats. No changes made.");
                return 1;
            }

            var total = SteamUserStats.GetNumAchievements();
            if (total is < 1 or > 1000)
            {
                Console.Error.WriteLine($"Unexpected number of achievements ({total}). No changes made.");
                return 1;
            }

            var locked = new List<string>();
            for (uint i = 0; i < total; i++)
            {
                var id = SteamUserStats.GetAchievementName(i);
                if (string.IsNullOrWhiteSpace(id) || !SteamUserStats.GetAchievement(id, out var unlocked))
                {
                    Console.Error.WriteLine($"Unable to read achievement {i}. No changes made.");
                    return 1;
                }
                Console.WriteLine($"{(unlocked ? "[UNLOCKED]" : "[ LOCKED ]")} {id}");
                if (!unlocked) locked.Add(id);
            }

            Console.WriteLine($"\nTotal: {total}; locked: {locked.Count}");
            if (locked.Count == 0 || args.Contains("--list")) return 0;

            if (!args.Contains("--unlock"))
                Console.WriteLine("Preview only. Use --unlock to enable confirmation prompt.");
            if (!args.Contains("--unlock")) return 0;

            Console.Write($"Type UNLOCK to attempt to unlock {locked.Count} achievement(s): ");
            if (Console.ReadLine() != "UNLOCK")
            {
                Console.WriteLine("Cancelled. No changes made.");
                return 0;
            }

            var applied = 0;
            foreach (var id in locked)
            {
                if (SteamUserStats.SetAchievement(id)) applied++;
                else Console.Error.WriteLine($"SetAchievement rejected: {id}");
            }
            if (applied == 0)
            {
                Console.Error.WriteLine("No achievements were accepted by Steam.");
                return 1;
            }

            if (!SteamUserStats.StoreStats())
            {
                Console.Error.WriteLine("StoreStats returned false. Steam may not have saved the changes.");
                return 1;
            }
            if (!Poll(() => storeReceived, TimeSpan.FromSeconds(20)) || !storeSucceeded)
            {
                Console.Error.WriteLine("Steam did not confirm saving stats. Check your Steam achievements page.");
                return 1;
            }
            Console.WriteLine($"Steam confirmed StoreStats. Unlock requests submitted: {applied}. Verify in Steam.");
            return 0;
        }
        catch (DllNotFoundException ex)
        {
            Console.Error.WriteLine("Steam native library not found: " + ex.Message);
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error: " + ex.Message);
            return 1;
        }
        finally
        {
            storeCallback?.Dispose();
            statsCallback?.Dispose();
            SteamAPI.Shutdown();
        }
    }

    private static bool Poll(Func<bool> done, TimeSpan timeout)
    {
        var timer = Stopwatch.StartNew();
        while (!done() && timer.Elapsed < timeout)
        {
            SteamAPI.RunCallbacks();
            System.Threading.Thread.Sleep(50);
        }
        return done();
    }
}
