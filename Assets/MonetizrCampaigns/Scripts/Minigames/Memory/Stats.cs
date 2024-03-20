using System;
using System.Collections.Generic;

namespace Monetizr.SDK
{
    internal partial class MonetizrMemoryGame
    {
        internal class Stats
        {
            public DateTime gameStartTime;
            public DateTime lastTapTime;
            public int amountOfTotalTaps;
            public int amountOfTapsOnKnownCells;
            public int amountOfTapsOnDisabledCells;
            public double totalTime;
            public double averageTimeBetweenTaps;
            public double timeBeforeFirstTap;
            public int firstTapPiece;
            public bool isSkipped;

            public Dictionary<string, string> GetDictionary()
            {
                var result = new Dictionary<string, string>();

                result.Add("amount_of_total_taps", amountOfTotalTaps.ToString());
                result.Add("amount_of_taps_on_known_cells", amountOfTapsOnKnownCells.ToString());
                result.Add("amount_of_taps_on_disabled_cells", amountOfTapsOnDisabledCells.ToString());
                result.Add("total_time", totalTime.ToString());
                result.Add("avarage_time_between_taps", averageTimeBetweenTaps.ToString());
                result.Add("time_beforefirsttap", timeBeforeFirstTap.ToString());
                result.Add("first_tap_piece", firstTapPiece.ToString());
                result.Add("is_skipped", isSkipped ? "true" : "false");

                return result;
            }

            public override string ToString()
            {
                return $"GameStartTime: {gameStartTime}, LastTapTime: {lastTapTime}, AmountOfTotalTaps: {amountOfTotalTaps}," +
                       $"AmountOfTapsOnOpenedCells: {amountOfTapsOnKnownCells}, AmountOfTapsOnDisabledCells: {amountOfTapsOnDisabledCells}, TotalTime: {totalTime}, Avarage TimeBetween Taps:" +
                        $"{averageTimeBetweenTaps}, TimeBeforeFirstTap:{timeBeforeFirstTap}, FirstTapPiece:{firstTapPiece}," +
                        $"IsSkipped:{isSkipped}";
            }
        }
    }

}