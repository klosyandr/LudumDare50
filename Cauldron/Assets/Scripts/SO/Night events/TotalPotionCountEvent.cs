using UnityEngine;
using EasyLoc;
using System.Collections.Generic;

namespace CauldronCodebase
{
    [CreateAssetMenu(fileName = "New condition", menuName = "Night event/Total potion check", order = 8)]
    public class TotalPotionCountEvent : ConditionalEvent
    {
        [Header("Conditions work only once")]
        public Potions type;
        public int threshold = 3;

        public override bool Valid(GameDataHandler game)
        {
            if (!StoryTagHelper.Check(requiredTag, game))
            {
                return false;
            }
            int count = 0;
            foreach (Potions potion in game.PotionsOnLastDays)
            {
                if (potion == type)
                {
                    count++;
                }

                if (count >= threshold)
                {
                    return true;
                }
            }
            return false;
        }
    }
}