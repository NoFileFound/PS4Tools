namespace ENVEncrypt.IO
{
    public class ByteArray
    {
        private int position = 0;
        private bool isBigEndian;
        private List<byte> buffer;

        private byte[] getBytes(int pos)
        {
            byte[] bytes = new byte[pos];

            for (int x = this.position, i = 0; x < this.position + pos; x++, i++)
            {
                bytes[i] = this.buffer[x];
            }
            if (BitConverter.IsLittleEndian && this.isBigEndian)
            {
                Array.Reverse(bytes);
            }

            this.position += pos;
            return bytes;
        }

        public byte this[int i] => this.buffer[i];

        public ByteArray(byte[] bytes)
        {
            this.buffer = new List<byte>(bytes);
            this.isBigEndian = false;
        }

        public ByteArray(byte[] bytes, bool bigEndian)
        {
            this.buffer = new List<byte>(bytes);
            this.isBigEndian = bigEndian;
        }

        public ByteArray(int length)
        {
            this.buffer = new List<byte>(length);
            this.isBigEndian = false;
        }

        public ByteArray(int length, bool bigEndian)
        {
            this.buffer = new List<byte>(length);
            this.isBigEndian = bigEndian;

        }

        public byte[] ToArray()
        {
            return this.buffer.ToArray();
        }

        public int Length()
        {
            return this.buffer.Count;
        }

        public byte ReadByte()
        {
            return this.getBytes(1)[0];
        }

        public byte[] ReadBytes(int length)
        {
            return this.getBytes(length);
        }

        public bool ReadBool()
        {
            return this.getBytes(1)[0] == 0 ? false : true;
        }

        public char ReadChar()
        {
            return (char)this.ReadByte();
        }

        public float ReadFloat()
        {
            return BitConverter.ToSingle(this.getBytes(4), 0);
        }

        public double ReadDouble()
        {
            return BitConverter.ToDouble(this.getBytes(8), 0);
        }

        public short ReadInt16()
        {
            return BitConverter.ToInt16(this.getBytes(2), 0);
        }

        public ushort ReadUInt16()
        {
            return BitConverter.ToUInt16(this.getBytes(2), 0);
        }

        public int ReadInt32()
        {
            return BitConverter.ToInt32(this.getBytes(4), 0);
        }

        public uint ReadUInt32()
        {
            return BitConverter.ToUInt32(this.getBytes(4), 0);
        }

        public long ReadInt64()
        {
            return BitConverter.ToInt64(this.getBytes(8), 0);
        }

        public ulong ReadUInt64()
        {
            return BitConverter.ToUInt64(this.getBytes(8), 0);
        }

        public string ReadString(int size)
        {
            string str = "";
            byte[] estring = new byte[size];

            for (int x = this.position, i = 0; x < this.position + size; x++, i++)
            {
                estring[i] = this.buffer[x];
            }

            foreach (byte x in estring)
            {
                str += (char)x;
            }

            this.position += size;
            return str;
        }

        public void WriteByte(byte x)
        {
            this.buffer.Add(x);
        }

        public void WriteBytes(byte[] bytes)
        {
            this.buffer.AddRange(bytes);
        }

        public void WriteBoolean(bool x)
        {
            this.WriteBoolean(x);
        }

        public void WriteChar(char x)
        {
            this.WriteByte((byte)x);
        }

        public void WriteFloat(float x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            if (BitConverter.IsLittleEndian && this.isBigEndian)
            {
                Array.Reverse(bytes);
            }
            this.buffer.AddRange(bytes);
        }

        public void WriteDouble(double x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            if (BitConverter.IsLittleEndian && this.isBigEndian)
            {
                Array.Reverse(bytes);
            }
            this.buffer.AddRange(bytes);
        }

        public void WriteInt16(short x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            if (BitConverter.IsLittleEndian && this.isBigEndian)
            {
                Array.Reverse(bytes);
            }
            this.buffer.AddRange(bytes);
        }

        public void WriteUInt16(ushort x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            if (BitConverter.IsLittleEndian && this.isBigEndian)
            {
                Array.Reverse(bytes);
            }
            this.buffer.AddRange(bytes);
        }

        public void WriteInt32(int v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian && this.isBigEndian)
            {
                Array.Reverse(bytes);
            }
            this.buffer.AddRange(bytes);
        }

        public void WriteUInt32(uint x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            if (BitConverter.IsLittleEndian && this.isBigEndian)
            {
                Array.Reverse(bytes);
            }
            this.buffer.AddRange(bytes);
        }

        public void WriteInt64(long x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            if (BitConverter.IsLittleEndian && this.isBigEndian)
            {
                Array.Reverse(bytes);
            }
            this.buffer.AddRange(bytes);
        }

        public void WriteUInt64(ulong x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            if (BitConverter.IsLittleEndian && this.isBigEndian)
            {
                Array.Reverse(bytes);
            }
            this.buffer.AddRange(bytes);
        }

        public void WriteString(string v)
        {
            byte[] bytes = new byte[v.Length];

            for (int x = 0; x < v.Length; x++)
            {
                bytes[x] = (byte)v[x];
            }

            this.buffer.AddRange(bytes);
        }
    }
}