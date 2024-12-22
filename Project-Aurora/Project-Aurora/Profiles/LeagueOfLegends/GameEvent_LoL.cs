using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AuroraRgb.Modules;
using AuroraRgb.Profiles.LeagueOfLegends.GSI;
using AuroraRgb.Profiles.LeagueOfLegends.GSI.Nodes;
using AuroraRgb.Utils;
using Common.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.LeagueOfLegends;

public sealed class GameEventLoL : LightEvent
{
    private static readonly Uri Uri = new("https://127.0.0.1:2999/liveclientdata/allgamedata");

    private readonly HttpClient _client;
    private readonly SingleConcurrentThread _updateThread;

    private _RootGameData? _allGameData;
    private bool _updatedOnce;
    private CancellationTokenSource _cancellationTokenSource = new();

    public GameEventLoL()
    {
        //ignore ssl errors
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        _client = new HttpClient(handler);
        _updateThread = new SingleConcurrentThread("LoL update thread", UpdateData, ExceptionCallback);
    }

    private void ExceptionCallback(object? arg1, SingleThreadExceptionEventArgs arg2)
    {
        Global.logger.Error(arg2.Exception, "Error updating LoL data");
        _updateThread.Trigger();
    }

    public override void OnStart()
    {
        var cancelSource = _cancellationTokenSource;
        _cancellationTokenSource = new CancellationTokenSource();
        cancelSource.Dispose();
        _updateThread.Trigger();
    }

    public override void OnStop()
    {
        var cancelSource = _cancellationTokenSource;
        cancelSource.Cancel();
    }

