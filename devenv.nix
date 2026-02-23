{
  pkgs,
  lib,
  config,
  inputs,
  ...
}:
rec {
  # https://devenv.sh/basics/
  # env.GREET = "devenv";
  env.R2_PROFILE = "dev";
  # env.PLUGINS_LOCATION = "$HOME/.config/r2modmanPlus-local/RiskOfRain2/profiles/${env.R2_PROFILE}/BepInEx/plugins";
  env.PLUGINS_LOCATION = "$HOME/.local/share/com.kesomannen.gale/riskofrain2/profiles/${env.R2_PROFILE}/BepInEx/plugins";
  env.EXPORT_LOCATION = "${env.PLUGINS_LOCATION}/EngineerRedux";

  # https://devenv.sh/packages/
  packages = [
    pkgs.git
    pkgs.ilspycmd
    pkgs.dotnet-outdated
    pkgs.zip
  ];

  # https://devenv.sh/languages/
  # languages.rust.enable = true;
  languages.dotnet = {
    enable = true;
    package = pkgs.dotnet-sdk_9;
    # lsp.enable = true;
  };

  # https://devenv.sh/processes/
  # processes.dev.exec = "${lib.getExe pkgs.watchexec} -n -- ls -la";

  # https://devenv.sh/services/
  # services.postgres.enable = true;

  # https://devenv.sh/scripts/
  # scripts.hello.exec = ''
  #   echo hello from $GREET
  # '';
  scripts = {
    update.exec = ''
      cd ${config.devenv.root}
      devenv update
      dotnet outdated --upgrade --exclude 'UnityEngine.Modules' --exclude 'BepInEx.Core' --exclude 'BepInEx.Analyzers' EngineerRedux/EngineerRedux.csproj
      cd -
    '';
    build.exec = ''
      cd ${config.devenv.root}
      echo "Building project"
      dotnet build EngineerRedux/EngineerRedux.csproj -o Output
      if [ $? -ne 0 ]; then
        echo "Build failed"
        exit 1
      else
        echo "Build succeeded"
      fi

      echo "Ensuring symlink to ${env.EXPORT_LOCATION}"
      rm ${env.EXPORT_LOCATION}
      ln -sf $(pwd)/Output ${env.EXPORT_LOCATION}
      if [ $? -ne 0 ]; then
        echo "Failed to create symlink"
        exit 1
      else
        echo "Symlink created successfully"
      fi
      cd -
    '';
    package.exec = ''
      cd ${config.devenv.root}
      build
      if [ $? -ne 0 ]; then
        echo "Build failed, skipping packaging"
        exit 1
      fi
      echo "Packaging project"
      mkdir -p Thunderstore
      rm -rf Thunderstore/*
      cp -r Output/* Thunderstore/
      cp README.md Thunderstore/
      cp -r Meta/* Thunderstore/
      cd Thunderstore/
      zip -r EngineerRedux.zip *
      cd -
    '';
  };

  # https://devenv.sh/basics/
  # enterShell = ''
  #   hello         # Run scripts directly
  #   git --version # Use packages
  # '';

  # https://devenv.sh/tasks/
  # tasks = {
  #   "myproj:setup".exec = "mytool build";
  #   "devenv:enterShell".after = [ "myproj:setup" ];
  # };

  # https://devenv.sh/tests/
  # enterTest = ''
  #   echo "Running tests"
  #   git --version | grep --color=auto "${pkgs.git.version}"
  # '';

  # https://devenv.sh/git-hooks/
  # git-hooks.hooks.shellcheck.enable = true;
  git-hooks.hooks = {
    dotnet-format = {
      enable = true;
      entry = "dotnet format EngineerRedux/";
      pass_filenames = false;
    };
    build = {
      enable = true;
      entry = "dotnet build EngineerRedux/EngineerRedux.csproj -o Output";
      after = [ "dotnet-format" ];
      pass_filenames = false;
    };
  };

  # See full reference at https://devenv.sh/reference/options/
}
