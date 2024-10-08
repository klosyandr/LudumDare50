using Spine.Unity;
using UnityEngine;
using Zenject;

namespace CauldronCodebase
{
    public class BubbleVisitorTimer: VisitorTimer
    {
        public SkeletonGraphic clockAnimation;
        public Transform prefab;
        public float angleSpan = 15f;

        private Animator[] items;
        private int currentAttempts;
        private static readonly int Use = Animator.StringToHash("Use");

        [Inject] private SoundManager soundManager;

        private void Start()
        {
            prefab.gameObject.SetActive(false);
        }

        public override void ReduceTimer()
        {
            soundManager.Play(Sounds.TimerBreak);
            currentAttempts--;
            clockAnimation.AnimationState.SetAnimation(1, "Active", false);
            clockAnimation.AnimationState.AddEmptyAnimation(1, 0.2f, 0f);
            items[currentAttempts].SetTrigger(Use);
        }

        public override void ResetTimer(int attempts)
        {
            Clear();

            items = new Animator[attempts];
            Vector3 position = prefab.localPosition;
            for (int i = 0; i < attempts; i++)
            {
                items[i] = Instantiate(prefab, transform).GetComponent<Animator>();
                items[i].transform.localPosition = position + Vector3.left * angleSpan * i;  
                items[i].gameObject.SetActive(true);
            }
            currentAttempts = attempts;
        }

        private void Clear()
        {
            if (items != null)
            {
                foreach (var animator in items)
                {
                    Destroy(animator.gameObject);
                }
            }
        }
    }
}