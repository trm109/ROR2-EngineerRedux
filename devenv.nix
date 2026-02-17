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
    pkgs.dotnet-sdk
    pkgs.ilspycmd
  ];

  # https://devenv.sh/languages/
  # languages.rust.enable = true;
  languages.dotnet = {
    enable = true;
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
  scripts.build.exec = ''
    echo "Building project"
    dotnet build -o Output
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
  '';

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

  # See full reference at https://devenv.sh/reference/options/
}
