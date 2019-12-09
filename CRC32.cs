using System.IO;
using System.Security.Cryptography;

internal class CRC32 : HashAlgorithm
{
	public const uint DefaultPolynomial = 3988292384u;

	public const uint DefaultSeed = uint.MaxValue;

	private uint hash;

	private uint seed;

	private uint[] table;

	private static uint[] defaultTable;

	public override int HashSize => 32;

	public CRC32()
	{
		table = InitializeTable(3988292384u);
		seed = uint.MaxValue;
		Initialize();
	}

	public CRC32(uint polynomial, uint seed)
	{
		table = InitializeTable(polynomial);
		this.seed = seed;
		Initialize();
	}

	public override void Initialize()
	{
		hash = seed;
	}

	protected override void HashCore(byte[] buffer, int start, int length)
	{
		hash = CalculateHash(table, hash, buffer, start, length);
	}

	protected override byte[] HashFinal()
	{
		return HashValue = UInt32ToBigEndianBytes(~hash);
	}

	public static uint Compute(byte[] buffer)
	{
		return ~CalculateHash(InitializeTable(3988292384u), uint.MaxValue, buffer, 0, buffer.Length);
	}

	public static uint Compute(uint seed, byte[] buffer)
	{
		return ~CalculateHash(InitializeTable(3988292384u), seed, buffer, 0, buffer.Length);
	}

	public static uint Compute(uint polynomial, uint seed, byte[] buffer)
	{
		return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
	}

	private static uint[] InitializeTable(uint polynomial)
	{
		if (polynomial == 3988292384u && defaultTable != null)
		{
			return defaultTable;
		}
		uint[] array = new uint[256];
		for (int i = 0; i < 256; i++)
		{
			uint num = (uint)i;
			for (int j = 0; j < 8; j++)
			{
				num = (((num & 1) != 1) ? (num >> 1) : ((num >> 1) ^ polynomial));
			}
			array[i] = num;
		}
		if (polynomial == 3988292384u)
		{
			defaultTable = array;
		}
		return array;
	}

	private static uint CalculateHash(uint[] table, uint seed, byte[] buffer, int start, int size)
	{
		uint num = seed;
		for (int i = start; i < size; i++)
		{
			num = ((num >> 8) ^ table[buffer[i] ^ (num & 0xFF)]);
		}
		return num;
	}

	private byte[] UInt32ToBigEndianBytes(uint x)
	{
		return new byte[4]
		{
			(byte)((x >> 24) & 0xFF),
			(byte)((x >> 16) & 0xFF),
			(byte)((x >> 8) & 0xFF),
			(byte)(x & 0xFF)
		};
	}

	public string GetCRC(string fname)
	{
		string text = "";
		using (FileStream inputStream = File.Open(fname, FileMode.Open))
		{
			byte[] array = ComputeHash(inputStream);
			foreach (byte b in array)
			{
				text += b.ToString("x2").ToLower();
			}
			return text;
		}
	}
}
