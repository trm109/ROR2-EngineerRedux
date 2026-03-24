# Engineer Redux

**! Almost definitely incompatible with any mod that adds/edits Engineer's special (and maybe also Engi & Turret's primary) !**

_For questions/concerns, make an issue on the GitHub, or contact me on Discord (\_s.k)_

_Massive thanks to **Omeletttte** for the skill icons!_

Ever wished you could have Mobile Turrets with bullets instead of lasers? Ever wished the engineer could use lasers instead of grenades? Ever wished your turrets could shoot grenades?

This is the mod for you!

**I highly recommend using [ProcPatcher](https://thunderstore.io/c/riskofrain2/p/RiskOfBrainrot/ProcPatcher/) alongside this mod so the laser attack isn't at a massive disadvantage with items like Ukelele and ATG**

## What it does:
### Gives Engineer access to turret primaries:

#### Gauss
![Engi Gauss Primary Icon](https://raw.githubusercontent.com/trm109/ROR2-EngineerRedux/refs/heads/master/EngineerRedux/Assets/SkillIcons/EngineerGauss.png)

- Fires bullets from both barrels, dealing **70%** damage, **6** times per second.
- PEW PEW.
- As usual, has damage falloff.
---
#### Laser
![Engi Laser Primary Icon](https://raw.githubusercontent.com/trm109/ROR2-EngineerRedux/refs/heads/master/EngineerRedux/Assets/SkillIcons/EngineerLaser.png)

- Fires two continuous lasers from both barrels, dealing **40%** damage, **10** times per second.
- Enemies hit by the laser are slowed by **50%**.
- **0.6** Proc Coeff (Default)
- Max range of **300m**
---

### Gives turrets access to engineer primaries:
#### Grenades
![Turret Grenade Icon](https://raw.githubusercontent.com/trm109/ROR2-EngineerRedux/refs/heads/master/EngineerRedux/Assets/SkillIcons/TurretGrenade.png)

- Turrets charge up **4** grenades, dealing **100%** damage each.
- Turrets are un-burdened of thought and have trouble aiming at fast-moving targets and compensating for gravity.
    - Be nice, it is their first day on the job.
---

#### Gauss
![Turret Gauss Icon](https://raw.githubusercontent.com/trm109/ROR2-EngineerRedux/refs/heads/master/EngineerRedux/Assets/SkillIcons/TurretGauss.png)

- Turrets fire bullets, dealing **70%** damage **3** times per second.
- unchanged from the default.
- As usual, has damage falloff.
---

#### Laser
![Turret Laser Icon](https://raw.githubusercontent.com/trm109/ROR2-EngineerRedux/refs/heads/master/EngineerRedux/Assets/SkillIcons/TurretLaser.png)

- Turrets fire a continuous laser, dealing **40%** damage, **5** times per second.
- Enemies hit by the laser are slowed by **50%**.
- **0.6** Proc Coeff (Default)
- Max range of **300m**
---

### Other changes:
- Makes turret primaries independent from their mobility type (mobile can use machine gun, and stationary can use laser beam).
- Buffs Mobile turrets' speed **(7 -> 8)** to keep up with Engineer.
- Buffs the Stationary turrets' base defensive stats by **50%**.
    - This is to make Stationary turrets not get 2 shot in higher difficulty scaling, and works as a tradeoff for the mobility.
- **Adds an extensible library for other mod devs to add customizations to Engi's Turret**

## Future Plans (Devs, feel free to help out!):
- Configs.
- RiskOfOptions Compat.
- AI improvements.
    - Increase max range.
    - Make mobile turrets not stupid.
    - Some target priority logic.
- Change the head of the turret based on selected turret primary.
- Fix skill names (needs to be human readable).
- Make Turret Skins independent of Engineer Skins.
- Add mod compatibility for some popular mods.
- EngineerReduxExtended:
    - Turret Body Types
        - 'Drone'
    - Weapons
        - Flamethrower
        - Railgun
        - Buzzsaw
        - Shotgun
- GitHub Action to build and publish to thunderstore.

## For Developers
To build the project yourself:
1. Install [Nix](https://nixos.org/download/) (the package manager)
2. Install [Devenv](https://devenv.sh/getting-started/#1-install-nix)
3. Modify the mod profile path in `./devenv.nix` to match your developer mod profile.
4. While in the repository, run `devenv shell` to activate the developer shell.or install [direnv](https://direnv.net/docs/installation.html) and run `direnv allow` to automatically enter the shell every time you open the repo.
5. type `build` in the terminal to build the project. The output `.dll` file should be symlinked to your mod profile, so just hit "launch modded game" in whatever modmanager you use.
