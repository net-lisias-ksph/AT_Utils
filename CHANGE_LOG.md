# AT Utils :: Change Log

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
* 2018-0913: 1.6.1 (allista) for KSP 1.4.5
	+ Added DebugUtils.formatPartJointsTree
	+ Fixed Local2Local; fixed UpdateAttachedPartPos*
	+ Fixed messed up part rotation when animating part resizing
	+ Added Transform.(Inverse)TransformPointUnscaled extensions
	+ Fixed VesselSpawner.SpawnShipConstruct positioning.
	+ Consider disabled renderers when searching for AffectedObjects in STS
* 2018-0831: 1.6.0.1 (allista) for KSP 1.4.5
	+ Recompiled against KSP-1.4.5
* 2018-0615: 1.6.0 (allista) for KSP 1.4.3
	+ Added ShipConstructLoader component that loads both complete .crafts and subsassemblies.
	+ GUIWindowBase.HUD_enabled takes into account level_loaded flag.
	+ Added SlowUpdate coroutine helper.
	+ Both IShipConstruction.Bounds and Metric support world-space calculation.
	+ Added variants of ButtonSwitch that change the label on switch as well as style.
		- Merged HangarSpaceManager into SpawnSpaceManager, which now uses .Init instead of .Load to get spawn transform/mesh.
			- Added more variants of GetSpawnTransform/Offset.
			- Added an option to setup a sensor collider to SpawnSpace.
	+ Added VesselSpawner class that generalizes Hangar's algorithm to launch vessels in flight from both ProtoVessels and ShipConstructs, and also to launch ShipConstructs to ground, as GC does.
	+ Added ATGroundAnchor module that combines GC's and Hangar's anchors.
	+ Added ATMagneticDamper module that slows down parts inside a trigger collider defined on the specified mesh.
		- Removed ModuleAsteroidFix
	+ Added check_module static method to SimplePartFilter.
	+ Fixed Metric and Bounds for RadialDrill via explicit workaround.
	+ Adde MeshesToSkip and BadParts lists to AT_UtilsGlobals.
	+ Optimized IShipConstruct.Bounds.
	+ Added Part.AllModelMeshes extension; used in Metric.
	+ Added MeshFilter.AddCollider extension; used in SpawnSpaceManager and ATMagneticDamper.
	+ Added MetricDebug module to visualize both Metric and .Bounds
* 2018-0510: 1.5.2 (allista) for KSP 1.4.3
	+ Added FeedForward input to PID controllers.
	+ Fixed FCO mode switching without changing the anchor.
		- Plus, OrbitAround anchors at CoMs of vessels.
	+ CNO saves non-public fileds.
	+ Added Ratched trigger; probably not a good name, but still.
		- Anyway, works like this: input is a boolean condition that normally should be True; when it first becomes False, the Ratchet becomes "armed"; then if it becomes True again, the Ratched becomes True itself and stays this way.
	+ Added formatting of Vector2(d) to convert_args
	+ Added .Draw() method to ITestScenario.
		- Scenarios are created at game start and then only initialized, updated and cleaned.
	+ Added Vector2d.Rotate extensions; AtmosphereParams are a struct now.
		- Added Orbit.Contains(UT), ApA/PeAUT, Ap/PeV extensions.
	+ Added Vector2d.Angle2 and Vector3(d).ClampDirection extensions.
	+ In ValueFileds iformat defaults to format instead of "F1"
		- Made runtime fields non-persistent.
	+ Added FuzzyThreshold.Reset() method
	+ OscillationDetector.max_filter is assymmetric.
	+ Added StallDetector class hierarchy; added ComputationBalancer KSPAddon.
		- ComputationBalancer computes tasks made as IEnumerables (like coroutines) inside its Update event, trying to keep FPS from dropping too low.
		- It works in Flight, Editor and KSC, and also when the game is paused.
		- Also moved ProgressIndicator here from TCA.
