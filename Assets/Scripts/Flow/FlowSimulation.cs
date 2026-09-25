using UnityEngine;

namespace FlowGame
{
    // "流れを結ぶ": 線をなぞるのではなく、動き続ける粒子の流れに触れるアート/ゲーム。
    // 節(node)を流れの向きに沿ってなぞると充填し、二節が繋がると安定して完成する。
    public class FlowSimulation : MonoBehaviour
    {
        public enum FlowState { Playing, Settling, Cleared, Failed }

        [Header("粒子")]
        public int ParticleCount = 160;
        public int TrailLength = 6;
        public float BaseAngularSpeed = 0.35f;

        [Header("リング形状")]
        public float RingBaseRadius = 3.2f;
        public float RingWobbleAmplitude = 0.35f;
        public int RingWobbleFrequency = 3;
        public float RadialWobbleAmplitude = 0.12f;
        public float RadialLaneRange = 0.22f;

        [Header("節")]
        public float[] NodeU = { 0.08f, Mathf.PI + 0.08f };
        public float NodeCaptureAngle = 0.4f;
        public float ChargeThreshold = 0.72f;
        public float InitialNodeCharge = 0.12f;
        public float ChargeGainRate = 0.9f;
        public float ChargeDecayRate = 0.5f;
        public float AlignThreshold = 0.5f;
        public float MinStrokeSpeed = 0.6f;
        public float MaxStrokeSpeed = 6.0f;

        [Header("乱流")]
        public float TurbulenceGainRate = 0.8f;
        public float TurbulenceDecayRate = 0.25f;
        public float TurbulenceSlowFactor = 0.5f;
        public float CalmRate = 0.4f;

        [Header("入力の影響")]
        public float InfluenceRadius = 1.1f;
        public float InputStrength = 1.4f;

        [Header("ドリフト物理（触れた余韻）")]
        public float SpringK = 1.6f;
        public float DampingK = 2.4f;
        public float DriftSpeedNormalizer = 2.2f;
        public float GlowNormalizer = 1.8f;

        [Header("ゲーム進行")]
        public int MaxStrokes = 4;
        public float SettleStabilityThreshold = 0.70f;
        public float SettleHoldRequired = 1.75f;

        [Header("再現性")]
        public int Seed = 19027;

        public float[] NodeCharge { get; private set; } = new float[2];
        public FlowState State { get; private set; } = FlowState.Playing;
        public int StrokesRemaining { get; private set; }
        public float Stability { get; private set; } = 1f;
        public float Turbulence { get; private set; }
        public float SettleHoldTimer { get; private set; }

        private float[] u;
        private float[] uSpeed;
        private float[] radialLane;
        private float[] radialFreq;
        private float[] radialPhase;
        private float[] driftX, driftY;
        private float[] driftVelX, driftVelY;

        private float[,] trailPosX;
        private float[,] trailPosY;
        private int trailHead;

        private float simTime;
        private bool dragActive;

        public int ActiveParticleCount { get; private set; }
        public int ActiveTrailLength { get; private set; }

        public void Setup()
        {
            Rebuild();
        }

        public void ResetSimulation()
        {
            Rebuild();
        }

