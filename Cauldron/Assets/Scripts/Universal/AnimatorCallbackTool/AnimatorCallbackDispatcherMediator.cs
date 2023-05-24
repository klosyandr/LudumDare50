using System.Collections.Generic;
using Client.Client.Scripts.Common.Utils;
using Client.Common.AnimatorCallbackTool;
using UnityEngine;

namespace Client.Common.AnimatorTools
{
    public class AnimatorCallbackDispatcherMediator: MonoBehaviour
    {
        [SerializeField] private List<AnimatorCallbackReceiverMediator> _receivers = new(1);
        
        public void Register(AnimatorCallbackReceiverMediator receiverMediator)
        {
            _receivers.Add(receiverMediator);
        }

        public void Unregister(AnimatorCallbackReceiverMediator receiverMediator)
        {
            _receivers.Remove(receiverMediator);
        }

        public void OnAnimationCallback(AnimatorCallbackInfo info)
        {
            foreach (AnimatorCallbackReceiverMediator receiver in _receivers)
            {
                if (receiver is null)
                {
                    GameLogger.LogWarning("Empty elements in animation callback mediator list on object "+gameObject.name);
                    continue;
                }
                receiver.OnAnimationCallback(info);
            }
        }
    }
}