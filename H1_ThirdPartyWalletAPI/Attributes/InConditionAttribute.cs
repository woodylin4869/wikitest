using System.ComponentModel.DataAnnotations;
using System.Linq;
using H1_ThirdPartyWalletAPI.Extensions;

namespace H1_ThirdPartyWalletAPI.Attributes
{
    /// <summary>
    /// 條件驗證
    /// </summary>
    public class InConditionAttribute : ValidationAttribute
    {
        /// <summary>
        /// 字串條件
        /// </summary>
        public string[] StringCondition { get; }

        public bool IsCaseSensitivity = true;

        /// <summary>
        /// 數字條件
        /// </summary>
        public int[] IntCondition { get; }

        /// <summary>
        /// 建構子
        /// </summary>
        public InConditionAttribute(params string[] stringCondition)
        {
            StringCondition = stringCondition;
        }

        /// <summary>
        /// 建構子
        /// </summary>
        public InConditionAttribute(bool isCaseSensitivity = true, params string[] stringCondition)
        {
            StringCondition = stringCondition;
            IsCaseSensitivity = isCaseSensitivity;
        }

        /// <summary>
        /// 建構子
        /// </summary>
        public InConditionAttribute(params int[] intCondition)
        {
            IntCondition = intCondition;
        }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        /// <returns></returns>
        public string GetErrorMessage() => ErrorMessage;

        /// <summary>
        /// 驗證
        /// </summary>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string strValue &&
                (IsCaseSensitivity == true ?
                    !StringCondition.Any(x => x == strValue) : !StringCondition.Any(x => x.ToLower() == strValue.ToLower())))
                return new ValidationResult(GetErrorMessage());

            if (value is int intValue &&
                !IntCondition.Any(x => x == intValue))
                return new ValidationResult(GetErrorMessage());

            return ValidationResult.Success;
        }
    }
}