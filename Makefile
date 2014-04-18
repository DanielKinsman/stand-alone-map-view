SHELL := /bin/bash

MDTOOL = mdtool
CSHARP_PROJECTS = client comms server utils
CSHARP_BINDIRS = $(foreach PROJECT,$(CSHARP_PROJECTS),$(PROJECT)/bin/)
TEMP_DIR := $(shell mktemp -d)
MOD_NAME = stand-alone-map-view
MOD_DIR = $(TEMP_DIR)/$(MOD_NAME)/

all: release debug

toolbaricon.png: toolbaricon.svg
	inkscape --export-png=toolbaricon.png toolbaricon.svg -h 24 -w 24 -z

release: toolbaricon.png
	$(MDTOOL) build -t:Build -c:"Release" StandAloneMapView.sln
	cp -fv toolbaricon.png server/bin/Release/
	cp -fv toolbaricon.png client/bin/Release/

debug:  toolbaricon.png
	$(MDTOOL) build -t:Build -c:"Debug" StandAloneMapView.sln
	cp -fv toolbaricon.png server/bin/Debug/
	cp -fv toolbaricon.png client/bin/Debug/

release.tar.gz: release
	rm -fv release.tar.gz
	mkdir -v $(MOD_DIR)
	cp -rv client/bin/Release/ $(MOD_DIR)samv_client
	cp -rv server/bin/Release/ $(MOD_DIR)samv_server
	cp -v license $(MOD_DIR)
	cp -v readme.md $(MOD_DIR)
	cp -v lib/protobuf-net_licence $(MOD_DIR)
	cp -v utils/ksp_toolbar_license $(MOD_DIR)
	pushd $(TEMP_DIR); tar -cavf release.tar.gz $(MOD_NAME); popd
	mv -v $(TEMP_DIR)/release.tar.gz ./release.tar.gz
	rm -rfv $(TEMP_DIR)

release.zip: release
	rm -fv release.zip
	mkdir -v $(MOD_DIR)
	cp -rv client/bin/Release/ $(MOD_DIR)samv_client
	cp -rv server/bin/Release/ $(MOD_DIR)samv_server
	cp -v license $(MOD_DIR)
	cp -v readme.md $(MOD_DIR)
	cp -v lib/protobuf-net_licence $(MOD_DIR)
	cp -v utils/ksp_toolbar_license $(MOD_DIR)
	pushd $(TEMP_DIR); zip -r release.zip $(MOD_NAME); popd
	mv -v $(TEMP_DIR)/release.zip ./release.zip
	rm -rfv $(TEMP_DIR)

clean:
	$(MDTOOL) build -t:Clean -c:"Release" StandAloneMapView.sln
	$(MDTOOL) build -t:Clean -c:"Debug" StandAloneMapView.sln
	rm -rfv $(CSHARP_BINDIRS)
	rm -fv release.tar.gz
	rm -fv release.zip

# toolbaricon.png not included in clean deliberately, don't want to
# require inkscape just to build.
