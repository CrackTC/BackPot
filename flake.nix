{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };
  outputs = { self, nixpkgs, flake-utils, ... }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = import nixpkgs { inherit system; };
      in
      {
        packages.default = pkgs.stdenv.mkDerivation {
          name = "download";
          buildInputs = with pkgs; [ curl jq ];
          src = ./scripts;
          phases = [ "installPhase" ];
          installPhase = ''
            mkdir -p $out/bin
            cp $src/download.sh $out/bin
          '';
        };
        apps.default = {
          type = "app";
          program = "${self.packages.${system}.default}/bin/download.sh";
        };
      }
    );
}
