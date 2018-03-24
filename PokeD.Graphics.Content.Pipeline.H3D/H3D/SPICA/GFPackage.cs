using System.IO;
using System.Text;

namespace SPICA.WinForms.Formats
{
    internal static class GFPackage
    {
        public struct Header
        {
            public string Magic;
            public Entry[] Entries;
        }

        public struct Entry
        {
            public uint Address;
            public int Length;
        }

        public static Header GetPackageHeader(Stream input)
        {
            var reader = new BinaryReader(input);

            var output = new Header();

            output.Magic = Encoding.ASCII.GetString(reader.ReadBytes(2));

            var entries = reader.ReadUInt16();

            output.Entries = new Entry[entries];

            var position = input.Position;

            for (var index = 0; index < entries; index++)
            {
                input.Seek(position + index * 4, SeekOrigin.Begin);

                var startAddress = reader.ReadUInt32();
                var endAddress = reader.ReadUInt32();

                var length = (int)(endAddress - startAddress);

                output.Entries[index] = new Entry
                {
                    Address = (uint)(position - 4) + startAddress,
                    Length = length
                };
            }

            return output;
        }

        public static bool IsValidPackage(Stream input)
        {
            var position = input.Position;

            var reader = new BinaryReader(input);

            var result = IsValidPackage(reader);

            input.Seek(position, SeekOrigin.Begin);

            return result;
        }

        private static bool IsValidPackage(BinaryReader reader)
        {
            if (reader.BaseStream.Length < 0x80) return false;

            var magic0 = reader.ReadByte();
            var magic1 = reader.ReadByte();

            if (magic0 < 'A' || magic0 > 'Z' ||
                magic1 < 'A' || magic1 > 'Z')
                return false;

            return true;
        }
    }
}