namespace Lib.Animation.Animator
{
    public static class MinionAnimationType
    {
        public const string Idle = "Idle";
        public const string SlidingBack = "SlidingBack";
        public const string SlidingFront = "SlidingFront";
        public const string SlidingSideLeft = "SlidingSideLeft";
        public const string SlidingSideRight = "SlidingSideRight";
    }

    public class MinionAnimationLibrary : AnimationInfoLibrary
    {
        protected override void InitializeAnimationDictionary()
        {
            AnimationDictionary.Add(MinionAnimationType.Idle, "Idle");
            AnimationDictionary.Add(MinionAnimationType.SlidingBack, "SlidingBack");
            AnimationDictionary.Add(MinionAnimationType.SlidingFront, "SlidingFront");
            AnimationDictionary.Add(MinionAnimationType.SlidingSideLeft, "SlidingSideLeft");
            AnimationDictionary.Add(MinionAnimationType.SlidingSideRight, "SlidingSideRight");
        }
    }
}

