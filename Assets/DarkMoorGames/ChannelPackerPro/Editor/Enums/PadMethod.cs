using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    public enum PadMethod
    {
        [Tooltip("The alpha channel of the image will be used to pad from.")]
        Alpha,
        [Tooltip("The Uvs from the selected display mesh will be used to create an alpha channel to then pad from.")]
        Uvs
    }
}
