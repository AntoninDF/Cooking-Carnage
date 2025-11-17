

using System;
using Unity.VisualScripting;

namespace GameSystem
{

    public struct MoneyInfo : IStatInfo
    {
        #region Properties

        #endregion

        #region Fields
        private float _amount;

        #endregion

        #region Methods
        public MoneyInfo(float amount)
        {
            this._amount = amount;
        }
        public static MoneyInfo operator +(MoneyInfo mi) => mi;
        public static MoneyInfo operator -(MoneyInfo mi) => new MoneyInfo(-mi._amount);
        public static MoneyInfo operator +(float amount, MoneyInfo mi) => new MoneyInfo(amount + mi._amount);
        public static MoneyInfo operator +(MoneyInfo mi, float amount) => amount + mi;
        public static MoneyInfo operator +(MoneyInfo left, MoneyInfo right) => new MoneyInfo(left._amount + right._amount);
        public static MoneyInfo operator -(float amount, MoneyInfo mi) => amount + (-mi);
        public static MoneyInfo operator -(MoneyInfo mi, float amount) => amount + (-mi);
        public static MoneyInfo operator -(MoneyInfo left, MoneyInfo right) => left + (-right);
        public static MoneyInfo operator ++(MoneyInfo mi) => new MoneyInfo(mi._amount + 1f);
        public static MoneyInfo operator --(MoneyInfo mi) => new MoneyInfo(mi._amount - 1f);


        public static implicit operator float(MoneyInfo mi) => mi._amount;
        public static implicit operator string(MoneyInfo mi) => MathF.Round(mi._amount, 2).ToString() + "€";
        public override string ToString() => MathF.Round(_amount, 2).ToString() + "€";
        

        #endregion
    }
}