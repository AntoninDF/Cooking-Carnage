using System;

namespace Features
{
    public class Cuttable : CookingFeature
    {
        #region Properties
        public override bool Completed => IsCut;
        public override bool CanBePerformed => CanBeCut();
        public bool IsCut
        {
            get => _isCut;
            set
            {
                if (_isCut == value) return;
                _isCut = value;
                if (_isCut) OnCut?.Invoke();
            }
        }
        public float CuttingDuration { get; set; } = 2.5f;



        public Action OnCut;
        public Action OnCuttingStart;
        public Action OnCuttingEnd;
        #endregion

        private bool _isCut;


        #region Methods

        public Cuttable(IFeatureCollection owner) : base(owner)
        {
        }

        public void StartCutting()
        {
            OnCuttingStart?.Invoke();
        }
        public void StopCutting()
        {
            OnCuttingEnd?.Invoke();
        }
        public bool RequestCut()
        {
            if (!CanBeCut())
                return false;
            Cut();
            return true;
        }
        public bool CanBeCut()
        {
            if (!Enabled) return false;
            if (IsCut) return false;

            return true;
        }
        private void Cut()
        {
            IsCut = true;
            StopCutting();
        }
        public override void Reset()
        {
            base.Reset();
            IsCut = false;
        }
        #endregion
    }
}