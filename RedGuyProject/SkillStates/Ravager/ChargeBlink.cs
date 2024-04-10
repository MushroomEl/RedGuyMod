using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RedGuyMod.SkillStates.Ravager
{
    public class ChargeBlink : ChargeJump
    {
        private Transform modelTransform;
        private GameObject predictionEffectInstance;

        public override void OnEnter()
        {
            this.duration = 0.6f;
            base.OnEnter();
            this.modelTransform = this.GetModelTransform();

            if (this.modelTransform)
            {
                TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = this.duration;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpDissolve.mat").WaitForCompletion();
                temporaryOverlay.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
            }

            if (base.isAuthority)
            {
                this.predictionEffectInstance = GameObject.Instantiate(Modules.Assets.blinkPredictionEffect);
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (this.predictionEffectInstance)
            {
                foreach( ParticleSystem i in this.predictionEffectInstance.GetComponentsInChildren<ParticleSystem>())
                {
                    if (i) i.Stop();
                }

                GameObject.Destroy(this.predictionEffectInstance, 5f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.characterBody.isSprinting = true;

            if (this.predictionEffectInstance)
            {
                Ray aimRay = this.GetAimRay();

                float movespeed = Mathf.Clamp(this.characterBody.moveSpeed, 1f, 18f);
                float charge = Mathf.Clamp01(Util.Remap(base.fixedAge, 0f, this.duration, 0f, 1f));
                float fakeDuration = 0.35f;
                float fakeJumpForce = this.jumpForce = (Util.Remap(charge, 0f, 1f, 0.17733990147f, 0.37334975369f) * this.characterBody.jumpPower * movespeed);

                Vector3 predictedPos = this.transform.position + (aimRay.direction * (this.jumpForce * 1.5f * fakeDuration));
                this.predictionEffectInstance.transform.position = predictedPos;
            }
        }

        protected override void PlayAnim()
        {
            base.PlayCrossfade("Body", "BlinkCharge", "Jump.playbackRate", this.duration, 0.1f);
        }

        protected override void SetJumpTime()
        {
            this.jumpTime = 0.5f;

            EffectData effectData = new EffectData();
            effectData.rotation = Util.QuaternionSafeLookRotation(this.jumpDir);
            effectData.origin = Util.GetCorePosition(this.gameObject);
            EffectManager.SpawnEffect(EntityStates.ImpMonster.BlinkState.blinkPrefab, effectData, false);
        }

        protected override void NextState()
        {
            this.outer.SetNextState(new BlinkBig
            {
                jumpDir = this.jumpDir,
                jumpForce = this.jumpForce * 1.5f
            });
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}