    public override void UpdateTick()
    {
        if (!_updatedOnce)
            return;

        var s = GameState as GameState_LoL;

        if (_allGameData == null)
        {
            GameState = new GameState_LoL();
            return;
        }

        try
        {
            #region Match

            s.Match.InGame = true;
            //s.Match.GameMode = EnumUtils.TryParseOr(allGameData.gameData.gameMode, true, GameMode.Unknown);
            s.Match.GameMode = _allGameData.gameData.gameMode;
            s.Match.GameTime = _allGameData.gameData.gameTime;

            #endregion

            #region Player

            var ap = _allGameData.activePlayer;
            s.Player.Level = ap.level;
            s.Player.Gold = ap.currentGold;
            s.Player.SummonerName = ap.summonerName;

            #region Abilities

            s.Player.Abilities.Q.Level = ap.abilities.Q.abilityLevel;
            s.Player.Abilities.Q.Name = ap.abilities.Q.displayName;
            s.Player.Abilities.W.Level = ap.abilities.W.abilityLevel;
            s.Player.Abilities.W.Name = ap.abilities.W.displayName;
            s.Player.Abilities.E.Level = ap.abilities.E.abilityLevel;
            s.Player.Abilities.E.Name = ap.abilities.E.displayName;
            s.Player.Abilities.R.Level = ap.abilities.R.abilityLevel;
            s.Player.Abilities.R.Name = ap.abilities.R.displayName;

            #endregion

            #region Stats

            s.Player.ChampionStats.AbilityPower = ap.championStats.abilityPower;
            s.Player.ChampionStats.Armor = ap.championStats.armor;
            s.Player.ChampionStats.ArmorPenetrationFlat = ap.championStats.armorPenetrationFlat;
            s.Player.ChampionStats.ArmorPenetrationPercent = ap.championStats.armorPenetrationPercent;
            s.Player.ChampionStats.AttackDamage = ap.championStats.attackDamage;
            s.Player.ChampionStats.AttackRange = ap.championStats.attackRange;
            s.Player.ChampionStats.AttackSpeed = ap.championStats.attackSpeed;
            s.Player.ChampionStats.BonusArmorPenetrationPercent = ap.championStats.bonusArmorPenetrationPercent;
            s.Player.ChampionStats.BonusMagicPenetrationPercent = ap.championStats.bonusMagicPenetrationPercent;
            s.Player.ChampionStats.CooldownReduction = ap.championStats.cooldownReduction;
            s.Player.ChampionStats.CritChance = ap.championStats.critChance;
            s.Player.ChampionStats.CritDamagePercent = ap.championStats.critDamage;
            s.Player.ChampionStats.HealthCurrent = ap.championStats.currentHealth;
            s.Player.ChampionStats.HealthRegenRate = ap.championStats.healthRegenRate;
            s.Player.ChampionStats.LifeSteal = ap.championStats.lifeSteal;
            s.Player.ChampionStats.MagicLethality = ap.championStats.magicLethality;
            s.Player.ChampionStats.MagicPenetrationFlat = ap.championStats.magicPenetrationFlat;
            s.Player.ChampionStats.MagicPenetrationPercent = ap.championStats.magicPenetrationPercent;
            s.Player.ChampionStats.MagicResist = ap.championStats.magicResist;
            s.Player.ChampionStats.HealthMax = ap.championStats.maxHealth;
            s.Player.ChampionStats.MoveSpeed = ap.championStats.moveSpeed;
            s.Player.ChampionStats.PhysicalLethality = ap.championStats.physicalLethality;
            s.Player.ChampionStats.ResourceMax = ap.championStats.resourceMax;
            s.Player.ChampionStats.ResourceRegenRate = ap.championStats.resourceRegenRate;
            s.Player.ChampionStats.ResourceType = EnumUtils.TryParseOr(ap.championStats.resourceType, true, ResourceType.Unknown);
            s.Player.ChampionStats.ResourceCurrent = ap.championStats.resourceValue;
            s.Player.ChampionStats.SpellVamp = ap.championStats.spellVamp;
            s.Player.ChampionStats.Tenacity = ap.championStats.tenacity;

            #endregion

            #region Runes

            //TODO

            #endregion

            #region allPlayer data

            //there's some data in allPlayers about the user that is not contained in activePlayer...
            var p = _allGameData.allPlayers.FirstOrDefault(a => a.summonerName == ap.summonerName);
            if (p == null)
                return;
            //if we can't find it, skip

            s.Player.Champion = EnumUtils.TryParseOr(p.championName.Replace(" ", "").Replace("'", "").Replace(".", ""), true, Champion.Unknown);
            s.Player.SpellD = EnumUtils.TryParseOr(p.summonerSpells.summonerSpellOne.displayName.Replace(" ", ""), true, SummonerSpell.Unknown);
            s.Player.SpellF = EnumUtils.TryParseOr(p.summonerSpells.summonerSpellTwo.displayName.Replace(" ", ""), true, SummonerSpell.Unknown);
            s.Player.Team = EnumUtils.TryParseOr(p.team, true, Team.Unknown);
            s.Player.Position = EnumUtils.TryParseOr(p.position, true, Position.Unknown);

            s.Player.IsDead = p.isDead;
            s.Player.RespawnTimer = p.respawnTimer;
            s.Player.Kills = p.scores.kills;
            s.Player.Deaths = p.scores.deaths;
            s.Player.Assists = p.scores.assists;
            s.Player.CreepScore = p.scores.creepScore;
            s.Player.WardScore = p.scores.wardScore;

            #endregion

            #region Events

            var drags = _allGameData.events.Events.OfType<_DragonKillEvent>();

            s.Match.InfernalDragonsKilled = drags.Count(d => d.DragonType.ToLower() == "fire");
            s.Match.EarthDragonsKilled = drags.Count(d => d.DragonType.ToLower() == "earth");
            s.Match.OceanDragonsKilled = drags.Count(d => d.DragonType.ToLower() == "water");
            s.Match.CloudDragonsKilled = drags.Count(d => d.DragonType.ToLower() == "air");
            s.Match.ChemtechDragonsKilled = drags.Count(d => d.DragonType.ToLower() == "chemtech");
            s.Match.HextechDragonsKilled = drags.Count(d => d.DragonType.ToLower() == "hextech");
            s.Match.ElderDragonsKilled = drags.Count(d => d.DragonType.ToLower() == "elder");

            s.Match.BaronsKilled = _allGameData.events.Events.Count(ev => ev is _BaronKillEvent);
            s.Match.HeraldsKilled = _allGameData.events.Events.Count(ev => ev is _HeraldKillEvent);
            s.Match.DragonsKilled = _allGameData.events.Events.Count(ev => ev is _DragonKillEvent);
            s.Match.TurretsKilled = _allGameData.events.Events.Count(ev => ev is _TurretKillEvent);
            s.Match.InhibsKilled = _allGameData.events.Events.Count(ev => ev is _InhibKillEvent);
            s.Match.MapTerrain = EnumUtils.TryParseOr(_allGameData.gameData.mapTerrain, true, MapTerrain.Unknown);

            #endregion

            #region Items

            s.Player.Items.Slot1 = GetItem(p, 0);
            s.Player.Items.Slot2 = GetItem(p, 1);
            s.Player.Items.Slot3 = GetItem(p, 2);
            s.Player.Items.Slot4 = GetItem(p, 3);
            s.Player.Items.Slot5 = GetItem(p, 4);
            s.Player.Items.Slot6 = GetItem(p, 5);
            s.Player.Items.Trinket = GetItem(p, 6);

            #endregion

            #endregion
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Error updating LoL game event");
        }
    }

    /// <summary>
    /// Tries to find an item from the nth <paramref name="slot"/>. Returns the item if it finds it, or an empty item otherwise.
    /// </summary>
    /// <param name="p"></param>
    /// <param name="slot"></param>
    /// <returns></returns>
    private static SlotNode GetItem(_AllPlayer p, int slot)
    {
        var newItem = p.items.FirstOrDefault(item => item.slot == slot);

        return newItem == null ? new SlotNode() : new SlotNode(newItem);
    }

    private async Task UpdateData()
    {
        if (!(await ProcessesModule.RunningProcessMonitor).IsProcessRunning("league of legends.exe"))
        {
            _allGameData = null;
            return;
        }

        var jsonData = "";
        using var res = await _client.GetAsync(Uri, _cancellationTokenSource.Token);
        if (res.IsSuccessStatusCode)
            jsonData = await res.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(jsonData) || jsonData.Contains("error"))
        {
            _allGameData = null;
            _updateThread.Trigger();
            return;
        }

        _allGameData = JsonConvert.DeserializeObject<_RootGameData>(jsonData);
        _updatedOnce = true;
        _updateThread.Trigger();
    }

    public override void Dispose()
    {
        base.Dispose();

        _client.Dispose();
        _updateThread.Dispose(200);
    }
}