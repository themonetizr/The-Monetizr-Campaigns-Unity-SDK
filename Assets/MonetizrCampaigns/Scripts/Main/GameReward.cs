using System;
using UnityEngine;
using Monetizr.SDK.Debug;

namespace Monetizr.SDK
{
    internal class GameReward
    {
        internal Sprite icon;
        internal string title;
        internal Func<ulong> _GetCurrencyFunc;
        internal Action<ulong> _AddCurrencyAction;
        internal ulong maximumAmount;

        public bool IsSetupValid ()
        {
            if (icon == null)
            {
                MonetizrLogger.PrintError("GameReward error: Icon is not set.");
                return false;
            }

            if (icon.rect.width < 128 || icon.rect.height < 128 || Mathf.Abs(icon.rect.height - icon.rect.width) > 0.1f)
            {
                MonetizrLogger.PrintError("GameReward error: Icon '" + icon.name + "' size less than 256 pixels on one or more dimensions or it's not square.");
                return false;
            }

            if (string.IsNullOrEmpty(title))
            {
                MonetizrLogger.PrintError("GameReward error: Title is empty.");
                return false;
            }

            if (_GetCurrencyFunc == null)
            {
                MonetizrLogger.PrintError("GameReward error: GetCurrency function is not set.");
                return false;
            }

            if (_AddCurrencyAction == null)
            {
                MonetizrLogger.PrintError("GameReward error: AddCurrency action is not set.");
                return false;
            }

            if (maximumAmount <= 0)
            {
                MonetizrLogger.PrintError("GameReward error: Maximum amount is zero or less.");
                return false;
            }

            return true;
        }

        internal ulong GetCurrencyFunc()
        {
            try
            {
                return _GetCurrencyFunc();
            }
            catch (Exception exception)
            {
                MonetizrLogger.PrintError($"Exception in GetCurrencyFunc of {title}\n{exception.Message}");
                return 0;
            }
        }

        internal void AddCurrencyAction(ulong amount)
        {
            try
            {
                _AddCurrencyAction(amount);
            }
            catch (Exception exception)
            {
                MonetizrLogger.PrintError($"Exception in AddCurrencyAction {amount} to {title}\n{exception}");
            }
        }

    }

}