* 2018-0327: 1.5.1 (allista) for KSP 1.4.1
	+ Moved HangarSpaceManagers and SubassemblySelector from Hangar here.
		- Moved all addons to Addons subfolder.
		- Moved all modules to Modules subfoldre.
		- Added ResourceInfo CNO to proxy calls to PartResourceDefinition.
	+ Added TextureCache static class to load and store fullres icons.
	+ SimplePartFilter uses TextureCache.
	+ Added direct support for GUID fields to CNO. Added PersistentDictS<T> class.
	+ Added ConvexHull3D.MakeMesh method.
	+ Fixed SkinnedMeshRenderer handling by the Metric.
		- Using ResourceInfo("ElectricCharge") for static EC information.
		- Added Metric.hull_mesh lazy property.
		- Converted one-liner methods to expression bodies.
* 2017-1109: 1.5.0 (allista) for KSP 1.3.1
	+ Using rel pivot distance instead of constant MAX_DIST for LookAt modes.
	+ OnPlanet uses additional PeR check using Orbit.MinPeR extension.
	+ Moved AngleTo/DistanceTo/*Pos methods to Coordinates.
	+ Added CDOS_Optimizer2D generic 2d-function optimizer. Added BackupLogger addon.
		- CDOS_Optimizer2d_Generic is an implementation of the CDOS algorithm for 2d case.
		- BackupLogger does two things: 1. it copies the full Unity player log each second to a time-tagged file (one per game start) to retain its content in case of accidental game restart. 2. it can log anything to a separate time-tagged (per game start) file.
	+ Fixed argument types for ClampMagnitude(Vector3d, double).
	+ Using BackupLogger to backup Utils.Log messages in DEBUG mode.
	+ Added HierarchicalComparer and ValueFieldBase classes.
		- HC is a container for optimization constrains of different importance. VFB is the base of all field widgets; it adds an important ability to set value with Return key.
	+ Removed redundant using-s; removed commented out class embryo.
	+ Reimplemented Fileds using ValueFieldBase class.
	+ Added Utils.GLLines method to draw polyline more efficiently.
	+ Fixed orto_shift; added some more debug logging. WIP still.
	+ Commented out logging of stack trace, since KSP Development Build does it anyway.
	+ 1.3.1 compatibility of SimplePartFilter (NRE fix)
		- Squad changed capitalization of "Filter by Function" and "Filter by Module". Also can be used #autoLOC_453547 for "Filter by Function" and #autoLOC_453705 for "Filter by Module".
	+ Merge pull request #5 from jarosm/patch-1
		- 1.3.1 compatibility of SimplePartFilter (NRE fix)
	+ Added optional node_name argument to CNO LoadFrom and SaveInto methods.
		- Added ModuleAsteroidFix to preven asteroids from changing form on load.
	+ Using Localizer as suggested by maja.
	+ Fixed CrewTransferBatch.
	+ Using Angle2 in Markers.
	+ Small refactoring of Filters.
	+ Added docstrings; added Angle2* methods to Utils.
	+ Added Vector6.Inverse and .Slice methods.
	+ Added formatComponents(Vector3) method to Utils.
	+ SimplePartFilter excludes parts in "none" category.
	+ Reimplemented and renamed RelSurf/OrbPos methods of Coordinates.
			- After the implementatino of the corresponding CB methods was changed.
	+ Made PID.Action and IntegralAction public. Added setClamp method.
			- Overloaded Update method with (error,speed) signature.
			- Added PIDf_Controller3.
	+ Added Reset to OD. Fixed normalization.
	+ Fixed CDOS optimizer that stuck at feasible borders.
	+ GL methods accept material as mat argument.
	+ ValueFields accept value both on Enter and KeypadEnter.
	+ Added style argument to *Field.Draw methods.
	+ Fixed metric and bounds calculation.
	+ Renamed Profiler to AT_Profiler to avoid name clash with Unity.
	+ Performance optimizations.
* 2017-0620: 1.4.4 (allista) for KSP 1.3
	+ Added kerbal traits and level to CrewTransferWindow; Closed #4
	+ Added a slghtly optimized version of GLDrawHull.
	+ Converted DebugInfo props of filters into ToString overrides.
	+ Added two Utils.LerpTime methods.
	+ In SimplePartFilter:
		- Fixed module matching with moduleInfo.moduleName: Converted MODULES to List of strings that is filled with SetMODULES methods that accepts IEnumerable of Types and converts Type.Name to moduleName/categoryName format.
		- Added default implementation of the filter method.
* 2017-0605: 1.4.3 (allista) for KSP 1.3
	+ Compatible with KSP-1.3
	+ Added simple emailer class (works only with local spooler). Added ScenarioTester framework for automated continious testing.
	+ Changed log message upon scenario registration.
	+ Added formatTimeDelta, formatCB, formatPatches methods to Utils.
	+ Added Extremum.ToString method.
	+ Added Vessel.BoundsWithExhaust, Orbit.MinPeR; improved Vessel.Radius calculation.
	+ Added ConfigNodeObjectGUI framework. Mainly for debugging.
	+ Made Float/Vector3Field implement ITypeUI.
	+ Added CreateDefaultOverride method to PluginConfig.
	+ Added ClampSignedH/L family of methods, added Circle method to Utils.
	+ Made PID.IntegralError public; changed notation of PID class names; added PIDvf and PIDv controllers.
	+ Added LookBetween and LookFromTo modes for FCO.
	+ Added OrbitAround mode for FCO; changed naming scheme for methods its.
	+ Made Bounds an extension of IShipconstruct.
* 2017-0222: 1.4.2 (allista) for KSP 1.2.2
	+ Added patch for FilterExtensions to properly classify APR parts as "proc" bulkheadProfiles.
	+ Added SimplePartFilter class to add custom part subcategories.
	+ Fixed calculations of subwindow placement in compound windows.
	+ Corrected formatOrbit method.
	+ Added double version of EWA.
	+ Added Multiplexer.ToString
	+ Added Coordinates.normailize_coordinates method to deal with any lat/lon.
		- Coordinates are automatically normalized on creation and Load.
		- Added NormalizedLatitude static method.
	+ Prev/Next extensions of SortedList return Key instead of Value.
	+ Corrected namespace for Markers class.
* 2017-0205: 1.4.1 (allista) for KSP 1.2.2
	+ Sorted GUI classes into subfolders. Added SimpleDialog framework.
	+ Included call to GUIWindowBase.can_draw() into doShow property.
	+ Removed Closed fields/property from transfer windows; using WindowEnabled instead.
	+ Fixed AddonWindowBase.ShowWithButton.
	+ Added Utils.MouseInLastElement() method.
	+ Changed default control lock type and renamed LockEditor to LockControls.
	+ Added Part.HighlightAlways extension.
	+ Vessel.Radius extension uses GetTotalMass call instead of totalMass field for unloaded vessels.
	+ Changed scroll-bar styles and tooltip color. Merged InitSkin and InitGUI methods of Styles class.
	+ Fixed tooltips for scroll-views; limited them in width, enabled word-wrap.
	+ Added State.Empty property.
	+ Utils.ParseLine now returns empty array instead of null if argument is empty.
	+ Added SubmoduleResizable module in a separate dll; for @Enceos.
	+ Added ToString overrides to all ResourceProxy classes; using ListDict instead of plane Dictionary in VesselResources.
	+ Removed transferNow flag from ResourceTransferWindow; added TransferAction delegate instead.
* 2017-0115: 1.4.0 (allista) for KSP 1.2.2
	+ Added no_window style for borderless, titleless transparent window.
	+ Fixed PartIsPurchased for ScienceSandbox mode.
		- TooltipManager is now a KSPAddon and is drawing tooltips above everything by itself.
	+ Reimplemented GUIWindowBase framework:
		- To save some field in the GUI_CFG, this field has to be tagged with
		- ConfigOption attribute. The Load/SaveConfig handle such fields using Reflection.
		- Added subwindows framework: any field that is a GUIWindowBase itself is automaticaly initialized in Awake and its Save/LoadConfig methods are called when needed.
		- ALL GUIWindowBase, being MonoBehaviour as they are, have to be instantiated using GameObject.AddComponent, not the new keyword. For subwindows this is done automatically. For any GUIWindowBase fields in other classes this should be done by hand.
		- GUIWindowBase tracks scene changes to hide itself when no UI should be shown.
		- Added Move method.
		- window_enabled is true by default.
		- UnlockControls unlocks controls for all subwindows.
		- Moved Show and Toggle methods from AddonWindowBase here.
		- Moved can_draw() from AddonWindowBase here.
		- Renamed AddonWindowBase.Show/Toggle static methods to ShowInstance/ToggleInstance. The instance itself is made public and is now called Instance.
		- Added AddonWindowBase.Show/ToggleWithButton static methods to handle AppLauncher buttons.
	+ SimpleDialog uses rich_label style.
	+ Added CompoundWindow framework.
		- Added AnchoredWindow, SubWindow and CompoundWindow base classes.
		- Added SubwindowSpec attribute to control SubWindow rendering.
		- Added FloatingWindow as a CompoundWindow subclass.
		- GUIWindowBase:
	+ Fixed NRE in Coordinates.GetAtPointer/Search methods.
	+ Added InOrbit/OnPlanet Vessel extension methods.
* 2017-0104: 1.3.1.1 (allista) for KSP 1.2.2
	+ Numerous small bugfixes.
* 2016-1229: 1.3.1 (allista) for KSP 1.2.2
	+ Moved CrewTransferBatch from Hangar here.
	+ Fixed resource transfer UI.
	+ Removed IsPhysicallySignificant as it was obsolete.
	+ Improved GLDrawBounds/Point.
	+ Fixed Metric calculation: disabled objects are not taken into account + all parts have mass. Fixed #176 issue in Hangar's bugtracker.
	+ Moved debug routines into Debugging. Added ResourceHack module to replenish resources in flight.
* 2016-1219: 1.3.0 (allista) for KSP 1.2.2
	+ Compiled against KSP-1.2.2
	+ Added SerializableFieldsPartModule -- a base PartModule that uses reflection to serialize any field with [SerializeField] attribute that is of either [Serializable] type, or an IConfigNode, or the ConfigNode itself.
		- So ConfigNodeWrapper is now obsolete.
	+ Added Resource Transfer framework that facilitates transfer of resources between ships represented by either a Vessel object, a ProtoVessel object or a ConfigNode produced by ProtoVessel.Save.
	+ Factored most of the SimpleTextureSwitcher into the new TextureSwitcherServer.
		- The STS now only provides the UI, while all the switching is done by TSS.
	+ Moved from TCA: Coordinates; Draw*Marker methods Markers static class.
	+ Added support for persistent ConfigNode fields. Added PersistentList/Queue. Added CNO.LoadFrom method.
	+ Added calculation of ConvexHull Volume and Area.
	+ Added Queue extensions: FillFrom, Remove, MoveUp.
	+ Added tooltip to LeftRightChooser; Added generic variants of the chooser.
	+ Added Coordinates.OnWater flag and SurfaceAlt(under_water) option.
	+ Fixes and improvements:
		- Fixed VectorCurve.Load, improved VectorCurve.Save performance using string.Join.
		- Optimized CNO Load/Save performance.
		- Fixed ConvexHull calculation in Metric.init_with_mesh.
		- Made LockName of GUIWindowBase unique.
		- Fixed Utils.formatSmallValue. Improved performance of convert_args.
		- Declared Metric as IConfigNode for easy persistence. Added Metric(IShipConstruct).
		- Made tooltips light-green with black text for better visibility.
		- Fixed Part.TotalCost.
		- Added ShipConstruct.Unload extension.
* 2016-1113: 1.2.2 (allista) for KSP 1.1.2
	+ Added ClampedAssymetricFilter3D.
		- Renamed AsymmetricFilter.alpha/Tau[R|F] to [Up|Down].
	+ Added Min property to Vector6.
	+ Fixed PIDv_2/3 omega handling in Update methods.
	+ No more default(Color) in GraphicsUtils._Draw_ methods.
	+ Documented Timer class. Added Restart, and StartIf methods. Start method works only when the timer was not already started.
	+ Added Vector3Field GUI for editing Vector3 components.
	+ Added State<T>.ToString method.
	+ Reworked OscillationDetector<T> framework.
		- Instead of a filtered boolean flag OD now provides filtered max.peak value.
		- Added OscillationDetectorF.
	+ DebugUtils.CSV writes directly to a file, not to a common log.
	+ DebugUtils.CSV now recognizes Vector3/4(d)
	+ Reworked PID_Controller class family. Added PIDv_Controller3 with Vector3 PID parameters.
	+ Added OscillationDetector3D that encapsulates 3 OscillationDetectorD for vector components.
	+ Added many Vector3(d) extension methods:
		- ScaleChain(Vector3[]) that sequentially sacales given vector with each of the given list. Returns the result.
		- ClampMagnitude(Vector3,Vector3,Vector3) to clamp magnitude component-wise.
		- ClampComponentsH/L, Squared/Sqrt/PowComponents
		- Max/MinI, Component, Exclude, Max/MinComponentV, Max/MinComponentD(Vector3d).
		- Added ForEach(IList).
	+ Fixed CNO.NodeName property. Added SaveInto method.
	+ Updated AT-Utils.netkan file; updated to CC with .netkans.
* 2016-1026: 1.2.1 (allista) for KSP 1.1.2
	+ Added alternative GLDrawPoint.
	+ Fixed Utils.SaveGame. Added optional with_message argument.
	+ Fixed AddonWindowBase configuration saving.
	+ Fixed NRE in PartIsPurchased.
	+ Fixed TypedConfigNodeObject.
	+ Made AddonWindow a true singleton.
	+ Added Part.UpdatePartMenu() extension method.
	+ LeftRightChooser's width parameter now defines the total width of the controls, not only the field itself.
	+ Added FloatField.IsSet property. Default formatting is changed to roundtrip. Added separate increment formatting.
	+ Moved ToolbarWrapper to AT_Utils. Updated it from upstream.
* 2016-1016: 1.2.0 (allista) for KSP 1.1.2
	+ Added KSP_AVC support for CC.
	+ Excluded CC Parts from library distributive.
	+ Added Utils.Log2File method.
	+ Updated to KSP-1.2 API.
	+ Renamed AddonWindowBase to GUIWindowBase.
	+ Added TooltipManager static class.
	+ Made GUIWindowBase the base class for most of the GUI windows.
	+ SimpleDialog.Show returns Answer instead of Rect now.
	+ Added FloatField.ClampValue method.
	+ Improved IEnumerable formatting.
	+ Catch exceptions while parsing resource collection.
	+ Added Utils.EnableEvent\* group of methods.
	+ Utils.Log now searches for the first caller method that does not start with Log to get assembly name.
	+ Moved SimpleTextureSwitcher to AT_Utils and fixed it.
* 2016-0905: 1.0.2 (allista) for KSP 1.1.2
	+ Added OscillationDetector class that detects oscillations in input data in real-time using recursive DCT.
	+ Renamed Timer.Check property to a more meaningful .TimePassed.
	+ Added GLDrawPoint, GLDrawHullLines and GLTriangleLines, renamed GLBounds to GLDrawBounds.
	+ DrawMesh now uses Graphics.DrawMeshNow to instantly draw a mesh OnObjectRender message.
	+ Added Metric(MeshFilter), Metric(Transform) constructors. Added optional offset vector to FitsAligned methods.
* 2016-0816: 1.0.1 (allista) for KSP 1.1.2
	+ Moved dll-s into Plugins subfolder.
	+ Added Vector3/d.IsInf and .Invalid (= .IsNan || .IsInf) methods.
* 2016-0715: 1.0.0 (allista) for KSP 1.1.2
	+ This is not a real release, but rather a snapshot of current work in progress. It is needed for CKAN to be able to install the mods that depend on AT_Utils properly.
