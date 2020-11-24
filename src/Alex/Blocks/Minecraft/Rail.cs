using Alex.API.Utils;
using Alex.API.World;

namespace Alex.Blocks.Minecraft
{
	public class Rail : Block
	{
		public Rail() : base()
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;

			RequiresUpdate = true;
			IsFullCube = false;

			Hardness = 0.7f;
		}
	}
}
