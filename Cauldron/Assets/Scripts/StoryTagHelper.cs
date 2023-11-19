using System;
using System.Collections.Generic;
using UnityEngine;

namespace CauldronCodebase
{
    public static class StoryTagHelper
    {
        public static void SaveMilestone(string tag)
        {
            StringListWrapper tags;
            if (PlayerPrefs.HasKey(PrefKeys.Milestones))
            {
                var encodedTags = PlayerPrefs.GetString(PrefKeys.Milestones);
                tags = JsonUtility.FromJson<StringListWrapper>(encodedTags);
            }
            else
            {
                tags = new StringListWrapper();
            }
            tags.list.Add(tag);
            PlayerPrefs.SetString(PrefKeys.Milestones, JsonUtility.ToJson(tags));
        }

        public static List<string> GetMilestones()
        {
            if (PlayerPrefs.HasKey(PrefKeys.Milestones))
            {
                var encodedTags = PlayerPrefs.GetString(PrefKeys.Milestones);
                return JsonUtility.FromJson<StringListWrapper>(encodedTags).list;
            }
            return new List<string>();
        }
        
        public static bool Check(Encounter card, GameDataHandler gameDataHandler)
        {
            return Check(card.requiredStoryTag, gameDataHandler);
        }
        
        public static bool Check(string tagLine, GameDataHandler gameDataHandler)
        {
            if (string.IsNullOrWhiteSpace(tagLine.Trim()))
            {
                return true;
            }

            string[] tags = tagLine.Split(',');
            bool valid = true;
            foreach (var tag in tags)
            {
                string trim = tag.Trim();
                if (trim == EndingsProvider.LOW_FAME || trim == EndingsProvider.LOW_FEAR ||
                    trim == EndingsProvider.HIGH_FAME || trim == EndingsProvider.HIGH_FEAR)
                {
                    continue;
                }
                if (tag.StartsWith("!"))
                {
                    valid = valid && !gameDataHandler.storyTags.Contains(trim.TrimStart('!'));
                }
                else
                {
                    valid = valid && gameDataHandler.storyTags.Contains(trim);
                }
            }

            return valid;
        }
    }
}