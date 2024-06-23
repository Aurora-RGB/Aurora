using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.CSGO.GSI.Nodes;

/// <summary>
/// Class representing information about player's weapons
/// </summary>
[method: JsonConstructor]
public class WeaponsNode(Dictionary<string, WeaponNode> weaponNodes)
{
    public static readonly WeaponsNode Default = new([]);

    private const string WeaponTaser = "weapon_taser";
    private const string WeaponHegrenade = "weapon_hegrenade";
    private const string WeaponFlashbang = "weapon_flashbang";
    private const string WeaponSmokegrenade = "weapon_smokegrenade";
    private const string WeaponDecoy = "weapon_decoy";
    private const string WeaponMolotov = "weapon_molotov";

    private readonly List<WeaponNode> _weapons = weaponNodes.Values.ToList();

    /// <summary>
    /// The number of weapons a player has in their inventory
    /// </summary>
    public int Count => _weapons.Count;

    /// <summary>
    /// Player's currently active weapon
    /// </summary>
    public WeaponNode ActiveWeapon
    {
        get
        {
            return _weapons.Where(w => w.State is WeaponState.Active or WeaponState.Reloading)
                .FirstOrDefault(WeaponNode.Default);
        }
    }

    public bool HasPrimary => _weapons.Exists(w => w.Type is WeaponType.Rifle or WeaponType.MachineGun or WeaponType.SniperRifle or WeaponType.SubmachineGun or WeaponType.Shotgun);
    public bool HasRifle => _weapons.Exists(w => w.Type == WeaponType.Rifle);
    public bool HasMachineGun => _weapons.Exists(w => w.Type == WeaponType.MachineGun);
    public bool HasShotgun => _weapons.Exists(w => w.Type == WeaponType.Shotgun);
    public bool HasSniper => _weapons.Exists(w => w.Type == WeaponType.SniperRifle);
    public bool HasKnife => _weapons.Exists(w => w.Type == WeaponType.Knife);
    public bool HasTaser => _weapons.Exists(w => w.Name == WeaponTaser);
    public bool HasSMG => _weapons.Exists(w => w.Type == WeaponType.SubmachineGun);
    public bool HasPistol => _weapons.Exists(w => w.Type == WeaponType.Pistol);
    public bool HasC4 => _weapons.Exists(w => w.Type == WeaponType.C4);
    public bool HasGrenade => _weapons.Exists(w => w.Type == WeaponType.Grenade);
    public bool HasHegrenade => _weapons.Exists(w => w.Name == WeaponHegrenade);
    public bool HasFlashbang => _weapons.Exists(w => w.Name == WeaponFlashbang);
    public bool HasSmoke => _weapons.Exists(w => w.Name == WeaponSmokegrenade);
    public bool HasDecoy => _weapons.Exists(w => w.Name == WeaponDecoy);
    public bool HasIncendiary => _weapons.Exists(w => w.Name == WeaponMolotov);
    public int GrenadeCount => _weapons.Sum(w => w.Type == WeaponType.Grenade ? 1 : 0);
}