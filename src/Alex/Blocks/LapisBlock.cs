using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class LapisBlock : Block
	{
		public LapisBlock() : base(142)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
