Stand Alone Map View
====================

A Kerbal Space Program mod that opens the map view in another window/monitor.
It works by launching a separate instance of the game, and syncing the time
and ship information via networking (udp).

**Still very much a work in progress, use at your own risk**

To get it working, you'll need two separate installs of KSP, your normal one
and a separate one just for this mod. The "server" stuff goes in your normal
game (in the GameData directory), the "client" stuff goes in the other
install (also under GameData). Start up an instance of both and you should
be in business, providing you have the CPU and GPU to handle it.

Current Features
----------------

* Syncing of time and time warp
* Syncing of active vessel's trajectory

Feature Wishlist
----------------

* Configurable ip adress/port settings
* Syncing of manoeuver nodes (bidirectional)
* Syncing of all vessels' orbits

Licenses
--------

Stand Alone Map View copyright 2014 Daniel Kinsman  
GNU General Public License v3

protobuf-net copyright 2008 Marc Gravell
Apache Licese v2.0

Contact
-------

* danielkinsman@riseup.net
* https://github.com/DanielKinsman/stand-alone-map-view

