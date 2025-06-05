using System;
using System.Collections.Generic;

namespace MyNotch
{
    public class CallList
    {
        readonly List<Action<Action>> _callBackList;
        int _endedNum = 0;

        public CallList()
        {
            _callBackList = new List<Action<Action>>();
            _endedNum = 0;
        }

        public void Join(Action<Action> action)
        {
            _callBackList.Add(action);
        }

        public void Begin(Action completeCallBack = null)
        {
            var count = _callBackList.Count;
            if (count > 0)
            {
                foreach (var call in _callBackList)
                {
                    call(() =>
                    {
                        _endedNum++;
                        if (_endedNum == count)
                        {
                            completeCallBack?.Invoke();
                        }
                    });
                }
            }
            else
            {
                completeCallBack?.Invoke();
            }
        }
    }
}