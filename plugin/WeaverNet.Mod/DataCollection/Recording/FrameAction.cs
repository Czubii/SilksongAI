namespace WeaverNet.Mod.DataCollection.Recording
{
    public class FrameAction
    {
        public float HorizontalInput { get; }
        public float VerticalInput { get; }
        public bool JumpButton { get; }
        public bool AttackButton { get; }
        public bool HealButton { get; }
        public bool SkillButton { get; }
        public bool DashButton { get; }
        public bool HarpoonButton { get; }
        public FrameAction(
            float horizontalInput, 
            float verticalInput, 
            bool jumpButton,
            bool attackButton, 
            bool healButton, 
            bool skillButton, 
            bool dashButton, 
            bool harpoonButton)
        {
            HorizontalInput = horizontalInput;
            VerticalInput = verticalInput;
            JumpButton = jumpButton;
            AttackButton = attackButton;
            HealButton = healButton;
            SkillButton = skillButton;
            DashButton = dashButton;
            HarpoonButton = harpoonButton;
        }
    }
}
