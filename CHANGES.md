# AT Utils :: Changes

* 2018-1016: 1.6.2 (allista) for KSP 1.4.5
	+ Added StringID extensions for classes in ksp object's hierarchy.
		- To better track what happens where during debugging.
	+ Fixed SelectMax; .Log extensions use GetID() for prefixes.
	+ Added NamedDockingPort module
	+ Vector3d and Orbit are serialized in CNO. Added bin->base64 de/serialization.
	+ Added SimpleScrollView GUI component
	+ Added default constructor to ResourceInfo
	+ Fixed NRE in UnlockControls and OnDestroy.
		- Apparently, reference type fields should be initialized in Awake, not in class definition.
	+ Fixed check for bad parts in Metric; added MKS drills' names to the list.
	+ Fixed UpdateAttachedPartPos extensions.
		- Now they distinguish between flight and editor and do not mess with the orbits.
	+ Fixed staging of spawned ship constructs.
