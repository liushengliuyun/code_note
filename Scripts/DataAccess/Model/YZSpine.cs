using Spine;

namespace DataAccess.Model
{
    public enum YZSpine
    {
        attack1,
        attack2,
        attack3,
        attack4,
        idea,
        win,
        death
    }

    public class SpineAnimationHandler
    {
        private AnimationState AnimationState;

        private YZSpine endState = YZSpine.idea;

        public SpineAnimationHandler(AnimationState animationState)
        {
            AnimationState = animationState;
            AnimationState.Complete += EndCallBack;
        }

        public void SetEndState(YZSpine state)
        {
            endState = state;
        }

        private void EndCallBack(TrackEntry trackEntry)
        {
            Play(endState);
        }

        public void Play(YZSpine animation, YZSpine endAnimation, float beginTime = 0)
        {
            var trackEntry = AnimationState.SetAnimation(0, animation.ToString(), false);

            SetEndState(endAnimation);

            if (beginTime > 0)
            {
                trackEntry.TrackTime = beginTime;
            }
        }

        public void Play(YZSpine animation, float beginTime)
        {
            var trackEntry = AnimationState.SetAnimation(0, animation.ToString(), false);


            if (beginTime > 0)
            {
                trackEntry.TrackTime = beginTime;
            }
        }

        public void Play(YZSpine animation, bool isLoop = false, float beginTime = 0)
        {
            var trackEntry = AnimationState.SetAnimation(0, animation.ToString(), false);

            if (isLoop)
            {
                SetEndState(animation);
            }

            if (beginTime > 0)
            {
                trackEntry.TrackTime = beginTime;
            }
        }
    }
}