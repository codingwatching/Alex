using Alex.Common.Utils.Vectors;
using Alex.Networking.Java.Util;
using fNbt;

namespace Alex.Networking.Java.Packets.Play
{
    public enum BlockEntityActionType : byte
    {
        _Init = 0,
        SetMobSpawnerData = 1,
        SetCommandBlockText = 2,
        SetBeaconData = 3,
        SetRotationAndSkinOfModHead = 4,
        DeclareConduit = 5,
        SetBannerProperties = 6,
        SetStructureData = 7,
        SetEndGatewayDimension = 8,
        SetSignText = 9,
        _ = 10,
        DeclareBed = 11,
        SetJigsawBlockData = 12,
        SetCampfireItems = 13,
        BeehiveInformation = 14
    }

    public class BlockEntityDataPacket : Packet<BlockEntityDataPacket>
    {
        public BlockCoordinates Location { get; set; }
        public BlockEntityActionType Action { get; set; }
        public NbtCompound Compound { get; set; }

        /// <inheritdoc />
        public override void Decode(MinecraftStream stream)
        {
            Location = stream.ReadBlockCoordinates();
            Action = (BlockEntityActionType)stream.ReadByte();
            Compound = stream.ReadNbtCompound();
        }

        /// <inheritdoc />
        public override void Encode(MinecraftStream stream)
        {
            throw new System.NotImplementedException();
        }
    }
}