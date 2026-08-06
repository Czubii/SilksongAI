namespace WeaverNet.Mod.DataCollection.Recording
{
    public class HeroObservation
    {
        public float PosX { get; }
        public float PosY { get; }
        public int HP { get; }
        public int Silk { get; }
        public bool IsStunned { get; }
        public bool CanJump { get; }
        public bool CanDoubleJump { get; }
        public bool CanAttack { get; }
        public bool CanSprint { get; }
        public bool CanBind { get; }
        public bool CanCast { get; }
        public bool CanNailArt { get; }
        public bool CanTryHarpoon { get; }
        public bool CanBackDash { get; }

        public HeroObservation(
            float posX,
            float posY,
            int hp,
            int silk,
            bool isStunned,
            bool canJump,
            bool canDoubleJump,
            bool canAttack,
            bool canSprint,
            bool canBind,
            bool canCast,
            bool canNailArt,
            bool canTryHarpoon,
            bool canBackDash)
        {
            PosX = posX;
            PosY = posY;
            HP = hp;
            Silk = silk;
            IsStunned = isStunned;
            CanJump = canJump;
            CanDoubleJump = canDoubleJump;
            CanAttack = canAttack;
            CanSprint = canSprint;
            CanBind = canBind;
            CanCast = canCast;
            CanNailArt = canNailArt;
            CanTryHarpoon = canTryHarpoon;
            CanBackDash = canBackDash;
        }
    }
}
