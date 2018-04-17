using System;

public class MRandom
{
	public UInt32 seed { get; set; }

	public uint getRand(int x, int y)
	{
		uint num = seed;
		for (uint i = 0; i < 16; i++)
		{
			num = num * 541 + (uint) x;
			num = (num << 16) | (num >> 16);
			num = num * 809 + (uint) y;
			num = (num << 16) | (num >> 16);
			num = num * 673 + i;
			num = (num << 16) | (num >> 16);
		}
		return num % 4;
	}
}