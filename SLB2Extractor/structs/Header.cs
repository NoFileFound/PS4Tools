namespace SLB2Extractor.structs
{
    struct Header
    {
        public string magic;
        public ulong version;
        public uint fileCount;
        public uint blockCount;
        public byte[] reserved;
        public Entry[] fileEntry;
    }
}