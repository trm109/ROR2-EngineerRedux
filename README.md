# Engineer Redux

**! Almost definitely incompatible with any mod that adds/edits Engineer's primary or special (also the Engi Turret's primary) !**

Does a few things:
- Gives Engineer access to turret primaries.
- Gives turrets access to engineer primary.
- Makes turret primaries independent from their mobility type (mobile can use machine gun, and stationary can use laser beam).
- Buffs the laser beam range significantly (25 -> 300m).
- Buffs laser beam proc coefficient (.6 -> 1)

Need to implement later:
- Configs.
- RiskOfOptions Compat.
- Change the head of the turret based on selected turret primary.
- Documentation for adding your own Turret skills.
- Add mod compatibility.
- Add my own asset references instead of grabbing them at runtime.
- AI improvements.
    - Increase max range.
    - Make mobile turrets not stupid.
    - Some target priority logic.
- EngineerReduxExtended:
    - 'Drone' turret body type.
    - Flamethrower weapon
    - Railgun weapon
    - Buzzsaw weapon
    - Shotgun weapon
- GitHub Action to build and publish to thunderstore.
- Add linter and formatter to pre-commit.

## For Developers
To build the project yourself:
1. Install [Nix](https://nixos.org/download/) (the package manager)
2. Install [Devenv](https://devenv.sh/getting-started/#1-install-nix)
3. Modify the mod profile path in `./devenv.nix` to match your developer mod profile.
4. While in the repository, run `devenv shell` to activate the developer shell.
5. type `build` in the terminal to build the project. The output `.dll` file should be symlinked to your mod profile, so just hit "launch modded game" in whatever modmanager you use.