        private void Rebuild()
        {
            ActiveParticleCount = Mathf.Max(1, ParticleCount);
            ActiveTrailLength = Mathf.Max(2, TrailLength);

            u = new float[ActiveParticleCount];
            uSpeed = new float[ActiveParticleCount];
            radialLane = new float[ActiveParticleCount];
            radialFreq = new float[ActiveParticleCount];
            radialPhase = new float[ActiveParticleCount];
            driftX = new float[ActiveParticleCount];
            driftY = new float[ActiveParticleCount];
            driftVelX = new float[ActiveParticleCount];
            driftVelY = new float[ActiveParticleCount];
            trailPosX = new float[ActiveParticleCount, ActiveTrailLength];
            trailPosY = new float[ActiveParticleCount, ActiveTrailLength];

            var rng = new FlowSeededRandom(Seed);
            for (int i = 0; i < ActiveParticleCount; i++)
            {
                u[i] = rng.Range(0f, Mathf.PI * 2f);
                uSpeed[i] = BaseAngularSpeed * rng.Range(0.85f, 1.15f);
                radialLane[i] = rng.Range(-RadialLaneRange, RadialLaneRange);
                radialFreq[i] = rng.Range(0.4f, 1.1f);
                radialPhase[i] = rng.Range(0f, Mathf.PI * 2f);
                driftX[i] = 0f;
                driftY[i] = 0f;
                driftVelX[i] = 0f;
                driftVelY[i] = 0f;
            }

            simTime = 0f;
            trailHead = 0;
            for (int i = 0; i < ActiveParticleCount; i++)
            {
                Vector2 p = ComputeParticlePosition(i);
                for (int k = 0; k < ActiveTrailLength; k++)
                {
                    trailPosX[i, k] = p.x;
                    trailPosY[i, k] = p.y;
                }
            }

            NodeCharge[0] = InitialNodeCharge;
            NodeCharge[1] = InitialNodeCharge;
            Turbulence = 0f;
            Stability = 1f;
            StrokesRemaining = MaxStrokes;
            SettleHoldTimer = 0f;
            dragActive = false;
            State = FlowState.Playing;
        }

        public Vector2 CurvePoint(float uu)
        {
            float r = RingBaseRadius + RingWobbleAmplitude * Mathf.Sin(RingWobbleFrequency * uu);
            return new Vector2(r * Mathf.Cos(uu), r * Mathf.Sin(uu));
        }

        public Vector2 CurveTangent(float uu)
        {
            const float h = 0.01f;
            Vector2 p0 = CurvePoint(uu - h);
            Vector2 p1 = CurvePoint(uu + h);
            Vector2 d = p1 - p0;
            return d.sqrMagnitude > 1e-8f ? d.normalized : Vector2.right;
        }

        public Vector2 NodeWorldPos(int n) => CurvePoint(NodeU[n]);
        public Vector2 NodeTangent(int n) => CurveTangent(NodeU[n]);

        private Vector2 ComputeParticlePosition(int i)
        {
            Vector2 baseP = CurvePoint(u[i]);
            Vector2 tangent = CurveTangent(u[i]);
            Vector2 normal = new Vector2(-tangent.y, tangent.x);
            float radial = radialLane[i] + RadialWobbleAmplitude * Mathf.Sin(simTime * radialFreq[i] + radialPhase[i]);
            return baseP + normal * radial + new Vector2(driftX[i], driftY[i]);
        }

        public Vector2 GetTrailPoint(int i, int age)
        {
            int idx = ((trailHead - age) % ActiveTrailLength + ActiveTrailLength) % ActiveTrailLength;
            return new Vector2(trailPosX[i, idx], trailPosY[i, idx]);
        }

        public float GetParticleGlow(int i)
        {
            float speed = new Vector2(driftVelX[i], driftVelY[i]).magnitude;
            return Mathf.Clamp01(speed / GlowNormalizer);
        }

        public void BeginDrag()
        {
            dragActive = true;
        }

        public void EndDrag(bool countsAsStroke)
        {
            dragActive = false;
            if (countsAsStroke && State == FlowState.Playing)
            {
                StrokesRemaining = Mathf.Max(0, StrokesRemaining - 1);
            }
        }

