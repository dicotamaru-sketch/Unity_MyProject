namespace FlowGame
{
    public class FlowSeededRandom
    {
        private uint state;

        public FlowSeededRandom(int seed)
        {
            state = seed == 0 ? 1u : unchecked((uint)seed);
        }

        public float NextFloat()
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return (state & 0x00FFFFFF) / (float)0x01000000;
        }

        public float Range(float min, float max)
        {
            return min + NextFloat() * (max - min);
        }
    }
}
