﻿//   ATMagneticDamper.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AT_Utils
{
    public class ATMagneticDamper : PartModule
    {
        public enum AttractorAxis { right, up, fwd }

        [KSPField(isPersistant = true,
            guiName = "Damper Field",
            guiActive = true,
            guiActiveEditor = true,
            guiActiveUnfocused = true,
            unfocusedRange = 50)]
        [UI_Toggle(scene = UI_Scene.All)]
        public bool DamperEnabled;

        [KSPField(isPersistant = true,
            guiActive = true,
            guiActiveEditor = true,
            guiName = "Attenuation")]
        [UI_FloatEdit(scene = UI_Scene.All,
            minValue = 0f,
            maxValue = 99.9f,
            incrementLarge = 10f,
            incrementSmall = 1f,
            incrementSlide = 0.1f,
            sigFigs = 1,
            unit = "%")]
        public float Attenuation = 50f;

        [KSPField(isPersistant = true,
            guiActive = true,
            guiActiveEditor = true,
            guiName = "Attr. Power")]
        [UI_FloatEdit(scene = UI_Scene.All,
            minValue = 0f,
            incrementLarge = 10f,
            incrementSmall = 1f,
            incrementSlide = 0.1f,
            sigFigs = 1,
            unit = "kN/t")]
        public float AttractorPower = 1f;

        [KSPField(isPersistant = true,
            guiName = "Attractor",
            guiActive = true,
            guiActiveEditor = true,
            guiActiveUnfocused = true,
            unfocusedRange = 50)]
        [UI_Toggle(scene = UI_Scene.All)]
        public bool AttractorEnabled = true;

        [KSPField(isPersistant = true,
            guiName = "Attractor Mode",
            guiActive = true,
            guiActiveEditor = true,
            guiActiveUnfocused = true,
            unfocusedRange = 50)]
        [UI_Toggle(scene = UI_Scene.All, enabledText = "Reverse", disabledText = "Direct")]
        public bool InvertAttractor;

        [KSPField(guiActive = true,
            guiActiveEditor = true,
            guiName = "Attractor Max. Force",
            guiUnits = "kN",
            guiFormat = "F1")]
        public float MaxForce = 100f;

        [KSPField(guiActiveEditor = true,
            guiName = "Maximum EC Current",
            guiUnits = "ec/s",
            guiFormat = "F1")]
        public float MaxEnergyConsumption = 50f;

        [KSPField(guiActiveEditor = true,
            guiName = "Idle EC Current",
            guiUnits = "ec/s",
            guiFormat = "F1")]
        public float IdleEnergyConsumption = 0.1f;

        private const float RelativeVelocityThreshold = 0.1f;
        [KSPField] public float EnergyConsumptionK = 1f;
        [KSPField] public string DamperID = string.Empty;
        [KSPField] public string Sensor = string.Empty;
        [KSPField] public string AttractorLocation = string.Empty;
        [KSPField] public AttractorAxis AttractorMainAxis = AttractorAxis.fwd;
        [KSPField] public string AffectedPartTags = string.Empty;
        [KSPField] public bool AffectKerbals;
        [KSPField] public bool VariableAttractorForce;
        [KSPField] public bool EnableControls = true;
        [KSPField] public float ReactivateAfterSeconds = 5f;
        private double reactivateAtUT = -1;

        [KSPField] public string AnimatorID = string.Empty;
        private IAnimator animator;
        protected Damper damper;
        protected ResourcePump socket;

        public bool HasDamper { get; private set; }
        public bool HasAttractor => HasDamper && damper.HasAttractor;

        public static ATMagneticDamper GetDamper(Part p, string id) =>
            string.IsNullOrEmpty(id)
                ? null
                : p.Modules.GetModules<ATMagneticDamper>()
                    .FirstOrDefault(d =>
                        !string.IsNullOrEmpty(d.DamperID) && d.DamperID.Equals(id));

        public override string GetInfo()
        {
            var info = StringBuilderCache.Acquire();
            info.AppendLine($"Attenuation: {Attenuation:F1} %");
            info.AppendLine($"Max.Force: {MaxForce:F1} kN");
            info.AppendLine($"Max.EC Current: {MaxEnergyConsumption:F1} ec/s");
            info.AppendLine($"Idle EC Current: {IdleEnergyConsumption:F1} ec/s");
            if(string.IsNullOrEmpty(AttractorLocation))
                info.AppendLine("Has attractor");
            info.AppendLine(string.IsNullOrEmpty(AffectedPartTags)
                ? "Affects all parts"
                : $"Affects only: {AffectedPartTags}");
            if(AffectKerbals)
                info.AppendLine("WARNING: Affects kerbals!");
            return info.ToStringAndRelease();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            HasDamper = false;
            EnergyConsumptionK = Utils.ClampL(EnergyConsumptionK, 1e-6f);
            if(!string.IsNullOrEmpty(Sensor))
            {
                var sensor = part.FindModelComponent<MeshFilter>(Sensor);
                if(sensor != null)
                {
                    sensor.gameObject.layer = state == StartState.Editor ? 21 : 2;
                    sensor.AddCollider(true);
                    damper = sensor.gameObject.AddComponent<Damper>();
                    damper.Init(this);
                    damper.enabled = DamperEnabled;
                    socket = part.CreateSocket();
                    animator = part.GetAnimator(AnimatorID);
                    if(DamperEnabled)
                        animator?.Open();
                    else
                        animator?.Close();
                    HasDamper = true;
                }
            }
            var damper_controllable = HasDamper && EnableControls;
            var attractor_controllable = damper_controllable && damper.HasAttractor;
            Fields[nameof(DamperEnabled)].OnValueModified += onDamperToggle;
            Utils.EnableField(Fields[nameof(DamperEnabled)], damper_controllable);
            Utils.EnableField(Fields[nameof(Attenuation)], damper_controllable);
            Actions[nameof(ToggleAction)].active = damper_controllable;
            Utils.EnableField(Fields[nameof(AttractorEnabled)], attractor_controllable);
            Utils.EnableField(Fields[nameof(AttractorPower)],
                attractor_controllable && VariableAttractorForce);
            Utils.EnableField(Fields[nameof(InvertAttractor)], attractor_controllable);
            Actions[nameof(ToggleAttractorAction)].active = attractor_controllable;
        }

        private void OnDestroy()
        {
            if(HasDamper)
                Destroy(damper);
            Fields[nameof(DamperEnabled)].OnValueModified -= onDamperToggle;
        }

        private void drainEnergy(float rate) =>
            socket.RequestTransfer(rate * TimeWarp.fixedDeltaTime);

        private void FixedUpdate()
        {
            if(!HighLogic.LoadedSceneIsFlight || FlightDriver.Pause)
                return;
            if(!HasDamper || !DamperEnabled || !damper.enabled)
                return;
            if(socket == null)
                return;
            drainEnergy(IdleEnergyConsumption);
            if(!socket.TransferResource())
                return;
            if(!socket.PartialTransfer)
                return;
            animator?.Close();
            damper.enabled = false;
            reactivateAtUT = Planetarium.GetUniversalTime() + ReactivateAfterSeconds;
            Utils.Message(ReactivateAfterSeconds,
                $"[{part.Title()}] Damper deactivated due to the lack of EC. Activating in {ReactivateAfterSeconds}");
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if(!HighLogic.LoadedSceneIsFlight || FlightDriver.Pause)
                return;
            if(!HasDamper || !DamperEnabled)
                return;
            if(reactivateAtUT > 0
               && !damper.enabled
               && Planetarium.GetUniversalTime() > reactivateAtUT)
            {
                animator?.Open();
                damper.enabled = true;
                reactivateAtUT = -1;
                Utils.Message($"[{part.Title()}] Damper reactivated");
            }
        }

        private void onDamperToggle(object value)
        {
            if(!HasDamper)
                return;
            damper.enabled = DamperEnabled;
            if(DamperEnabled)
                animator?.Open();
            else
                animator?.Close();
        }

        public void EnableDamper(bool enable)
        {
            DamperEnabled = enable;
            onDamperToggle(null);
        }

        [KSPAction(guiName = "Toggle Damper")]
        public void ToggleAction(KSPActionParam data) => EnableDamper(!DamperEnabled);

        [KSPAction(guiName = "Toggle Attractor")]
        public void ToggleAttractorAction(KSPActionParam data) =>
            AttractorEnabled = !AttractorEnabled;

        [KSPAction(guiName = "Invert Attractor")]
        public void InvertAttractorAction(KSPActionParam data) =>
            InvertAttractor = !InvertAttractor;

        protected class Damper : MonoBehaviour
        {
            private ATMagneticDamper controller;
            private Transform attractor;
            private Vector3 attractorAxis;
            private string[] tags;

            public bool HasAttractor => attractor != null;

            private struct VesselInfo
            {
                public uint id;
                public Vessel vessel;
                public Vector3 position;
                public float energyConsumption;
                public bool positionSet;

                public void SetPosition(Vector3 pos)
                {
                    position = pos;
                    positionSet = true;
                }
            }

            private struct RBInfo
            {
                public Part part;
                public Rigidbody rb;
                public Vector3 relV;
                public Vector3 dP;
                public Vector3 dAv;
            }

            /// <summary>
            /// For holding damped packed vessels in place. 
            /// </summary>
            private readonly Dictionary<uint, VesselInfo> dampedVessels =
                new Dictionary<uint, VesselInfo>();

            /// <summary>
            /// For damping unpacked vessels, per Rigidbody
            /// </summary>
            private readonly List<RBInfo> dampedBodies =
                new List<RBInfo>();

            public void Init(ATMagneticDamper damper_module)
            {
                controller = damper_module;
                if(!string.IsNullOrEmpty(controller.AttractorLocation))
                    attractor = controller.part.FindModelTransform(controller.AttractorLocation);
                if(!string.IsNullOrEmpty(controller.AffectedPartTags))
                    tags = Utils.ParseLine(controller.AffectedPartTags, Utils.Comma);
                if(attractor != null)
                {
                    controller.AttractorEnabled = true;
                    switch(controller.AttractorMainAxis)
                    {
                        case AttractorAxis.right:
                            attractorAxis = Vector3.right;
                            break;
                        case AttractorAxis.up:
                            attractorAxis = Vector3.up;
                            break;
                        case AttractorAxis.fwd:
                            attractorAxis = Vector3.forward;
                            break;
                        default:
                            attractorAxis = Vector3.forward;
                            break;
                    }
                }
                StartCoroutine(damp_packed_vessels());
            }

            private void FixedUpdate()
            {
                if(FlightDriver.Pause || controller == null)
                    return;
                if(dampedBodies.Count <= 0 || controller.part.Rigidbody == null)
                    return;
                var A = controller.Attenuation / 100f;
                var total_energy = 0f;
                var attractorEnabled = controller.AttractorEnabled && attractor != null;
                var attractorPosition = attractorEnabled ? attractor.position : Vector3.zero;
                var attractorAxisW =
                    attractorEnabled ? attractor.rotation * attractorAxis : Vector3.zero;
                var h = controller.part.Rigidbody;
                var nBodies = dampedBodies.Count;
                for(var i = 0; i < nBodies; i++)
                {
                    var b = dampedBodies[i];
                    if(b.rb == null || b.part == null)
                        continue;
                    if(b.part.packed)
                    {
                        track_packed_vessel(b.part);
                        continue;
                    }
                    var dist = b.rb.position - h.position;
                    b.relV = b.rb.velocity
                             - h.velocity
                             - Vector3.Cross(h.angularVelocity, dist);
                    if(A > 0)
                    {
                        b.dAv = A * (h.angularVelocity - b.rb.angularVelocity);
                        b.dP = A * b.rb.mass * b.relV;
                    }
                    if(attractorEnabled)
                    {
                        var toAttractor = b.rb.worldCenterOfMass - attractorPosition;
                        if(!toAttractor.IsZero())
                        {
                            var toAttractorDist = toAttractor.magnitude;
                            toAttractor /= toAttractorDist;
                            var rVel2attractor = -Vector3.Dot(b.relV, toAttractor);
                            var dV = Mathf.Min(
                                controller.part.crashTolerance * 0.9f - rVel2attractor,
                                TimeWarp.fixedDeltaTime * controller.AttractorPower);
                            if(dV > 0)
                            {
                                if(controller.InvertAttractor)
                                {
                                    var toCenter = Vector3.ProjectOnPlane(
                                        toAttractor,
                                        attractorAxisW);
                                    toAttractor = 2 * toCenter - toAttractor;
                                    toAttractor.Normalize();
                                }
                                if(!controller.InvertAttractor && toAttractorDist < 1)
                                    dV *= toAttractorDist;
                                b.dP += b.rb.mass * dV * toAttractor;
                            }
                        }
                    }
                    b.dP = b.dP.ClampMagnitudeH(controller.MaxForce * TimeWarp.fixedDeltaTime);
                    var dL2 = Vector3.Dot(b.dAv.SquaredComponents(), b.rb.inertiaTensor);
                    var dP2 = b.dP.sqrMagnitude
                              * Utils.ClampH(b.relV.magnitude / RelativeVelocityThreshold, 1);
                    total_energy += dP2 / b.rb.mass + dP2 / h.mass + dL2;
                    dampedBodies[i] = b;
                }
                if(total_energy > 0)
                {
                    var energy_consumption = total_energy
                                             / TimeWarp.fixedDeltaTime
                                             * controller.EnergyConsumptionK;
                    var K = 1f;
                    if(energy_consumption > controller.MaxEnergyConsumption)
                    {
                        K = Mathf.Sqrt(controller.MaxEnergyConsumption / energy_consumption);
                        energy_consumption = controller.MaxEnergyConsumption;
                    }
                    for(var i = 0; i < nBodies; i++)
                    {
                        var b = dampedBodies[i];
                        if(b.rb == null)
                            continue;
                        if(K < 1)
                        {
                            b.dP *= K;
                            b.dAv *= K;
                        }
                        b.rb.AddTorque(b.dAv, ForceMode.VelocityChange);
                        b.rb.AddForce(-b.dP, ForceMode.Impulse);
                        h.AddForceAtPosition(b.dP, b.rb.position, ForceMode.Impulse);
                    }
                    controller.drainEnergy(energy_consumption);
                }
                dampedBodies.Clear();
            }

            private IEnumerator<YieldInstruction> damp_packed_vessels()
            {
                while(true)
                {
                    yield return new WaitForFixedUpdate();
                    if(dampedVessels.Count <= 0)
                        continue;
                    foreach(var vsl_info in dampedVessels.Values.ToList())
                    {
                        if(vsl_info.vessel != null && vsl_info.vessel.packed)
                        {
                            if(!vsl_info.positionSet)
                            {
                                vsl_info.SetPosition(transform
                                    .InverseTransformPoint(vsl_info.vessel.vesselTransform
                                        .position));
                                dampedVessels[vsl_info.id] = vsl_info;
                            }
                            vsl_info.vessel.SetPosition(
                                transform.TransformPoint(vsl_info.position));
                            if(vsl_info.energyConsumption > 0)
                                controller.drainEnergy(vsl_info.energyConsumption);
                        }
                        else
                            dampedVessels.Remove(vsl_info.id);
                    }
                }
            }

            private void track_packed_vessel(Part p)
            {
                if(dampedVessels.ContainsKey(p.vessel.persistentId))
                    return;
                dampedVessels[p.vessel.persistentId] = new VesselInfo
                {
                    id = p.vessel.persistentId,
                    vessel = p.vessel,
                    energyConsumption = p.TotalMass()
                                        * controller.EnergyConsumptionK
                                        * 0.01f
                };
            }

            private void OnTriggerStay(Collider col)
            {
                if(!enabled
                   || col == null
                   || col.attachedRigidbody == null)
                    return;
                if(!col.CompareTag("Untagged"))
                    return;
                var p = col.attachedRigidbody.GetComponent<Part>();
                if(p == null
                   || p.vessel == null
                   || p.vessel == controller.vessel
                   || !p.vessel.loaded)
                    return;
                if(p.vessel.isEVA && !controller.AffectKerbals)
                    return;
                if(tags != null && !tags.Any(t => p.partInfo.tags.Contains(t)))
                    return;
                if(!p.packed && !controller.part.packed)
                {
                    var r = col.attachedRigidbody;
                    dampedBodies.Add(new RBInfo { rb = r, part = p });
                    dampedVessels.Remove(p.vessel.persistentId);
                }
                else
                    track_packed_vessel(p);
            }
        }
    }
}