        public void ApplyDragSample(Vector2 worldPos, Vector2 worldVelocity, float dt)
        {
            if (dt <= 0f) return;

            float speed = worldVelocity.magnitude;
            Vector2 velDir = speed > 0.0001f ? worldVelocity / speed : Vector2.zero;

            for (int i = 0; i < ActiveParticleCount; i++)
            {
                Vector2 pos = ComputeParticlePosition(i);
                float dist = Vector2.Distance(pos, worldPos);
                if (dist < InfluenceRadius)
                {
                    float falloff = 1f - dist / InfluenceRadius;
                    driftVelX[i] += worldVelocity.x * InputStrength * falloff * dt;
                    driftVelY[i] += worldVelocity.y * InputStrength * falloff * dt;
                }
            }

            if (State != FlowState.Playing && State != FlowState.Settling) return;

            Vector2 center = transform.position;
            Vector2 rel = worldPos - center;
            float pointerAngle = Mathf.Atan2(rel.y, rel.x);
            if (pointerAngle < 0f) pointerAngle += Mathf.PI * 2f;

            for (int n = 0; n < 2; n++)
            {
                float angDiff = Mathf.Abs(Mathf.DeltaAngle(pointerAngle * Mathf.Rad2Deg, NodeU[n] * Mathf.Rad2Deg)) * Mathf.Deg2Rad;
                if (angDiff >= NodeCaptureAngle) continue;

                Vector2 tangent = CurveTangent(NodeU[n]);
                float align = speed > 0.0001f ? Vector2.Dot(velDir, tangent) : 0f;

                if (align > AlignThreshold && speed > MinStrokeSpeed && speed < MaxStrokeSpeed)
                {
                    NodeCharge[n] = Mathf.Clamp01(NodeCharge[n] + ChargeGainRate * align * dt);
                    Turbulence = Mathf.Max(0f, Turbulence - CalmRate * dt);
                }
                else if (align < -AlignThreshold)
                {
                    Turbulence = Mathf.Clamp01(Turbulence + TurbulenceGainRate * dt);
                    NodeCharge[n] = Mathf.Clamp01(NodeCharge[n] - ChargeDecayRate * dt);
                }
                else if (speed >= MaxStrokeSpeed)
                {
                    Turbulence = Mathf.Clamp01(Turbulence + TurbulenceGainRate * 0.6f * dt);
                }
            }
        }

        private void FixedUpdate()
        {
            if (u == null) return;

            float dt = Time.fixedDeltaTime;
            simTime += dt;
            Turbulence = Mathf.Max(0f, Turbulence - TurbulenceDecayRate * dt);

            for (int i = 0; i < ActiveParticleCount; i++)
            {
                u[i] += uSpeed[i] * dt * (1f + Turbulence * TurbulenceSlowFactor);
                if (u[i] > Mathf.PI * 2f) u[i] -= Mathf.PI * 2f;

                Vector2 d = new Vector2(driftX[i], driftY[i]);
                Vector2 dv = new Vector2(driftVelX[i], driftVelY[i]);
                Vector2 accel = -SpringK * d - DampingK * dv;
                dv += accel * dt;
                d += dv * dt;
                driftX[i] = d.x;
                driftY[i] = d.y;
                driftVelX[i] = dv.x;
                driftVelY[i] = dv.y;
            }

            trailHead = (trailHead + 1) % ActiveTrailLength;
            float driftSpeedSum = 0f;
            for (int i = 0; i < ActiveParticleCount; i++)
            {
                Vector2 pos = ComputeParticlePosition(i);
                trailPosX[i, trailHead] = pos.x;
                trailPosY[i, trailHead] = pos.y;
                driftSpeedSum += new Vector2(driftVelX[i], driftVelY[i]).magnitude;
            }

            Stability = 1f - Mathf.Clamp01((driftSpeedSum / ActiveParticleCount) / DriftSpeedNormalizer);

            UpdateStateMachine(dt);
        }

        private void UpdateStateMachine(float dt)
        {
            bool bothConnected = NodeCharge[0] >= ChargeThreshold && NodeCharge[1] >= ChargeThreshold;

            switch (State)
            {
                case FlowState.Playing:
                    if (bothConnected && !dragActive)
                    {
                        State = FlowState.Settling;
                        SettleHoldTimer = 0f;
                    }
                    else if (StrokesRemaining <= 0 && !bothConnected)
                    {
                        State = FlowState.Failed;
                    }
                    break;

                case FlowState.Settling:
                    if (!bothConnected)
                    {
                        State = FlowState.Playing;
                        break;
                    }
                    if (Stability >= SettleStabilityThreshold)
                        SettleHoldTimer += dt;
                    else
                        SettleHoldTimer = 0f;

                    if (SettleHoldTimer >= SettleHoldRequired)
                        State = FlowState.Cleared;
                    break;

                case FlowState.Cleared:
                case FlowState.Failed:
                    break;
            }
        }
    }
}
