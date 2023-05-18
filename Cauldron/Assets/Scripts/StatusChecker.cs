﻿namespace CauldronCodebase
{
    public class StatusChecker
    {
        private GameDataHandler gameDataHandler;
        private MainSettings _settings;

        public StatusChecker(MainSettings settings,
                             GameDataHandler dataHandler)
        {
            _settings = settings;
            gameDataHandler = dataHandler;
        }
        
        public EndingsProvider.Unlocks Run()
        {
            if (gameDataHandler.Fame >= _settings.statusBars.Total)
            {
                return EndingsProvider.Unlocks.HighFame;
            }

            if (gameDataHandler.Fear >= _settings.statusBars.Total)
            {
                return EndingsProvider.Unlocks.HighFear;
            }

            if (gameDataHandler.Money >= _settings.statusBars.Total)
            {
                return EndingsProvider.Unlocks.HighMoney;

            }

            if (gameDataHandler.Fame <= 0)
            {
                return EndingsProvider.Unlocks.LowFame;
            }

            if (gameDataHandler.Fear <= 0)
            {
                return EndingsProvider.Unlocks.LowFear;
            }

            return EndingsProvider.Unlocks.None;
        }

        public void CheckStatusesThreshold()
        {
            AddHighLowTag("high fear", Statustype.Fear);
            AddHighLowTag("low fear", Statustype.Fear, false);
            AddHighLowTag("high fame", Statustype.Fame);
            AddHighLowTag("low fame", Statustype.Fame, false);
        }

        private bool CheckThreshold(Statustype type, bool checkHigh)
        {
            int currentStatus = gameDataHandler.Get(type);
            if (gameDataHandler.ReachedMaxThreshold(type, checkHigh))
            {
                return false;
            }
            bool thresholdReached = false;
            bool nextThreshold = false;
            do
            {
                nextThreshold = checkHigh
                    ? currentStatus > gameDataHandler.GetThreshold(type, true)
                    : currentStatus < gameDataHandler.GetThreshold(type, false);
                if (nextThreshold)
                {
                    thresholdReached = true;
                }
            } while (nextThreshold && gameDataHandler.ChangeThreshold(type, checkHigh));

            return thresholdReached;
        }

        private void AddHighLowTag(string tag, Statustype type, bool checkHigh = true)
        {
            bool thresholdReached = CheckThreshold(type, checkHigh);
            if (thresholdReached)
            {
                gameDataHandler.AddTag(tag);
            }
            else
            {
                if(gameDataHandler.storyTags.Contains(tag))
                    gameDataHandler.storyTags.Remove(tag);
            }
        }
    }
}