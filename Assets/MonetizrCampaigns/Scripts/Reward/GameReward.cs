using System;
using UnityEngine;
using Monetizr.SDK.Debug;

namespace Monetizr.SDK.Core
{
    public partial class MonetizrManager
    {
        internal class GameReward
        {
            internal Sprite icon;
            internal string title;
            internal Func<ulong> _GetCurrencyFunc;
            internal Action<ulong> _AddCurrencyAction;
            internal ulong maximumAmount;

            public bool Validate()
            {
                bool isRewardValid = true;

                if (icon == null)
                {
                    Log.PrintError("GameReward error: Icon is not set.");
                    isRewardValid = false;
                }
                else
                {
                    float iconWidth = icon.rect.width;
                    float iconHeight = icon.rect.height;

                    if (iconWidth < 128 || iconHeight < 128 || Mathf.Abs(iconHeight - iconWidth) > 0.1f)
                    {
                        Log.PrintError("GameReward error: Icon '" + icon.name + "' size less than 256 pixels on one or more dimensions or it's not square.");
                        isRewardValid = false;
                    }
                }

                if (string.IsNullOrEmpty(title))
                {
                    Log.PrintError("GameReward error: Title is empty.");
                    isRewardValid = false;
                }

                if (_GetCurrencyFunc == null)
                {
                    Log.PrintError("GameReward error: GetCurrency function is not set.");
                    isRewardValid = false;
                }

                if (_AddCurrencyAction == null)
                {
                    Log.PrintError("GameReward error: AddCurrency action is not set.");
                    isRewardValid = false;
                }

                if (maximumAmount <= 0)
                {
                    Log.PrintError("GameReward error: Maximum amount is zero or less.");
                    isRewardValid = false;
                }

                return isRewardValid;
            }

            internal ulong GetCurrencyFunc()
            {
                try
                {
                    return _GetCurrencyFunc();
                }
                catch (Exception exception)
                {
                    Log.PrintError($"Exception in GetCurrencyFunc of {title}\n{exception.Message}");
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
                    Log.PrintError($"Exception in AddCurrencyAction {amount} to {title}\n{exception}");
                }
            }
        }

    }

}