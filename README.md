# Engineer Redux

**! Almost definitely incompatible with any mod that adds/edits Engineer's primary or special (also the Engi Turret's primary) !**

_For questions/concerns, make an issue on the GitHub, or contact me on Discord (\_s.k)_

Ever wished you could have Mobile Turrets with bullets instead of lasers? Ever wished the engineer could use lasers instead of grenades? Ever wished your turrets could shoot grenades?

This is the mod for you!

Does a few things:
- Gives Engineer access to turret primaries.
- Gives turrets access to engineer primary.
- Makes turret primaries independent from their mobility type (mobile can use machine gun, and stationary can use laser beam).
- Buffs the laser beam range significantly (25 -> 300m).
- Buffs laser beam proc coefficient (.6 -> 1)
- Buffs Mobile turrets' speed (7 -> 8) to keep up with Engineer.
- Buffs the Stationary turrets' base defensive stats by 50% + 30 armor.
    - This is to make Stationary turrets not get 2 shot in higher difficulty scaling, and works as a tradeoff for the mobility.
- **Adds an extensible library for other mod devs to add customizations to Engi's Turret**

Future Plans (Devs, feel free to help out!):
- Configs.
- RiskOfOptions Compat.
- AI improvements.
    - Increase max range.
    - Make mobile turrets not stupid.
    - Some target priority logic.
- Change the head of the turret based on selected turret primary.
- Fix skill names (needs to be human readable).
- Make Turret Skins independent of Engineer Skins.
- Add mod compatibility.
- Add my own asset references instead of grabbing them at runtime.
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
