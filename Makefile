include .env
MODULE_NAME=ZetymPatch
GAME_DIR=$(STEAM_DIR)steamapps/common/Supermarket\ Together/
GAME_MANAGED_DIR=$(GAME_DIR)Supermarket\ Together_Data/Managed/

default: release

debug:
	STEAM_DIR=$(STEAM_DIR) dotnet build -c Debug

release: 
	STEAM_DIR=$(STEAM_DIR) dotnet build -c Release

clean:
	dotnet clean

install: release
	cp bin/Release/netstandard2.1/$(MODULE_NAME).dll $(GAME_DIR)BepInEx/Plugins/

launch: install
	pkill -9 -e  "Supermarket Tog" &
	xdg-open "steam://launch/2709570"

uninstall:
	rm -f $(GAME_DIR)BepInEx/Plugins/$(MODULE_NAME).dll

