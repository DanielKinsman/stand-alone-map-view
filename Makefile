MDTOOL = mdtool
CSHARP_PROJECTS = client comms server utils
CSHARP_BINDIRS = $(foreach PROJECT,$(CSHARP_PROJECTS),$(PROJECT)/bin/)

all: release debug

release:
	$(MDTOOL) build -t:Build -c:"Release" StandAloneMapView.sln

debug: $(CSHARP_SOURCE)
	$(MDTOOL) build -t:Build -c:"Debug" StandAloneMapView.sln	

clean:
	$(MDTOOL) build -t:Clean -c:"Release" StandAloneMapView.sln
	$(MDTOOL) build -t:Clean -c:"Debug" StandAloneMapView.sln
	rm -rfv $(CSHARP_BINDIRS)
